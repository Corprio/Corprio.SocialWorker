using System;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Serilog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Corprio.SocialWorker.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Corprio.CorprioAPIClient;
using Microsoft.Extensions.Configuration;
using Corprio.SocialWorker.Helpers;
using Corprio.SocialWorker.Dictionaries;
using Corprio.DataModel.Shared;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using Corprio.AspNetCore.Site.Services;

namespace Corprio.SocialWorker.Controllers
{
    public class WebhookController : Controller
    {
        private readonly ApplicationDbContext db;
        readonly IHttpClientFactory httpClientFactory;
        readonly IConfiguration configuration;
        
        private readonly string ApiVersion;
        private readonly string AppSecret;
        private readonly string BaseUrl;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="httpClientFactory"></param>
        /// <param name="configuration"></param>
        public WebhookController(ApplicationDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration) : base()
        {
            db = context;
            this.httpClientFactory = httpClientFactory;
            this.configuration = configuration;
            
            ApiVersion = configuration["MetaApiSetting:ApiVersion"];
            BaseUrl = configuration["MetaApiSetting:BaseUrl"];
            AppSecret = configuration["MetaApiSetting:AppSecret"];
        }

        /// <summary>
        /// Encode non-ASCII characters
        /// </summary>
        /// <param name="text">Text to be encoded</param>
        /// <returns>Encoded text</returns>
        private string EncodeNonAsciiCharacters(string text)
        {
            var sb = new StringBuilder();
            foreach (char c in text)
            {
                if (c > 127)
                {
                    string encodedValue = "\\u" + ((int)c).ToString("x4");
                    sb.Append(encodedValue);
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Convert a byte-array hash into a string for verifying webhook payload that purportedly comes from Meta
        /// </summary>
        /// <param name="bytes">Hash as a byte array</param>
        /// <returns>Hash in string format</returns>
        private string ByteArrayToString(byte[] bytes)
        {
            string hex = BitConverter.ToString(bytes);
            return hex.Replace("-", "");
        }

        /// <summary>
        /// Verify if a HTTP request really comes from Meta
        /// https://developers.facebook.com/docs/messenger-platform/webhooks
        /// </summary>
        /// <param name="request">HTTP request in question</param>
        /// <param name="metaHash">Hash included in the HTTP request headers</param>
        /// <returns>(1) True if the request is genuine and (2) the request body in string format</returns>
        public async Task<Tuple<bool, string>> HashCheck(HttpRequest request, string metaHash)
        {
            if (string.IsNullOrWhiteSpace(metaHash)) return new Tuple<bool, string>(false, string.Empty);

            // note: I didn't use Split() or Replace() because there is a chance that the hash also contains "sha256="
            int index = metaHash.IndexOf("sha256=");
            if (index >= 0) metaHash = metaHash.Substring(index + "sha256=".Length, metaHash.Length - "sha256=".Length);

            request.Body.Position = 0;
            StreamReader reader = new(request.Body);
            string requestBody = await reader.ReadToEndAsync();

            // note: we MUST compute HMAC based on escaped unicode version of the payload
            requestBody = EncodeNonAsciiCharacters(requestBody);
            var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(AppSecret));
            hmac.Initialize();
            byte[] rawHmac = hmac.ComputeHash(Encoding.UTF8.GetBytes(requestBody));
            // note: do NOT use ToBase64String()
            string computedHash = ByteArrayToString(rawHmac).ToLower();

            return new Tuple<bool, string>((computedHash == metaHash), requestBody);
        }

        /// <summary>
        /// Find the bot that was chatting with the interlocutor; if none is found, a new bot is created
        /// </summary>
        /// <param name="corprioClient">Client for executing API requests among Corprio projects</param>
        /// <param name="organizationID">Organization ID</param>
        /// <param name="facebookUser">Owner of the bot</param>
        /// <param name="interlocutorID">Meta ID of the person chatting with the bot</param>
        /// <returns>Bot</returns>
        public async Task<DbFriendlyBot> ReinventTheBot(APIClient corprioClient, Guid organizationID, MetaUser facebookUser, string interlocutorID)
        {
            DbFriendlyBot botStatus = facebookUser.Bots.FirstOrDefault(x => x.BuyerID == interlocutorID);
            if (botStatus == null)
            {
                botStatus = new DbFriendlyBot()
                {
                    ID = Guid.NewGuid(),
                    FacebookUserID = facebookUser.ID,
                    BuyerID = interlocutorID,
                    ThinkingOf = BotTopic.Limbo,
                };

                List<dynamic> existingCustomers = await corprioClient.CustomerApi.Query(
                    organizationID: organizationID,
                    selector: "new (ID)",
                    where: "EntityProperties.Any(Name==@0 && Value==@1)",
                    orderBy: "ID",
                    whereArguments: new string[] { BabelFish.CustomerEpName, interlocutorID },
                    skip: 0,
                    take: 1);
                if (existingCustomers.Count > 0)
                {
                    botStatus.BuyerCorprioID = Guid.Parse(existingCustomers[0].ID);
                }
                db.MetaBotStatuses.Add(botStatus);
                await db.SaveChangesAsync();                
            }
            return botStatus;
        }
        
        /// <summary>
        /// Respond to webhook triggered by comments on IG media items
        /// </summary>
        /// <param name="httpClient">HTTP client for executing API query</param>
        /// <param name="corprioClient">Client for Api requests among Corprio projects</param>
        /// <param name="applicationSettingService">Application setting service</param>
        /// <param name="payload">Webhook payload</param>
        /// <returns>Status code</returns>
        public async Task<IActionResult> HandleCommentWebhook(HttpClient httpClient, APIClient corprioClient,
            ApplicationSettingService applicationSettingService, CommentWebhookPayload payload)
        {
            MetaPost post;
            List<dynamic> existingProducts;
            Guid productId;
            DbFriendlyBot botStatus;
            ApplicationSetting setting;
            DomesticHelper bot;

            // note: it is possible for more than one entry/change to come in, as Meta aggregates up to 1,000 event notifications
            // see Frequency at https://developers.facebook.com/docs/graph-api/webhooks/getting-started
            foreach (CommentWebhookEntry entry in payload.Entry)
            {
                foreach (CommentWebhookChange change in entry.Changes)
                {                    
                    // assumption: we only process comment on a feed and ignore comment on another comment
                    if (change.Value.Media.MediaProductType != "FEED") continue;

                    if (null != db.CommentWebhooks.FirstOrDefault(x => x.MediaItemID == change.Value.Media.Id && x.WebhookChangeID == change.Value.WebhookChangeID))
                    {
                        Log.Information($"Ignoring duplicated webhook change {change.Value.WebhookChangeID} on media item {change.Value.Media.Id}.");
                        continue;
                    }
                                        
                    db.CommentWebhooks.Add(new CommentWebhook
                    {
                        ID = Guid.NewGuid(),                        
                        MediaItemID = change.Value.Media.Id,
                        WebhookChangeID = change.Value.WebhookChangeID,
                    });
                    await db.SaveChangesAsync();

                    post = db.MetaPosts
                        .Include(x => x.FacebookPage)
                        .ThenInclude(x => x.FacebookUser)
                        .ThenInclude(x => x.Bots)
                        .FirstOrDefault(x => x.PostId == change.Value.Media.Id && x.FacebookPage.FacebookUser.Dormant == false);
                    if (post?.FacebookPage?.FacebookUser?.OrganizationID == null)
                    {
                        Log.Error($"Failed to find media item {change.Value.Media.Id} and its parent objects.");
                        continue;
                    }

                    // note 1: we use the keyword stored at the post level, NOT at the user level, because the keyword may be udpated after a post is made
                    // note 2: if the keyword is, for example, <a+>, then it was saved as &lt;a+&gt; in DB, while the user can input either <a+> or <A+>                    
                    if (!string.Equals(post.KeywordForShoppingIntention, UtilityHelper.UncleanAndClean(change.Value.Text.Trim()), StringComparison.OrdinalIgnoreCase))
                        continue;

                    existingProducts = await corprioClient.ProductApi.Query(
                        organizationID: post.FacebookPage.FacebookUser.OrganizationID,
                        selector: "new (ID)",
                        where: "EntityProperties.Any(Name==@0 && Value==@1)",
                        orderBy: "ID",
                        whereArguments: new string[] { BabelFish.ProductEpName, post.PostId },
                        skip: 0,
                        take: 1);
                    if (existingProducts.Count == 0)
                    {
                        Log.Error($"Failed to find the product posted as post {post.PostId}.");
                        continue;
                    }
                    productId = Guid.Parse(existingProducts[0].ID);

                    setting = await applicationSettingService.GetSetting<ApplicationSetting>(post.FacebookPage.FacebookUser.OrganizationID);
                    botStatus = await ReinventTheBot(corprioClient: corprioClient, organizationID: post.FacebookPage.FacebookUser.OrganizationID,
                        facebookUser: post.FacebookPage.FacebookUser, interlocutorID: change.Value.From.Id);
                    bot = new DomesticHelper(context: db, configuration: configuration, client: corprioClient,
                        organizationID: post.FacebookPage.FacebookUser.OrganizationID,
                        botStatus: botStatus, pageName: post.FacebookPage.Name, setting: setting);
                    string message;
                    try
                    {
                        message = await bot.ReachOut(productId);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Failed to reach out to sell {productId}. {ex.Message}");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(message))
                    {
                        Log.Error($"The bot failed to provide any message.");
                        continue;
                    }
                    message = bot.ThusSpokeBabel("ChatbotSays") + message;

                    string endPoint = post.PostedWith == MetaProduct.Facebook
                        ? $"{BaseUrl}/{ApiVersion}/{post.FacebookPage.PageId}/messages"
                        : $"{BaseUrl}/{ApiVersion}/me/messages";

                    await ApiActionHelper.SendMessage(
                        httpClient: httpClient,
                        accessToken: post.FacebookPage.Token,
                        endPoint: endPoint,
                        message: message,
                        recipientId: change.Value.From.Id);
                }
            }
            return StatusCode(200);
        }

        /// <summary>
        /// Respond to webhook triggered by feeds
        /// </summary>
        /// <param name="httpClient">HTTP client for executing API query</param>
        /// <param name="corprioClient">Client for Api requests among Corprio projects</param>
        /// <param name="applicationSettingService">Application setting service</param>
        /// <param name="payload">Webhook payload</param>
        /// <returns>Status code</returns>
        public async Task<IActionResult> HandleFeedWebhook(HttpClient httpClient, APIClient corprioClient, 
            ApplicationSettingService applicationSettingService, FeedWebhookPayload payload)
        {
            MetaPost post;
            List<dynamic> existingProducts;
            Guid productId;
            DbFriendlyBot botStatus;
            ApplicationSetting setting;
            DomesticHelper bot;

            // note: it is possible for more than one entry/change to come in, as Meta aggregates up to 1,000 event notifications
            // see Frequency at https://developers.facebook.com/docs/graph-api/webhooks/getting-started
            foreach (FeedWebhookEntry entry in payload.Entry)
            {
                foreach (FeedWebhookChange change in entry.Changes)
                {                    
                    if (null != db.FeedWebhooks.FirstOrDefault(x => x.CreatedTime == change.Value.CreatedTime
                        && x.SenderID == change.Value.From.Id && x.PostID == change.Value.PostId))
                    {                        
                        Log.Information($"Ignoring duplicated webhook from {change.Value.From.Id} on post {change.Value.PostId} at {change.Value.CreatedTime}.");
                        continue;
                    }

                    db.FeedWebhooks.Add(new FeedWebhook
                    {
                        ID = Guid.NewGuid(),
                        CreatedTime = change.Value.CreatedTime,
                        PostID = change.Value.PostId,
                        SenderID = change.Value.From.Id,
                    });
                    await db.SaveChangesAsync();

                    post = db.MetaPosts
                        .Include(x => x.FacebookPage)
                        .ThenInclude(x => x.FacebookUser)
                        .ThenInclude(x => x.Bots)
                        .FirstOrDefault(x => x.PostId == change.Value.PostId && x.FacebookPage.FacebookUser.Dormant == false);
                    if (post?.FacebookPage?.FacebookUser?.OrganizationID == null)
                    {
                        Log.Error($"Failed to find post {change.Value.PostId} and its parent objects.");
                        continue;
                    }                    

                    // note 1: we use the keyword stored at the post level, NOT at the user level, because the keyword may be udpated after a post is made
                    // note 2: if the keyword is, for example, <a+>, then it was saved as &lt;a+&gt; in DB, while the user can input either <a+> or <A+>                    
                    if (!string.Equals(post.KeywordForShoppingIntention, UtilityHelper.UncleanAndClean(change.Value.Message.Trim()), StringComparison.OrdinalIgnoreCase)) 
                        continue;

                    existingProducts = await corprioClient.ProductApi.Query(
                        organizationID: post.FacebookPage.FacebookUser.OrganizationID,
                        selector: "new (ID)",
                        where: "EntityProperties.Any(Name==@0 && Value==@1)",
                        orderBy: "ID",
                        whereArguments: new string[] { BabelFish.ProductEpName, post.PostId },
                        skip: 0,
                        take: 1);
                    if (existingProducts.Count == 0)
                    {
                        Log.Error($"Failed to find the product posted as {post.PostId}.");
                        continue;
                    }
                    productId = Guid.Parse(existingProducts[0].ID);

                    setting = await applicationSettingService.GetSetting<ApplicationSetting>(post.FacebookPage.FacebookUser.OrganizationID);
                    botStatus = await ReinventTheBot(corprioClient: corprioClient, organizationID: post.FacebookPage.FacebookUser.OrganizationID,
                        facebookUser: post.FacebookPage.FacebookUser, interlocutorID: change.Value.From.Id);
                    bot = new DomesticHelper(context: db, configuration: configuration, client: corprioClient,
                        organizationID: post.FacebookPage.FacebookUser.OrganizationID,
                        botStatus: botStatus, pageName: post.FacebookPage.Name, setting: setting);
                    
                    string message;
                    try
                    {
                        message = await bot.ReachOut(productId);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Failed to reach out to sell {productId}. {ex.Message}");
                        continue;
                    }
                    if (string.IsNullOrWhiteSpace(message))
                    {
                        Log.Error($"The bot failed to provide any message.");
                        continue;
                    }
                    message = bot.ThusSpokeBabel("ChatbotSays") + message;

                    string endPoint = post.PostedWith == MetaProduct.Facebook
                        ? $"{BaseUrl}/{ApiVersion}/{post.FacebookPage.PageId}/messages"
                        : $"{BaseUrl}/{ApiVersion}/me/messages";

                    await ApiActionHelper.SendMessage(
                        httpClient: httpClient,
                        accessToken: post.FacebookPage.Token,
                        endPoint: endPoint,
                        message: message,
                        recipientId: change.Value.From.Id);
                }
            }
            return StatusCode(200);
        }

        /// <summary>
        /// Respond to webhook triggered by messages
        /// </summary>
        /// <param name="httpClient">HTTP client for executing API query</param>
        /// <param name="corprioClient">Client for Api requests among Corprio projects</param>
        /// <param name="applicationSettingService">Application setting service</param>
        /// <param name="payload">Webhook payload</param>
        /// <returns>Status code</returns>
        /// <exception cref="Exception"></exception>
        public async Task<IActionResult> HandleMessageWebhook(HttpClient httpClient, APIClient corprioClient, 
            ApplicationSettingService applicationSettingService, MessageWebhookPayload payload)
        {
            ApplicationSetting setting;
            MetaPage page;
            DbFriendlyBot botStatus;
            DomesticHelper bot;
            string response;
            string endPoint;

            // note: it is possible for more than one entry/messaging to come in, as Meta aggregates up to 1,000 event notifications
            // see Frequency at https://developers.facebook.com/docs/graph-api/webhooks/getting-started
            foreach (MessageWebhookEntry entry in payload.Entry)
            {
                foreach (MessageWebhookMessaging messaging in entry.Messaging)
                {
                    if (string.IsNullOrWhiteSpace(messaging.Message?.Text))
                    {
                        // note: we move on to the next message instead of throwing errors
                        Log.Error("The message's text is blank.");
                        continue;
                    }
                    
                    if (BabelFish.Vocab["ChatbotSays"].Any(x => messaging.Message.Text.StartsWith(x.Value)))
                    {
                        Log.Information($"Ignoring bot-generated message from {messaging.Recipient?.MetaID} to {messaging.Sender?.MetaID}");
                        continue;
                    }                    

                    if (string.IsNullOrWhiteSpace(messaging.Recipient?.MetaID) || string.IsNullOrWhiteSpace(messaging.Sender?.MetaID))
                    {
                        // note: we move on to the next message instead of throwing errors
                        Log.Error($"Recipient/sender ID for \"{messaging.Message?.Text}\" is blank.");
                        continue;
                    }                                        

                    if (messaging.Message?.IsEcho == true || messaging.Sender.MetaID == entry.WebhookEntryID)
                    {
                        Log.Information($"Ignoring echo message from {messaging.Sender.MetaID}.");
                        continue;
                    }

                    if (null != db.MessageWebhooks.FirstOrDefault(x => x.TimeStamp == messaging.Timestamp 
                        && x.SenderID == messaging.Sender.MetaID && x.RecipientID == messaging.Recipient.MetaID))
                    {                        
                        Log.Information($"Ignoring duplicated webhook from {messaging.Sender.MetaID} to {messaging.Recipient.MetaID} at {messaging.Timestamp}.");
                        continue;
                    }

                    db.MessageWebhooks.Add(new MessageWebhook
                    {
                        ID = Guid.NewGuid(),
                        RecipientID = messaging.Recipient.MetaID,
                        SenderID = messaging.Sender.MetaID,
                        TimeStamp = messaging.Timestamp,
                    });
                    await db.SaveChangesAsync();

                    page = payload.Object == "instagram"
                        ? db.MetaPages.Include(x => x.FacebookUser).ThenInclude(x => x.Bots).FirstOrDefault(x => x.InstagramID == messaging.Recipient.MetaID && x.FacebookUser.Dormant == false)
                        : db.MetaPages.Include(x => x.FacebookUser).ThenInclude(x => x.Bots).FirstOrDefault(x => x.PageId == messaging.Recipient.MetaID && x.FacebookUser.Dormant == false);
                    if (page?.FacebookUser?.OrganizationID == null)
                    {
                        // apparently Facebook may send multiple notifications for the same message,
                        // each of which uses a different sender ID and receipient ID to represent the same sender and receipient
                        Log.Error($"Failed to find page, and its associated objects, based on recipient ID {messaging.Recipient.MetaID}.");
                        Log.Information($"SenderID: {messaging.Sender.MetaID}; ReceipientID: {messaging.Recipient.MetaID}; Message: {messaging.Message.Text}");
                        continue;                                                
                    }
                    Log.Information($"Found relevant page with senderID: {messaging.Sender.MetaID}, receipientID: {messaging.Recipient.MetaID}; message: {messaging.Message.Text}");

                    setting = await applicationSettingService.GetSetting<ApplicationSetting>(page.FacebookUser.OrganizationID);

                    // assumption: senderId is the same regardless if (i) the sender made a comment on a post or (ii) sent a message via messenger
                    botStatus = await ReinventTheBot(corprioClient: corprioClient, organizationID: page.FacebookUser.OrganizationID, 
                        facebookUser: page.FacebookUser, interlocutorID: messaging.Sender.MetaID);
                    bot = new DomesticHelper(context: db, configuration: configuration, client: corprioClient, organizationID: page.FacebookUser.OrganizationID, 
                        botStatus: botStatus, detectedLocales: messaging.Message?.NLP?.DetectedLocales, pageName: page.Name, setting: setting);
                    
                    try
                    {
                        response = await bot.ThinkBeforeSpeak(messaging.Message.Text);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.Message);
                        response = bot.ThusSpokeBabel("Err_DefaultMsg");
                    }
                    // note: the bot must respond with something in 30 seconds (source: https://developers.facebook.com/docs/messenger-platform/policy/responsiveness)
                    if (string.IsNullOrWhiteSpace(response)) response = bot.ThusSpokeBabel("Err_DefaultMsg");
                    response = bot.ThusSpokeBabel("ChatbotSays") + response;

                    endPoint = payload.Object == "instagram" ? $"{BaseUrl}/{ApiVersion}/me/messages" : $"{BaseUrl}/{ApiVersion}/{messaging.Recipient.MetaID}/messages";
                    await ApiActionHelper.SendMessage(
                        httpClient: httpClient,
                        accessToken: page.Token,
                        endPoint: endPoint,
                        message: response,
                        recipientId: messaging.Sender.MetaID);
                }
            }
            return StatusCode(200);
        }

        /// <summary>
        /// Handle webhook notification sent via HTTP GET method, which normally is for verifying our callback URL
        /// </summary>
        /// <returns>Status code along with a challenge code</returns>
        [HttpGet("/webhook")]
        public IActionResult HandleWebhookGet()
        {
            string challengeCode = Request.Query.ContainsKey("hub.challenge") ? Request.Query["hub.challenge"] : string.Empty;
            return StatusCode(200, challengeCode);
        }

        /// <summary>
        /// Handle webhook notification sent via HTTP POST method
        /// </summary>
        /// <param name="applicationSettingService">Application setting service</param>
        /// <param name="httpClient">HTTP client for executing API query</param>
        /// <returns></returns>
        [HttpPost("/webhook")]
        public async Task<IActionResult> HandleWebhookPost([FromServices] ApplicationSettingService applicationSettingService, [FromServices] HttpClient httpClient)
        {            
            string hash = Request.Headers.ContainsKey("x-hub-signature-256") ? Request.Headers["x-hub-signature-256"] : string.Empty;
            (bool verified, string requestString) = await HashCheck(request: Request, metaHash: hash);
            if (!verified)
            {
                Log.Information($"Invalid webhook notification. Request: {requestString}");
                return StatusCode(200);
            }
            APIClient corprioClient = new(httpClientFactory.CreateClient("webhookClient"));
            Log.Information($"Webhook content: {requestString}");

            // if the payload includes "Messaging" and one of its element has sender, then presumably it is for webhook on messages
            MessageWebhookPayload messageWebhookPayload = requestString == null ? null : JsonConvert.DeserializeObject<MessageWebhookPayload>(requestString)!;
            if (messageWebhookPayload?.Entry?.Any(x => x.Messaging?.Any(y => !string.IsNullOrWhiteSpace(y.Sender?.MetaID)) ?? false) ?? false)
            {
                Log.Information("The webhook appears to be a message webhook.");
                return await HandleMessageWebhook(httpClient: httpClient, corprioClient: corprioClient, 
                    applicationSettingService: applicationSettingService, payload: messageWebhookPayload);
            }

            // if the payload includes "Changes" and field is "feed", then presumably it is for webhook on feed
            FeedWebhookPayload feedWebhookPayload = requestString == null ? null : JsonConvert.DeserializeObject<FeedWebhookPayload>(requestString)!;
            if (feedWebhookPayload?.Entry?.Any(x => x.Changes.Any(y => y.Field == "feed")) ?? false)
            {
                Log.Information("The webhook appears to be a feed webhook.");
                return await HandleFeedWebhook(httpClient: httpClient, corprioClient: corprioClient,
                    applicationSettingService: applicationSettingService, payload: feedWebhookPayload);
            }

            // if the payload includes "Changes" and field is "comment", then presumably it is for webhook on comment
            CommentWebhookPayload commentWebhookPayload = requestString == null ? null : JsonConvert.DeserializeObject<CommentWebhookPayload>(requestString)!;
            if (commentWebhookPayload?.Entry?.Any(x => x.Changes.Any(y => y.Field == "comments")) ?? false)
            {
                Log.Information("The webhook appears to be a comment webhook.");
                return await HandleCommentWebhook(httpClient: httpClient, corprioClient: corprioClient,
                    applicationSettingService: applicationSettingService, payload: commentWebhookPayload);
            }

            Log.Information("Cannot recognize payload in webhook notification.");
            return StatusCode(200);
        }
        
        /// <summary>
        /// Trigger the publication of a catalogue / product list (WIP - this function is expected to cater API requests from other Apps)
        /// </summary>
        /// <param name="httpClient">HTTP client for executing API query</param>
        /// <param name="organizationID">Organization ID</param>
        /// <param name="productlistID">Entity ID of product list</param>        
        /// <returns>Status code</returns>        
        [HttpPost("/{organizationID:guid}/PublishCatalogue/{productlistID:guid}")]
        public async Task<IActionResult> TriggerPostingCatalogue([FromServices] HttpClient httpClient, [FromRoute] Guid organizationID,
            [FromHeader] CorprioRequestHeader header, [FromBody] ComputeHashRequest body, [FromRoute] Guid productlistID)
        {
            APIClient corprioClient = new(httpClientFactory.CreateClient("webhookClient"));
            var hashRequest = new ComputeHashRequest()
            {
                OrganizationID = body.OrganizationID,
                Payload = System.Text.Json.JsonSerializer.Serialize(body),
                RequestApplicationID = body.RequestApplicationID,
                RequestUserID = body.RequestUserID,
                Timestamp = body.Timestamp,
                RequiredDataPermissions = body.RequiredDataPermissions,
            };
            bool isValidHash = await corprioClient.ApplicationApi.ValidateHash(computeHashRequest: hashRequest, hash: header.Hash);
            if (!isValidHash) return StatusCode(401);
            return StatusCode(200);
            //(bool success, List<string> errorMessages) = await PublishCatalogue(httpClient: httpClient, corprioClient: corprioClient,
            //    organizationID: organizationID, productlistID: productlistID);
            //return success ? StatusCode(200) : StatusCode(400, errorMessages);
        }

        /// <summary>
        /// Trigger the publication of a product (WIP - this function is expected to cater API requests from other Apps)
        /// </summary>
        /// <param name="httpClient">HTTP client for executing API query</param>        
        /// <param name="organizationID">Organization ID</param>
        /// <param name="productID">Entity ID of product</param>
        /// <returns>Status code</returns>
        [HttpPost("/{organizationID:guid}/PublishProduct/{productID:guid}")]
        public async Task<IActionResult> TriggerPostingProduct([FromServices] HttpClient httpClient, [FromRoute] Guid organizationID, 
            [FromHeader] CorprioRequestHeader header, [FromBody] ComputeHashRequest body, [FromRoute] Guid productID)
        {            
            APIClient corprioClient = new(httpClientFactory.CreateClient("webhookClient"));            
            bool isValidHash = await corprioClient.ApplicationApi.ValidateHash(computeHashRequest: body, hash: header.Hash);
            if (!isValidHash) return StatusCode(401);
            return StatusCode(200);
            //(bool success, List<string> errorMessages) = await PublishProduct(httpClient: httpClient, corprioClient: corprioClient,
            //    organizationID: organizationID, productID: productID);
            //return success ? StatusCode(200) : StatusCode(400, errorMessages);
        }
    }
}

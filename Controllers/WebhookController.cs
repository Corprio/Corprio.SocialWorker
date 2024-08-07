﻿using System;
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
using Corprio.DataModel.Business;
using Corprio.Core;
using Corprio.SocialWorker.Services;
using Line.Messaging.Webhooks;
using Corprio.SocialWorker.Models.Meta;
using Corprio.SocialWorker.Models.Line;

namespace Corprio.SocialWorker.Controllers
{
    /// <summary>
    /// Handles webhook notifications
    /// </summary>
    public class WebhookController : Controller
    {
        private readonly ApplicationDbContext _db;
        readonly IHttpClientFactory _httpClientFactory;
        readonly IConfiguration _configuration;
        readonly EmailHelper _emailHelper;        
        
        readonly string _metaApiVersion;
        readonly string _metaAppSecret;
        readonly string _metaBaseUrl;        
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="httpClientFactory"></param>
        /// <param name="configuration"></param>
        public WebhookController(ApplicationDbContext context, IHttpClientFactory httpClientFactory, 
            IConfiguration configuration, EmailHelper emailHelper) : base()
        {
            _db = context;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _emailHelper = emailHelper;            

            _metaApiVersion = configuration["MetaApiSetting:ApiVersion"];
            _metaBaseUrl = configuration["MetaApiSetting:BaseUrl"];
            _metaAppSecret = configuration["MetaApiSetting:AppSecret"];            
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
            var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_metaAppSecret));
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
        /// <param name="facebookUser">Owner of the bot</param>
        /// <param name="interlocutorID">Meta ID of the person chatting with the bot</param>
        /// <param name="metaUsername">Meta username of the person chatting with the bot</param>
        /// <param name="unMute">If true, unmute the bot so that it reads and responds to webhook payload</param>
        /// <returns>Bot</returns>
        public async Task<DbFriendlyBot> ReinventTheBot(APIClient corprioClient, MetaUser facebookUser, 
            string interlocutorID, string metaUsername = null, bool unMute = false)
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
                    BuyerUserName = metaUsername
                };

                List<dynamic> existingCustomers = await corprioClient.CustomerApi.Query(
                    organizationID: facebookUser.OrganizationID,
                    selector: "new (ID)",
                    where: "EntityProperties.Any(Name==@0 && Value==@1)",
                    orderBy: "ID",
                    whereArguments: new string[] { BabelFish.MetaCustomerEpName, interlocutorID },
                    skip: 0,
                    take: 1);
                if (existingCustomers.Count > 0)
                {
                    botStatus.BuyerCorprioID = Guid.Parse(existingCustomers[0].ID);
                }
                _db.BotStatuses.Add(botStatus);
                await _db.SaveChangesAsync();
            }
            else
            {
                bool updated = false;
                if (unMute && botStatus.IsMuted)
                {
                    updated = true;
                    botStatus.IsMuted = false;
                }

                if (!string.IsNullOrEmpty(metaUsername) && metaUsername != botStatus.BuyerUserName)
                {
                    updated = true;
                    botStatus.BuyerUserName = metaUsername;
                }

                if (updated)
                {
                    _db.BotStatuses.Update(botStatus);
                    await _db.SaveChangesAsync();
                }                
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
                    if (entry.WebhookEntryID == change.Value?.From?.Id)
                    {
                        Log.Information("Ignoring comment changes triggered by the IG business account itself.");
                        continue;
                    }
                                                                                
                    if (string.IsNullOrWhiteSpace(change.Value?.Text) || change.Value?.Media?.MediaProductType != "FEED")
                    {                        
                        Log.Information("Ignoring comment changes that contain no text (e.g., LIKE) or are NOT on feed level (e.g., comment on another comment.");
                        continue;
                    }

                    if (null != _db.MetaCommentWebhooks.FirstOrDefault(x => x.MediaItemID == change.Value.Media.Id && x.WebhookChangeID == change.Value.WebhookChangeID))
                    {
                        Log.Information($"Ignoring duplicated webhook change {change.Value.WebhookChangeID} on media item {change.Value.Media.Id}.");
                        continue;
                    }
                                        
                    _db.MetaCommentWebhooks.Add(new MetaCommentWebhook
                    {
                        ID = Guid.NewGuid(),                        
                        MediaItemID = change.Value.Media.Id,
                        WebhookChangeID = change.Value.WebhookChangeID,
                    });
                    await _db.SaveChangesAsync();

                    post = _db.MetaPosts
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
                    {
                        Log.Information($"Ignoring message {change.Value.Text} that does not indicate shopping intention.");
                        continue;
                    }

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
                    botStatus = await ReinventTheBot(
                        corprioClient: corprioClient,                         
                        facebookUser: post.FacebookPage.FacebookUser, 
                        interlocutorID: change.Value.From.Id, 
                        metaUsername: change.Value.From.Username,
                        unMute: true);
                    bot = new DomesticHelper(context: _db, configuration: _configuration, client: corprioClient,
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
                    message += BabelFish.RobotEmoji;

                    string endPoint = post.PostedWith == MetaProduct.Facebook
                        ? $"{_metaBaseUrl}/{_metaApiVersion}/{post.FacebookPage.PageId}/messages"
                        : $"{_metaBaseUrl}/{_metaApiVersion}/me/messages";
                    
                    await ApiActionHelper.SendMessage(
                        httpClient: httpClient,
                        accessToken: post.FacebookPage.Token,
                        messageEndPoint: endPoint,
                        threadEndPoint: $"{_metaBaseUrl}/{_metaApiVersion}/{post.FacebookPage.PageId}/release_thread_control",
                        message: message,
                        recipientId: change.Value.From.Id);
                }
            }
            return StatusCode(200);
        }

        /// <summary>
        /// Respond to webhook triggered by changes on feed (e.g., post, like)
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
                    if (entry.WebhookEntryID == change.Value?.From?.Id)
                    {
                        Log.Information("Ignoring feed changes triggered by the page itself.");
                        continue;
                    }
                    
                    if (change.Value?.Item != "comment" || string.IsNullOrWhiteSpace(change.Value?.Message))
                    {
                        Log.Information("Ignoring feed changes that are not comments or contain no message.");
                        continue;
                    }
                    
                    if (null != _db.MetaFeedWebhooks.FirstOrDefault(x => x.CreatedTime == change.Value.CreatedTime
                        && x.SenderID == change.Value.From.Id && x.PostID == change.Value.PostId))
                    {                        
                        Log.Information($"Ignoring duplicated webhook from {change.Value.From.Id} on post {change.Value.PostId} at {change.Value.CreatedTime}.");
                        continue;
                    }

                    _db.MetaFeedWebhooks.Add(new MetaFeedWebhook
                    {
                        ID = Guid.NewGuid(),
                        CreatedTime = change.Value.CreatedTime,
                        PostID = change.Value.PostId,
                        SenderID = change.Value.From.Id,
                    });
                    await _db.SaveChangesAsync();

                    post = _db.MetaPosts
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
                    {
                        Log.Information($"Ignoring message {change.Value.Message} that does not indicate shopping intention.");
                        continue;
                    }

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
                    botStatus = await ReinventTheBot(
                        corprioClient: corprioClient,                         
                        facebookUser: post.FacebookPage.FacebookUser, 
                        interlocutorID: change.Value.From.Id, 
                        metaUsername: change.Value.From.Name,
                        unMute: true);
                    bot = new DomesticHelper(context: _db, configuration: _configuration, client: corprioClient,
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
                    message += BabelFish.RobotEmoji;

                    string messageEndPoint = post.PostedWith == MetaProduct.Facebook
                        ? $"{_metaBaseUrl}/{_metaApiVersion}/{post.FacebookPage.PageId}/messages"
                        : $"{_metaBaseUrl}/{_metaApiVersion}/me/messages";                    

                    MessageFeedback feedback = await ApiActionHelper.SendMessage(
                        httpClient: httpClient,
                        accessToken: post.FacebookPage.Token,
                        messageEndPoint: messageEndPoint,
                        threadEndPoint: $"{_metaBaseUrl}/{_metaApiVersion}/{post.FacebookPage.PageId}/release_thread_control",
                        message: message,
                        recipientId: change.Value.From.Id);

                    // if the message failed to be sent, make a reply to the post
                    if (feedback == null && !string.IsNullOrWhiteSpace(change.Value.CommentId))
                    {
                        await ApiActionHelper.PostOrComment(
                            httpClient: httpClient, 
                            accessToken: post.FacebookPage.Token, 
                            endPoint: $"{_metaBaseUrl}/{_metaApiVersion}/{change.Value.CommentId}/comments", 
                            message: string.Format(Resources.SharedResource.AutoReplyComment, setting.KeywordForShoppingIntention));

                    }
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
        public async Task<IActionResult> HandleMetaMessageWebhook(HttpClient httpClient, APIClient corprioClient, 
            ApplicationSettingService applicationSettingService, MessageWebhookPayload payload)
        {
            ApplicationSetting setting;
            MetaPage page;
            DbFriendlyBot botStatus;
            DomesticHelper bot;
            string response;
            string endPoint;
            bool isStoryMention;
            string messageText;

            // note: it is possible for more than one entry/messaging to come in, as Meta aggregates up to 1,000 event notifications
            // see Frequency at https://developers.facebook.com/docs/graph-api/webhooks/getting-started
            foreach (MessageWebhookEntry entry in payload.Entry)
            {
                foreach (MessageWebhookMessaging messaging in entry.Messaging)
                {                    
                    if (string.IsNullOrWhiteSpace(messaging.Recipient?.MetaID) || string.IsNullOrWhiteSpace(messaging.Sender?.MetaID))
                    {
                        // note: we move on to the next message instead of throwing errors
                        Log.Information($"Ignoring webhook entry without sender/recipient's ID.");
                        continue;
                    }                                        

                    if (messaging.Message?.IsEcho == true || messaging.Sender.MetaID == entry.WebhookEntryID)
                    {
                        Log.Information($"Ignoring echo message from {messaging.Sender.MetaID}.");
                        continue;
                    }

                    if (null != _db.MetaMessageWebhooks.FirstOrDefault(x => x.TimeStamp == messaging.Timestamp 
                        && x.SenderID == messaging.Sender.MetaID && x.RecipientID == messaging.Recipient.MetaID))
                    {                        
                        Log.Information($"Ignoring duplicated webhook from {messaging.Sender.MetaID} to {messaging.Recipient.MetaID} at {messaging.Timestamp}.");
                        continue;
                    }

                    isStoryMention = payload.Object == "instagram" && (messaging.Message.Attachments?.Any(a => a.Type == "story_mention") ?? false);                    
                    messageText = messaging.Message?.IsDeleted == true 
                        ? BabelFish.KillCode  // regardless of which message the customer unsent, we take it as an attempt to cancel the current operation
                        : string.IsNullOrEmpty(messaging.Message?.Text) ? string.Empty : messaging.Message.Text;
                    messageText = messageText.Trim();

                    // check if the message is empty or bot-generated ONLY IF it is NOT a story_metnion
                    if (!isStoryMention)
                    {
                        if (string.IsNullOrWhiteSpace(messageText))
                        {
                            Log.Information("Ignoring message that does not contain text (e.g., thumb-up).");
                            continue;
                        }

                        if (messageText.EndsWith(BabelFish.RobotEmoji))
                        {
                            Log.Information($"Ignoring bot-generated message from {messaging.Sender?.MetaID} to {messaging.Recipient?.MetaID}");
                            continue;
                        }
                        Log.Information($"The webhook appears to be for {payload.Object} direct message.");
                    }
                    else
                    {
                        Log.Information("The webhook appears to be for Instagram story mention.");
                    }

                    // "remember" which webhooks have been processed
                    _db.MetaMessageWebhooks.Add(new MetaMessageWebhook
                    {
                        ID = Guid.NewGuid(),
                        RecipientID = messaging.Recipient.MetaID,
                        SenderID = messaging.Sender.MetaID,
                        TimeStamp = messaging.Timestamp,
                    });
                    await _db.SaveChangesAsync();

                    page = payload.Object == "instagram"
                        ? _db.MetaPages.Include(x => x.FacebookUser).ThenInclude(x => x.Bots).FirstOrDefault(x => x.InstagramID == messaging.Recipient.MetaID && x.FacebookUser.Dormant == false)
                        : _db.MetaPages.Include(x => x.FacebookUser).ThenInclude(x => x.Bots).FirstOrDefault(x => x.PageId == messaging.Recipient.MetaID && x.FacebookUser.Dormant == false);
                    if (page?.FacebookUser?.OrganizationID == null)
                    {
                        // apparently Facebook may send multiple notifications for the same message,
                        // each of which uses a different sender ID and recipient ID to represent the same sender and recipient
                        Log.Error($"Failed to find page, and its associated objects, based on recipient ID {messaging.Recipient.MetaID}.");
                        continue;
                    }
                    Log.Information($"Found relevant page with recipientID: {messaging.Recipient.MetaID}; senderID: {messaging.Sender.MetaID}, message: {messaging.Message.Text}");

                    setting = await applicationSettingService.GetSetting<ApplicationSetting>(page.FacebookUser.OrganizationID);

                    // confirmed via testing: senderId is the same regardless if the sender (i) made a comment on a post
                    // or (ii) sent a message via messenger
                    botStatus = await ReinventTheBot(corprioClient: corprioClient,
                        facebookUser: page.FacebookUser,
                        interlocutorID: messaging.Sender.MetaID);

                    if (string.IsNullOrWhiteSpace(botStatus.BuyerUserName))
                    {
                        endPoint = payload.Object == "instagram" ? $"{_metaBaseUrl}/{_metaApiVersion}/{messaging.Sender.MetaID}?fields=username" : $"{_metaBaseUrl}/{_metaApiVersion}/{messaging.Sender.MetaID}?fields=name";
                        string queryResult = await ApiActionHelper.GetQuery(httpClient: httpClient, accessToken: page.Token, endPoint: endPoint);
                        if (!string.IsNullOrWhiteSpace(queryResult))
                        {
                            botStatus.BuyerUserName = payload.Object == "instagram"
                                ? JsonConvert.DeserializeObject<IgUser>(queryResult)?.Username
                                : JsonConvert.DeserializeObject<FbUser>(queryResult)?.Name;
                        }
                        _db.BotStatuses.Update(botStatus);
                        await _db.SaveChangesAsync();
                    }                                        

                    // if muted, either unmute or do nothing
                    if (botStatus.IsMuted)
                    {
                        if (messageText.Equals(BabelFish.BotSummon, StringComparison.OrdinalIgnoreCase) || messageText.Equals(BabelFish.BotSummon_CN, StringComparison.OrdinalIgnoreCase))
                        {
                            Log.Information($"The interlocutor {messaging.Sender.MetaID} has opted in to receiving automated responses from Facebook user {page.FacebookUser.FacebookUserID}.");

                            bot = new DomesticHelper(context: _db, configuration: _configuration, client: corprioClient, organizationID: page.FacebookUser.OrganizationID,
                                botStatus: botStatus, detectedLocales: messaging.Message?.NLP?.DetectedLocales, pageName: page.Name, setting: setting);

                            //await ApiActionHelper.ControlConversationThread(httpClient: httpClient, accessToken: page.Token, 
                            //    endPoint: $"{BaseUrl}/{ApiVersion}/{page.PageId}/request_thread_control",
                            //    interlocutorId: messaging.Sender.MetaID);

                            endPoint = payload.Object == "instagram" ? $"{_metaBaseUrl}/{_metaApiVersion}/me/messages" : $"{_metaBaseUrl}/{_metaApiVersion}/{messaging.Recipient.MetaID}/messages";
                            await ApiActionHelper.SendMessage(
                                httpClient: httpClient,
                                accessToken: page.Token,
                                messageEndPoint: endPoint,
                                threadEndPoint: $"{_metaBaseUrl}/{_metaApiVersion}/{page.PageId}/release_thread_control",
                                message: await bot.getWoke(),
                                recipientId: messaging.Sender.MetaID);                            
                        }
                        else
                        {
                            Log.Information($"The interlocutor {messaging.Sender.MetaID} has opted out of receiving automated responses from Facebook user {page.FacebookUser.FacebookUserID}.");
                        }
                        
                        continue;
                    }

                    bot = new DomesticHelper(context: _db, configuration: _configuration, client: corprioClient, organizationID: page.FacebookUser.OrganizationID,
                        botStatus: botStatus, detectedLocales: messaging.Message?.NLP?.DetectedLocales, pageName: page.Name, setting: setting);

                    // handle 'Story Mentions' to meet Facebook App review requirement
                    if (isStoryMention)
                    {
                        try
                        {
                            response = await bot.AcknowledgeMention();
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"The bot failed to generate a message. Error: {ex.Message}");
                            response = bot.ThusSpokeBabel("Err_DefaultMsg");
                        }
                        // note: the bot must respond with something in 30 seconds (source: https://developers.facebook.com/docs/messenger-platform/policy/responsiveness)
                        if (string.IsNullOrWhiteSpace(response)) response = bot.ThusSpokeBabel("Err_DefaultMsg");
                        response += BabelFish.RobotEmoji;

                        await ApiActionHelper.SendMessage(
                            httpClient: httpClient,
                            accessToken: page.Token,
                            messageEndPoint: $"{_metaBaseUrl}/{_metaApiVersion}/me/messages",
                            threadEndPoint: $"{_metaBaseUrl}/{_metaApiVersion}/{page.PageId}/release_thread_control",
                            message: response,
                            recipientId: messaging.Sender.MetaID);
                        
                        // save the CDN URL
                        string igNameQueryResult = await ApiActionHelper.GetQuery(
                            httpClient: httpClient, 
                            accessToken: page.Token, 
                            endPoint: $"{_metaBaseUrl}/{_metaApiVersion}/{messaging.Sender.MetaID}?fields=username");

                        var metaMention = new MetaMention()
                        {
                            ID = Guid.NewGuid(),
                            FacebookPageID = page.ID,
                            CreatorID = messaging.Sender.MetaID,
                            CreatorName = botStatus.BuyerUserName,
                            CDNUrl = messaging.Message.Attachments[0].Payload.URL,
                            Mentioned = string.IsNullOrWhiteSpace(igNameQueryResult) ? string.Empty : JsonConvert.DeserializeObject<IgUser>(igNameQueryResult)?.Username,
                        };
                        _db.MetaMentions.Add(metaMention);
                        await _db.SaveChangesAsync();

                        //// re-do the following part
                        //OrganizationCoreInfo coreInfo = await corprioClient.OrganizationApi.GetCoreInfo(page.FacebookUser.OrganizationID);
                        //if (coreInfo == null)
                        //{
                        //    Log.Error("Failed to retrieve the organization email address.");
                        //    continue;
                        //}

                        //var email = new Core.EmailMessage()
                        //{
                        //    ToEmails = [coreInfo.EmailAddress],
                        //    Subject = "[" + Resources.SharedResource.AppName + "] " + string.Format(Resources.SharedResource.StoryMention_EmailSubject, page.Name),
                        //    Body = string.Format(Resources.SharedResource.StoryMention_EmailBody, page.Name, botStatus.MetaUserName, messaging.Sender.MetaID, messaging.Message.Attachments[0].Payload?.URL)
                        //};

                        //try
                        //{
                        //    await emailHelper.SendEmailAsync(email);
                        //}
                        //catch (Exception ex)
                        //{
                        //    Log.Error($"Failed to notify the merchant about the story mention. {ex.Message}");
                        //}

                        continue;
                    }
                    
                    // escalate to human agent if the user requests to speak with a human rather than a bot
                    if (messageText.Equals(BabelFish.SOS, StringComparison.OrdinalIgnoreCase) || messageText.Equals(BabelFish.SOS_CN, StringComparison.OrdinalIgnoreCase))
                    {
                        Log.Information($"The interlocutor elected to speak with human agent by inputting {messageText}");

                        botStatus.IsMuted = true;
                        _db.BotStatuses.Update(botStatus);
                        await _db.SaveChangesAsync();

                        endPoint = payload.Object == "instagram" ? $"{_metaBaseUrl}/{_metaApiVersion}/me/messages" : $"{_metaBaseUrl}/{_metaApiVersion}/{messaging.Recipient.MetaID}/messages";
                        await ApiActionHelper.SendMessage(
                            httpClient: httpClient,
                            accessToken: page.Token,
                            messageEndPoint: endPoint,
                            threadEndPoint: $"{_metaBaseUrl}/{_metaApiVersion}/{page.PageId}/release_thread_control",
                            message: bot.ThusSpokeBabel("Escalated"),
                            recipientId: messaging.Sender.MetaID);

                        //if (payload.Object == "instagram")
                        //{                            
                        //    await ApiActionHelper.ControlConversationThread(httpClient: httpClient, accessToken: page.Token,
                        //        endPoint: $"{BaseUrl}/{ApiVersion}/{page.PageId}/pass_thread_control",
                        //        interlocutorId: messaging.Sender.MetaID,
                        //        targetAppId: "1217981644879628");
                        //}

                        string platform = payload.Object == "instagram" ? "Instagram" : "Facebook";
                        string customerName = string.IsNullOrWhiteSpace(botStatus.BuyerUserName) ? Resources.SharedResource.Unknown_Italic : botStatus.BuyerUserName;
                        string customerEmail = string.IsNullOrWhiteSpace(botStatus.BuyerEmail) ? Resources.SharedResource.Unknown_Italic : botStatus.BuyerEmail;
                        OrganizationCoreInfo org = await corprioClient.OrganizationApi.GetCoreInfo(page.FacebookUser.OrganizationID);
                        if (org == null)
                        {
                            Log.Error("Failed to retrieve the organization email address.");
                            continue;
                        }

                        var email = new Core.EmailMessage()
                        {                            
                            ToEmails = [org.EmailAddress],
                            Subject = "[" + Resources.SharedResource.AppName + "] " + string.Format(Resources.SharedResource.HumanEscalation_EmailSubject, page.Name, platform),
                            Body = string.Format(Resources.SharedResource.HumanEscalation_EmailBody, platform, customerName, messaging.Sender.MetaID, customerEmail, page.Name)
                        };

                        try
                        {
                            await _emailHelper.SendEmailAsync(email);
                        }
                        catch (Exception ex) 
                        {
                            Log.Error($"Failed to escalate to the merchant. {ex.Message}");
                        }

                        continue;
                    }                    
                    
                    // reply to direct message that does NOT involve human agent escalation
                    try
                    {
                        response = await bot.ThinkBeforeSpeak(messageText);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"The bot failed to generate a message. Error: {ex.Message}");
                        response = bot.ThusSpokeBabel("Err_DefaultMsg");
                    }
                    // note: the bot must respond with something in 30 seconds (source: https://developers.facebook.com/docs/messenger-platform/policy/responsiveness)
                    if (string.IsNullOrWhiteSpace(response)) response = bot.ThusSpokeBabel("Err_DefaultMsg");
                    response += BabelFish.RobotEmoji;

                    endPoint = payload.Object == "instagram" ? $"{_metaBaseUrl}/{_metaApiVersion}/me/messages" : $"{_metaBaseUrl}/{_metaApiVersion}/{messaging.Recipient.MetaID}/messages";
                    await ApiActionHelper.SendMessage(
                        httpClient: httpClient,
                        accessToken: page.Token,
                        messageEndPoint: endPoint,
                        threadEndPoint: $"{_metaBaseUrl}/{_metaApiVersion}/{page.PageId}/release_thread_control",
                        message: response,
                        recipientId: messaging.Sender.MetaID);
                }
            }
            return StatusCode(200);
        }

        /// <summary>
        /// Handle webhook notifications from Line
        /// Even dummy notification from Line (for testing endpoint) is sent via HTTP POST method
        /// </summary>
        /// <param name="applicationSettingService"></param>
        /// <param name="organizationID"></param>
        /// <param name="channelID"></param>
        /// <returns></returns>
        [HttpPost("{organizationID:guid}/{channelID:guid}/line")]
        public async Task<IActionResult> HandleLineWebhookPost([FromServices] ApplicationSettingService applicationSettingService, 
            [FromRoute] Guid organizationID, [FromRoute] Guid channelID)
        {
            LineChannel channel = _db.LineChannels.FirstOrDefault(
                r => r.OrganizationID == organizationID && r.ID == channelID && r.Dormant == false);
            if (channel == null) return BadRequest("Channel not found.");

            string hash = Request.Headers?.ContainsKey("x-line-signature") ?? false 
                ? Request.Headers["x-line-signature"] 
                : string.Empty;

            Request.Body.Position = 0;
            StreamReader reader = new(Request.Body);
            string requestBody = await reader.ReadToEndAsync();

            var lineApiService = new LineApiService(context: _db, 
                httpClientFactory: _httpClientFactory,                 
                configuration: _configuration, 
                lineChannel: channel,
                applicationSettingService: applicationSettingService,
                emailHelper: _emailHelper);

            if (!lineApiService.ValidateWebhook(hash: hash, requestBody: requestBody)) 
                return BadRequest("Invalid webhook.");


            dynamic val = JsonConvert.DeserializeObject(requestBody);
            IEnumerable<WebhookEvent> events = WebhookEventParser.ParseEvents(val.events);

            if (events == null)
            {
                Log.Information("Line's webhook content contains null events.");
                return Ok();
            }
            await lineApiService.RunAsync(events);

            return Ok();
        }

        /// <summary>
        /// Handle webhook notification sent via HTTP GET method, which normally is for verifying our callback URL
        /// </summary>
        /// <returns>Status code along with a challenge code</returns>
        [HttpGet("/webhook")]
        public IActionResult HandleMetaWebhookGet()
        {
            string challengeCode = Request.Query.ContainsKey("hub.challenge") ? Request.Query["hub.challenge"] : string.Empty;
            return StatusCode(200, challengeCode);
        }

        /// <summary>
        /// Handle Meta webhook notification sent via HTTP POST method
        /// </summary>
        /// <param name="applicationSettingService">Application setting service</param>
        /// <param name="httpClient">HTTP client for executing API query</param>
        /// <returns></returns>
        [HttpPost("/webhook")]
        public async Task<IActionResult> HandleMetaWebhookPost([FromServices] ApplicationSettingService applicationSettingService, [FromServices] HttpClient httpClient)
        {
            string hash = Request.Headers.ContainsKey("x-hub-signature-256") ? Request.Headers["x-hub-signature-256"] : string.Empty;
            (bool verified, string requestString) = await HashCheck(request: Request, metaHash: hash);
            if (!verified)
            {
                Log.Information($"Invalid webhook notification. Request: {requestString}");
                return StatusCode(200);
            }
            APIClient corprioClient = new(_httpClientFactory.CreateClient("webhookClient"));
            Log.Information($"Webhook content: {requestString}");

            // if the payload includes "Messaging" and one of its element has sender, then presumably it is for webhook on messages
            MessageWebhookPayload messageWebhookPayload;
            try
            {
                messageWebhookPayload = JsonConvert.DeserializeObject<MessageWebhookPayload>(requestString);
            }
            catch
            {
                messageWebhookPayload = null;
            }            
            if (messageWebhookPayload?.Entry?.Any(e => e.Messaging?.Any(m => !string.IsNullOrWhiteSpace(m.Sender?.MetaID)) ?? false) ?? false)
            {
                Log.Information("The webhook appears to be a message webhook.");
                return await HandleMetaMessageWebhook(httpClient: httpClient, corprioClient: corprioClient, 
                    applicationSettingService: applicationSettingService, payload: messageWebhookPayload);
            }            

            //// if the payload includes "Changes" and field is "feed", then presumably it is for webhook on feed
            //FeedWebhookPayload feedWebhookPayload = requestString == null ? null : JsonConvert.DeserializeObject<FeedWebhookPayload>(requestString);
            //if (feedWebhookPayload?.Object == "page" && (feedWebhookPayload?.Entry?.Any(x => x.Changes.Any(y => y.Field == "feed")) ?? false))
            //{
            //    Log.Information("The webhook appears to be a feed webhook.");
            //    return await HandleFeedWebhook(httpClient: httpClient, corprioClient: corprioClient,
            //        applicationSettingService: applicationSettingService, payload: feedWebhookPayload);
            //}

            //// if the payload includes "Changes" and field is "comment", then presumably it is for webhook on comment
            //CommentWebhookPayload commentWebhookPayload = requestString == null ? null : JsonConvert.DeserializeObject<CommentWebhookPayload>(requestString);
            //if (commentWebhookPayload?.Object == "instagram" && (commentWebhookPayload?.Entry?.Any(x => x.Changes.Any(y => y.Field == "comments")) ?? false))
            //{
            //    Log.Information("The webhook appears to be a comment webhook.");
            //    return await HandleCommentWebhook(httpClient: httpClient, corprioClient: corprioClient,
            //        applicationSettingService: applicationSettingService, payload: commentWebhookPayload);
            //}

            Log.Information("Cannot recognize the payload in webhook notification.");
            return StatusCode(200);
        }        
    }
}

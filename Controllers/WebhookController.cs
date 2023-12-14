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
using System.Dynamic;
using Microsoft.Extensions.Configuration;
using Corprio.SocialWorker.Helpers;
using Corprio.SocialWorker.Dictionaries;
using Corprio.AspNetCore.Site.Filters;
using Corprio.DataModel;
using Corprio.Core.Exceptions;
using Corprio.DataModel.Business.Products.ViewModel;
using Corprio.DataModel.Business.Products;
using Corprio.DataModel.Business;
using Corprio.DataModel.Shared;

namespace Corprio.SocialWorker.Controllers
{
    public class WebhookController : Controller
    {
        readonly IHttpClientFactory httpClientFactory;
        readonly IConfiguration configuration;
        
        private readonly string ApiVersion;
        private readonly string AppSecret;
        private readonly string BaseUrl;

        public WebhookController(IHttpClientFactory httpClientFactory, IConfiguration configuration) : base()
        {
            this.httpClientFactory = httpClientFactory;
            this.configuration = configuration;
            
            ApiVersion = configuration["MetaApiSetting:ApiVersion"];
            BaseUrl = configuration["MetaApiSetting:BaseUrl"];
            AppSecret = "b8956ab840c1d15a8bb0bee5551a6ccb"; // Put this in a vault!!!
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
        /// Respond to webhook triggered by feeds
        /// </summary>
        /// <param name="httpClient">HTTP client for executing API query</param>
        /// <param name="payload">Webhook payload</param>
        /// <param name="payload">Webhook payload</param>
        /// <returns>Status code</returns>
        public async Task<IActionResult> HandleFeedWebhook(HttpClient httpClient, APIClient corprioClient, FeedWebhookPayload payload)
        {
            // note: it is possible for more than one entry/change to come in, as Meta aggregates up to 1,000 event notifications
            // see Frequency at https://developers.facebook.com/docs/graph-api/webhooks/getting-started
            foreach (var entry in payload.Entry)
            {
                foreach (var change in entry.Changes)
                {
                    //var bot = new DomesticHelper(client: corprioClient, organizationID: Guid.Parse("3C83FE2B-FE0A-420C-AF24-E76FCF5BD7E7"), botStatus: new MetaChatBot() { Id = "24286888740958239" }, pageName: "Yo-Yo");
                    //Guid productId = Guid.Parse("AF112F6B-2ABC-431C-A16F-ED4487BFD71F");
                    //MetaPost post = new() { IsFbPost = false };
                    //MetaPage page = new() { Token = "EAAEcibYeGIUBOZCjbwHaILofgZAajzBlzX9PFbDZCpLKafFPQjmrmEaNVZAVskYLKGNvSd0g8BT4DgbQAeRFoHf26sscYNGzCpjz7sfyFHuBRk6c5zvechmGliH2NHLO4JOaccoANwAvEAoT7GiJiW7QqAo204J4i3fZCOA2yDaNZCsdHomAEfKBOlaZBSH" };

                    if (!BabelFish.Buy.Contains(change.Value.Message.ToUpper().Trim())) continue;

                    // we use the post ID to find the organization ID, then use both IDs to find the page ID,
                    // and finally use the page ID to find the token
                    List<dynamic> queryResult = await corprioClient.ApplicationSubscriptionApi.QueryCustomData(
                        new DataModel.DynamicQuery
                        {
                            Selector = "new (ApplicationSubscription.OrganizationID)",
                            Where = "Key=@0",
                            WhereArguments = new object[] { change.Value.PostId }
                        });
                    Guid organizationID = UtilityHelper.GetGuidFromDynamicQueryResult(queryResult, "OrganizationID");
                    if (organizationID == Guid.Empty)
                    {
                        Log.Error($"Failed to get organization ID based on post ID {change.Value.PostId}.");
                        continue;
                    }
                    MetaPost post = null;
                    string customData = await corprioClient.ApplicationSubscriptionApi.GetCustomData(organizationID: organizationID, key: change.Value.PostId);
                    if (!string.IsNullOrWhiteSpace(customData))
                    {
                        post = JsonConvert.DeserializeObject<MetaPost>(customData)!;
                    }
                    if (string.IsNullOrWhiteSpace(post?.PageId))
                    {
                        Log.Error($"Failed to get page ID based on post ID {change.Value.PostId}.");
                        continue;
                    }
                    MetaPage page = null;
                    customData = await corprioClient.ApplicationSubscriptionApi.GetCustomData(organizationID: organizationID, key: post.PageId);
                    if (!string.IsNullOrWhiteSpace(customData))
                    {
                        page = JsonConvert.DeserializeObject<MetaPage>(customData)!;
                    }
                    if (string.IsNullOrWhiteSpace(page?.Token))
                    {
                        Log.Error($"Failed to get access token of page {post.PageId}.");
                        continue;
                    }

                    List<dynamic> existingProducts = await corprioClient.ProductApi.Query(
                        organizationID: organizationID,
                        selector: "new (ID, EntityProperties)",
                        where: "EntityProperties.Any(Name==@0 && Value==@1)",
                        orderBy: "ID",
                        whereArguments: new string[] { BabelFish.ProductEpName, change.Value.PostId },
                        skip: 0,
                        take: 1);
                    Guid productId = UtilityHelper.GetGuidFromDynamicQueryResult(existingProducts, "ID");
                    if (productId == Guid.Empty)
                    {
                        Log.Error($"Failed to find the product posted as {change.Value.PostId}.");
                        continue;
                    }

                    MetaChatBot botStatus = await ActionHelper.GetCustomData<MetaChatBot>(corprioClient: corprioClient,
                        organizationID: organizationID, key: change.Value.From.Id);
                    botStatus ??= new MetaChatBot() { Id = change.Value.From.Id };
                    var bot = new DomesticHelper(client: corprioClient, organizationID: organizationID, botStatus: botStatus, pageName: page.Name);
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

                    if (string.IsNullOrWhiteSpace (message))
                    {
                        Log.Error($"The bot failed to provide any message.");
                        continue;
                    }

                    string endPoint = post.IsFbPost 
                        ? $"{BaseUrl}/{ApiVersion}/{change.Value.From.Id}/messages" 
                        : $"{BaseUrl}/{ApiVersion}/me/messages";
                    await ActionHelper.SendMessage(
                        httpClient: httpClient,
                        accessToken: page.Token,
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
        /// <param name="payload">Webhook payload</param>
        /// <returns>Status code</returns>
        /// <exception cref="Exception"></exception>
        public async Task<IActionResult> HandleMessageWebhook(HttpClient httpClient, APIClient corprioClient, MessageWebhookPayload payload)
        {
            // note: it is possible for more than one entry/messaging to come in, as Meta aggregates up to 1,000 event notifications
            // see Frequency at https://developers.facebook.com/docs/graph-api/webhooks/getting-started
            foreach (var entry in payload.Entry)
            {
                foreach (var messaging in entry.Messaging)
                {                    
                    if (string.IsNullOrWhiteSpace(messaging.Recipient?.Id))
                    {
                        // note: we move on to the next message instead of throwing errors
                        Log.Error($"Recipient ID for \"{messaging.Message?.Text}\" is blank.");
                        continue;
                    }
                    
                    List <dynamic> queryResult = await corprioClient.ApplicationSubscriptionApi.QueryCustomData(
                        new DataModel.DynamicQuery {
                            Selector = "new (ApplicationSubscription.OrganizationID)",
                            Where = "Key=@0",
                            WhereArguments = new object[] { messaging.Recipient.Id }
                        });
                    Guid organizationID = UtilityHelper.GetGuidFromDynamicQueryResult(queryResult, "OrganizationID");
                    if (organizationID == Guid.Empty) 
                    {
                        Log.Error($"Failed to get organization ID based on page ID {messaging.Recipient.Id}.");
                        continue;
                    }

                    string pageId = payload.Object == "instagram"
                        ? await corprioClient.ApplicationSubscriptionApi.GetCustomData(organizationID: organizationID, key: messaging.Recipient.Id)
                        : messaging.Recipient.Id;
                    
                    if (string.IsNullOrWhiteSpace(pageId))
                    {
                        Log.Error($"Failed to get ID of the page that received \"{messaging.Message?.Text}\".");
                        continue;
                    }
                    MetaPage page = await ActionHelper.GetCustomData<MetaPage>(corprioClient: corprioClient, 
                        organizationID: organizationID, key: pageId.Replace("\"", ""));  // need to replace the quotation marks returned by GetCustomData
                    
                    if (string.IsNullOrWhiteSpace(page?.Token))
                    {
                        // note: we move on to the next message instead of throwing errors
                        Log.Error($"No page token can be found for {payload.Object} message recipient - {messaging.Recipient.Id}.");
                        continue;
                    }

                    // assumption: senderId is the same regardless if (i) the sender made a comment on a post or (ii) sent a message via messenger
                    MetaChatBot botStatus = await ActionHelper.GetCustomData<MetaChatBot>(corprioClient: corprioClient, 
                        organizationID: organizationID, key: messaging.Sender.Id);
                    botStatus ??= new MetaChatBot() { Id = messaging.Sender.Id };
                    var bot = new DomesticHelper(client: corprioClient, organizationID: organizationID, botStatus: botStatus, 
                        detectedLocales: messaging.Message?.NLP?.DetectedLocales, pageName: page.Name);
                    string response;
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

                    string endPoint = payload.Object == "instagram" ? $"{BaseUrl}/{ApiVersion}/me/messages" : $"{BaseUrl}/{ApiVersion}/{messaging.Recipient.Id}/messages";
                    await ActionHelper.SendMessage(
                        httpClient: httpClient,
                        accessToken: page.Token,
                        endPoint: endPoint,
                        message: response,
                        recipientId: messaging.Sender.Id);
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
        /// <returns>Status code</returns>
        [HttpPost("/webhook")]
        public async Task<IActionResult> HandleWebhookPost([FromServices] HttpClient httpClient)
        {            
            string hash = Request.Headers.ContainsKey("x-hub-signature-256") ? Request.Headers["x-hub-signature-256"] : string.Empty;
            (bool verified, string requestString) = await HashCheck(request: Request, metaHash: hash);
            if (!verified)
            {
                Log.Information($"Invalid webhook notification. Request: {requestString}");
                return StatusCode(200);
            }
            APIClient corprioClient = new(httpClientFactory.CreateClient("webhookClient"));

            // if the payload includes "Messaging" and one of its element has sender, then presumably it is for webhook on messages
            MessageWebhookPayload messageWebhookPayload = requestString == null ? null : JsonConvert.DeserializeObject<MessageWebhookPayload>(requestString)!;
            if (messageWebhookPayload?.Entry?.Any(x => x.Messaging?.Any(y => !string.IsNullOrWhiteSpace(y.Sender?.Id)) ?? false) ?? false)
            {                
                return await HandleMessageWebhook(httpClient: httpClient, corprioClient: corprioClient, payload: messageWebhookPayload);
            }

            // if the payload includes "Changes", then presumably it is for webhook on feed
            FeedWebhookPayload feedWebhookPayload = requestString == null ? null : JsonConvert.DeserializeObject<FeedWebhookPayload>(requestString)!;
            if (feedWebhookPayload?.Entry?.Any(x => x.Changes?.Count > 0) ?? false)
            {
                return await HandleFeedWebhook(httpClient: httpClient, corprioClient: corprioClient, payload: feedWebhookPayload);
            }

            Log.Information($"Cannot recognize payload in webhook notification. Payload: {requestString}");
            return StatusCode(200);
        }
        
        /// <summary>
        /// Trigger the publication of a catalogue / product list
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
        /// Trigger the publication of a product
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

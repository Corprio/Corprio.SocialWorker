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

namespace Corprio.SocialWorker.Controllers
{
    public class WebhookController : Controller
    {
        readonly IHttpClientFactory httpClientFactory;
        readonly IConfiguration configuration;

        private readonly string AppId;
        private readonly string ApiVersion;
        private readonly string AppSecret;
        private readonly string BaseUrl;

        public WebhookController(IHttpClientFactory httpClientFactory, IConfiguration configuration) : base()
        {
            this.httpClientFactory = httpClientFactory;
            this.configuration = configuration;

            AppId = configuration["MetaApiSetting:AppId"];
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

        ///// <summary>
        ///// Make a reply to an IG comment
        ///// </summary>
        ///// <param name="accessToken">Page access token</param>
        ///// <param name="commentId">ID of the comment in question</param>
        ///// <param name="message">Message of the reply</param>
        ///// <returns>ID of the reply comment</returns>
        //public string MakeIgCommentReply(string accessToken, string commentId, string message)
        //{
        //    using var restClient = new RestClient($"{BaseUrl}/{ApiVersion}/{commentId}/replies");
        //    RestRequest request = MetaRestRequest(method: Method.Post, accessToken: accessToken);
        //    request.AddParameter("message", message);
        //    RestResponse response = restClient.Execute(request);
        //    OneLinePayload payload = response?.Content == null ? new() : JsonConvert.DeserializeObject<OneLinePayload>(response.Content)!;
        //    if (payload.Error != null)
        //    {
        //        Log.Information($"Encountered an error in posting message {message} to {endPoint} with token {accesstoken}.");                
        //    }
        //    return payload?.Id;
        //}        

        /// <summary>
        /// Respond to webhook triggered by feeds
        /// </summary>
        /// <param name="httpClient">HTTP client for executing API query</param>
        /// <param name="payload">Webhook payload</param>
        /// <param name="metaProfile">Meta profile of the organization with which the webhook is related</param>
        /// <returns>Status code</returns>
        public async Task<IActionResult> HandleFeedWebhook(HttpClient httpClient, FeedWebhookPayload payload, MetaProfileModel metaProfile)
        {            
            foreach (var entry in payload.Entry)
            {
                foreach (var change in entry.Changes)
                {
                    // note: the page ID included in the webhook notification may not be assocated with any of the access tokens in our DB
                    MetaPage relevantPage = metaProfile?.Pages?.FirstOrDefault(x => x.PageId == change.Value.From.Id);
                    if (string.IsNullOrWhiteSpace(relevantPage?.Token)) continue;
                    await ActionHelper.PostOrComment(
                        httpClient: httpClient,
                        accessToken: relevantPage.Token,
                        message: $"{change.Value.From.Name}, thank you for your message; we will get back to you as soon as possible.",
                        endPoint: $"{BaseUrl}/{ApiVersion}/{change.Value.PostId}/comments");
                }
            }
            return StatusCode(200);
        }
                
        /// <summary>
        /// Respond to webhook triggered by messages
        /// </summary>
        /// <param name="payload">Webhook payload</param>
        /// <returns>Status code</returns>
        public async Task<IActionResult> HandleMessageWebhook(HttpClient httpClient, APIClient corprioClient, Guid organizationID, 
            MessageWebhookPayload payload, MetaProfileModel metaProfile)
        {
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

                    MetaPage page = payload.Object == "instagram" 
                        ? metaProfile.Pages.FirstOrDefault(x => x.IgId == messaging.Recipient.Id)
                        : metaProfile.Pages.FirstOrDefault(x => x.PageId == messaging.Recipient.Id);

                    if (string.IsNullOrWhiteSpace(page?.Token))
                    {
                        // note: we move on to the next message instead of throwing errors
                        Log.Error($"No page token can be found for {payload.Object} message recipient - {messaging.Recipient.Id}.");
                        continue;
                    }

                    var bot = new DomesticHelper(client: corprioClient, organizationID: organizationID, metaProfile: metaProfile,
                        senderId: messaging.Sender.Id, detectedLocales: messaging.Message?.NLP?.DetectedLocales);
                    string response = await bot.ThinkBeforeSpeak(messaging.Message.Text);
                    if (string.IsNullOrWhiteSpace(response)) continue;

                    string endPoint = payload.Object == "instagram" ? $"{BaseUrl}/{ApiVersion}/me/messages" : $"{BaseUrl}/{ApiVersion}/{messaging.Recipient.Id}/messages";
                    await ActionHelper.SendMessage(
                        httpClient: httpClient,
                        accessToken: page.Token,
                        endPoint: endPoint,
                        message: response,
                        recipientId: messaging.Sender.Id,
                        senderId: messaging.Recipient.Id);                    
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
            if (messageWebhookPayload?.Entry?.Any(x => x?.Messaging.Any(y => !string.IsNullOrWhiteSpace(y.Sender?.Id)) ?? false) ?? false)
            {                
                // DEV ONLY - we should replace the following codes with a search for meta profile in ApplicationSubscriptions
                Guid organizationID = Guid.Parse("3C83FE2B-FE0A-420C-AF24-E76FCF5BD7E7");
                string setting = await corprioClient.OrganizationApi.GetApplicationSetting(organizationID);                
                if (string.IsNullOrWhiteSpace(setting)) throw new Exception($"No application setting found for {organizationID}");
                MetaProfileModel metaProfile = JsonConvert.DeserializeObject<MetaProfileModel>(setting)! ?? throw new Exception($"No Meta profile for {organizationID}.");

                return await HandleMessageWebhook(httpClient: httpClient, corprioClient: corprioClient, organizationID: organizationID, 
                    payload: messageWebhookPayload, metaProfile: metaProfile);
            }

            //// if the payload includes "Changes", then presumably it is for webhook on feed
            //FeedWebhookPayload feedWebhookPayload = requestString == null ? null : JsonConvert.DeserializeObject<FeedWebhookPayload>(requestString)!;
            //if (feedWebhookPayload?.Entry?.Any(x => x.Changes?.Count > 0) ?? false)
            //{
            //    // TODO - we should crawl through ApplicationSubscriptions to find the organization with a page whose ID = 
            //    Guid organizationID = Guid.NewGuid();
            //    return await HandleFeedWebhook(feedWebhookPayload);
            //}

            Log.Information($"Cannot recognize payload in webhook notification. Payload: {requestString}");
            return StatusCode(200);
        }
    }
}

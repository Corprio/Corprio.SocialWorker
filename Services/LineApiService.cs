using Line.Messaging.Webhooks;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using Microsoft.Extensions.Configuration;
using Serilog;
using Corprio.SocialWorker.Models;
using Corprio.SocialWorker.Models.Line;
using System.Linq;
using System.Security.Cryptography;
using System.Net;
using Newtonsoft.Json;
using Corprio.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Corprio.SocialWorker.Dictionaries;
using Corprio.AspNetCore.Site.Services;
using Corprio.SocialWorker.Models.Meta;
using Corprio.CorprioRestClient;
using System.Collections.Generic;
using Corprio.CorprioAPIClient;
using Corprio.SocialWorker.Helpers;
using Corprio.Core;
using Corprio.DataModel.Business;
using System.ServiceModel.Channels;

namespace Corprio.SocialWorker.Services
{
    /// <summary>
    /// Line API service, instantiated for each Line channel
    /// WebhookApplication is part of LineMessagingApi SDK. For its documentation, see:
    /// https://github.com/pierre3/LineMessagingApi/blob/master/README.md
    /// </summary>
    public class LineApiService : Line.Messaging.Webhooks.WebhookApplication
    {
        readonly ApplicationDbContext _db;         
        readonly IHttpClientFactory _httpClientFactory;
        readonly ApplicationSettingService _applicationSettingService;
        readonly IConfiguration _configuration;
        readonly EmailHelper _emailHelper;

        readonly string _baseUrl;        
        readonly LineChannel _channel;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Database connection</param>
        /// <param name="httpClientFactory">HTTP client factory</param>
        /// <param name="configuration">Configuration</param>
        /// <param name="lineChannel">Line channel</param>
        /// <param name="applicationSettingService">Application setting service, required for chatbot</param>
        /// <param name="emailHelper">Email helper, required for OnMessageAsync</param>
        public LineApiService(ApplicationDbContext context, IHttpClientFactory httpClientFactory, 
            IConfiguration configuration, LineChannel lineChannel,
            ApplicationSettingService applicationSettingService = null, EmailHelper emailHelper = null)
        {
            _db = context;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _applicationSettingService = applicationSettingService;
            _emailHelper = emailHelper;

            _baseUrl = configuration["LineApiSetting:BaseUrl"];            
            _channel = lineChannel;            
        }
        
        /// <summary>
        /// Validate if a HTTP request genuinely came from Line
        /// </summary>
        /// <param name="hash">Hash included in the HTTP request's header</param>
        /// <param name="requestBody">The HTTP request's body</param>
        /// <returns></returns>
        public bool ValidateWebhook(string hash, string requestBody)
        {
            var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_channel.ChannelSecret));
            hmac.Initialize();
            byte[] rawHmac = hmac.ComputeHash(Encoding.UTF8.GetBytes(requestBody));
            string computedHash = Convert.ToBase64String(rawHmac);

            return hash == computedHash;
        }

        /// <summary>
        /// Checks if the configured webhook endpoint can receive a test webhook event.
        /// https://developers.line.biz/en/reference/messaging-api/#test-webhook-endpoint
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidJsonException"></exception>
        public async Task<bool> TestWebhookEndpoint()
        {
            string response = await LineApiRequest(method: HttpMethod.Post,
                endPoint: "v2/bot/channel/webhook/test");

            LineWebhookTestResponse payload;
            try
            {
                payload = JsonConvert.DeserializeObject<LineWebhookTestResponse>(response);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to deserialize response to Line's response to TestWebhookEndpoint: {ex.Message}");
                throw new InvalidJsonException(message: "Line returned an invalid JSON string for TestWebhookEndpoint.");
            }
            return payload?.Success ?? false;
        }

        /// <summary>
        /// Sets the webhook endpoint URL. It may take up to 1 minute for changes to take place due to caching.
        /// https://developers.line.biz/en/reference/messaging-api/#set-webhook-endpoint-url
        /// </summary>
        /// <returns></returns>
        public async Task SetWebhookEndpoint()
        {            
            string baseUrl = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production" 
                ? _configuration["AppBaseUrl"] 
                : _configuration["WebhookCallbackUrl"];  // this URL is not local host

            await LineApiRequest(method: HttpMethod.Put, 
                endPoint: "v2/bot/channel/webhook/endpoint", 
                json: new { endpoint = $"{baseUrl}/{_channel.OrganizationID}/{_channel.ID}/line" });
        }

        /// <summary>
        /// Execute Messaging API query
        /// </summary>
        /// <param name="method">HTTP method</param>
        /// <param name="endPoint">API service endpoint</param>
        /// <param name="json">Json body of the query, if any</param>
        /// <returns>Serialized response from Line</returns>
        /// <exception cref="Core.Exceptions.ApiExecutionException"></exception>
        private async Task<string> LineApiRequest(HttpMethod method, string endPoint, dynamic json = null)
        {
            string content = JsonConvert.SerializeObject(json);
            Log.Information($"Request to {endPoint}: {content}");

            var httpRequest = new HttpRequestMessage
            {
                Method = method,
                RequestUri = new Uri($"{_baseUrl}/{endPoint}"),
                Content = json == null ? null : new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json"),
            };
            httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _channel.ChannelToken);

            HttpClient client = _httpClientFactory.CreateClient();
            HttpResponseMessage response = await client.SendAsync(httpRequest);
            
            // do not run EnsureSuccessStatusCode() because we want to deserialize the response and display human-readable error messages to user
            string responseString = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(responseString))
                throw new Core.Exceptions.ApiExecutionException("Line returned an empty response.");
            Log.Information($"Line's response to {endPoint}: {responseString}");            

            return responseString;
        }

        /// <summary>
        /// Send a broadcast message through the Line channel
        /// https://developers.line.biz/en/reference/messaging-api/#send-broadcast-message
        /// </summary>
        /// <param name="messages">Collection of Line message objects (up to 5)</param>
        /// <returns></returns>
        /// <exception cref="InvalidJsonException"></exception>
        /// <exception cref="ApiExecutionException"></exception>
        public async Task SendBroadcastMessage(List<ILineMessage> messages)
        {
            string responseString = await LineApiRequest(method: HttpMethod.Post, 
                endPoint: "v2/bot/message/broadcast",
                json: new { messages }
                );

            LineErrorResponse payload;
            try
            {
                payload = JsonConvert.DeserializeObject<LineErrorResponse>(responseString);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to deserialize Line's response to SendBroadcastMessage: {ex.Message}");
                throw new InvalidJsonException(message: $"Line returned an invalid JSON string for SendBroadcastMessage.");
            }

            if (payload?.Details?.Count > 0)
            {
                string errorMessage = "";
                foreach (LineErrorDetail detail in payload.Details)
                {
                    errorMessage += $"{detail.Property}: {detail.Message}";
                }
                throw new ApiExecutionException($"Error in executing Messaging API - SendBroadcastMessage. {errorMessage}" );
            }
        }

        /// <summary>
        /// Send reply message
        /// https://developers.line.biz/en/reference/messaging-api/#send-reply-message
        /// </summary>
        /// <param name="message">Text message to be sent</param>
        /// <param name="replyToken">Token of the conversation to which the reply is made in response</param>
        /// <returns></returns>
        private async Task SendReplyMessage(string message, string replyToken)
        {            
            await LineApiRequest(method: HttpMethod.Post, endPoint: "v2/bot/message/reply", 
                json: new {
                    replyToken = replyToken,
                    messages = new object[] { new { type = "text", text = message } } 
                }
                );
        }

        /// <summary>
        /// Query a Line user profile
        /// </summary>
        /// <param name="userId">ID of the Line user to be queried</param>
        /// <returns></returns>
        private async Task<LineUser> GetUserProfile(string userId)
        {
            string response = await LineApiRequest(method: HttpMethod.Get, endPoint: $"v2/bot/profile/{userId}");

            LineUser lineUser;
            try
            {
                lineUser = JsonConvert.DeserializeObject<LineUser>(response);
            }
            catch (Exception ex) 
            {
                Log.Error($"Failed to deserialize Line user profile. {ex.Message}");
                lineUser = null;
            }
            return lineUser;
        }

        /// <summary>
        /// Handle message event webhook
        /// </summary>
        /// <param name="ev">Message event</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected override async Task OnMessageAsync(MessageEvent ev)
        {            
            if (ev.Message.Type != EventMessageType.Text)
            {
                Log.Information($"Ignoring event message type: {ev.Message.Type}.");
                return; 
            }

            TextEventMessage textMessage = (TextEventMessage)ev.Message;
            if (string.IsNullOrWhiteSpace (textMessage.Text))
            {
                Log.Information($"Ignoring empty text message.");
                return;
            }

            if (textMessage.Text.EndsWith(BabelFish.RobotEmoji))
            {
                Log.Information($"Ignoring bot-generated message from {ev.Source.UserId}.");
                return;
            }

            APIClient corprioClient = new(_httpClientFactory.CreateClient("webhookClient"));
            if (_applicationSettingService == null) 
                throw new Exception("ApplicationSettingService was not provided to the constructor of LineApiService.");
            ApplicationSetting setting = await _applicationSettingService.GetSetting<ApplicationSetting>(_channel.OrganizationID);

            DbFriendlyBot botStatus = _db.BotStatuses.FirstOrDefault(r => r.LineChannelID == _channel.ID && r.BuyerID == ev.Source.UserId);
            if (botStatus == null)
            {
                LineUser lineUser = await GetUserProfile(ev.Source.UserId);
                
                botStatus = new DbFriendlyBot 
                { 
                    ID = Guid.NewGuid(),
                    LineChannelID = _channel.ID,
                    BuyerID = ev.Source.UserId,
                    ThinkingOf = BotTopic.Limbo,
                    BuyerUserName = lineUser?.DisplayName,
                };
                
                List<dynamic> existingCustomers = await corprioClient.CustomerApi.Query(
                    organizationID: _channel.OrganizationID,
                    selector: "new (ID)",
                    where: "EntityProperties.Any(Name==@0 && Value==@1)",
                    orderBy: "ID",
                    whereArguments: new string[] { BabelFish.LineCustomerEpName, ev.Source.UserId },
                    skip: 0,
                    take: 1);
                if (existingCustomers.Count > 0)
                {
                    botStatus.BuyerCorprioID = Guid.Parse(existingCustomers[0].ID);
                }
                _db.BotStatuses.Add(botStatus);
                await _db.SaveChangesAsync();
            }

            var bot = new DomesticHelper(context: _db, configuration: _configuration, 
                client: corprioClient, organizationID: _channel.OrganizationID, botStatus: botStatus, 
                pageName: _channel.ChannelName, setting: setting);

            // if muted, either unmute or do nothing
            if (botStatus.IsMuted)
            {
                if (textMessage.Text.Equals(BabelFish.BotSummon, StringComparison.OrdinalIgnoreCase) || textMessage.Text.Equals(BabelFish.BotSummon_CN, StringComparison.OrdinalIgnoreCase))
                {
                    Log.Information($"The interlocutor {ev.Source.UserId} has opted in to receiving automated responses from Line channel {_channel.ID}.");
                    await SendReplyMessage(replyToken: ev.ReplyToken, message: await bot.getWoke());                    
                }
                else
                {
                    Log.Information($"The interlocutor {ev.Source.UserId} has opted out of receiving automated responses from Line channel {_channel.ID}.");
                }

                return;
            }

            // escalate to human agent if the user requests to speak with a human rather than a bot
            if (textMessage.Text.Equals(BabelFish.SOS, StringComparison.OrdinalIgnoreCase) 
                || textMessage.Text.Equals(BabelFish.SOS_CN, StringComparison.OrdinalIgnoreCase))
            {
                Log.Information($"The interlocutor elected to speak with human agent by inputting {textMessage.Text}");

                botStatus.IsMuted = true;
                _db.BotStatuses.Update(botStatus);
                await _db.SaveChangesAsync();

                await SendReplyMessage(replyToken: ev.ReplyToken, message: bot.ThusSpokeBabel("Escalated"));

                string customerName = string.IsNullOrWhiteSpace(botStatus.BuyerUserName) ? Resources.SharedResource.Unknown_Italic : botStatus.BuyerUserName;
                string customerEmail = string.IsNullOrWhiteSpace(botStatus.BuyerEmail) ? Resources.SharedResource.Unknown_Italic : botStatus.BuyerEmail;
                OrganizationCoreInfo org = await corprioClient.OrganizationApi.GetCoreInfo(_channel.OrganizationID);
                if (org == null)
                {
                    Log.Error("Failed to retrieve the organization email address.");
                    return;
                }
                
                var email = new Core.EmailMessage()
                {
                    ToEmails = [org.EmailAddress],
                    Subject = "[" + Resources.SharedResource.AppName + "] " + string.Format(Resources.SharedResource.HumanEscalation_EmailSubject, _channel.ChannelName, "Line"),
                    Body = string.Format(Resources.SharedResource.HumanEscalation_EmailBody, "Line", customerName, ev.Source.UserId, customerEmail, _channel.ChannelName)
                };

                try
                {
                    if (_emailHelper == null)
                        throw new Exception("EmailHelper was not provided to the constructor of LineApiService.");

                    await _emailHelper.SendEmailAsync(email);
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to escalate to the merchant. {ex.Message}");
                }

                return;
            }

            // reply to direct message that does NOT involve human agent escalation
            string response;
            try
            {
                response = await bot.ThinkBeforeSpeak(textMessage.Text);
            }
            catch (Exception ex)
            {
                Log.Error($"The bot failed to generate a message. Error: {ex.Message}");
                response = bot.ThusSpokeBabel("Err_DefaultMsg");
            }            
            if (string.IsNullOrWhiteSpace(response)) response = bot.ThusSpokeBabel("Err_DefaultMsg");
            response += BabelFish.RobotEmoji;

            await SendReplyMessage(replyToken: ev.ReplyToken, message: response);
        }
    }
}

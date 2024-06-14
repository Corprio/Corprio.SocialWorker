using Newtonsoft.Json;
using System.Dynamic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.XtraRichEdit.Import.Html;
using Azure.Core;
using Org.BouncyCastle.Cms;
using System.Net.Http.Headers;
using Corprio.SocialWorker.Models.Meta;
using Corprio.CorprioAPIClient;
using Corprio.SocialWorker.Dictionaries;
using Corprio.SocialWorker.Models;

namespace Corprio.SocialWorker.Helpers
{
    public class ApiActionHelper
    {
        /// <summary>
        /// Get information from Facebook API
        /// </summary>
        /// <param name="httpClient">HTTP client for executing API query</param>        
        /// <param name="accessToken">Access token</param>
        /// <param name="endPoint">Endpoint at which the query is performed</param>
        /// <returns>API response in string format</returns>
        public static async Task<string> GetQuery(HttpClient httpClient, string accessToken, string endPoint)
        {
            var httpRequest = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(endPoint),
            };
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage response = await httpClient.SendAsync(httpRequest);
            if (!response.IsSuccessStatusCode)
            {
                Log.Error($"HTTP request to get Facebook account info fails. Response: {System.Text.Json.JsonSerializer.Serialize(response)}");
                return string.Empty;
            }
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Make a post or comment
        /// publish to a page: https://developers.facebook.com/docs/graph-api/reference/v18.0/page/feed#publish
        /// upload a photo: https://developers.facebook.com/docs/graph-api/reference/photo/#Creating
        /// upload photos: https://developers.facebook.com/docs/graph-api/reference/page/photos#upload
        /// </summary>
        /// <param name="httpClient">HTTP client for executing API query</param>
        /// <param name="accessToken">Page access token</param>
        /// <param name="message">Message of the post or comment</param>
        /// <param name="endPoint">End point to which the API request will be posted</param>
        /// <param name="mediaUrl">URL of media to be included in a post</param>
        /// <param name="link">Link to be included in a post</param>
        /// <param name="photoIds">IDs of photos to be included in a multi-photo post</param>
        /// <param name="published">False when uploading a photo without publishing it</param>
        /// <returns>ID of the post or comment</returns>
        public static async Task<string> PostOrComment(HttpClient httpClient, string accessToken, string endPoint, string message = null,
            string mediaUrl = null, string link = null, List<string> photoIds = null, bool published = true)
        {
            dynamic queryParams = new ExpandoObject();
            queryParams.access_token = accessToken;
            if (!string.IsNullOrWhiteSpace(message)) queryParams.message = message;
            if (endPoint.Contains("feed"))
            {
                if (!string.IsNullOrWhiteSpace(link)) queryParams.link = link;

                photoIds ??= [];
                if (photoIds.Count > 0)
                {
                    List<object> media = [];
                    foreach (string photoId in photoIds)
                    {
                        media.Add(new { media_fbid = photoId });
                    }
                    queryParams.attached_media = media;
                }
            }
            else if (endPoint.Contains("photos"))
            {
                // note: tested that including "link" will not do anything
                queryParams.url = mediaUrl;
                if (!published) queryParams.published = published;
            }
            string content = System.Text.Json.JsonSerializer.Serialize(queryParams);

            var httpRequest = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(endPoint),
                Content = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json")
            };
            Log.Information($"Posting to {endPoint}: {content}");

            HttpResponseMessage response = await httpClient.SendAsync(httpRequest);
            if (!response.IsSuccessStatusCode)
            {
                Log.Error($"HTTP request to make a post/comment fails. Response: {System.Text.Json.JsonSerializer.Serialize(response)}");
                return null;
            }
            string responseString = await response.Content.ReadAsStringAsync();
            OneLinePayload payload = JsonConvert.DeserializeObject<OneLinePayload>(responseString);
            if (payload?.Error != null)
            {
                Log.Error($"Encountered an error in posting to {endPoint}. {payload?.Error?.CustomErrorMessage()}");
            }
            return payload?.MetaID;
        }

        /// <summary>
        /// Pass/release/request control from a conversation
        /// https://developers.facebook.com/docs/messenger-platform/instagram/features/handover-protocol#releasing-thread-control
        /// </summary>
        /// <param name="httpClient">HTTP client for executing API query</param>
        /// <param name="accessToken">Page access token</param>
        /// <param name="endPoint">API Endpoint, which indicates if the thread is released, passed or requested</param>
        /// <param name="interlocutorId">ID of the person having a conversation with the App</param>
        /// <param name="targetAppId">App to receive control of the conversation</param>
        /// <returns></returns>
        public static async Task ControlConversationThread(HttpClient httpClient, string accessToken, string endPoint, 
            string interlocutorId, string targetAppId = null)
        {
            dynamic queryParams = new ExpandoObject();
            queryParams.access_token = accessToken;
            queryParams.recipient = new { id = interlocutorId };
            if (!string.IsNullOrEmpty(targetAppId)) { queryParams.target_app_id = targetAppId; }
            string content = System.Text.Json.JsonSerializer.Serialize(queryParams);

            var releaseControlRequest = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(endPoint),
                Content = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json")
            };
            Log.Information($"Handover protocol - endpoint: {endPoint}; request: {content}");

            HttpResponseMessage response = await httpClient.SendAsync(releaseControlRequest);
            string responseString = await response.Content.ReadAsStringAsync();
            Log.Information($"Response to thread control control: {responseString}");
        }

        /// <summary>
        /// Send a message: 
        /// - from IG Professional account (https://developers.facebook.com/docs/messenger-platform/instagram/features/send-message), or
        /// - from a FB page (https://developers.facebook.com/docs/messenger-platform/send-messages)        
        /// </summary>
        /// <param name="httpClient">HTTP client for executing API query</param>
        /// <param name="accessToken">A Page access token requested from a person who can perform the MESSAGE task on the FB Page (linked to the IG Professional account)</param>
        /// <param name="messageEndPoint">Endpoint to which the message will be sent</param>
        /// <param name="threadEndPoint">Endpoint to release control</param>
        /// <param name="message">Message to be sent</param>
        /// <param name="recipientId">ID of the recipient, which - in the case of IG Messenger - MUST have sent the business a message</param>        
        /// <returns>Payload returned by Meta API</returns>
        public static async Task<MessageFeedback> SendMessage(HttpClient httpClient, string accessToken, string messageEndPoint, 
            string threadEndPoint, string message, string recipientId)
        {            
            dynamic queryParams = new ExpandoObject();
            queryParams.access_token = accessToken;
            queryParams.recipient = new { id = recipientId };
            queryParams.message = new { text = message };
            // see Mesaging Types for FB: https://developers.facebook.com/docs/messenger-platform/send-messages#messaging_types
            if (!messageEndPoint.Contains(@"me/messages")) { queryParams.messaging_type = "RESPONSE"; }
            string content = System.Text.Json.JsonSerializer.Serialize(queryParams);
            var httpRequest = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(messageEndPoint),
                Content = new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json")
            };
            Log.Information($"Sending message to {messageEndPoint}: {content}");

            // note: the bot is supposed to send message ONLY when the thread is idle, so we don't bother to request control of conversation
            // reference: https://developers.facebook.com/docs/messenger-platform/handover-protocol/conversation-control#request-control
            HttpResponseMessage response = await httpClient.SendAsync(httpRequest);
            if (!response.IsSuccessStatusCode)
            {
                Log.Error($"HTTP request to send message fails. Response: {System.Text.Json.JsonSerializer.Serialize(response)}");                                
                //if (allowResend)
                //{
                //    string header = response.Headers.GetValues("WWW-Authenticate").FirstOrDefault();
                //    // note 1: apparently Facebook has a bug that sometimes fails to send message to an un-opt-out user
                //    // note 2: cannot solely rely on the error code = #551 because someone on Stackoverflow reported it could be #200
                //    // note 3: The error message can be "This person isn't available right now" or "This person isn't available at the moment"
                //    // reference: https://stackoverflow.com/questions/44379656/551-error-with-facebook-messenger-bot-this-person-isnt-available-right-now
                //    if (!string.IsNullOrWhiteSpace(header) && (header.Contains("#551") || header.Contains("This person isn't available")))
                //    {
                //        Log.Information("Resending the message once to handle this-person-isnt-available-right-now error.");
                //        return await SendMessage(httpClient: httpClient, accessToken: accessToken,
                //            messageEndPoint: messageEndPoint, threadEndPoint: threadEndPoint, message: message, 
                //            recipientId: recipientId, allowResend: false);
                //    }
                //}
                return null;
            }            
            string responseString = await response.Content.ReadAsStringAsync();
            MessageFeedback feedback = response?.Content == null ? new() : JsonConvert.DeserializeObject<MessageFeedback>(responseString);
            if (feedback?.Error != null)
            {
                Log.Error($"Encountered an error when sending a message to {recipientId}. {feedback?.Error?.CustomErrorMessage()}");
            }
            
            // as a good practice, we release control of conversation thread after sending each message
            // reference: https://developers.facebook.com/docs/messenger-platform/handover-protocol/conversation-control#releasing-control
            await ControlConversationThread(endPoint: threadEndPoint, 
                accessToken: accessToken, interlocutorId: recipientId, httpClient: httpClient);

            return feedback;
        }        
    }
}

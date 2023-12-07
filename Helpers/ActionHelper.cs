using Corprio.SocialWorker.Models;
using Newtonsoft.Json;
using System.Dynamic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using System;

namespace Corprio.SocialWorker.Helpers
{
    public class ActionHelper
    {
        /// <summary>
        /// Make a post or comment
        /// </summary>
        /// <param name="httpClient">HTTP client for executing API query</param>
        /// <param name="accessToken">Page access token</param>
        /// <param name="message">Message of the post or comment</param>
        /// <param name="endPoint">End point to which the API request will be posted</param>
        /// <param name="mediaUrl">URL of media to be included in a post</param>
        /// <param name="link">Link to be included in a post</param>
        /// <returns>ID of the post or comment</returns>
        public static async Task<string> PostOrComment(HttpClient httpClient, string accessToken, string message, string endPoint,
            string mediaUrl = null, string link = null)
        {
            dynamic queryParams = new ExpandoObject();
            queryParams.message = message;
            queryParams.access_token = accessToken;
            if (endPoint.Contains("feed"))
            {
                if (!string.IsNullOrWhiteSpace(link)) queryParams.link = link;
            }
            else if (endPoint.Contains("photos"))
            {
                // note: tested that including "link" will not do anything
                queryParams.url = mediaUrl;
            }
            var httpRequest = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(endPoint),
                Content = new StringContent(content: System.Text.Json.JsonSerializer.Serialize(queryParams), encoding: Encoding.UTF8, mediaType: "application/json")
            };
            HttpResponseMessage response = await httpClient.SendAsync(httpRequest);
            if (!response.IsSuccessStatusCode)
            {
                Log.Error($"HTTP request to make a post/comment fails. Response: {System.Text.Json.JsonSerializer.Serialize(response)}");
                return null;
            }
            string responseString = await response.Content.ReadAsStringAsync();
            OneLinePayload payload = JsonConvert.DeserializeObject<OneLinePayload>(responseString)!;
            if (payload?.Error != null)
            {
                Log.Error($"Encountered an error in posting message {message} to {endPoint}. {payload?.Error?.CustomErrorMessage()}");
            }
            return payload?.Id;
        }

        /// <summary>
        /// Send a message: 
        /// - from IG Professional account (https://developers.facebook.com/docs/messenger-platform/instagram/features/send-message), or
        /// - from a FB page (https://developers.facebook.com/docs/messenger-platform/send-messages)
        /// </summary>
        /// <param name="httpClient">HTTP client for executing API query</param>
        /// <param name="accessToken">A Page access token requested from a person who can perform the MESSAGE task on the FB Page (linked to the IG Professional account)</param>
        /// <param name="endPoint">Endpoint to which the message will be sent</param>
        /// <param name="message">Message to be sent</param>
        /// <param name="recipientId">ID of the recipient, which - in the case of IG Messenger - MUST have sent the business a message</param>
        /// <param name="senderId">ID of the sender, which is required only when sending a message via FB messenger</param>
        /// <returns>Payload returned by Meta API</returns>
        public static async Task<MessageFeedback> SendMessage(HttpClient httpClient, string accessToken, string endPoint, string message,
            string recipientId, string senderId = null)
        {
            // note: message for FB messaging is NOT sent to me/messages
            if (!endPoint.Contains(@"me/messages") && string.IsNullOrWhiteSpace(senderId))
                throw new Exception("Sender ID is required for FB messaging.");

            dynamic queryParams = new ExpandoObject();
            queryParams.access_token = accessToken;
            queryParams.recipient = new { id = recipientId };
            queryParams.message = new { text = message };
            if (!endPoint.Contains(@"me/messages")) { queryParams.messaging_type = "RESPONSE"; }
            var httpRequest = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(endPoint),
                Content = new StringContent(content: System.Text.Json.JsonSerializer.Serialize(queryParams), encoding: Encoding.UTF8, mediaType: "application/json")
            };
            HttpResponseMessage response = await httpClient.SendAsync(httpRequest);
            if (!response.IsSuccessStatusCode)
            {
                Log.Error($"HTTP request to send message fails. Response: {System.Text.Json.JsonSerializer.Serialize(response)}");
                return null;
            }
            string responseString = await response.Content.ReadAsStringAsync();
            MessageFeedback feedback = response?.Content == null ? new() : JsonConvert.DeserializeObject<MessageFeedback>(responseString)!;
            if (feedback?.Error != null)
            {
                Log.Error($"Encountered an error when sending a message to {recipientId}. {feedback?.Error?.CustomErrorMessage()}");
            }
            return feedback;
        }
    }
}

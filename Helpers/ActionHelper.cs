using Corprio.SocialWorker.Models;
using Newtonsoft.Json;
using System.Dynamic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using Corprio.CorprioAPIClient;
using Corprio.SocialWorker.Dictionaries;
using Corprio.CorprioRestClient;
using Corprio.DataModel.Business.Products.ViewModel;
using Corprio.DataModel.Business.Products;
using Corprio.DataModel.Business;
using Corprio.DataModel;
using Corprio.Core.Exceptions;
using Corprio.DataModel.Shared;

namespace Corprio.SocialWorker.Helpers
{
    public class ActionHelper
    {
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

                if (photoIds?.Any() ?? false)
                {
                    List<object> media = new();
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
                Log.Error($"Encountered an error in posting to {endPoint}. {payload?.Error?.CustomErrorMessage()}");
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
        /// <returns>Payload returned by Meta API</returns>
        public static async Task<MessageFeedback> SendMessage(HttpClient httpClient, string accessToken, string endPoint, string message,
            string recipientId)
        {            
            dynamic queryParams = new ExpandoObject();
            queryParams.access_token = accessToken;
            queryParams.recipient = new { id = recipientId };
            queryParams.message = new { text = message };
            // see Mesaging Types for FB: https://developers.facebook.com/docs/messenger-platform/send-messages#messaging_types
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

        /// <summary>
        /// Get custom data
        /// </summary>
        /// <param name="corprioClient">Client for Api requests among Corprio projects</param>
        /// <param name="organizationID">Organization ID</param>
        /// <param name="key">Key for custom data</param>
        /// <returns>Custom data as a class object</returns>
        public static async Task<T> GetCustomData<T>(APIClient corprioClient, Guid organizationID, string key)
        {
            string customData = await corprioClient.ApplicationSubscriptionApi.GetCustomData(organizationID: organizationID, key: key);
            if (string.IsNullOrWhiteSpace(customData))
            {
                Log.Error($"No custom data found for {organizationID}");
                return default;
            }
            return JsonConvert.DeserializeObject<T>(customData)!;
        }        
    }
}

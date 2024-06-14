using System.Collections.Generic;
using System.Dynamic;
using System;
using System.Net.Http;
using Corprio.SocialWorker.Models.Meta;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;

namespace Corprio.SocialWorker.Services
{
    /// <summary>
    /// WORK IN PROGRESS - rewriting MetaApiController and ApiActionHelper into a service
    /// </summary>
    public class MetaApiService
    {
        protected readonly IConfiguration _configuration;
        protected readonly IHttpClientFactory _httpClientFactory;
        protected readonly string AppId;
        protected readonly string ApiVersion;
        protected readonly string AppSecret;
        protected readonly string BaseUrl;
        protected readonly string GoBuyClickUrl;

        public MetaApiService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            AppId = configuration["MetaApiSetting:AppId"];
            ApiVersion = configuration["MetaApiSetting:ApiVersion"];
            BaseUrl = configuration["MetaApiSetting:BaseUrl"];
            AppSecret = configuration["MetaApiSetting:AppSecret"];
            GoBuyClickUrl = configuration["GoBuyClickUrl"];
        }

        public async Task<string> GetServiceResponse(HttpMethod method, string endPoint, 
            string accessToken = null, object json = null)
        {
            string content = JsonConvert.SerializeObject(json);
            Log.Information($"Request to {endPoint}: {content}");

            var httpRequest = new HttpRequestMessage()
            {
                Method = method,
                RequestUri = new Uri($"{BaseUrl}/{ApiVersion}/{endPoint}"),
                Content = json == null ? null : new StringContent(content: content, encoding: Encoding.UTF8, mediaType: "application/json"),
            };
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
            
            HttpClient httpClient = _httpClientFactory.CreateClient();
            HttpResponseMessage response = await httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();
            
            string responseString = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(responseString))
                throw new Core.Exceptions.ApiExecutionException("Meta returned an empty response.");
            Log.Information($"Meta's response to {endPoint}: {responseString}");

            return responseString;
        }

        /// <summary>
        /// Make a post or comment
        /// publish to a page: https://developers.facebook.com/docs/graph-api/reference/v18.0/page/feed#publish
        /// upload a photo: https://developers.facebook.com/docs/graph-api/reference/photo/#Creating
        /// upload photos: https://developers.facebook.com/docs/graph-api/reference/page/photos#upload
        /// </summary>        
        /// <param name="accessToken">Page access token</param>
        /// <param name="message">Message of the post or comment</param>
        /// <param name="endPoint">End point to which the API request will be posted</param>
        /// <param name="mediaUrl">URL of media to be included in a post</param>
        /// <param name="link">Link to be included in a post</param>
        /// <param name="photoIds">IDs of photos to be included in a multi-photo post</param>
        /// <param name="published">False when uploading a photo without publishing it</param>
        /// <returns>ID of the post or comment</returns>
        public async Task<string> PostOrComment(string accessToken, string endPoint, string message = null,
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
            
            string responseString = await GetServiceResponse(
                method: HttpMethod.Post, 
                endPoint: endPoint, 
                json: queryParams);
            
            OneLinePayload payload = JsonConvert.DeserializeObject<OneLinePayload>(responseString);
            if (payload?.Error != null)
            {
                Log.Error($"Encountered an error in posting to {endPoint}. {payload?.Error?.CustomErrorMessage()}");
            }
            return payload?.MetaID;
        }
    }
}

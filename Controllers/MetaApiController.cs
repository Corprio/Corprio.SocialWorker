using Corprio.AspNetCore.Site.Controllers;
using Corprio.SocialWorker.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Serilog;
using Corprio.DataModel.Business.Products;
using System.Dynamic;
using Microsoft.Extensions.Configuration;
using Corprio.SocialWorker.Helpers;
using Corprio.Core;
using System.Linq;
using System.Net.Http.Headers;

namespace Corprio.SocialWorker.Controllers
{
    public abstract class MetaApiController : OrganizationBaseController
    {                
        readonly IConfiguration configuration;
        protected readonly string AppId;
        protected readonly string ApiVersion;
        protected readonly string AppSecret;
        protected readonly string BaseUrl;
        protected readonly string GoBuyClickUrl;

        public MetaApiController(IConfiguration configuration) : base()
        {            
            this.configuration = configuration;
            AppId = configuration["MetaApiSetting:AppId"];
            ApiVersion = configuration["MetaApiSetting:ApiVersion"];
            BaseUrl = configuration["MetaApiSetting:BaseUrl"];
            AppSecret = configuration["MetaApiSetting:AppSecret"];
            GoBuyClickUrl = configuration["GoBuyClickUrl"];
        }                
        
        public override IActionResult Index([FromRoute] Guid organizationID)
        {
            return base.Index(organizationID);
        }

        /// <summary>
        /// Get information from Facebook API
        /// </summary>
        /// <param name="httpClient">HTTP client for executing API query</param>        
        /// <param name="userAccessToken">User access token</param>
        /// <param name="endPoint">Endpoint at which the query is performed</param>
        /// <returns>API response in string format</returns>
        protected async Task<string> GetQuery(HttpClient httpClient, string userAccessToken, string endPoint)
        {
            var httpRequest = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(endPoint),
            };
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userAccessToken);
            HttpResponseMessage response = await httpClient.SendAsync(httpRequest);
            if (!response.IsSuccessStatusCode)
            {
                Log.Error($"HTTP request to get Facebook account info fails. Response: {System.Text.Json.JsonSerializer.Serialize(response)}");
                return string.Empty;
            }
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Turn on Meta's Built-in NLP to help detect locale (and meaning)
        /// https://developers.facebook.com/docs/graph-api/reference/page/nlp_configs/
        /// https://developers.facebook.com/docs/messenger-platform/built-in-nlp/
        /// </summary>
        /// <param name="httpClient">HTTP client for executing API query</param>
        /// <param name="accessToken">Page access token</param>
        /// <param name="language">Language to be selected as the default model</param>
        /// <returns>True if the operation succeeds</returns>
        protected async Task<bool> TurnOnNLP(HttpClient httpClient, string accessToken, MetaLanguageModel language)
        {
            var queryParams = new { nlp_enabled = true, model = EnumHelper.GetDescription(language), access_token = accessToken };
            var httpRequest = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new System.Uri($"{BaseUrl}/{ApiVersion}/me/nlp_configs"),
                Content = new StringContent(content: System.Text.Json.JsonSerializer.Serialize(queryParams), encoding: Encoding.UTF8, mediaType: "application/json")
            };
            HttpResponseMessage response = await httpClient.SendAsync(httpRequest);
            if (!response.IsSuccessStatusCode)
            {
                Log.Error($"HTTP request to turn on NLP failed. Response: {System.Text.Json.JsonSerializer.Serialize(response)}");
                return false;
            }
            string responseString = await response.Content.ReadAsStringAsync();
            ActionResultPayload payload = JsonConvert.DeserializeObject<ActionResultPayload>(responseString)!;
            if (payload?.Error != null)
            {
                Log.Error($"Encountered an error when configuring NLP setting. {payload?.Error?.CustomErrorMessage()}");
            }
            return payload?.Success ?? false;
        }

        /// <summary>
        /// Randomly generate an image URL for testing in development
        /// </summary>
        /// <returns>An image URL</returns>
        protected string GenerateImageUrlForDev()
        {
            var imageUrls = new List<string>()
            {
                "https://www.kasandbox.org/programming-images/avatars/spunky-sam.png",
                "https://www.kasandbox.org/programming-images/avatars/spunky-sam-green.png",
                "https://www.kasandbox.org/programming-images/avatars/purple-pi.png",
                "https://www.kasandbox.org/programming-images/avatars/purple-pi-teal.png",
                "https://www.kasandbox.org/programming-images/avatars/purple-pi-pink.png",
                "https://www.kasandbox.org/programming-images/avatars/primosaur-ultimate.png",
                "https://www.kasandbox.org/programming-images/avatars/primosaur-tree.png",
                "https://www.kasandbox.org/programming-images/avatars/primosaur-sapling.png",
                "https://www.kasandbox.org/programming-images/avatars/orange-juice-squid.png",
                "https://www.kasandbox.org/programming-images/avatars/old-spice-man.png",
                "https://www.kasandbox.org/programming-images/avatars/old-spice-man-blue.png",
                "https://www.kasandbox.org/programming-images/avatars/mr-pants.png",
                "https://www.kasandbox.org/programming-images/avatars/mr-pants-purple.png",
                "https://www.kasandbox.org/programming-images/avatars/mr-pants-green.png",
                "https://www.kasandbox.org/programming-images/avatars/marcimus.png",
                "https://www.kasandbox.org/programming-images/avatars/marcimus-red.png",
                "https://www.kasandbox.org/programming-images/avatars/marcimus-purple.png",
                "https://www.kasandbox.org/programming-images/avatars/marcimus-orange.png",
                "https://www.kasandbox.org/programming-images/avatars/duskpin-ultimate.png",
                "https://www.kasandbox.org/programming-images/avatars/duskpin-tree.png",
                "https://www.kasandbox.org/programming-images/avatars/duskpin-seedling.png",
                "https://www.kasandbox.org/programming-images/avatars/duskpin-seed.png",
                "https://www.kasandbox.org/programming-images/avatars/duskpin-sapling.png",
            };
            var rand = new Random();
            return imageUrls[rand.Next(imageUrls.Count)];
        }

        /// <summary>
        /// Create a media container on IG
        /// https://developers.facebook.com/docs/instagram-api/guides/content-publishing/
        /// https://developers.facebook.com/docs/instagram-api/reference/ig-user/media#creating
        /// </summary>
        /// <param name="httpClient">HTTP client for executing API query</param>
        /// <param name="igUserId">IG user ID</param>
        /// <param name="accessToken">Page access token</param>
        /// <param name="mediaType">Type of media to be published</param>
        /// <param name="mediaUrl">Publicly accessible URL of the picture to be posted</param>
        /// <param name="caption">Caption to be posted</param>
        /// <param name="isCarouselItem">True if the media item is part of a carousel post</param>
        /// <param name="carouselChildren">Up to 10 container IDs of images/videos that will be part of the carousel post</param>
        /// <returns>ID of the media container</returns>
        protected async Task<string> MakeIgItemContainer(HttpClient httpClient, string igUserId, string accessToken, IgMediaType mediaType,
            string mediaUrl = null, string caption = null, bool isCarouselItem = false, List<string> carouselChildren = null)
        {
            if (string.IsNullOrWhiteSpace(mediaUrl) && mediaType != IgMediaType.Carousel)
            {
                Log.Error($"Media url was blank when the media type was {mediaType}.");
                return null;
            }

            dynamic queryParams = new ExpandoObject();
            queryParams.access_token = accessToken;
            switch (mediaType)
            {
                case IgMediaType.Image:
                    if (isCarouselItem)
                    {
                        queryParams.is_carousel_item = true;
                    }
                    else
                    {
                        queryParams.caption = caption;
                    }
                    queryParams.image_url = mediaUrl;
                    break;
                case IgMediaType.Video:
                    if (isCarouselItem)
                    {
                        queryParams.is_carousel_item = true;
                    }
                    else
                    {
                        queryParams.caption = caption;
                    }
                    queryParams.video_url = mediaUrl;
                    break;
                case IgMediaType.Stories:
                    // PLACEHOLDER
                    queryParams.media_type = EnumHelper.GetDescription(mediaType);
                    break;
                case IgMediaType.Reels:
                    // PLACEHOLDER
                    queryParams.media_type = EnumHelper.GetDescription(mediaType);
                    queryParams.video_url = mediaUrl;
                    break;
                case IgMediaType.Carousel:
                    if (!(carouselChildren?.Any() ?? false))
                    {
                        Log.Error("No children container ids were provided for making a carousel container.");
                        return null;
                    }
                    queryParams.media_type = EnumHelper.GetDescription(mediaType);
                    queryParams.caption = caption;
                    queryParams.children = carouselChildren;
                    break;
            }

            var httpRequest = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new System.Uri($"{BaseUrl}/{ApiVersion}/{igUserId}/media"),
                Content = new StringContent(content: System.Text.Json.JsonSerializer.Serialize(queryParams), encoding: Encoding.UTF8, mediaType: "application/json")
            };
            HttpResponseMessage response = await httpClient.SendAsync(httpRequest);
            if (!response.IsSuccessStatusCode)
            {
                Log.Error($"HTTP request to create IG container fails. Response: {System.Text.Json.JsonSerializer.Serialize(response)}");
                return null;
            }
            string responseString = await response.Content.ReadAsStringAsync();
            OneLinePayload payload = JsonConvert.DeserializeObject<OneLinePayload>(responseString)!;
            if (payload?.Error != null)
            {
                Log.Error($"Encountered an error in creating container for {mediaUrl}. {payload?.Error?.CustomErrorMessage()}");
            }
            return payload?.MetaID;
        }

        /// <summary>
        /// Publish a media container
        /// https://developers.facebook.com/docs/instagram-api/guides/content-publishing/
        /// </summary>
        /// <param name="httpClient">HTTP client for executing API query</param>
        /// <param name="igUserId">IG user ID</param>
        /// <param name="accessToken">Page access token</param>
        /// <param name="mediaContainerId">ID of the media container to be published</param>
        /// <returns>ID of the published item</returns>
        protected async Task<string> PublishIgMedia(HttpClient httpClient, string igUserId, string accessToken, string mediaContainerId)
        {
            dynamic queryParams = new ExpandoObject();
            queryParams.access_token = accessToken;
            queryParams.creation_id = mediaContainerId;
            var httpRequest = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new System.Uri($"{BaseUrl}/{ApiVersion}/{igUserId}/media_publish"),
                Content = new StringContent(content: System.Text.Json.JsonSerializer.Serialize(queryParams), encoding: Encoding.UTF8, mediaType: "application/json")
            };
            HttpResponseMessage response = await httpClient.SendAsync(httpRequest);
            if (!response.IsSuccessStatusCode)
            {
                Log.Error($"HTTP request to publish IG media fails. Response: {System.Text.Json.JsonSerializer.Serialize(response)}");
                return null;
            }

            string responseString = await response.Content.ReadAsStringAsync();
            OneLinePayload payload = JsonConvert.DeserializeObject<OneLinePayload>(responseString)!;
            if (payload.Error != null)
            {
                Log.Error($"Encountered an error in publising media {mediaContainerId} for {igUserId}. {payload?.Error?.CustomErrorMessage()}");
            }
            return payload?.MetaID;
        }

        /// <summary>
        /// Publish a carousel post on IG
        /// https://developers.facebook.com/docs/instagram-api/guides/content-publishing/
        /// </summary>
        /// <param name="httpClient">HTTP client for executing API query</param>
        /// <param name="accessToken">Page access token</param>
        /// <param name="igUserId">IG user ID</param>
        /// <param name="mediaUrls">List of publicly accessible URLs of the images to be posted</param>
        /// <param name="message">Message to be posted</param>
        /// <returns>ID of the media item posted</returns>
        protected async Task<string> MakeIgCarouselPost(HttpClient httpClient, string accessToken, string igUserId, List<string> mediaUrls, string message)
        {
            if (mediaUrls == null)
            {
                Log.Error("No media URLs were provided.");
                return null;
            }
            
            // we need a cache mapping container Id against mediaUrl in case we sucessfully create ONLY 1 container (e.g., due to file size or format issue)
            Dictionary<string, string> cache = new();
            foreach (string mediaUrl in mediaUrls)
            {
                // note: image is hardcoded as the media type as we don't have a user case for posting video yet
                string mediaContainerId = await MakeIgItemContainer(httpClient: httpClient, igUserId: igUserId, accessToken: accessToken,
                    mediaType: IgMediaType.Image, mediaUrl: mediaUrl, isCarouselItem: true);

                if (string.IsNullOrWhiteSpace(mediaContainerId))
                {
                    Log.Error($"Failed to create a container for image {mediaUrl}");
                    continue;
                }

                // note: only up to 10 images can be included in a carousel post                
                cache[mediaContainerId] = mediaUrl;
                if (cache.Count == 10) break;
            }

            // IG does not allow text-only posts
            if (cache.Count == 0)
            {
                Log.Error("Failed to create any image/video container for a carousel post.");
                return null;
            }

            // IG would return error message if there is only 1 carousel item
            if (cache.Count == 1)
            {
                return await MakeIgNonCarouselPost(httpClient: httpClient, igUserId: igUserId, accessToken: accessToken,
                    mediaUrl: cache.Select(x => x.Value).ToList().FirstOrDefault(), message: message, mediaType: IgMediaType.Image);
            }

            string carouselId = await MakeIgItemContainer(httpClient: httpClient, igUserId: igUserId, accessToken: accessToken,
                mediaType: IgMediaType.Carousel, caption: message, carouselChildren: cache.Select(x => x.Key).ToList());
            if (string.IsNullOrWhiteSpace(carouselId))
            {
                Log.Error($"Failed to create a container for carousel.");
                return null;
            }

            return await PublishIgMedia(httpClient: httpClient, igUserId: igUserId, accessToken: accessToken, mediaContainerId: carouselId);
        }

        /// <summary>
        /// Make a non-carousel post on IG
        /// </summary>
        /// <param name="httpClient">HTTP client for executing API query</param>
        /// <param name="accessToken">Page access token</param>
        /// <param name="igUserId">IG user ID</param>
        /// <param name="mediaUrl">Publicly accessible URL of the image/video to be posted</param>
        /// <param name="message">Message to be posted</param>
        /// <param name="mediaType">Type of media to be posted</param>
        /// <returns>ID of the media item posted</returns>
        protected async Task<string> MakeIgNonCarouselPost(HttpClient httpClient, string accessToken, string igUserId, string mediaUrl, string message, IgMediaType mediaType)
        {
            string mediaContainerId = await MakeIgItemContainer(httpClient: httpClient, igUserId: igUserId, accessToken: accessToken,
                mediaUrl: mediaUrl, caption: message, mediaType: mediaType);
            if (string.IsNullOrWhiteSpace(mediaContainerId)) return string.Empty;

            string mediaId = await PublishIgMedia(httpClient: httpClient, igUserId: igUserId, accessToken: accessToken,
                mediaContainerId: mediaContainerId);
            return mediaId;
        }

        /// <summary>
        /// Create a multi-photo post on FB
        /// https://developers.facebook.com/docs/pages-api/posts/
        /// https://developers.facebook.com/docs/graph-api/reference/v18.0/page/feed
        /// </summary>
        /// <param name="httpClient">HTTP client for executing API query</param>
        /// <param name="accessToken">Page access token</param>
        /// <param name="pageId">Page ID</param>
        /// <param name="imageUrls">List of publicly accessible URLs of the images to be posted</param>
        /// <param name="message">Message to be posted</param>
        /// <returns>ID of the post</returns>
        protected async Task<string> MakeFbMultiPhotoPost(HttpClient httpClient, string accessToken, string pageId, List<string> imageUrls, string message)
        {
            if (imageUrls == null)
            {
                Log.Error("No image URLs were provided.");
                return null;
            }

            List<string> photoIds = new();
            string photoId;
            foreach (string imageUrl in imageUrls)
            {
                photoId = await ApiActionHelper.PostOrComment(httpClient: httpClient, accessToken: accessToken,
                    endPoint: $"{BaseUrl}/{ApiVersion}/{pageId}/photos",
                    mediaUrl: imageUrl, published: false);
                if (string.IsNullOrWhiteSpace(photoId))
                {
                    Log.Error($"Failed to upload photo {imageUrl}");
                    continue;
                }
                photoIds.Add(photoId);
            }

            return await ApiActionHelper.PostOrComment(httpClient: httpClient, accessToken: accessToken,
                endPoint: $"{BaseUrl}/{ApiVersion}/{pageId}/feed",
                message: message, photoIds: photoIds);
        }        
    }
}

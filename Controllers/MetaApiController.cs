﻿using Corprio.AspNetCore.Site.Controllers;
using Corprio.CorprioAPIClient;
using Corprio.SocialWorker.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Serilog;
using Corprio.DataModel.Business;
using System.Net.Http.Headers;
using Corprio.AspNetCore.Site.Filters;
using Corprio.DataModel;
using Corprio.DataModel.Business.Products;
using System.Dynamic;
using Corprio.DataModel.Business.Products.ViewModel;
using Microsoft.Extensions.Configuration;
using Corprio.SocialWorker.Helpers;
using Corprio.Core;
using System.Linq;
using Corprio.SocialWorker.Dictionaries;
using Corprio.DataModel.Shared;
using Corprio.Core.Exceptions;

namespace Corprio.SocialWorker.Controllers
{
    public class MetaApiController : OrganizationBaseController
    {
        readonly IConfiguration configuration;
        public readonly string AppId;
        public readonly string ApiVersion;
        public readonly string AppSecret;
        public readonly string BaseUrl;        

        public MetaApiController(IConfiguration configuration) : base()
        {
            this.configuration = configuration;
            AppId = configuration["MetaApiSetting:AppId"];
            ApiVersion = configuration["MetaApiSetting:ApiVersion"];
            BaseUrl = configuration["MetaApiSetting:BaseUrl"];            
            AppSecret = "b8956ab840c1d15a8bb0bee5551a6ccb"; // Put this in a vault!!!
        }                
        
        public override IActionResult Index([FromRoute] Guid organizationID) => base.Index(organizationID);

        /// <summary>
        /// Randomly generate an image URL for testing in development
        /// </summary>
        /// <returns>An image URL</returns>
        public string GenerateImageUrlForDev()
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
        public async Task<string> MakeIgItemContainer(HttpClient httpClient, string igUserId, string accessToken, IgMediaType mediaType,
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
            return payload?.Id;
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
        public async Task<string> PublishIgMedia(HttpClient httpClient, string igUserId, string accessToken, string mediaContainerId)
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
            return payload?.Id;
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
        public async Task<string> MakeIgCarouselPost(HttpClient httpClient, string accessToken, string igUserId, List<string> mediaUrls, string message)
        {
            // we need a cache mapping container Id against mediaUrl in case we sucessfully create ONLY 1 container (e.g., due to file size or format issue)
            Dictionary<string, string> cache = new Dictionary<string, string>();
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

            if (cache.Count == 0)
            {
                Log.Error("Failed to create any image/video container for a carousel post.");
                return null;
            }

            // IG would return error message if there is only 1 few carousel item
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
        public async Task<string> MakeIgNonCarouselPost(HttpClient httpClient, string accessToken, string igUserId, string mediaUrl, string message, IgMediaType mediaType)
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
        public async Task<string> MakeFbMultiPhotoPost(HttpClient httpClient, string accessToken, string pageId, List<string> imageUrls, string message)
        {
            List<string> photoIds = new();
            string photoId;
            foreach (string imageUrl in imageUrls)
            {
                photoId = await ActionHelper.PostOrComment(httpClient: httpClient, accessToken: accessToken,
                    endPoint: $"{BaseUrl}/{ApiVersion}/{pageId}/photos",
                    mediaUrl: imageUrl, published: false);
                if (string.IsNullOrWhiteSpace(photoId))
                {
                    Log.Error($"Failed to upload photo {imageUrl}");
                    continue;
                }
                photoIds.Add(photoId);
            }

            return await ActionHelper.PostOrComment(httpClient: httpClient, accessToken: accessToken,
                endPoint: $"{BaseUrl}/{ApiVersion}/{pageId}/feed",
                message: message, photoIds: photoIds);
        }

        /// <summary>
        /// Extract non-null image IDs from a product
        /// </summary>
        /// <param name="product">An object of Product class</param>
        /// <returns>A set of image IDs</returns>
        public HashSet<Guid> ReturnImageUrls(Product product)
        {
            HashSet<Guid> validIds = new();
            if (product == null) return validIds;

            List<Guid?> imageIDs = new() { product.Image01ID, product.Image02ID, product.Image03ID, product.Image04ID,
                product.Image05ID, product.Image06ID, product.Image07ID, product.Image08ID };
            foreach (Guid? imageID in imageIDs)
            {
                if (imageID.HasValue) validIds.Add(imageID.Value);
            }
            return validIds;
        }
    }
}

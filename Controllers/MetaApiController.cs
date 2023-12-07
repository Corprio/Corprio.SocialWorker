using Corprio.AspNetCore.Site.Controllers;
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

namespace Corprio.SocialWorker.Controllers
{
    public class MetaApiController : OrganizationBaseController
    {
        readonly IConfiguration configuration;
        private readonly string AppId;
        private readonly string ApiVersion;
        private readonly string AppSecret;
        private readonly string BaseUrl;

        public MetaApiController(IConfiguration configuration) : base()
        {
            this.configuration = configuration;
            AppId = configuration["MetaApiSetting:AppId"];
            ApiVersion = configuration["MetaApiSetting:ApiVersion"];
            BaseUrl = configuration["MetaApiSetting:BaseUrl"];
            AppSecret = "b8956ab840c1d15a8bb0bee5551a6ccb"; // Put this in a vault!!!
        }
        
        private Dictionary<Guid, Dictionary<string, Dictionary<string, string>>> TokenCache = new();

        

        public override IActionResult Index([FromRoute] Guid organizationID) => base.Index(organizationID);                        

        /// <summary>
        /// Create a media container on IG
        /// https://developers.facebook.com/docs/instagram-api/guides/content-publishing/
        /// https://developers.facebook.com/docs/instagram-api/reference/ig-user/media#creating
        /// </summary>
        /// <param name="httpClient">HTTP client for executing API query</param>
        /// <param name="igUserId">IG user ID</param>
        /// <param name="accessToken">Page access token</param>
        /// <param name="mediaUrl">Publicly accessible URL of the picture to be posted</param>
        /// <param name="caption">Caption to be posted</param>
        /// <param name="mediaType">Type of media to be published</param>
        /// <param name="isCarouselItem">True if the media item is part of a </param>
        /// <returns>ID of the media container</returns>
        public async Task<string> MakeIgItemContainer(HttpClient httpClient, string igUserId, string accessToken, 
            string mediaUrl, string caption, IgMediaType mediaType, bool isCarouselItem = false)
        {
            dynamic queryParams = new ExpandoObject();
            queryParams.access_token = accessToken;
            switch (mediaType)
            {
                case IgMediaType.Image:
                    queryParams.caption = caption;
                    queryParams.image_url = mediaUrl;
                    if (isCarouselItem) queryParams.is_carousel_item = true;
                    //queryParams.product_tags = "";  // TODO
                    break;
                case IgMediaType.Video:
                    queryParams.caption = caption;
                    queryParams.video_url = mediaUrl;
                    if (isCarouselItem) queryParams.is_carousel_item = true;
                    //queryParams.product_tags = "";  // TODO
                    break;
                case IgMediaType.Stories:
                    queryParams.media_type = nameof(mediaType);
                    break;
                case IgMediaType.Reels:
                    queryParams.media_type = nameof(mediaType);
                    queryParams.video_url = mediaUrl;
                    //queryParams.cover_url = "";  // TODO
                    break;
                case IgMediaType.Carousel:
                    queryParams.media_type = nameof(mediaType);
                    queryParams.caption = caption;
                    //queryParams.children = "";  // TODO                    
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
        /// Post a media item (e.g., picture) to IG
        /// </summary>
        /// <param name="httpClient">HTTP client for executing API query</param>
        /// <param name="accessToken">Page access token</param>
        /// <param name="igUserId">IG user ID</param>
        /// <param name="mediaUrl">Publicly accessible URL of the picture to be posted</param>
        /// <param name="message">Message to be posted</param>
        /// <param name="mediaType">Type of media to be posted</param>
        /// <returns>ID of the media item posted</returns>        
        public async Task<string> MakeIgPost(HttpClient httpClient, string accessToken, string igUserId, string mediaUrl, string message, IgMediaType mediaType)
        {                                                
            string mediaContainerId = await MakeIgItemContainer(httpClient: httpClient, igUserId: igUserId, accessToken: accessToken, 
                mediaUrl: mediaUrl, caption: message, mediaType: mediaType);
            if (string.IsNullOrWhiteSpace(mediaContainerId)) return string.Empty;

            string mediaId = await PublishIgMedia(httpClient: httpClient, igUserId: igUserId, accessToken: accessToken, 
                mediaContainerId: mediaContainerId);            
            return mediaId;
        }

        /// <summary>
        /// Trigger post making
        /// https://developers.facebook.com/docs/pages-api/posts/
        /// https://developers.facebook.com/docs/graph-api/reference/v18.0/page/feed
        /// </summary>
        /// <param name="httpClient">HTTP client for executing API query</param>
        /// <param name="pageId">ID of the page on which the post will be made</param>
        /// <param name="token">Page access token</param>
        /// <param name="message">Message of the post</param>
        /// <param name="mediaUrl">URL of media to be included in a post</param>
        /// <param name="link">URL to be included in the post</param>
        /// <returns>ID of the post </returns>        
        public async Task<string> MakeFbPost(HttpClient httpClient, string pageId, string token, string message, string mediaUrl = null, string link = null)
        {
            string endPoint = string.IsNullOrWhiteSpace(mediaUrl)
                ? $"{BaseUrl}/{ApiVersion}/{pageId}/feed"
                : $"{BaseUrl}/{ApiVersion}/{pageId}/photos";

            return await ActionHelper.PostOrComment(httpClient: httpClient, accessToken: token, message: message, endPoint: endPoint, mediaUrl: mediaUrl, link: link);
        }        

        

        /// <summary>
        /// Publish a product list
        /// </summary>
        /// <param name="httpClient">HTTP client for executing API query</param>
        /// <param name="corprioClient">Client for Api requests among Corprio projects</param>
        /// <param name="organizationID">Organization ID</param>
        /// <param name="productlistID">Entity ID of product list</param>        
        /// <returns></returns>
        public async Task PublishCatalogue(HttpClient httpClient, APIClient corprioClient, Guid organizationID, Guid productlistID)
        {
            string setting = await corprioClient.OrganizationApi.GetApplicationSetting(organizationID);
            if (string.IsNullOrWhiteSpace(setting)) throw new Exception($"No application setting found for {organizationID}");
            MetaProfileModel metaProfile = JsonConvert.DeserializeObject<MetaProfileModel>(setting)! ?? throw new Exception($"No Meta profile for {organizationID}.");

            ProductList productList = await corprioClient.ProductListApi.Get(organizationID: organizationID, id: productlistID)
                ?? throw new Exception($"Product list {productlistID} cannot be found.");

            string orgShortName = await corprioClient.OrganizationApi.GetShortName(organizationID);
            if (string.IsNullOrWhiteSpace(orgShortName)) throw new Exception($"Failed to get short name for organization {organizationID}.");

            PagedList<ProductInfo> products = await corprioClient.ProductListApi.GetProductsPageOfList(
                    organizationID: organizationID,
                    productListID: productlistID,
                    loadDataOptions: new LoadDataOptions()
                    {
                        PageIndex = 0,
                        PageSize = 10,
                        RequireTotalCount = false,
                        Sort = new SortingInfo[] { new SortingInfo { Desc = false, Selector = "CreateDate" } },
                    });

            foreach (ProductInfo productInfo in products.Items)
            {
                Product product = await corprioClient.ProductApi.Get(organizationID: organizationID, id: productInfo.ID);
                if (product == null)
                {
                    Log.Error($"Product {productInfo.ID} in product list {productlistID} cannot be found.");
                    continue;
                }
                // note: cannot find anyway to make the hyperlink clickable...
                string message = $"{product.Name}(Code: {product.Code})\nLowest selling price: {product.ListPrice_CurrencyCode}{productInfo.LowPrice}\nTo learn more, visit {configuration["GoBuyClickUrl"]}/Catalogue/{orgShortName}/{productList.Code}\n\nProduct descriptions:\n{product.Description}";

                // note: if there is no product image, then only a FB post is made
                // TODO - we need to make sure only .jpg is included in IG media, although apparently .png also works...
                string imageUrlKey = productInfo.Image01ID == null ? string.Empty : await corprioClient.ImageApi.UrlKey(organizationID: organizationID, id: (Guid)productInfo.Image01ID);
                string imageUrl = string.IsNullOrWhiteSpace(imageUrlKey) ? string.Empty : corprioClient.ImageApi.Url(organizationID: organizationID, imageUrlKey: imageUrlKey);

                // DEV ONLY: the imageUrl generated in DEV environment won't work, so we re-assign a publicly accessible URL to it FOR NOW
                var imageUrls = new List<string>(){
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
                imageUrl = imageUrls[rand.Next(imageUrls.Count)];

                foreach (var page in metaProfile.Pages)
                {
                    await MakeFbPost(httpClient: httpClient, pageId: page.PageId, token: page.Token, message: message, mediaUrl: imageUrl);
                    if (!string.IsNullOrWhiteSpace(imageUrl) && !string.IsNullOrWhiteSpace(page.IgId))
                    {
                        await MakeIgPost(httpClient: httpClient, accessToken: page.Token, igUserId: page.IgId,
                            mediaUrl: imageUrl, message: message, mediaType: IgMediaType.Image);
                    }
                }
            }
        }

        /// <summary>
        /// Trigger the publication of a catalogue / product list
        /// </summary>
        /// <param name="httpClient">HTTP client for executing API query</param>
        /// <param name="corprioClient">Client for Api requests among Corprio projects</param>
        /// <param name="organizationID">Organization ID</param>
        /// <param name="productlistID">Entity ID of product list</param>        
        /// <returns></returns>
        [OrganizationAuthorizationCheck(
             ActionEntityTypes = new EntityType[] { EntityType.Invoice },
             RequiredPermissions = new DataAction[] { DataAction.Read })]
        [HttpPost("/{organizationID:guid}/publish/{productlistID:guid}")]
        public async Task TriggerPostingCatalogue([FromServices] HttpClient httpClient, [FromServices] APIClient corprioClient,
            [FromRoute] Guid organizationID, [FromRoute] Guid productlistID)
        {
            await PublishCatalogue(httpClient: httpClient, corprioClient: corprioClient, organizationID: organizationID, productlistID: productlistID);
        }

        /// <summary>
        /// Query Meta to obtain the ID of IG Professional account linked to a particular FB page
        /// </summary>
        /// <param name="httpClient">HTTP client for executing API query</param>
        /// <param name="accessToken">Page access token</param>
        /// <param name="pageId">Page ID</param>
        /// <returns>ID of IG user</returns>
        public async Task<string> GetIgUserId(HttpClient httpClient, string accessToken, string pageId)
        {            
            var httpRequest = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new System.Uri($"{BaseUrl}/{ApiVersion}/{pageId}?fields=instagram_business_account"),                
            };
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage response = await httpClient.SendAsync(httpRequest);
            if (!response.IsSuccessStatusCode)
            {
                Log.Error($"HTTP request to obtain Instagram Professional account ID fails. Response: {System.Text.Json.JsonSerializer.Serialize(response)}");
                return null;
            }
            string responseString = await response.Content.ReadAsStringAsync();
            FbPagePayload payload = response?.Content == null ? new() : JsonConvert.DeserializeObject<FbPagePayload>(responseString)!;
            if (payload?.Error != null)
            {
                Log.Error($"Encountered an error in obtaining IG user ID for {pageId}. {payload?.Error?.CustomErrorMessage()}");
            }
            return payload?.InstagramBusinessAccount?.Id;
        }

        /// <summary>
        /// Get a list of FB pages on which the user has a role
        /// </summary>
        /// <param name="httpClient">HTTP client for executing API query</param>
        /// <param name="userId">ID of the user in question</param>
        /// <param name="userAccessToken">User access token</param>
        /// <returns>List of FB pages on which the user has a role</returns>
        public async Task<List<FbPage>> GetMeAccounts(HttpClient httpClient, string userId, string userAccessToken)
        {            
            var httpRequest = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new System.Uri($"{BaseUrl}/{userId}/accounts"),                                
            };
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userAccessToken);
            HttpResponseMessage response = await httpClient.SendAsync(httpRequest);
            if (!response.IsSuccessStatusCode)
            {
                Log.Error($"HTTP request to obtain get Facebook pages fails. Response: {System.Text.Json.JsonSerializer.Serialize(response)}");
                return null;
            }
            string responseString = await response.Content.ReadAsStringAsync();
            MeAccountsPayload payload = JsonConvert.DeserializeObject<MeAccountsPayload>(responseString)!;
            if (payload?.Error != null)
            {
                Log.Error($"Encountered an error when getting pages owned by {userId}. {payload?.Error?.CustomErrorMessage()}");
            }
            return payload?.Data;
        }

        /// <summary>
        /// Get a long-lived user access token from Meta
        /// </summary>
        /// <param name="httpClient">HTTP client for executing API query</param>
        /// <param name="userAccessToken">Short-lived user access token in exchange for a long-lived token</param>
        /// <returns>Long-lived user access token</returns>
        public async Task<string> GetLongLivedAccessToken(HttpClient httpClient, string userAccessToken)
        {
            var queryParams = new { grant_type = "fb_exchange_token", client_id = AppId, client_secret = AppSecret, fb_exchange_token = userAccessToken };
            var httpRequest = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new System.Uri($"{BaseUrl}/{ApiVersion}/oauth/access_token"),
                Content = new StringContent(content: System.Text.Json.JsonSerializer.Serialize(queryParams), encoding: Encoding.UTF8, mediaType: "application/json")
            };
            HttpResponseMessage response = await httpClient.SendAsync(httpRequest);
            if (!response.IsSuccessStatusCode) 
            {
                Log.Error($"HTTP request to obtain long-lived access token fails. Response: {System.Text.Json.JsonSerializer.Serialize(response)}");
                return null;
            }
            string responseString = await response.Content.ReadAsStringAsync();
            LongLivedUserAccessTokenPayload payload = JsonConvert.DeserializeObject<LongLivedUserAccessTokenPayload>(responseString)!;
            if (payload?.Error != null)
            {
                Log.Error($"Encountered an error when getting long-lived access token. {payload?.Error?.CustomErrorMessage()}");
            }
            return payload?.AccessToken;
        }

        /// <summary>
        /// Update the TokenCache
        /// </summary>
        /// <param name="organizationID">Organization ID</param>
        /// <param name="metaProfile">An object of MetaProfileModel class</param>
        public void UpdateTokenCache(Guid organizationID, MetaProfileModel metaProfile)
        {
            if (!TokenCache.ContainsKey(organizationID))
            {                
                TokenCache.Add(
                    key: organizationID,
                    value: new Dictionary<string, Dictionary<string, string>>()
                    {
                        [metaProfile.Id] = new Dictionary<string, string>()
                    });
                foreach (var page in metaProfile.Pages)
                    TokenCache[organizationID][metaProfile.Id].Add(key: page.PageId, value: page.Token);
                return;
            }
            
            if (!TokenCache[organizationID].ContainsKey(metaProfile.Id))
            {
                TokenCache[organizationID] ??= new Dictionary<string, Dictionary<string, string>>();
                TokenCache[organizationID].Add(key: metaProfile.Id, value: new Dictionary<string, string>());
                foreach (var page in metaProfile.Pages)
                    TokenCache[organizationID][metaProfile.Id].Add(key: page.PageId, value: page.Token);
                return;
            }

            foreach (var page in metaProfile.Pages)
            {
                TokenCache[organizationID][metaProfile.Id] ??= new Dictionary<string, string>();
                if (TokenCache[organizationID][metaProfile.Id].ContainsKey(page.PageId))
                    TokenCache[organizationID][metaProfile.Id][page.PageId] = page.Token;
                else
                    TokenCache[organizationID][metaProfile.Id].Add(key: page.PageId, value: page.Token);
            }
            return;
        }

        /// <summary>
        /// Turn on Meta's Built-in NLP to help detect meaning
        /// https://developers.facebook.com/docs/graph-api/reference/page/nlp_configs/
        /// https://developers.facebook.com/docs/messenger-platform/built-in-nlp/
        /// </summary>
        /// <param name="httpClient">HTTP client for executing API query</param>
        /// <param name="accessToken">Page access token</param>
        /// <param name="language">Language to be selected as the default model</param>
        /// <returns>True if the operation succeeds</returns>
        public async Task<bool> TurnOnNLP(HttpClient httpClient, string accessToken, MetaLanguageModel language)
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
        /// Register/refresh access token(s) and relevant FB pages and Meta user in the database
        /// </summary>
        /// <param name="httpClient">HTTP client for executing API query</param>
        /// <param name="corprioClient">Client for Api requests among Corprio projects</param>
        /// <param name="organizationID">Organization ID</param>
        /// <param name="metaId">Meta entity ID associated with the access token</param>
        /// <param name="token">Access token</param>
        /// <returns>Status code</returns>
        /// <exception cref="Exception"></exception>
        public async Task<IActionResult> RefreshAccessToken([FromServices] HttpClient httpClient, [FromServices] APIClient corprioClient,
            [FromRoute] Guid organizationID, string metaId, string token)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(metaId))
                return StatusCode(400, "Token and Meta entity ID cannot be blank. ");
                        
            token = await GetLongLivedAccessToken(httpClient: httpClient, userAccessToken: token);
            if (string.IsNullOrWhiteSpace(token)) return StatusCode(400, "Failed to obtain long-lived user access token.");
            var metaProfile = new MetaProfileModel() { Id = metaId, Token = token };
            List<FbPage> fbPages = await GetMeAccounts(httpClient: httpClient, userId: metaId, userAccessToken: token)
                ?? throw new Exception($"Encountered an error in retrieving pages on which {metaId} has a role.");            
            foreach (FbPage page in fbPages)
            {
                metaProfile.Pages.Add(new MetaPage() { 
                    PageId = page.Id, 
                    Name = page.Name, 
                    Token = page.AccessToken,
                    // note: pages are NOT necessarily associated with any igUserId, so null is acceptable
                    IgId = await GetIgUserId(httpClient: httpClient, accessToken: page.AccessToken, pageId: page.Id)
                });                
                await TurnOnNLP(httpClient: httpClient, accessToken: page.AccessToken, language: MetaLanguageModel.Chinese);
            }

            string settingString = await corprioClient.OrganizationApi.GetApplicationSetting(organizationID);
            if (!string.IsNullOrWhiteSpace(settingString))
            {
                MetaProfileModel existingProfile = JsonConvert.DeserializeObject<MetaProfileModel>(settingString)!;
                if (existingProfile != null) metaProfile.Bots = existingProfile.Bots;
            }

            // TODO - we should save the Meta profile in ApplicationSubscriptions when it has an EP
            await corprioClient.OrganizationApi.SaveApplicationSetting(
                organizationID: organizationID,
                value: System.Text.Json.JsonSerializer.Serialize(metaProfile));            

            //try
            //{
            //    await PublishCatalogue(httpClient: httpClient, corprioClient: corprioClient, organizationID: organizationID,
            //        productlistID: Guid.Parse("6939a8d7-4cc9-422a-a66f-15d9287d1035"));
            //}
            //catch (Exception ex)
            //{
            //    throw;
            //}

            return StatusCode(200);
        }
    }
}

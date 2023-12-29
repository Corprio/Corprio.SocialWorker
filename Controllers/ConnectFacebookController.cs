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
using Corprio.Core;
using System.Linq;
using Corprio.SocialWorker.Dictionaries;
using Corprio.DataModel.Shared;
using Corprio.Core.Exceptions;
using Corprio.Core.Utility;
using Microsoft.EntityFrameworkCore;

namespace Corprio.SocialWorker.Controllers
{
    public class ConnectFacebookController : MetaApiController
    {
        private readonly ApplicationDbContext db;        

        public ConnectFacebookController(ApplicationDbContext context, IConfiguration configuration) : base(configuration)
        {
            db = context;   
        }                
        
        public override IActionResult Index([FromRoute] Guid organizationID) => base.Index(organizationID);

        ///// <summary>
        ///// Randomly generate an image URL for testing in development
        ///// </summary>
        ///// <returns>An image URL</returns>
        //public string GenerateImageUrlForDev()
        //{
        //    var imageUrls = new List<string>()
        //    {
        //        "https://www.kasandbox.org/programming-images/avatars/spunky-sam.png",
        //        "https://www.kasandbox.org/programming-images/avatars/spunky-sam-green.png",
        //        "https://www.kasandbox.org/programming-images/avatars/purple-pi.png",
        //        "https://www.kasandbox.org/programming-images/avatars/purple-pi-teal.png",
        //        "https://www.kasandbox.org/programming-images/avatars/purple-pi-pink.png",
        //        "https://www.kasandbox.org/programming-images/avatars/primosaur-ultimate.png",
        //        "https://www.kasandbox.org/programming-images/avatars/primosaur-tree.png",
        //        "https://www.kasandbox.org/programming-images/avatars/primosaur-sapling.png",
        //        "https://www.kasandbox.org/programming-images/avatars/orange-juice-squid.png",
        //        "https://www.kasandbox.org/programming-images/avatars/old-spice-man.png",
        //        "https://www.kasandbox.org/programming-images/avatars/old-spice-man-blue.png",
        //        "https://www.kasandbox.org/programming-images/avatars/mr-pants.png",
        //        "https://www.kasandbox.org/programming-images/avatars/mr-pants-purple.png",
        //        "https://www.kasandbox.org/programming-images/avatars/mr-pants-green.png",
        //        "https://www.kasandbox.org/programming-images/avatars/marcimus.png",
        //        "https://www.kasandbox.org/programming-images/avatars/marcimus-red.png",
        //        "https://www.kasandbox.org/programming-images/avatars/marcimus-purple.png",
        //        "https://www.kasandbox.org/programming-images/avatars/marcimus-orange.png",
        //        "https://www.kasandbox.org/programming-images/avatars/duskpin-ultimate.png",
        //        "https://www.kasandbox.org/programming-images/avatars/duskpin-tree.png",
        //        "https://www.kasandbox.org/programming-images/avatars/duskpin-seedling.png",
        //        "https://www.kasandbox.org/programming-images/avatars/duskpin-seed.png",
        //        "https://www.kasandbox.org/programming-images/avatars/duskpin-sapling.png",
        //    };
        //    var rand = new Random();
        //    return imageUrls[rand.Next(imageUrls.Count)];
        //}

        ///// <summary>
        ///// Create a media container on IG
        ///// https://developers.facebook.com/docs/instagram-api/guides/content-publishing/
        ///// https://developers.facebook.com/docs/instagram-api/reference/ig-user/media#creating
        ///// </summary>
        ///// <param name="httpClient">HTTP client for executing API query</param>
        ///// <param name="igUserId">IG user ID</param>
        ///// <param name="accessToken">Page access token</param>
        ///// <param name="mediaType">Type of media to be published</param>
        ///// <param name="mediaUrl">Publicly accessible URL of the picture to be posted</param>
        ///// <param name="caption">Caption to be posted</param>
        ///// <param name="isCarouselItem">True if the media item is part of a carousel post</param>
        ///// <param name="carouselChildren">Up to 10 container IDs of images/videos that will be part of the carousel post</param>
        ///// <returns>ID of the media container</returns>
        //public async Task<string> MakeIgItemContainer(HttpClient httpClient, string igUserId, string accessToken, IgMediaType mediaType,
        //    string mediaUrl = null, string caption = null, bool isCarouselItem = false, List<string> carouselChildren = null)
        //{
        //    if (string.IsNullOrWhiteSpace(mediaUrl) && mediaType != IgMediaType.Carousel)
        //    {
        //        Log.Error($"Media url was blank when the media type was {mediaType}.");
        //        return null;
        //    }

        //    dynamic queryParams = new ExpandoObject();
        //    queryParams.access_token = accessToken;
        //    switch (mediaType)
        //    {
        //        case IgMediaType.Image:
        //            if (isCarouselItem)
        //            {
        //                queryParams.is_carousel_item = true;
        //            }
        //            else
        //            {
        //                queryParams.caption = caption;
        //            }
        //            queryParams.image_url = mediaUrl;
        //            break;
        //        case IgMediaType.Video:
        //            if (isCarouselItem)
        //            {
        //                queryParams.is_carousel_item = true;
        //            }
        //            else
        //            {
        //                queryParams.caption = caption;
        //            }
        //            queryParams.video_url = mediaUrl;
        //            break;
        //        case IgMediaType.Stories:
        //            // PLACEHOLDER
        //            queryParams.media_type = EnumHelper.GetDescription(mediaType);
        //            break;
        //        case IgMediaType.Reels:
        //            // PLACEHOLDER
        //            queryParams.media_type = EnumHelper.GetDescription(mediaType);
        //            queryParams.video_url = mediaUrl;
        //            break;
        //        case IgMediaType.Carousel:
        //            if (!(carouselChildren?.Any() ?? false))
        //            {
        //                Log.Error("No children container ids were provided for making a carousel container.");
        //                return null;
        //            }
        //            queryParams.media_type = EnumHelper.GetDescription(mediaType);
        //            queryParams.caption = caption;
        //            queryParams.children = carouselChildren;
        //            break;
        //    }

        //    var httpRequest = new HttpRequestMessage()
        //    {
        //        Method = HttpMethod.Post,
        //        RequestUri = new System.Uri($"{BaseUrl}/{ApiVersion}/{igUserId}/media"),
        //        Content = new StringContent(content: System.Text.Json.JsonSerializer.Serialize(queryParams), encoding: Encoding.UTF8, mediaType: "application/json")
        //    };
        //    HttpResponseMessage response = await httpClient.SendAsync(httpRequest);
        //    if (!response.IsSuccessStatusCode)
        //    {
        //        Log.Error($"HTTP request to create IG container fails. Response: {System.Text.Json.JsonSerializer.Serialize(response)}");
        //        return null;
        //    }
        //    string responseString = await response.Content.ReadAsStringAsync();
        //    OneLinePayload payload = JsonConvert.DeserializeObject<OneLinePayload>(responseString)!;
        //    if (payload?.Error != null)
        //    {
        //        Log.Error($"Encountered an error in creating container for {mediaUrl}. {payload?.Error?.CustomErrorMessage()}");
        //    }
        //    return payload?.Id;
        //}

        ///// <summary>
        ///// Publish a media container
        ///// https://developers.facebook.com/docs/instagram-api/guides/content-publishing/
        ///// </summary>
        ///// <param name="httpClient">HTTP client for executing API query</param>
        ///// <param name="igUserId">IG user ID</param>
        ///// <param name="accessToken">Page access token</param>
        ///// <param name="mediaContainerId">ID of the media container to be published</param>
        ///// <returns>ID of the published item</returns>
        //public async Task<string> PublishIgMedia(HttpClient httpClient, string igUserId, string accessToken, string mediaContainerId)
        //{
        //    dynamic queryParams = new ExpandoObject();
        //    queryParams.access_token = accessToken;
        //    queryParams.creation_id = mediaContainerId;
        //    var httpRequest = new HttpRequestMessage()
        //    {
        //        Method = HttpMethod.Post,
        //        RequestUri = new System.Uri($"{BaseUrl}/{ApiVersion}/{igUserId}/media_publish"),
        //        Content = new StringContent(content: System.Text.Json.JsonSerializer.Serialize(queryParams), encoding: Encoding.UTF8, mediaType: "application/json")
        //    };
        //    HttpResponseMessage response = await httpClient.SendAsync(httpRequest);
        //    if (!response.IsSuccessStatusCode)
        //    {
        //        Log.Error($"HTTP request to publish IG media fails. Response: {System.Text.Json.JsonSerializer.Serialize(response)}");
        //        return null;
        //    }

        //    string responseString = await response.Content.ReadAsStringAsync();
        //    OneLinePayload payload = JsonConvert.DeserializeObject<OneLinePayload>(responseString)!;
        //    if (payload.Error != null)
        //    {
        //        Log.Error($"Encountered an error in publising media {mediaContainerId} for {igUserId}. {payload?.Error?.CustomErrorMessage()}");
        //    }
        //    return payload?.Id;
        //}

        ///// <summary>
        ///// Publish a carousel post on IG
        ///// https://developers.facebook.com/docs/instagram-api/guides/content-publishing/
        ///// </summary>
        ///// <param name="httpClient">HTTP client for executing API query</param>
        ///// <param name="accessToken">Page access token</param>
        ///// <param name="igUserId">IG user ID</param>
        ///// <param name="mediaUrls">List of publicly accessible URLs of the images to be posted</param>
        ///// <param name="message">Message to be posted</param>
        ///// <returns>ID of the media item posted</returns>
        //public async Task<string> MakeIgCarouselPost(HttpClient httpClient, string accessToken, string igUserId, List<string> mediaUrls, string message)
        //{
        //    // we need a cache mapping container Id against mediaUrl in case we sucessfully create ONLY 1 container (e.g., due to file size or format issue)
        //    Dictionary<string, string> cache = new Dictionary<string, string>();
        //    foreach (string mediaUrl in mediaUrls)
        //    {
        //        // note: image is hardcoded as the media type as we don't have a user case for posting video yet
        //        string mediaContainerId = await MakeIgItemContainer(httpClient: httpClient, igUserId: igUserId, accessToken: accessToken,
        //            mediaType: IgMediaType.Image, mediaUrl: mediaUrl, isCarouselItem: true);

        //        if (string.IsNullOrWhiteSpace(mediaContainerId))
        //        {
        //            Log.Error($"Failed to create a container for image {mediaUrl}");
        //            continue;
        //        }

        //        // note: only up to 10 images can be included in a carousel post                
        //        cache[mediaContainerId] = mediaUrl;
        //        if (cache.Count == 10) break;
        //    }

        //    if (cache.Count == 0)
        //    {
        //        Log.Error("Failed to create any image/video container for a carousel post.");
        //        return null;
        //    }

        //    // IG would return error message if there is only 1 few carousel item
        //    if (cache.Count == 1)
        //    {
        //        return await MakeIgNonCarouselPost(httpClient: httpClient, igUserId: igUserId, accessToken: accessToken,
        //            mediaUrl: cache.Select(x => x.Value).ToList().FirstOrDefault(), message: message, mediaType: IgMediaType.Image);
        //    }

        //    string carouselId = await MakeIgItemContainer(httpClient: httpClient, igUserId: igUserId, accessToken: accessToken,
        //        mediaType: IgMediaType.Carousel, caption: message, carouselChildren: cache.Select(x => x.Key).ToList());
        //    if (string.IsNullOrWhiteSpace(carouselId))
        //    {
        //        Log.Error($"Failed to create a container for carousel.");
        //        return null;
        //    }

        //    return await PublishIgMedia(httpClient: httpClient, igUserId: igUserId, accessToken: accessToken, mediaContainerId: carouselId);
        //}

        ///// <summary>
        ///// Make a non-carousel post on IG
        ///// </summary>
        ///// <param name="httpClient">HTTP client for executing API query</param>
        ///// <param name="accessToken">Page access token</param>
        ///// <param name="igUserId">IG user ID</param>
        ///// <param name="mediaUrl">Publicly accessible URL of the image/video to be posted</param>
        ///// <param name="message">Message to be posted</param>
        ///// <param name="mediaType">Type of media to be posted</param>
        ///// <returns>ID of the media item posted</returns>
        //public async Task<string> MakeIgNonCarouselPost(HttpClient httpClient, string accessToken, string igUserId, string mediaUrl, string message, IgMediaType mediaType)
        //{
        //    string mediaContainerId = await MakeIgItemContainer(httpClient: httpClient, igUserId: igUserId, accessToken: accessToken,
        //        mediaUrl: mediaUrl, caption: message, mediaType: mediaType);
        //    if (string.IsNullOrWhiteSpace(mediaContainerId)) return string.Empty;

        //    string mediaId = await PublishIgMedia(httpClient: httpClient, igUserId: igUserId, accessToken: accessToken,
        //        mediaContainerId: mediaContainerId);
        //    return mediaId;
        //}

        ///// <summary>
        ///// Create a multi-photo post on FB
        ///// https://developers.facebook.com/docs/pages-api/posts/
        ///// https://developers.facebook.com/docs/graph-api/reference/v18.0/page/feed
        ///// </summary>
        ///// <param name="httpClient">HTTP client for executing API query</param>
        ///// <param name="accessToken">Page access token</param>
        ///// <param name="pageId">Page ID</param>
        ///// <param name="imageUrls">List of publicly accessible URLs of the images to be posted</param>
        ///// <param name="message">Message to be posted</param>
        ///// <returns>ID of the post</returns>
        //public async Task<string> MakeFbMultiPhotoPost(HttpClient httpClient, string accessToken, string pageId, List<string> imageUrls, string message)
        //{
        //    List<string> photoIds = new();
        //    string photoId;
        //    foreach (string imageUrl in imageUrls)
        //    {
        //        photoId = await ActionHelper.PostOrComment(httpClient: httpClient, accessToken: accessToken,
        //            endPoint: $"{BaseUrl}/{ApiVersion}/{pageId}/photos",
        //            mediaUrl: imageUrl, published: false);
        //        if (string.IsNullOrWhiteSpace(photoId))
        //        {
        //            Log.Error($"Failed to upload photo {imageUrl}");
        //            continue;
        //        }
        //        photoIds.Add(photoId);
        //    }

        //    return await ActionHelper.PostOrComment(httpClient: httpClient, accessToken: accessToken,
        //        endPoint: $"{BaseUrl}/{ApiVersion}/{pageId}/feed",
        //        message: message, photoIds: photoIds);
        //}



        ///// <summary>
        ///// Extract non-null image IDs from a product
        ///// </summary>
        ///// <param name="product">An object of Product class</param>
        ///// <returns>A set of image IDs</returns>
        //public HashSet<Guid> ReturnImageUrls(Product product)
        //{
        //    HashSet<Guid> validIds = new();
        //    if (product == null) return validIds;

        //    List<Guid?> imageIDs = new() { product.Image01ID, product.Image02ID, product.Image03ID, product.Image04ID,
        //        product.Image05ID, product.Image06ID, product.Image07ID, product.Image08ID };
        //    foreach (Guid? imageID in imageIDs)
        //    {
        //        if (imageID.HasValue) validIds.Add(imageID.Value);
        //    }
        //    return validIds;
        //}

        ///// <summary>
        ///// Publish a product to social media
        ///// </summary>
        ///// <param name="httpClient">HTTP client for executing API query</param>
        ///// <param name="corprioClient">Client for Api requests among Corprio projects</param>
        ///// <param name="organizationID">Organization ID</param>
        ///// <param name="productID">ID of the product to be published</param>
        ///// <returns>(1) True if the whole operation is completed and (2) list of any error messages</returns>
        ///// <exception cref="Exception"></exception>
        //public async Task<Tuple<bool, List<string>>> PublishProduct(HttpClient httpClient, APIClient corprioClient, Guid organizationID, Guid productID)
        //{
        //    List<string> errorMessages = new();
        //    MetaUser metaUser = await ActionHelper.GetCustomData<MetaUser>(corprioClient: corprioClient, organizationID: organizationID,
        //        key: BabelFish.CustomDataKeyForMetaUser) ?? throw new Exception($"Failed to get Meta user for {organizationID}.");

        //    if (metaUser.PageIds.Count == 0)
        //    {
        //        errorMessages.Add($"{organizationID} has no pages to publish product {productID}.");
        //        Log.Information(errorMessages.Last());
        //        return new Tuple<bool, List<string>>(false, errorMessages);
        //    }

        //    Product product = await corprioClient.ProductApi.Get(organizationID: organizationID, id: productID)
        //        ?? throw new Exception($"Product {productID} cannot be found.");
        //    OrganizationCoreInfo coreInfo = await corprioClient.OrganizationApi.GetCoreInfo(organizationID)
        //        ?? throw new Exception($"Failed to get core information of organization {organizationID}.");
        //    string message = $"{product.Name} - {BabelFish.Vocab["ListPrice"][UtilityHelper.NICAM(coreInfo)]}:{product.ListPrice_CurrencyCode}{product.ListPrice_Value}\n{product.Description}\n{BabelFish.Vocab["HowToBuy"][UtilityHelper.NICAM(coreInfo)].Replace("{0}", product.Name)}";

        //    // we need to use a set because child products may share the same image(s) with their parent or peers
        //    HashSet<Guid> imageIds = ReturnImageUrls(product);
        //    if (product.IsMasterProduct && (product.Variations?.Any() ?? false))
        //    {
        //        PagedList<Product> childProducts;
        //        int pageNum = 0;
        //        do
        //        {
        //            childProducts = await corprioClient.ProductApi.QueryPageChildProducts<Product>(
        //                organizationID: organizationID,
        //                masterProductID: productID,
        //                selector: "new (ID, Image01ID, Image02ID, Image03ID, Image04ID, Image05ID, Image06ID, Image07ID, Image08ID)",
        //                loadDataOptions: new LoadDataOptions
        //                {
        //                    PageIndex = pageNum,
        //                    RequireTotalCount = false,
        //                    Sort = new SortingInfo[] { new SortingInfo { Desc = false, Selector = "ID" } }
        //                });
        //            pageNum++;

        //            foreach (Product childProduct in childProducts.Items)
        //            {
        //                imageIds.UnionWith(ReturnImageUrls(childProduct));
        //            }
        //        }
        //        while (childProducts?.HasNextPage ?? false);
        //    }

        //    List<string> imageUrls = new();
        //    string imageUrlKey, imageUrl;
        //    foreach (Guid imageId in imageIds)
        //    {
        //        imageUrlKey = await corprioClient.ImageApi.UrlKey(organizationID: organizationID, id: imageId);
        //        if (string.IsNullOrWhiteSpace(imageUrlKey)) continue;
        //        imageUrl = corprioClient.ImageApi.Url(organizationID: organizationID, imageUrlKey: imageUrlKey);
        //        if (string.IsNullOrWhiteSpace(imageUrl)) continue;

        //        // DEV ONLY: the imageUrl generated in DEV environment won't work, so we re-assign a publicly accessible URL to it FOR NOW
        //        if (imageUrl.Contains("localhost")) imageUrl = GenerateImageUrlForDev();
        //        imageUrls.Add(imageUrl);
        //    }

        //    string postId;
        //    MetaPost post;
        //    product.EntityProperties ??= new List<EntityProperty>();
        //    bool success = true;
        //    foreach (string pageId in metaUser.PageIds)
        //    {
        //        MetaPage page = await ActionHelper.GetCustomData<MetaPage>(corprioClient: corprioClient, organizationID: organizationID, key: pageId);
        //        if (string.IsNullOrWhiteSpace(page?.Token))
        //        {
        //            errorMessages.Add($"Failed to retrieve page access token for {pageId}.");
        //            Log.Information(errorMessages.Last());
        //            success = false;
        //            continue;
        //        }

        //        if (!string.IsNullOrWhiteSpace(page.IgId))
        //        {
        //            postId = await MakeIgCarouselPost(httpClient: httpClient, accessToken: page.Token, igUserId: page.IgId,
        //                mediaUrls: imageUrls, message: message);
        //            if (string.IsNullOrWhiteSpace(postId))
        //            {
        //                errorMessages.Add($"Failed to publish carousel post to {page.Name}-{page.IgId}");
        //                Log.Error(errorMessages.Last());
        //                success = false;
        //            }
        //            else
        //            {
        //                post = new MetaPost() { IsFbPost = false, PageId = page.PageId };
        //                try
        //                {
        //                    await corprioClient.ApplicationSubscriptionApi.SetCustomData(organizationID: organizationID, key: postId,
        //                        value: System.Text.Json.JsonSerializer.Serialize(post));
        //                }
        //                catch (ApiExecutionException ex)
        //                {
        //                    errorMessages.Add($"Failed to save IG post ID {postId}. {ex.Message}");
        //                    Log.Error(errorMessages.Last());
        //                    success = false;
        //                }
        //                product.EntityProperties.Add(new EntityProperty() { Name = BabelFish.ProductEpName, Value = postId });
        //            }
        //        }

        //        postId = await MakeFbMultiPhotoPost(httpClient: httpClient, accessToken: page.Token,
        //            pageId: page.PageId, imageUrls: imageUrls, message: message);
        //        if (string.IsNullOrWhiteSpace(postId))
        //        {
        //            errorMessages.Add($"Failed to make multi-photo post to {page.Name}");
        //            Log.Error(errorMessages.Last());
        //            success = false;
        //        }
        //        else
        //        {
        //            post = new MetaPost() { IsFbPost = true, PageId = page.PageId };
        //            try
        //            {
        //                await corprioClient.ApplicationSubscriptionApi.SetCustomData(organizationID: organizationID, key: postId,
        //                    value: System.Text.Json.JsonSerializer.Serialize(post));
        //            }
        //            catch (ApiExecutionException ex)
        //            {
        //                errorMessages.Add($"Failed to save FB post ID {postId}. {ex.Message}");
        //                Log.Error(errorMessages.Last());
        //                success = false;
        //            }
        //            product.EntityProperties.Add(new EntityProperty() { Name = BabelFish.ProductEpName, Value = postId });
        //        }
        //    }

        //    try
        //    {
        //        // note: we allow 1 product to be associated with more than 1 post (e.g., 1 on FB and 1 on IG), so we don't use AddUpdateEntityProperty(),
        //        // which may over-write any EP with the same name
        //        await corprioClient.ProductApi.Update(organizationID: organizationID, product: product, updateChildren: true);
        //    }
        //    catch (ApiExecutionException ex)
        //    {
        //        Log.Error($"Failed to update the entity properties of product {productID} with post IDs. {ex.Message}");
        //        success = false;
        //    }
        //    return new Tuple<bool, List<string>>(success, errorMessages);
        //}

        ///// <summary>
        ///// Publish a product list
        ///// </summary>
        ///// <param name="httpClient">HTTP client for executing API query</param>
        ///// <param name="corprioClient">Client for Api requests among Corprio projects</param>
        ///// <param name="organizationID">Organization ID</param>
        ///// <param name="productlistID">Entity ID of product list</param>
        ///// <returns>(1) True if the whole operation is completed and (2) list of any error messages</returns>
        //public async Task<Tuple<bool, List<string>>> PublishCatalogue(HttpClient httpClient, APIClient corprioClient, Guid organizationID, Guid productlistID)
        //{
        //    List<string> errorMessages = new();
        //    MetaUser metaUser = await ActionHelper.GetCustomData<MetaUser>(corprioClient: corprioClient, organizationID: organizationID,
        //        key: BabelFish.CustomDataKeyForMetaUser) ?? throw new Exception($"Failed to get Meta user for {organizationID}.");

        //    if (metaUser.PageIds.Count == 0)
        //    {
        //        errorMessages.Add($"{organizationID} has no pages to publish catalogue {productlistID}.");
        //        Log.Information(errorMessages.Last());
        //        return new Tuple<bool, List<string>>(false, errorMessages);
        //    }

        //    ProductList productList = await corprioClient.ProductListApi.Get(organizationID: organizationID, id: productlistID)
        //        ?? throw new Exception($"Product list {productlistID} could not be found.");
        //    OrganizationCoreInfo coreInfo = await corprioClient.OrganizationApi.GetCoreInfo(organizationID)
        //        ?? throw new Exception($"Failed to get core information of organization {organizationID}.");
        //    // note: do not use the description in product list because it includes HTML tags
        //    string message = $"{productList.Name}\n{BabelFish.Vocab["VisitCatalogue"][UtilityHelper.NICAM(coreInfo)]}\n{configuration["GoBuyClickUrl"]}/Catalogue/{coreInfo.ShortName}/{productList.Code}";

        //    PagedList<ProductInfo> products;
        //    List<string> imageUrls = new();
        //    int pageIndex = 0;
        //    do
        //    {
        //        products = await corprioClient.ProductListApi.GetProductsPageOfList(
        //            organizationID: organizationID,
        //            productListID: productlistID,
        //            loadDataOptions: new LoadDataOptions()
        //            {
        //                PageIndex = pageIndex,
        //                PageSize = 10,
        //                RequireTotalCount = false,
        //                Sort = new SortingInfo[] { new SortingInfo { Desc = false, Selector = "CreateDate" } },
        //            });
        //        pageIndex++;

        //        foreach (ProductInfo productInfo in products.Items)
        //        {
        //            // note 1: Meta claims that only JPEG can be used in IG media item, although we found that png also worked...
        //            // note 2: For FB posts, .jpeg, .bmp, .png, .gif and .tiff are allowed
        //            string imageUrlKey = productInfo.Image01ID == null ? string.Empty : await corprioClient.ImageApi.UrlKey(organizationID: organizationID, id: (Guid)productInfo.Image01ID);
        //            string imageUrl = string.IsNullOrWhiteSpace(imageUrlKey) ? string.Empty : corprioClient.ImageApi.Url(organizationID: organizationID, imageUrlKey: imageUrlKey);

        //            // DEV ONLY: the imageUrl generated in DEV environment won't work, so we re-assign a publicly accessible URL to it FOR NOW
        //            if (imageUrl.Contains("localhost")) imageUrl = GenerateImageUrlForDev();

        //            if (!string.IsNullOrWhiteSpace(imageUrl)) imageUrls.Add(imageUrl);
        //        }
        //    }
        //    while (products?.HasNextPage ?? false);

        //    string postId;
        //    bool success = true;
        //    foreach (string pageId in metaUser.PageIds)
        //    {
        //        MetaPage page = await ActionHelper.GetCustomData<MetaPage>(corprioClient: corprioClient, organizationID: organizationID, key: pageId);
        //        if (string.IsNullOrWhiteSpace(page?.Token))
        //        {
        //            errorMessages.Add($"Failed to retrieve page access token for {pageId}.");
        //            Log.Information(errorMessages.Last());
        //            success = false;
        //            continue;
        //        }

        //        if (!string.IsNullOrWhiteSpace(page.IgId))
        //        {
        //            postId = await MakeIgCarouselPost(httpClient: httpClient, accessToken: page.Token, igUserId: page.IgId,
        //                mediaUrls: imageUrls, message: message);
        //            if (string.IsNullOrWhiteSpace(postId))
        //            {
        //                errorMessages.Add($"Failed to publish carousel post to {page.Name}-{page.IgId}");
        //                Log.Information(errorMessages.Last());
        //                success = false;
        //            }
        //        }

        //        postId = await MakeFbMultiPhotoPost(httpClient: httpClient, accessToken: page.Token,
        //            pageId: page.PageId, imageUrls: imageUrls, message: message);
        //        if (string.IsNullOrWhiteSpace(postId))
        //        {
        //            errorMessages.Add($"Failed to make multi-photo post to {page.Name}");
        //            Log.Information(errorMessages.Last());
        //            success = false;
        //        }
        //    }

        //    return new Tuple<bool, List<string>>(success, errorMessages);
        //}

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
        /// Turn on Meta's Built-in NLP to help detect locale (and meaning)
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
            MetaUser metaUser = db.MetaUsers.Include(x => x.Pages).FirstOrDefault(x => x.FacebookUserID == metaId);
            bool newMetaUser = metaUser == null;
            if (newMetaUser)
            {
                OrganizationCoreInfo coreInfo = await corprioClient.OrganizationApi.GetCoreInfo(organizationID);
                metaUser = new MetaUser()
                { 
                    ID = Guid.NewGuid(), 
                    FacebookUserID = metaId, 
                    KeywordForShoppingIntention = BabelFish.Vocab["DefaultKeyWordForShoppingIntention"][UtilityHelper.NICAM(coreInfo)] 
                };

                // TODO - create a new application setting
            }
            metaUser.OrganizationID = organizationID;
            metaUser.Token = token;
            if (newMetaUser)
            {
                db.MetaUsers.Add(metaUser);
                // TODO - add application setting
            }
            else
            {
                db.MetaUsers.Update(metaUser);
                // TODO - update application setting
            }
            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to save Facebook user {metaUser.ID}. {ex.Message}");
                throw;
            }

            List<FbPage> fbPages = await GetMeAccounts(httpClient: httpClient, userId: metaId, userAccessToken: token)
                ?? throw new Exception($"Encountered an error in retrieving pages on which {metaId} has a role.");

            bool newMetaPage;
            foreach (FbPage page in fbPages)
            {
                MetaPage metaPage = metaUser.Pages.FirstOrDefault(x => x.PageId == page.Id);
                newMetaPage = metaPage == null;
                if (newMetaPage) metaPage = new MetaPage() { ID = Guid.NewGuid(), FacebookUserID = metaUser.ID, PageId = page.Id };
                metaPage.Name = StringHelper.StringTruncate(page.Name, 300);
                metaPage.Token = page.AccessToken;
                metaPage.InstagramID = await GetIgUserId(httpClient: httpClient, accessToken: page.AccessToken, pageId: page.Id);

                if (newMetaPage)
                {
                    db.MetaPages.Add(metaPage);
                }
                else
                {
                    db.MetaPages.Update(metaPage);
                }
                try
                {
                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to save page {metaPage.ID}. {ex.Message}");
                    throw;
                }                
            }
            
            return StatusCode(200);
        }
    }
}

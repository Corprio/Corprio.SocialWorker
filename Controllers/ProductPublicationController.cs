using Corprio.CorprioAPIClient;
using Corprio.SocialWorker.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Serilog;
using Corprio.DataModel.Business;
using Corprio.DataModel;
using Corprio.DataModel.Business.Products;
using Corprio.DataModel.Business.Products.ViewModel;
using Microsoft.Extensions.Configuration;
using Corprio.SocialWorker.Helpers;
using System.Linq;
using Corprio.SocialWorker.Dictionaries;
using Corprio.Core.Exceptions;
using Corprio.DataModel.Shared;

namespace Corprio.SocialWorker.Controllers
{
    public class ProductPublicationController : MetaApiController
    {
        readonly IConfiguration configuration;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration"></param>
        public ProductPublicationController(IConfiguration configuration) : base(configuration)
        {
        }

        public IActionResult Index() => View();

        /// <summary>
        /// Publish a product to social media
        /// </summary>
        /// <param name="httpClient">HTTP client for executing API query</param>
        /// <param name="corprioClient">Client for Api requests among Corprio projects</param>
        /// <param name="organizationID">Organization ID</param>
        /// <param name="productID">ID of the product to be published</param>
        /// <returns>(1) True if the whole operation is completed and (2) list of any error messages</returns>
        /// <exception cref="Exception"></exception>
        public async Task<Tuple<bool, List<string>>> PublishProduct([FromServices] HttpClient httpClient, [FromServices] APIClient corprioClient, 
            [FromRoute] Guid organizationID, [FromRoute] Guid productID)
        {
            List<string> errorMessages = new();
            MetaUser metaUser = await ActionHelper.GetCustomData<MetaUser>(corprioClient: corprioClient, organizationID: organizationID,
                key: BabelFish.CustomDataKeyForMetaUser) ?? throw new Exception($"Failed to get Meta user for {organizationID}.");

            if (metaUser.PageIds.Count == 0)
            {
                errorMessages.Add($"{organizationID} has no pages to publish product {productID}.");
                Log.Information(errorMessages.Last());
                return new Tuple<bool, List<string>>(false, errorMessages);
            }

            Product product = await corprioClient.ProductApi.Get(organizationID: organizationID, id: productID)
                ?? throw new Exception($"Product {productID} cannot be found.");
            OrganizationCoreInfo coreInfo = await corprioClient.OrganizationApi.GetCoreInfo(organizationID)
                ?? throw new Exception($"Failed to get core information of organization {organizationID}.");
            string message = $"{product.Name} - {BabelFish.Vocab["ListPrice"][UtilityHelper.NICAM(coreInfo)]}:{product.ListPrice_CurrencyCode}{product.ListPrice_Value}\n{product.Description}\n{BabelFish.Vocab["HowToBuy"][UtilityHelper.NICAM(coreInfo)].Replace("{0}", product.Name)}";

            // we need to use a set because child products may share the same image(s) with their parent or peers
            HashSet<Guid> imageIds = ReturnImageUrls(product);
            if (product.IsMasterProduct && (product.Variations?.Any() ?? false))
            {
                PagedList<Product> childProducts;
                int pageNum = 0;
                do
                {
                    childProducts = await corprioClient.ProductApi.QueryPageChildProducts<Product>(
                        organizationID: organizationID,
                        masterProductID: productID,
                        selector: "new (ID, Image01ID, Image02ID, Image03ID, Image04ID, Image05ID, Image06ID, Image07ID, Image08ID)",
                        loadDataOptions: new LoadDataOptions
                        {
                            PageIndex = pageNum,
                            RequireTotalCount = false,
                            Sort = new SortingInfo[] { new SortingInfo { Desc = false, Selector = "ID" } }
                        });
                    pageNum++;

                    foreach (Product childProduct in childProducts.Items)
                    {
                        imageIds.UnionWith(ReturnImageUrls(childProduct));
                    }
                }
                while (childProducts?.HasNextPage ?? false);
            }

            List<string> imageUrls = new();
            string imageUrlKey, imageUrl;
            foreach (Guid imageId in imageIds)
            {
                imageUrlKey = await corprioClient.ImageApi.UrlKey(organizationID: organizationID, id: imageId);
                if (string.IsNullOrWhiteSpace(imageUrlKey)) continue;
                imageUrl = corprioClient.ImageApi.Url(organizationID: organizationID, imageUrlKey: imageUrlKey);
                if (string.IsNullOrWhiteSpace(imageUrl)) continue;

                // DEV ONLY: the imageUrl generated in DEV environment won't work, so we re-assign a publicly accessible URL to it FOR NOW
                if (imageUrl.Contains("localhost")) imageUrl = GenerateImageUrlForDev();
                imageUrls.Add(imageUrl);
            }

            string postId;
            MetaPost post;
            product.EntityProperties ??= new List<EntityProperty>();
            bool success = true;
            foreach (string pageId in metaUser.PageIds)
            {
                MetaPage page = await ActionHelper.GetCustomData<MetaPage>(corprioClient: corprioClient, organizationID: organizationID, key: pageId);
                if (string.IsNullOrWhiteSpace(page?.Token))
                {
                    errorMessages.Add($"Failed to retrieve page access token for {pageId}.");
                    Log.Information(errorMessages.Last());
                    success = false;
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(page.IgId))
                {
                    postId = await MakeIgCarouselPost(httpClient: httpClient, accessToken: page.Token, igUserId: page.IgId,
                        mediaUrls: imageUrls, message: message);
                    if (string.IsNullOrWhiteSpace(postId))
                    {
                        errorMessages.Add($"Failed to publish carousel post to {page.Name}-{page.IgId}");
                        Log.Error(errorMessages.Last());
                        success = false;
                    }
                    else
                    {
                        post = new MetaPost() { IsFbPost = false, PageId = page.PageId };
                        try
                        {
                            await corprioClient.ApplicationSubscriptionApi.SetCustomData(organizationID: organizationID, key: postId,
                                value: System.Text.Json.JsonSerializer.Serialize(post));
                        }
                        catch (ApiExecutionException ex)
                        {
                            errorMessages.Add($"Failed to save IG post ID {postId}. {ex.Message}");
                            Log.Error(errorMessages.Last());
                            success = false;
                        }
                        product.EntityProperties.Add(new EntityProperty() { Name = BabelFish.ProductEpName, Value = postId });
                    }
                }

                postId = await MakeFbMultiPhotoPost(httpClient: httpClient, accessToken: page.Token,
                    pageId: page.PageId, imageUrls: imageUrls, message: message);
                if (string.IsNullOrWhiteSpace(postId))
                {
                    errorMessages.Add($"Failed to make multi-photo post to {page.Name}");
                    Log.Error(errorMessages.Last());
                    success = false;
                }
                else
                {
                    post = new MetaPost() { IsFbPost = true, PageId = page.PageId };
                    try
                    {
                        await corprioClient.ApplicationSubscriptionApi.SetCustomData(organizationID: organizationID, key: postId,
                            value: System.Text.Json.JsonSerializer.Serialize(post));
                    }
                    catch (ApiExecutionException ex)
                    {
                        errorMessages.Add($"Failed to save FB post ID {postId}. {ex.Message}");
                        Log.Error(errorMessages.Last());
                        success = false;
                    }
                    product.EntityProperties.Add(new EntityProperty() { Name = BabelFish.ProductEpName, Value = postId });
                }
            }

            try
            {
                // note: we allow 1 product to be associated with more than 1 post (e.g., 1 on FB and 1 on IG), so we don't use AddUpdateEntityProperty(),
                // which may over-write any EP with the same name
                await corprioClient.ProductApi.Update(organizationID: organizationID, product: product, updateChildren: true);
            }
            catch (ApiExecutionException ex)
            {
                Log.Error($"Failed to update the entity properties of product {productID} with post IDs. {ex.Message}");
                success = false;
            }
            return new Tuple<bool, List<string>>(success, errorMessages);
        }
    }
}
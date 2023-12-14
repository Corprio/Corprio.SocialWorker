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

namespace Corprio.SocialWorker.Controllers
{
    public class CataloguePublicationController : MetaApiController
    {
        readonly IConfiguration configuration;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration"></param>
        public CataloguePublicationController(IConfiguration configuration) : base(configuration)
        {
        }

        public IActionResult Index() => View();

        /// <summary>
        /// Publish a product list
        /// </summary>
        /// <param name="httpClient">HTTP client for executing API query</param>
        /// <param name="corprioClient">Client for Api requests among Corprio projects</param>
        /// <param name="organizationID">Organization ID</param>
        /// <param name="productlistID">Entity ID of product list</param>
        /// <returns>(1) True if the whole operation is completed and (2) list of any error messages</returns>
        public async Task<Tuple<bool, List<string>>> PublishCatalogue([FromServices] HttpClient httpClient, [FromServices] APIClient corprioClient,
            [FromRoute] Guid organizationID, [FromRoute] Guid productlistID)
        {
            List<string> errorMessages = new();
            MetaUser metaUser = await ActionHelper.GetCustomData<MetaUser>(corprioClient: corprioClient, organizationID: organizationID,
                key: BabelFish.CustomDataKeyForMetaUser) ?? throw new Exception($"Failed to get Meta user for {organizationID}.");

            if (metaUser.PageIds.Count == 0)
            {
                errorMessages.Add($"{organizationID} has no pages to publish catalogue {productlistID}.");
                Log.Information(errorMessages.Last());
                return new Tuple<bool, List<string>>(false, errorMessages);
            }

            ProductList productList = await corprioClient.ProductListApi.Get(organizationID: organizationID, id: productlistID)
                ?? throw new Exception($"Product list {productlistID} could not be found.");
            OrganizationCoreInfo coreInfo = await corprioClient.OrganizationApi.GetCoreInfo(organizationID)
                ?? throw new Exception($"Failed to get core information of organization {organizationID}.");
            // note: do not use the description in product list because it includes HTML tags
            string message = $"{productList.Name}\n{BabelFish.Vocab["VisitCatalogue"][UtilityHelper.NICAM(coreInfo)]}\n{configuration["GoBuyClickUrl"]}/Catalogue/{coreInfo.ShortName}/{productList.Code}";

            PagedList<ProductInfo> products;
            List<string> imageUrls = new();
            int pageIndex = 0;
            do
            {
                products = await corprioClient.ProductListApi.GetProductsPageOfList(
                    organizationID: organizationID,
                    productListID: productlistID,
                    loadDataOptions: new LoadDataOptions()
                    {
                        PageIndex = pageIndex,
                        PageSize = 10,
                        RequireTotalCount = false,
                        Sort = new SortingInfo[] { new SortingInfo { Desc = false, Selector = "CreateDate" } },
                    });
                pageIndex++;

                foreach (ProductInfo productInfo in products.Items)
                {
                    // note 1: Meta claims that only JPEG can be used in IG media item, although we found that png also worked...
                    // note 2: For FB posts, .jpeg, .bmp, .png, .gif and .tiff are allowed
                    string imageUrlKey = productInfo.Image01ID == null ? string.Empty : await corprioClient.ImageApi.UrlKey(organizationID: organizationID, id: (Guid)productInfo.Image01ID);
                    string imageUrl = string.IsNullOrWhiteSpace(imageUrlKey) ? string.Empty : corprioClient.ImageApi.Url(organizationID: organizationID, imageUrlKey: imageUrlKey);

                    // DEV ONLY: the imageUrl generated in DEV environment won't work, so we re-assign a publicly accessible URL to it FOR NOW
                    if (imageUrl.Contains("localhost")) imageUrl = GenerateImageUrlForDev();

                    if (!string.IsNullOrWhiteSpace(imageUrl)) imageUrls.Add(imageUrl);
                }
            }
            while (products?.HasNextPage ?? false);

            string postId;
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
                        Log.Information(errorMessages.Last());
                        success = false;
                    }
                }

                postId = await MakeFbMultiPhotoPost(httpClient: httpClient, accessToken: page.Token,
                    pageId: page.PageId, imageUrls: imageUrls, message: message);
                if (string.IsNullOrWhiteSpace(postId))
                {
                    errorMessages.Add($"Failed to make multi-photo post to {page.Name}");
                    Log.Information(errorMessages.Last());
                    success = false;
                }
            }

            return new Tuple<bool, List<string>>(success, errorMessages);
        }
    }
}
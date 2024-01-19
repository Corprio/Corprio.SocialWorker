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
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Corprio.AspNetCore.Site.Filters;
using DevExtreme.AspNet.Mvc;
using Corprio.DevExtremeLib;
using Corprio.AspNetCore.Site.Services;

namespace Corprio.SocialWorker.Controllers
{
    public class CataloguePublicationController : MetaApiController
    {
        private readonly ApplicationDbContext db;        

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration"></param>
        public CataloguePublicationController(ApplicationDbContext context, IConfiguration configuration) : base(configuration)
        {
            db = context;
        }

        public override IActionResult Index([FromRoute] Guid organizationID)
        {
            return View(organizationID);
        }

        /// <summary>
        /// Get catalogue records
        /// </summary>
        /// <param name="corprioClient">Client for executing API requests among Corprio projects</param>
        /// <param name="loadDataOptions">DataGrid datasource load options</param>
        /// <returns>Paged records to be shown in DataGrid</returns>
        [OrganizationAuthorizationCheck(
           ActionEntityTypes = new EntityType[] { EntityType.ProductList },
           RequiredPermissions = new DataAction[] { DataAction.Read })]
        public async Task<IActionResult> GetPage([FromServices] APIClient corprioClient, DataSourceLoadOptions loadDataOptions)
        {
            PagedList<CatalogueViewModel> list = await corprioClient.ProductListApi.QueryPage<CatalogueViewModel>(OrganizationID,
                "new (ID,Code,Name,StartDate,EndDate,Remark)", loadDataOptions?.ToCorprioLoadDataOption());

            return list.FormGridPageResult();
        }

        /// <summary>
        /// Publish a product list
        /// </summary>
        /// <param name="httpClient">HTTP client for executing API query</param>
        /// <param name="corprioClient">Client for Api requests among Corprio projects</param>
        /// <param name="organizationID">Organization ID</param>
        /// <param name="applicationSettingService">Application setting service</param>
        /// <param name="productlistID">Entity ID of product list</param>
        /// <returns>(1) True if the whole operation is completed and (2) list of any error messages</returns>
        [HttpPost]
        public async Task<IActionResult> PublishCatalogue([FromServices] HttpClient httpClient, [FromServices] APIClient corprioClient,
            [FromServices] ApplicationSettingService applicationSettingService, [FromRoute] Guid organizationID, Guid productlistID)
        {
            if (productlistID.Equals(Guid.Empty)) throw new Exception("Product list ID is invalid.");            
            MetaUser metaUser = db.MetaUsers.Include(x => x.Pages).FirstOrDefault(x => x.OrganizationID == organizationID && x.Dormant == false)
                ?? throw new Exception(Resources.SharedResource.ResourceManager.GetString("ErrMsg_ValidMetaProfileNotFound"));

            List<string> errorMessages = new();
            if (metaUser.Pages.Count == 0)
            {
                errorMessages.Add($"{organizationID} has no pages to publish catalogue {productlistID}.");
                Log.Information(errorMessages.Last());
                return StatusCode(400, errorMessages);
            }

            ProductList productList = await corprioClient.ProductListApi.Get(organizationID: organizationID, id: productlistID)
                ?? throw new Exception($"Product list {productlistID} could not be found.");
            OrganizationCoreInfo coreInfo = await corprioClient.OrganizationApi.GetCoreInfo(organizationID)
                ?? throw new Exception($"Failed to get core information of organization {organizationID}.");
            ApplicationSetting applicationSetting = await applicationSettingService.GetSetting<ApplicationSetting>(organizationID) 
                ?? throw new Exception($"Failed to retrieve application setting of organization {organizationID}.");            
            string message = applicationSetting.CataloguePostMessage(productList: productList, coreInfo: coreInfo, goBuyClickUrl: GoBuyClickUrl);                        

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

                    // DEV ONLY: the imageUrl generated in DEV environment won't work, so we re-assign a publicly accessible URL to it
                    if (imageUrl.Contains("localhost")) imageUrl = GenerateImageUrlForDev();

                    if (!string.IsNullOrWhiteSpace(imageUrl)) imageUrls.Add(imageUrl);
                }
            }
            while (products?.HasNextPage ?? false);

            string postId;
            bool success = true;
            foreach (MetaPage page in metaUser.Pages)
            {
                // note: if there are no images to begin with, we simply don't attempt to post anything to IG
                if (!string.IsNullOrWhiteSpace(page.InstagramID) && imageUrls.Any())
                {
                    postId = await MakeIgCarouselPost(httpClient: httpClient, accessToken: page.Token, igUserId: page.InstagramID,
                        mediaUrls: imageUrls, message: message);
                    if (string.IsNullOrWhiteSpace(postId))
                    {
                        errorMessages.Add($"Failed to publish carousel post to {page.Name}-{page.InstagramID}");
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

            return success ? StatusCode(200) : StatusCode(400, errorMessages);            
        }
    }
}
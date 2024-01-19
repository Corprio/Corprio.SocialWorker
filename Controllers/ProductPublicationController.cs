﻿using Corprio.CorprioAPIClient;
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
using Microsoft.Extensions.Configuration;
using Corprio.SocialWorker.Helpers;
using System.Linq;
using Corprio.SocialWorker.Dictionaries;
using Corprio.Core.Exceptions;
using Corprio.DataModel.Shared;
using Corprio.AspNetCore.Site.Filters;
using Corprio.DevExtremeLib;
using DevExtreme.AspNet.Mvc;
using Microsoft.EntityFrameworkCore;
using Corprio.DataModel.Business.Sales;
using Corprio.Global.Measure;
using Corprio.AspNetCore.Site.Services;

namespace Corprio.SocialWorker.Controllers
{
    public class ProductPublicationController : MetaApiController
    {
        private readonly ApplicationDbContext db;        

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration"></param>
        public ProductPublicationController(ApplicationDbContext context, IConfiguration configuration) : base(configuration)
        {
            db = context;
        }

        /// <summary>
        /// Render the view
        /// </summary>
        /// <param name="organizationID">Organization ID</param>
        /// <returns></returns>
        public override IActionResult Index([FromRoute] Guid organizationID)
        {
            return View(organizationID);
        }

        /// <summary>
        /// Retrieve entity property values
        /// </summary>
        /// <param name="corprio">Client for executing API requests among Corprio projects</param>
        /// <param name="organizationID">Organization ID</param>
        /// <param name="propertyName">Name of EP to be retrieved</param>        
        /// <returns></returns>
        [OrganizationNeeded(false)]
        [HttpGet("/ProductPublication/GetDistinctProductPropertyValues")]
        public Task<IEnumerable<string>> GetDistinctProductPropertyValues([FromServices] APIClient corprio, Guid organizationID, string propertyName)
        {
            return corprio.ProductApi.GetDistinctPropertyValues(organizationID: organizationID, propertyName: propertyName);
        }

        /// <summary>
        /// Return a page of records
        /// </summary>
        /// <param name="corprioClient">API client</param>
        /// <param name="loadOptions">DataGrid datasource load options</param>
        /// <returns>Paged records to be shown in DataGrid</returns>
        [OrganizationAuthorizationCheck(
            ActionEntityTypes = new EntityType[] { EntityType.Product },
            RequiredPermissions = new DataAction[] { DataAction.Read })]
        public async Task<IActionResult> GetPage([FromServices] APIClient corprioClient, DataSourceLoadOptions loadOptions)
        {
            var orgInfo = await corprioClient.OrganizationApi.GetCoreInfo(OrganizationID);
            PagedList<dynamic> list = await corprioClient.ProductApi.QueryPage(
                organizationID: OrganizationID,
                selector: "new (ID,Code,Name,EntityProperties.Where(Name=\"tag\").Select(Value) as Tags,"
                + "Description,GlobalizedName,GlobalizedDescription,GlobalizedLongDescription,DefaultBarcode,Nature,ProductTypeID,ProductType.Name as ProductTypeName,"
                + "BrandID,Brand.Name as BrandName,StockUOMCode,Model,GrossWeight,NetWeight,WeightUnit,Length,Width,Height,LengthUnit,Disabled,Image01ID,Image01ID=null?null:new(Image01.ID,Image01.UrlKey) as Image01,CreateDate,LastUpdateDate,ListPrice_Value," +
                $"np(ListPrice_CurrencyCode, \"{orgInfo.CurrencyCode}\") as ListPrice_CurrencyCode," +
                "IsMasterProduct,MasterProductID,MasterProduct.Code as MasterProductCode,ExternalOrganizationID)",
                loadDataOptions: loadOptions?.ToCorprioLoadDataOption());
            PagedList<ProductViewModel> pagedList = list.ConvertTo<ProductViewModel>();

            return Json(pagedList.ToLoadResult());
        }

        /// <summary>
        /// Publish a product to social media
        /// </summary>
        /// <param name="httpClient">HTTP client for executing API query</param>
        /// <param name="corprioClient">Client for Api requests among Corprio projects</param>
        /// <param name="applicationSettingService">Application setting service</param>
        /// <param name="organizationID">Organization ID</param>
        /// <param name="productID">ID of the product to be published</param>
        /// <returns>Status code</returns>
        /// <exception cref="Exception"></exception>
        [HttpPost]
        public async Task<IActionResult> PublishProduct([FromServices] HttpClient httpClient, [FromServices] APIClient corprioClient,
            [FromServices] ApplicationSettingService applicationSettingService, [FromRoute] Guid organizationID, Guid productID)
        {
            if (productID.Equals(Guid.Empty)) throw new Exception("Product ID is invalid.");
            List<string> errorMessages = new();
            MetaUser metaUser = db.MetaUsers.Include(x => x.Pages).FirstOrDefault(x => x.OrganizationID == organizationID && x.Dormant == false)
                ?? throw new Exception(Resources.SharedResource.ResourceManager.GetString("ErrMsg_ValidMetaProfileNotFound"));

            if (metaUser.Pages.Count == 0)
            {
                errorMessages.Add($"{organizationID} has no pages to publish product {productID}.");
                Log.Information(errorMessages.Last());
                return StatusCode(400, errorMessages);
            }

            Product product = await corprioClient.ProductApi.Get(organizationID: organizationID, id: productID)
                ?? throw new Exception($"Product {productID} cannot be found.");
            OrganizationCoreInfo coreInfo = await corprioClient.OrganizationApi.GetCoreInfo(organizationID)
                ?? throw new Exception($"Failed to get core information of organization {organizationID}.");
            ApplicationSetting applicationSetting = await applicationSettingService.GetSetting<ApplicationSetting>(organizationID)
                ?? throw new Exception($"Failed to retrieve application setting of organization {organizationID}.");            
            PriceWithCurrency price = null;
            if (applicationSetting.ProductPostTemplate?.Contains(TemplateComponent.ProductPublicPrice) ?? false)
            {
                price = await corprioClient.SellingPriceApi.GetPriceForCustomerPriceGroup(
                    organizationID: organizationID,
                    productID: productID,
                    customerPriceGroupID: CustomerPriceGroup.PublicGroupID,
                    quantity: new Quantity { UOMCode = product.ListPrice_UOMCode, Value = 1 },
                    currencyCode: coreInfo.CurrencyCode);
            }
            string message = applicationSetting.ProductPostMessage(product: product, coreInfo: coreInfo, publicPrice: price);

            // we need to use a set because child products may share the same image(s) with their parent or peers
            HashSet<Guid> imageIds = UtilityHelper.ReturnImageUrls(product);
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
                        imageIds.UnionWith(UtilityHelper.ReturnImageUrls(childProduct));
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
                        Log.Error(errorMessages.Last());
                        success = false;
                    }
                    else
                    {
                        post = new MetaPost()
                        {
                            ID = Guid.NewGuid(),
                            FacebookPageID = page.ID,
                            KeywordForShoppingIntention = applicationSetting.KeywordForShoppingIntention,
                            PostId = postId,
                            PostedWith = MetaProduct.Instagram
                        };
                        try
                        {
                            db.MetaPosts.Add(post);
                            await db.SaveChangesAsync();
                        }
                        catch (Exception ex)
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
                    post = new MetaPost() 
                    { 
                        ID = Guid.NewGuid(),
                        FacebookPageID = page.ID,
                        KeywordForShoppingIntention = applicationSetting.KeywordForShoppingIntention,
                        PostId = postId,
                        PostedWith = MetaProduct.Facebook                         
                    };
                    try
                    {
                        db.MetaPosts.Add(post);
                        await db.SaveChangesAsync();
                    }
                    catch (Exception ex)
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

            return success ? StatusCode(200) : StatusCode(400, string.Join("\n", errorMessages.ToArray()));
        }
    }
}
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
using Corprio.SocialWorker.Models.Meta;
using Corprio.SocialWorker.Models.Line;
using Corprio.SocialWorker.Services;

namespace Corprio.SocialWorker.Controllers
{
    public class ProductPublicationController : MetaApiController
    {
        readonly ApplicationDbContext _db;
        readonly APIClient _corprioClient;                
        readonly ApplicationSettingService _applicationSettingService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        public ProductPublicationController(ApplicationDbContext context, APIClient corprioClient, 
            ApplicationSettingService applicationSettingService, IConfiguration configuration, 
            IHttpClientFactory httpClientFactory) : base(configuration, httpClientFactory)
        {
            _db = context;
            _corprioClient = corprioClient;            
            _applicationSettingService = applicationSettingService;            
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
        /// <param name="organizationID">Organization ID</param>
        /// <param name="propertyName">Name of EP to be retrieved</param>        
        /// <returns></returns>
        [OrganizationNeeded(false)]
        [HttpGet("/ProductPublication/GetDistinctProductPropertyValues")]
        public Task<IEnumerable<string>> GetDistinctProductPropertyValues(Guid organizationID, string propertyName)
        {
            return _corprioClient.ProductApi.GetDistinctPropertyValues(organizationID: organizationID, propertyName: propertyName);
        }

        /// <summary>
        /// Return a page of records
        /// </summary>        
        /// <param name="loadOptions">DataGrid datasource load options</param>
        /// <returns>Paged records to be shown in DataGrid</returns>
        [OrganizationAuthorizationCheck(
            ActionEntityTypes = new EntityType[] { EntityType.Product },
            RequiredPermissions = new DataAction[] { DataAction.Read })]
        public async Task<IActionResult> GetPage(DataSourceLoadOptions loadOptions)
        {
            var orgInfo = await _corprioClient.OrganizationApi.GetCoreInfo(OrganizationID);
            PagedList<dynamic> list = await _corprioClient.ProductApi.QueryPage(
                organizationID: OrganizationID,
                selector: $"new (ID,Code,Name,EntityProperties.Where(Name=\"tag\").Select(Value) as Tags,EntityProperties.Where(Name=\"{BabelFish.ProductEpName}\").Select(Value) as PostIDs,"
                + "Description,GlobalizedName,GlobalizedDescription,GlobalizedLongDescription,DefaultBarcode,Nature,ProductTypeID,ProductType.Name as ProductTypeName,"
                + "BrandID,Brand.Name as BrandName,StockUOMCode,Model,GrossWeight,NetWeight,WeightUnit,Length,Width,Height,LengthUnit,Disabled,Image01ID,Image01ID=null?null:new(Image01.ID,Image01.UrlKey) as Image01,CreateDate,LastUpdateDate,ListPrice_Value," +
                $"np(ListPrice_CurrencyCode, \"{orgInfo.CurrencyCode}\") as ListPrice_CurrencyCode," +
                "IsMasterProduct,MasterProductID,MasterProduct.Code as MasterProductCode,ExternalOrganizationID)",
                loadDataOptions: loadOptions?.ToCorprioLoadDataOption());
            PagedList<ProductViewModel> pagedList = list.ConvertTo<ProductViewModel>();
            
            return Json(pagedList.ToLoadResult());
        }

        /// <summary>
        /// Returns a product publication post based on the template configured in application setting
        /// </summary>
        /// <param name="organizationID">Organization ID</param>
        /// <param name="productID">Product ID</param>
        /// <returns>A product publication post</returns>
        /// <exception cref="Exception"></exception>
        [HttpPost]
        public async Task<string> PreviewProductPost([FromRoute] Guid organizationID, Guid productID)
        {
            if (productID.Equals(Guid.Empty)) throw new Exception(Resources.SharedResource.ErrMsg_InvalidProductID);
            Product product = await _corprioClient.ProductApi.Get(organizationID: organizationID, id: productID)
                ?? throw new Exception(Resources.SharedResource.ErrMsg_ProductNotFound);
            OrganizationCoreInfo coreInfo = await _corprioClient.OrganizationApi.GetCoreInfo(organizationID)
                ?? throw new Exception(Resources.SharedResource.ErrMsg_OrganizationInfoNotFound);
            ApplicationSetting applicationSetting = await _applicationSettingService.GetSetting<ApplicationSetting>(organizationID)
                ?? throw new Exception(Resources.SharedResource.ErrMsg_AppSettingNotFound);
            PriceWithCurrency price = null;
            if (applicationSetting.ProductPostTemplate?.Contains(TemplateComponent.ProductPublicPrice) ?? false)
            {
                price = await _corprioClient.SellingPriceApi.GetPriceForCustomerPriceGroup(
                    organizationID: organizationID,
                    productID: productID,
                    customerPriceGroupID: CustomerPriceGroup.PublicGroupID,
                    quantity: new Quantity { UOMCode = product.ListPrice_UOMCode, Value = 1 },
                    currencyCode: coreInfo.CurrencyCode);
            }

            return applicationSetting.ProductPostMessage(product: product, coreInfo: coreInfo, publicPrice: price);

            //PostTemplateSummary summary = new() { 
            //    Keyword = applicationSetting.KeywordForShoppingIntention,
            //    Preview = applicationSetting.ProductPostMessage(product: product, coreInfo: coreInfo, publicPrice: price), 
            //};
            //return summary;            
        }

        /// <summary>
        /// Publish a product to social media
        /// </summary>        
        /// <param name="organizationID">Organization ID</param>
        /// <param name="productID">ID of the product to be published</param>
        /// <param name="message">Text to be included in the post</param>
        /// <param name="facebookUserID">Facebook user ID assigned by Meta (optional)</param>
        /// <returns>Status code</returns>
        /// <exception cref="Exception"></exception>
        [HttpPost]
        public async Task<IActionResult> PublishProduct([FromRoute] Guid organizationID, Guid productID, 
            string message, string facebookUserID)
        {
            if (productID.Equals(Guid.Empty)) throw new Exception(Resources.SharedResource.ErrMsg_InvalidProductID);
            if (string.IsNullOrWhiteSpace(message)) throw new Exception(Resources.SharedResource.ErrMsg_BlankPostMsg);
            message = UtilityHelper.UncleanAndClean(userInput: message, onceIsOK: true);
            List<string> errorMessages = [];
            
            // it is possible that one organization is linked to multiple Facebook accounts and Line channels
            List<MetaUser> activeMetaUsers = _db.MetaUsers.Include(x => x.Pages).Where(x => x.OrganizationID == organizationID && x.Dormant == false).ToList();
            List<LineChannel> activeLineChannels = _db.LineChannels.Where(r => r.OrganizationID == organizationID && r.Dormant == false).ToList();
            if (activeMetaUsers.Count == 0 && activeLineChannels.Count == 0) 
                throw new Exception(Resources.SharedResource.ErrMsg_ValidMetaProfileAndLineChannelNotFound);

            MetaUser metaUser = null;
            if (activeMetaUsers.Count > 0)
            {
                // we require the user to have logged in Facebook because we want to make sure the token has been refreshed
                if (string.IsNullOrWhiteSpace(facebookUserID))
                {
                    if (activeLineChannels.Count == 0) 
                        throw new Exception(Resources.SharedResource.ErrMsg_FacebookNotLoggedIn);

                    // continue to publish to Line even if the user hasn't logged in to Facebook
                    errorMessages.Add(Resources.SharedResource.ErrMsg_FacebookNotLoggedIn);
                }
                else
                {
                    metaUser = activeMetaUsers.FirstOrDefault(x => x.FacebookUserID == facebookUserID)
                        ?? throw new Exception(Resources.SharedResource.ErrMsg_ValidMetaProfileNotFound);
                }
            }

            #region Create image URLs
            Product product = await _corprioClient.ProductApi.Get(organizationID: organizationID, id: productID)
                ?? throw new Exception(Resources.SharedResource.ErrMsg_ProductNotFound);
            ApplicationSetting applicationSetting = await _applicationSettingService.GetSetting<ApplicationSetting>(organizationID)
                ?? throw new Exception(Resources.SharedResource.ErrMsg_AppSettingNotFound);

            // we need to use a set because child products may share the same image(s) with their parent or peers
            HashSet<Guid> imageIds = UtilityHelper.ReturnImageUrls(product);                        
            if (product.IsMasterProduct && product.Variations?.Count > 0)
            {
                PagedList<Product> childProducts;
                int pageNum = 0;
                do
                {
                    childProducts = await _corprioClient.ProductApi.QueryPageChildProducts<Product>(
                        organizationID: organizationID,
                        masterProductID: productID,
                        selector: "new (ID, Image01ID, Image02ID, Image03ID, Image04ID, Image05ID, Image06ID, Image07ID, Image08ID)",
                        loadDataOptions: new LoadDataOptions
                        {
                            PageIndex = pageNum,
                            RequireTotalCount = false,
                            Sort = [new SortingInfo { Desc = false, Selector = "ID" }]
                        });
                    pageNum++;

                    foreach (Product childProduct in childProducts.Items)
                    {
                        imageIds.UnionWith(UtilityHelper.ReturnImageUrls(childProduct));
                    }
                }
                while (childProducts?.HasNextPage ?? false);
            }

            List<string> imageUrls = [];
            string imageUrlKey, imageUrl;
            foreach (Guid imageId in imageIds)
            {
                imageUrlKey = await _corprioClient.ImageApi.UrlKey(organizationID: organizationID, id: imageId);
                if (string.IsNullOrWhiteSpace(imageUrlKey)) continue;
                imageUrl = _corprioClient.ImageApi.Url(organizationID: organizationID, imageUrlKey: imageUrlKey);
                if (string.IsNullOrWhiteSpace(imageUrl)) continue;

                // DEV ONLY: the imageUrl generated in DEV environment won't work, so we re-assign a publicly accessible URL to it FOR NOW
                if (imageUrl.Contains("localhost")) imageUrl = GenerateImageUrlForDev();                
                imageUrls.Add(imageUrl);
            }
            #endregion
            product.EntityProperties ??= [];  // make sure the EP is not null, as it will be updated later

            // assumption: the user want to publish the same messages to all Line channels connected with Corprio
            LineApiService lineApiService;
            List<ILineMessage> lineMessages;
            foreach (LineChannel lineChannel in activeLineChannels)
            {
                lineApiService = new LineApiService(
                    context: _db,
                    httpClientFactory: _httpClientFactory,
                    configuration: _configuration,
                    lineChannel: lineChannel,
                    applicationSettingService: _applicationSettingService);
                
                lineMessages = [
                    new LineFlexMessage 
                    {                         
                        AltText = message,
                        Contents = new LineFlexMessageBubble
                        {
                            Styles = new LineFlexMessageBlockStyle
                            {
                                Header = new LineFlexMessageStyle { BackgroundColor = "#5C85FF" },
                                Body = new LineFlexMessageStyle { BackgroundColor = "#5C85FF" },                                
                            },
                            Header = new LineFlexMessageBox
                            {                                
                                Layout = "vertical",
                                Contents = [
                                    new LineFlexMessageText 
                                    { 
                                        Text = !string.IsNullOrWhiteSpace(applicationSetting.LineBroadcastHeader) 
                                        ? applicationSetting.LineBroadcastHeader 
                                        : "!!!"
                                    }],
                            },
                            Hero = new LineFlexMessageImage
                            {                                
                                Url = imageUrls[0],
                                Size = "full",
                            },
                            Body = new LineFlexMessageBox
                            {                                
                                Layout = "vertical",
                                Contents = [new LineFlexMessageText { Text = message }],
                            },
                        },
                    }
                    ];

                try
                {
                    await lineApiService.SendBroadcastMessage(lineMessages);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                    errorMessages.Add(ex.Message);
                }
            }
            // need this EP to indicate that the product has been published
            product.EntityProperties.Add(new EntityProperty() { Name = BabelFish.ProductEpName, Value = "NA" });
           
            if (metaUser != null)
            {
                if (metaUser.Pages == null || metaUser.Pages.Count == 0)
                {
                    errorMessages.Add($"Facebook user {metaUser.FacebookUserID} has no Facebook pages to publish product {productID}.");
                    Log.Information(errorMessages.Last());
                }

                string postId;
                MetaPost post;
                foreach (MetaPage page in metaUser.Pages)
                {
                    postId = await MakeFbMultiPhotoPost(accessToken: page.Token,
                        pageId: page.PageId, imageUrls: imageUrls, message: message);
                    if (string.IsNullOrWhiteSpace(postId))
                    {
                        errorMessages.Add($"Failed to make multi-photo post to {page.Name}");
                        Log.Error(errorMessages.Last());                        
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
                            _db.MetaPosts.Add(post);
                            await _db.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            errorMessages.Add($"Failed to save FB post ID {postId}. {ex.Message}");
                            Log.Error(errorMessages.Last());                            
                        }
                        product.EntityProperties.Add(new EntityProperty() { Name = BabelFish.ProductEpName, Value = postId });
                    }

                    // note: if there are no images to begin with, we simply don't attempt to post anything to IG
                    if (string.IsNullOrWhiteSpace(page.InstagramID) && imageUrls.Count > 0) continue;

                    postId = await MakeIgCarouselPost(accessToken: page.Token, igUserId: page.InstagramID,
                            mediaUrls: imageUrls, message: message);
                    if (string.IsNullOrWhiteSpace(postId))
                    {
                        errorMessages.Add($"Failed to publish carousel post to {page.Name}-{page.InstagramID}");
                        Log.Error(errorMessages.Last());                        
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
                            _db.MetaPosts.Add(post);
                            await _db.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            errorMessages.Add($"Failed to save IG post ID {postId}. {ex.Message}");
                            Log.Error(errorMessages.Last());                            
                        }
                        product.EntityProperties.Add(new EntityProperty() { Name = BabelFish.ProductEpName, Value = postId });
                    }
                }
            }

            try
            {
                // note: we allow 1 product to be associated with more than 1 post (e.g., 1 on FB and 1 on IG), so we don't use AddUpdateEntityProperty(),
                // which may over-write any EP with the same name
                await _corprioClient.ProductApi.Update(organizationID: organizationID, product: product, updateChildren: true);
            }
            catch (ApiExecutionException ex)
            {                
                errorMessages.Add($"Failed to update database for product {productID} with post IDs. {ex.Message}");
                Log.Error(errorMessages.Last());
            }

            return errorMessages.Count == 0 ? StatusCode(200) : StatusCode(400, string.Join("\n", errorMessages));
        }
    }
}
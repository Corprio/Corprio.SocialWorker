using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Corprio.SocialWorker.Models;
using System.Linq.Dynamic.Core;
using System.Linq;
using Corprio.SocialWorker.Helpers;
using Serilog;
using Corprio.DataModel;
using Corprio.CorprioAPIClient;
using Corprio.Core.Exceptions;
using Corprio.SocialWorker.Dictionaries;
using Corprio.DataModel.Business;
using System.Collections.Generic;
using Corprio.AspNetCore.Site.Services;
using Corprio.AspNetCore.Site.Filters;
using Corprio.Global.Geography;
using Corprio.DataModel.Business.Sales;
using Corprio.SocialWorker.Models.Line;
using Corprio.SocialWorker.Services;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Corprio.Core;

namespace Corprio.SocialWorker.Controllers
{
    public class GetStartedController : AspNetCore.XtraReportSite.Controllers.BaseController
    {
        private readonly ApplicationDbContext _db;
        readonly ApplicationSettingService _applicationSettingService;
        readonly APIClient _corprioClient;
        readonly IConfiguration _configuration;
        readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="client">Client for Api requests among Corprio projects</param>
        /// <param name="appSettingService">Application setting service</param>
        /// <param name="httpClientFactory">HTTP client factory</param>
        public GetStartedController(ApplicationDbContext context, APIClient client, 
            ApplicationSettingService appSettingService, IConfiguration configuration, 
            IHttpClientFactory httpClientFactory) : base()
        {
            _db = context;
            _corprioClient = client;
            _applicationSettingService = appSettingService;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }
                
        /// <summary>
        /// Retrieve/initialize application setting and render the relevant view
        /// </summary>
        /// <param name="organizationID">Organization ID</param>
        /// <returns>View</returns>
        public override IActionResult Index([FromRoute] Guid organizationID)
        {            
            ApplicationSetting applicationSetting = _applicationSettingService.GetSetting<ApplicationSetting>(organizationID).ConfigureAwait(false).GetAwaiter().GetResult();
            bool firstVisit = applicationSetting == null;
            bool updated = false;  // if true, then the setting needs to be saved
            if (firstVisit)
            {
                try
                {
                    OrganizationCoreInfo coreInfo = _corprioClient.OrganizationApi.GetCoreInfo(organizationID).ConfigureAwait(false).GetAwaiter().GetResult();
                    applicationSetting = new()
                    {
                        CataloguePostTemplate = string.Join(TemplateComponent.Separator, DefaultTemplate.DefaultTempalte_Catalogue),
                        KeywordForShoppingIntention = BabelFish.Vocab["DefaultKeyWordForShoppingIntention"][UtilityHelper.NICAM(coreInfo)],
                        ProductPostTemplate = string.Join(TemplateComponent.Separator, DefaultTemplate.DefaultTempalte_Product),
                    };
                }
                catch (Core.Exceptions.ApplicationSubscriptionException)
                {
                    return RedirectToAction("subscribe", "home", new { organizationID });
                }
            }                        

            if (string.IsNullOrWhiteSpace(applicationSetting.EmailToReceiveOrder))
            {
                applicationSetting.EmailToReceiveOrder = User.Email();
                updated = true;
            }
            
            if (applicationSetting.DeliveryChargeProductID == null)
            {
                List<dynamic> productResult = _corprioClient.ProductApi.Query(
                    organizationID: organizationID,
                    selector: "new(ID)",
                    where: "Code=@0 and Disabled=false",
                    whereArguments: new object[] { Constant.DefaultDeliveryChargeProductCode },
                    orderBy: "").ConfigureAwait(false).GetAwaiter().GetResult();
                if (productResult.Any())
                {
                    applicationSetting.DeliveryChargeProductID = Guid.Parse(productResult[0].ID);
                    updated = true;
                }
            }
            
            if (applicationSetting.WarehouseID == null)
            {
                List<dynamic> warehouseResult = _corprioClient.WarehouseApi.Query(
                    organizationID: organizationID,
                    dynamicQuery: new DynamicQuery()
                    {
                        Selector = "new(ID)",
                        Where = "Code=@0 and Disabled=false",
                        WhereArguments = new object[] { Constant.DefaultWarehouseCode },
                        OrderBy = ""
                    }).ConfigureAwait(false).GetAwaiter().GetResult();
                if (warehouseResult.Any())
                {
                    applicationSetting.WarehouseID = Guid.Parse(warehouseResult[0].ID);
                    updated = true;
                }                
            }

            try
            {
                applicationSetting.IsSmtpSet = _corprioClient.Execute<bool>(
                    request: new CorprioRestClient.ApiRequest($"/organization/{organizationID}/IsSMTPSet", System.Net.Http.HttpMethod.Get)).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (ApiExecutionException ex)
            {
                Log.Error($"Failed to test the latest SMTP setting of organization {organizationID}. {ex.Message}");
            }

            if (firstVisit || updated)
            {
                _applicationSettingService.SaveSetting(organizationID: organizationID, setting: applicationSetting).ConfigureAwait(false).GetAwaiter(); ;
            }
            
            return View(applicationSetting);
        }

        /// <summary>
        /// Save the application setting
        /// </summary>
        /// <param name="organizationID">Organization ID</param>
        /// <param name="model">Application setting submitted from the client side</param>
        /// <returns>Status code</returns>
        public async Task<IActionResult> Save([FromRoute] Guid organizationID, [FromForm] ApplicationSetting model)
        {
            //if (string.IsNullOrWhiteSpace(model.KeywordForShoppingIntention)) throw new Exception("Keyword cannot be blank.");

            ApplicationSetting setting = await _applicationSettingService.GetSetting<ApplicationSetting>(organizationID);            
            Core.Utility.PropertyCopier.Copy(source: model, target: setting, ignoreNotUpdatable: false, copyKeyProperties: false, excludeProperties: null);
            _applicationSettingService.SaveSetting(organizationID: organizationID, setting: setting).ConfigureAwait(false).GetAwaiter();

            return StatusCode(200);
        }

        /// <summary>
        /// Retrieve Line credentials of an organization
        /// </summary>
        /// <param name="organizationID">Organization ID</param>
        /// <returns>Collection of Line channels</returns>
        public IEnumerable<LineChannel> GetLineCredential([FromRoute] Guid organizationID)
        {
            return _db.LineChannels.Where(r => r.OrganizationID == organizationID && r.Dormant == false);
        }
        
        /// <summary>
        /// Disable a Line channel
        /// </summary>
        /// <param name="organizationID">Organization ID</param>
        /// <param name="channelID">ID of the Line channel to be disabled</param>
        /// <returns></returns>
        /// <exception cref="RecordNotFoundException"></exception>
        [HttpDelete]
        public async Task<IActionResult> DisableLineCredential([FromRoute] Guid organizationID, Guid channelID)
        {
            LineChannel channel = _db.LineChannels.FirstOrDefault(r => r.ID == channelID && r.OrganizationID == organizationID) 
                ?? throw new RecordNotFoundException(entity: nameof(LineChannel), findByValues: [channelID, organizationID]);

            channel.Dormant = true;
            _db.LineChannels.Update(channel);
            await _db.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Test and then save Line credential
        /// </summary>        
        /// <param name="organizationID">Organization ID</param>
        /// <param name="credential">Line credential in the shape of LineChannel class</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<IActionResult> SaveLineCredential([FromRoute] Guid organizationID, LineChannel credential)
        {
            if (organizationID.Equals(Guid.Empty)) throw new ArgumentNullException(nameof(organizationID));
            ArgumentNullException.ThrowIfNull(credential);            
            if (string.IsNullOrWhiteSpace(credential.ChannelName)) throw new Exception(Resources.SharedResource.ErrMsg_MissingChannelName);
            if (credential.ChannelName.Length > 20) throw new Exception(Resources.SharedResource.ErrMsg_ChannelNameTooLong);
            if (string.IsNullOrWhiteSpace(credential.ChannelSecret)) throw new Exception(Resources.SharedResource.ErrMsg_MissingChannelSecret);
            if (string.IsNullOrWhiteSpace(credential.ChannelToken)) throw new Exception(Resources.SharedResource.ErrMsg_MissingChannelToken);

            LineChannel channelInStore;
            LineChannel backupChannel = new();  // if the credential is invalid, then restore the name/secret/token from this backup

            if (credential.ID.Equals(Guid.Empty))
            {
                // prevent duplication of Line channels
                if (null != _db.LineChannels.FirstOrDefault(r => r.ChannelSecret == credential.ChannelSecret && r.Dormant == false))
                    throw new Exception(Resources.SharedResource.ErrMsg_DuplicatedChannelSecret);
                
                channelInStore = new LineChannel();
                Core.Utility.PropertyCopier.Copy(source: credential, target: channelInStore);
                channelInStore.ID = Guid.NewGuid();
                channelInStore.OrganizationID = organizationID;

                _db.LineChannels.Add(channelInStore);
            }
            else
            {
                channelInStore = _db.LineChannels.FirstOrDefault(r => r.ID == credential.ID) 
                    ?? throw new RecordNotFoundException(entity: nameof(LineChannel), findByValues: credential.ID);

                Core.Utility.PropertyCopier.Copy(source: channelInStore, target: backupChannel);

                channelInStore.ChannelID = credential.ChannelID;
                channelInStore.ChannelName = credential.ChannelName;
                channelInStore.ChannelSecret = credential.ChannelSecret;
                channelInStore.ChannelToken = credential.ChannelToken;

                _db.LineChannels.Update(channelInStore);
            }            
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}. Inner exception: {ex.InnerException.Message}");
            }

            var lineApiService = new LineApiService(context: _db, httpClientFactory: _httpClientFactory,                 
                configuration: _configuration, lineChannel: channelInStore);

            string errorMessage = string.Empty;
            try
            {
                await lineApiService.SetWebhookEndpoint();
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to set webhook endpoint. {ex.Message}");                
                errorMessage = Resources.SharedResource.ErrMsg_InvalidChannelToken;
            }

            if (string.IsNullOrWhiteSpace(errorMessage))
            {                
                errorMessage = await lineApiService.TestWebhookEndpoint() 
                    ? string.Empty 
                    : Resources.SharedResource.ErrMsg_InvalidChannelSecret;  // webhook endpoint test most likely fails due to incorrect channel secret
            }

            if (string.IsNullOrWhiteSpace(errorMessage)) return Ok();

            // 'forget' the credential if the webhook endpoint fails to be set/tested
            if (string.IsNullOrWhiteSpace(backupChannel.ChannelSecret) || string.IsNullOrWhiteSpace(backupChannel.ChannelToken))
            {
                _db.LineChannels.Remove(channelInStore);
            }
            else
            {
                channelInStore.ChannelID = backupChannel.ChannelID;
                channelInStore.ChannelName = backupChannel.ChannelName;
                channelInStore.ChannelSecret = backupChannel.ChannelSecret;
                channelInStore.ChannelToken = backupChannel.ChannelToken;
                _db.LineChannels.Update(channelInStore);
            }                        
            await _db.SaveChangesAsync();

            throw new Exception(errorMessage);
        }

        /// <summary>
        /// Render a thank-you view based on the organization's application setting
        /// </summary>
        /// <param name="organizationID">Organization ID</param>
        /// <returns>View</returns>
        [HttpGet]
        [OrganizationOwnerOnly]
        public async Task<IActionResult> PreviewThankYou([FromRoute] Guid organizationID)
        {
            OrganizationCoreInfo orgInfo = await _corprioClient.OrganizationApi.GetCoreInfo(organizationID);
            ApplicationSetting applicationSetting = await _applicationSettingService.GetSetting<ApplicationSetting>(organizationID);

            OrderConfirmationDataModel model = new()
            {
                SalesOrderID = Guid.NewGuid(),
                OrderDate = new DateTimeOffset(DateTime.Now),
                DocumentNum = "SO123456",
                CurrencyCode = orgInfo.CurrencyCode,
                Amount = 300,
                DeliveryContact_DisplayName = "John Doe",
                DeliveryPhoneNumbers = new List<PhoneNumber> { new PhoneNumber() },
                BillName = "John Doe",
                BillEmail = "sampleemail@email.com",
                BillPhoneNumbers = new List<PhoneNumber> { new PhoneNumber() },
                Products = new List<ProductViewModel> {
                        new ProductViewModel{
                            Code = "AC0001",
                            Name = "Sample Accessories",
                            UnitPrice = 50,
                            Qty = 2,
                            ImageUrls = new List<string>{"https://app.gobuy.click/images/Accessories.png" },  // TODO: replace this URL with one residing in this app's production
                        },
                        new ProductViewModel{
                            Code = "JN0001",
                            Name = "Sample Shoes",
                            UnitPrice = 200,
                            Qty = 1,
                            ImageUrls = new List<string>{"https://app.gobuy.click/images/Shoes.png" },  // TODO: replace this URL with one residing in this app's production
                        },
                    },
                ThankYouMessage = applicationSetting.ThankYouMessage,
            };

            model.LogoImageUrl = await _corprioClient.OrganizationApi.GetApplicationImageUrlByKey(organizationID, Common.LogoImageKey, ImageSize.Original);

            if (applicationSetting.ShipToCustomer)
            {
                model.DeliveryAddress_Line1 = "Sample address line 1";
                model.DeliveryAddress_Line2 = "Sample address line 2";
                model.DeliveryAddress_City = "Sample City";
                model.DeliveryAddress_State = "Sample State";
                model.DeliveryAddress_PostalCode = "00000";
                model.DeliveryAddress_CountryAlphaCode = "COUNTRY";
            }

            return View("../Checkout/ThankYou", model);
        }

        /// <summary>
        /// Render a checkout view based on the organization's application setting
        /// </summary>
        /// <param name="organizationID">Organization ID</param>
        /// <param name="ui">Culture name for user interface in the format of languagecode2-country/regioncode2</param>
        /// <returns>View</returns>
        [HttpGet]
        [OrganizationOwnerOnly]
        public async Task<IActionResult> PreviewCheckout([FromRoute] Guid organizationID, [FromQuery] string ui)
        {

            OrganizationCoreInfo coreInfo = await _corprioClient.OrganizationApi.GetCoreInfo(organizationID);
            ApplicationSetting applicationSetting = await _applicationSettingService.GetSetting<ApplicationSetting>(organizationID);

            PhoneNumber dummyPhone = new() { NumberType = Global.PhoneNumberType.Mobile, CountryCallingCode = "852", SubscriberNumber = "61234567" };
            CheckoutViewModel checkoutView = new()
            {
                AllowSelfPickUp = applicationSetting.SelfPickUp,
                BillContactPhone = dummyPhone,
                BillPerson = new Global.Person.Person { FamilyName = "Doe", GivenName = "John" },
                ChosenDeliveryMethod = applicationSetting.SelfPickUp ? DeliveryOption.SelfPickup : (applicationSetting.ShipToCustomer ? DeliveryOption.Shipping : DeliveryOption.NoOption),
                CurrencyCode = coreInfo.CurrencyCode,
                DefaultCountryCode = coreInfo.CountryCode,
                DeliveryAddress = new Address(),
                DeliveryCharge = applicationSetting.DeliveryCharge,
                DeliveryContact = new Global.Person.Person(),
                DeliveryContactPhone = dummyPhone,
                DocumentNum = "SO123456",
                FreeShippingAmount = applicationSetting.FreeShippingAmount,
                Footer = applicationSetting.Footer,
                IsOrderVoidOrPaid = false,
                IsPaymentClicked = false,
                IsPreview = true,
                Language = string.IsNullOrWhiteSpace(ui) ? System.Threading.Thread.CurrentThread.CurrentCulture.Name : ui,                                
                Lines = new List<OrderLine>
                {
                    new OrderLine
                    {
                        ChildProductInfo = new List<ChildProductInfo>(),
                        DisallowOutOfStock = true,
                        NetUnitPrice = 50,
                        ProductDesc = "Sample product for preview",
                        ProductID = Guid.NewGuid(),
                        ProductName = "Sample Accessories",
                        ProductStockLevel = 100,
                        Quantity = 1,
                        SalesOrderLineID = Guid.NewGuid(),
                        UOMCode = "unit",
                        URL = "https://app.gobuy.click/images/Accessories.png"  // TODO: replace this URL with one residing in this app's production
                    },
                    new OrderLine
                    {
                        ChildProductInfo = new List<ChildProductInfo>(),
                        DisallowOutOfStock = true,
                        NetUnitPrice = 200,
                        ProductDesc = "Sample product for preview",
                        ProductID = Guid.NewGuid(),
                        ProductName = "Sample Shoes",
                        ProductStockLevel = 100,
                        Quantity = 2,
                        SalesOrderLineID = Guid.NewGuid(),
                        UOMCode = "unit",
                        URL = "https://app.gobuy.click/images/Shoes.png"  // TODO: replace this URL with one residing in this app's production
                    },
                },
                OrderDate = new DateTimeOffset(DateTime.Now),
                OrganizationID = organizationID,
                OrganizationShortName = coreInfo.ShortName,
                OrganizationEmailAddress = coreInfo.EmailAddress.Address,
                ProvideDelivery = applicationSetting.ShipToCustomer,
                SalesOrderID = Guid.NewGuid(),
                SelfPickUpInstruction = applicationSetting.SelfPickUpInstruction,                
            };
            
            if (checkoutView.ChosenDeliveryMethod == DeliveryOption.Shipping)
            {
                checkoutView.DeliveryContact = new Global.Person.Person { GivenName = "John", FamilyName = "Doe" };
                checkoutView.DeliveryAddress.Line1 = "Sample address line 1";
                checkoutView.DeliveryAddress.Line2 = "Sample address line 2";
                checkoutView.DeliveryAddress.City = "Sample city";
                checkoutView.DeliveryAddress.State = "Sample state";
                checkoutView.DeliveryAddress.PostalCode = "00000";
                checkoutView.DeliveryAddress.CountryAlphaCode = "COUNTRY";
            }
            
            checkoutView.OrderLineJsonString = System.Text.Json.JsonSerializer.Serialize(checkoutView.Lines);

            return View("../Checkout/Index", checkoutView);
        }
    }
}
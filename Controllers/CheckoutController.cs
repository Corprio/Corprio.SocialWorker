using Corprio.AspNetCore.Site.Filters;
using Corprio.CorprioAPIClient;
using Corprio.DataModel.Business.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Corprio.SocialWorker.Models;
using System.Collections.Generic;
using Corprio.DataModel.Business.Products;
using System.Linq;
using Corprio.Core.Exceptions;
using Serilog;
using Corprio.Core.Utility;
using System.Text;
using Microsoft.Extensions.Configuration;
using Corprio.Core;
using Corprio.DataModel;
using Corprio.DataModel.Business;
using Corprio.Global.Geography;
using Corprio.SocialWorker.Helpers;
using Corprio.DataModel.Business.Sales.ViewModel;
using Corprio.DataModel.Shared;
using Newtonsoft.Json;
using Corprio.DataModel.Business.Logistic;
using Newtonsoft.Json.Linq;
using Corprio.AspNetCore.Site.Services;

namespace Corprio.SocialWorker.Controllers
{
    public class CheckoutController : AspNetCore.XtraReportSite.Controllers.BaseController
    {
        private readonly ApplicationDbContext db;
        private readonly ApplicationSettingService applicationSettingService;
        readonly IConfiguration configuration;
        private const string EntityPropertyName = "social-work-checkout";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="applicationSettingService"></param>
        public CheckoutController(ApplicationDbContext context, IConfiguration configuration, 
            ApplicationSettingService applicationSettingService) : base()
        {
            db = context;
            this.configuration = configuration;
            this.applicationSettingService = applicationSettingService;
        }

        /// <summary>
        /// Returns a view for customers to perform checkout
        /// </summary>
        /// <param name="httpClientFactory">HttpClientFactory for resolving the httpClient for client access</param>
        /// <param name="organizationID">Organization ID</param>
        /// <param name="salesOrderID">Sales order ID</param>
        /// <param name="ui">Culture name for user interface in the format of languagecode2-country/regioncode2</param>
        /// <returns>View</returns>
        [AllowAnonymous]
        [OrganizationNeeded(false)]
        [HttpGet("/{organizationID:guid}/checkout/{salesOrderID:guid}")]
        public async Task<IActionResult> Checkout([FromServices] IHttpClientFactory httpClientFactory,
            [FromRoute] Guid organizationID, [FromRoute] Guid salesOrderID, [FromQuery] string ui)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("appClient");
            var corprioClient = new APIClient(httpClient);

            SalesOrder salesOrder = await corprioClient.SalesOrderApi.Get(organizationID: organizationID, id: salesOrderID);
            if (salesOrder == null) return NotFound(Resources.SharedResource.ResourceManager.GetString("ErrMsg_SalesOrderNotFound"));

            Customer customer = await corprioClient.CustomerApi.Get(organizationID: organizationID, id: salesOrder.CustomerID);
            if (customer?.BusinessPartner == null) return NotFound(Resources.SharedResource.ResourceManager.GetString("ErrMsg_CustomerNotFound"));
            
            ApplicationSetting applicationSetting = await applicationSettingService.GetSetting<ApplicationSetting>(organizationID);
            if (applicationSetting == null) return NotFound(Resources.SharedResource.ResourceManager.GetString("ErrMsg_AppSettingNotFound"));

            OrganizationCoreInfo coreInfo = await corprioClient.OrganizationApi.GetCoreInfo(organizationID);
            if (coreInfo == null) return NotFound(Resources.SharedResource.ResourceManager.GetString("ErrMsg_OrganizationInfoNotFound"));
            
            CheckoutViewModel checkoutView = new()
            {
                AllowSelfPickUp = applicationSetting.SelfPickUp,
                BillContactPhone = salesOrder.BillPhoneNumbers?.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.SubscriberNumber)) ?? customer.BusinessPartner.PrimaryMobilePhoneNumber(),
                CurrencyCode = salesOrder.CurrencyCode,
                DefaultCountryCode = coreInfo.CountryCode,
                DeliveryAddress = new Address(),
                DeliveryCharge = applicationSetting.DeliveryCharge,
                DeliveryContact = new Global.Person.Person(),
                DeliveryContactPhone = salesOrder.DeliveryPhoneNumbers?.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.SubscriberNumber)) ?? customer.BusinessPartner.PrimaryMobilePhoneNumber(),
                DocumentNum = salesOrder.DocumentNum,
                FreeShippingAmount = applicationSetting.FreeShippingAmount,
                Footer = applicationSetting.Footer,
                IsOrderVoidOrPaid = (salesOrder.IsVoided || (salesOrder.BilledStatus != null && salesOrder.BilledStatus.InvoicedQty > 0)),
                IsPreview = false,
                Language = string.IsNullOrWhiteSpace(ui) ? System.Threading.Thread.CurrentThread.CurrentCulture.Name : ui,
                Lines = new List<OrderLine>(),
                OrderDate = salesOrder.OrderDate,
                OrganizationID = organizationID,
                OrganizationShortName = coreInfo.ShortName,
                OrganizationEmailAddress = coreInfo.EmailAddress.Address,
                ProvideDelivery = applicationSetting.ShipToCustomer,
                SalesOrderID = salesOrderID,
                SelfPickUpInstruction = applicationSetting.SelfPickUpInstruction,                
            };

            // since the view may be rendered AFTER checkout, we try to restore the order with user's input stored in the sales order's EP
            salesOrder.EntityProperties ??= new List<EntityProperty>();
            EntityProperty checkoutEP = salesOrder.EntityProperties.FirstOrDefault(x => x.Name == EntityPropertyName);
            if (checkoutEP != null)
            {
                SalesOrderCheckoutState state = JsonConvert.DeserializeObject<SalesOrderCheckoutState>(checkoutEP.Value)!;
                if (state == null)
                {
                    // note: we don't throw error, but act as if this is the first time this view is rendered for the customer
                    Log.Error($"Failed to deserialize the entity property value for checkout state of sales order {salesOrderID}");
                }
                checkoutView.BillPerson = new Global.Person.Person
                {
                    FamilyName = state?.BillPerson_FamilyName,
                    GivenName = state?.BillPerson_GivenName,
                };
                checkoutView.ChosenDeliveryMethod = state?.ChosenDeliveryMethod ?? DeliveryOption.NoOption;
                checkoutView.IsPaymentClicked = state?.IsPaymentClicked ?? false;
            }
            else
            {
                checkoutView.BillPerson = new Global.Person.Person
                {                    
                    FamilyName = customer.BusinessPartner.FamilyName,
                    GivenName = customer.BusinessPartner.GivenName,
                };

                salesOrder.EntityProperties.Add(new EntityProperty
                {
                    Name = EntityPropertyName,
                    Value = System.Text.Json.JsonSerializer.Serialize(new SalesOrderCheckoutState 
                    {                        
                        BillPerson_FamilyName = customer.BusinessPartner.FamilyName,
                        BillPerson_GivenName = customer.BusinessPartner.GivenName,
                        ChosenDeliveryMethod = DeliveryOption.NoOption,
                        IsPaymentClicked = false,
                    })
                });
                try
                {
                    // note: "updateLines" actually means "updateChildren", i.e., True means EP is also impacted
                    await corprioClient.SalesOrderApi.Update(organizationID: organizationID, salesOrder: salesOrder, updateLines: true);
                }
                catch (ApiExecutionException ex)
                {
                    throw new Exception($"The sales order could not be updated. Details: {ex.Message}");
                }
            }            
                        
            // note: the customer may have provided a delivery address different from their primary address (or chosen self-pickup),
            // so we should use the former if it isn't blank
            if (!string.IsNullOrWhiteSpace(salesOrder.DeliveryAddress_Line1))
            {
                checkoutView.DeliveryAddress.Line1 = salesOrder.DeliveryAddress_Line1;
                checkoutView.DeliveryAddress.Line2 = salesOrder.DeliveryAddress_Line2;
                checkoutView.DeliveryAddress.City = salesOrder.DeliveryAddress_City;
                checkoutView.DeliveryAddress.State = salesOrder.DeliveryAddress_State;
                checkoutView.DeliveryAddress.PostalCode = salesOrder.DeliveryAddress_PostalCode;
                checkoutView.DeliveryAddress.CountryAlphaCode = salesOrder.DeliveryAddress_CountryAlphaCode;
            }
            else
            {
                checkoutView.DeliveryAddress.Line1 = customer.BusinessPartner.PrimaryAddress_Line1;
                checkoutView.DeliveryAddress.Line2 = customer.BusinessPartner.PrimaryAddress_Line2;
                checkoutView.DeliveryAddress.City = customer.BusinessPartner.PrimaryAddress_City;
                checkoutView.DeliveryAddress.State = customer.BusinessPartner.PrimaryAddress_State;
                checkoutView.DeliveryAddress.PostalCode = customer.BusinessPartner.PrimaryAddress_PostalCode;
                checkoutView.DeliveryAddress.CountryAlphaCode = customer.BusinessPartner.PrimaryAddress_CountryAlphaCode;
            }

            // note: the customer may have provided a delivery contact that is not themselves, so we should use the former whenever possible
            if (!string.IsNullOrWhiteSpace(salesOrder.DeliveryContact_GivenName) || !string.IsNullOrWhiteSpace(salesOrder.DeliveryContact_FamilyName))
            {
                checkoutView.DeliveryContact.FamilyName = salesOrder.DeliveryContact_FamilyName;
                checkoutView.DeliveryContact.GivenName = salesOrder.DeliveryContact_GivenName;                
            }
            else
            {
                checkoutView.DeliveryContact.FamilyName = customer.BusinessPartner.FamilyName;
                checkoutView.DeliveryContact.GivenName = customer.BusinessPartner.GivenName;
            }

            Product product;            
            salesOrder.Lines = salesOrder.Lines.OrderBy(x => x.SortOrder).ToList();
            foreach (SalesOrderLine line in salesOrder.Lines)
            {
                product = await corprioClient.ProductApi.Get(organizationID: organizationID, id: line.ProductID);
                if (product == null) return NotFound(Resources.SharedResource.ResourceManager.GetString("ErrMsg_ProductNotFound"));
                
                checkoutView.Lines.Add(await PrepareOrderLine(corprioClient: corprioClient, organizationID: organizationID, product: product, line: line, warehouseID: (Guid)applicationSetting.WarehouseID));
            }
            checkoutView.OrderLineJsonString = System.Text.Json.JsonSerializer.Serialize(checkoutView.Lines);

            return View(viewName: "Index", model: checkoutView);
        }

        /// <summary>
        /// Generate an order line that will be included in the checkout page
        /// </summary>
        /// <param name="corprioClient">Client for API requests among Corprio projects</param>
        /// <param name="organizationID">Organization ID</param>
        /// <param name="product">Product to be included in the order line</param>
        /// <param name="line">Sales order line from which the order line is derived</param>
        /// <param name="warehouseID">ID of the warehouse at which the stock level is checked</param>
        /// <returns>An order line that will be included in the checkout page</returns>
        public async Task<OrderLine> PrepareOrderLine(APIClient corprioClient, Guid organizationID, Product product, 
            SalesOrderLine line, Guid warehouseID)
        {
            var orderLine = new OrderLine
            {
                SalesOrderLineID = line.ID,
                ProductID = product.ID,
                ProductName = product.Name,
                ProductDesc = product.Description,
                ProductStockLevel = 0,
                DisallowOutOfStock = product.Nature == ProductNature.Inventory,
                NetUnitPrice = line.NetUnitPrice,
                Quantity = line.Qty,
                UOMCode = line.UOMCode,
            };

            // the following variables are reusable in the while loop below
            List<StockTotalByProductWarehouse> stockLevels;
            decimal reservedStock;
            if (orderLine.DisallowOutOfStock)
            {
                stockLevels = await corprioClient.StockApi.GetCurrentStocks(organizationID: organizationID, productID: line.ProductID, warehouseID: warehouseID);
                if (stockLevels.Any())
                {
                    reservedStock = await corprioClient.StockApi.GetReservedStock(organizationID: organizationID, productID: line.ProductID, warehouseID: warehouseID);
                    // note: the orderline's Qty is added back to the stock level because this amount has been included in reserved stock
                    orderLine.ProductStockLevel = stockLevels.First().BaseQty - reservedStock + orderLine.Quantity;
                }
            }

            HashSet<Guid> imageIDs = UtilityHelper.ReturnImageUrls(product);
            if (imageIDs.Any())
            {
                // assumption: we don't care which image of the product is used
                string imageUrlKey = await corprioClient.ImageApi.UrlKey(organizationID: organizationID, id: imageIDs.First());
                orderLine.URL = corprioClient.ImageApi.Url(organizationID: organizationID, imageUrlKey: imageUrlKey);
            }
            
            orderLine.ChildProductInfo = new List<ChildProductInfo>();
            int childPageNum = 0;
            ChildProductInfo childProductInfo;
            PagedList<Product> childProductList;
            do
            {
                childProductList = await corprioClient.ProductApi.GetChildProductPageWithChildEntities(
                        organizationID: organizationID,
                        loadDataOptions: new LoadDataOptions()
                        {
                            PageIndex = childPageNum,
                            PageSize = 20,
                            RequireTotalCount = false,
                            Sort = new SortingInfo[] { new SortingInfo { Desc = false, Selector = "CreateDate" } },
                            Filter = new List<object> { new JArray("MasterProductID", "=", product.MasterProductID.HasValue ? product.MasterProductID?.ToString() : product.ID.ToString()) }
                        });

                foreach (Product childProduct in childProductList.Items)
                {
                    // note: if for some reason the line's product has become disabled, we still keep it
                    // because it would be weird to exclude this product's variations from the select boxes
                    if (childProduct.Disabled && childProduct.ID != product.ID) continue;

                    childProductInfo = new()
                    {
                        ID = childProduct.ID,                        
                        DisallowOutOfStock = childProduct.Nature == ProductNature.Inventory,
                    };

                    if (childProductInfo.DisallowOutOfStock)
                    {
                        stockLevels = await corprioClient.StockApi.GetCurrentStocks(organizationID: organizationID, productID: childProduct.ID, warehouseID: warehouseID);
                        if (!stockLevels.Any()) continue;

                        reservedStock = await corprioClient.StockApi.GetReservedStock(organizationID: organizationID, productID: childProduct.ID, warehouseID: warehouseID);
                        // note: the order line's Qty is added back for the same reason mentioned above
                        childProductInfo.ProductStockLevel = childProduct.ID == product.ID
                            ? stockLevels.First().BaseQty - reservedStock + orderLine.Quantity
                            : stockLevels.First().BaseQty - reservedStock;

                        // note: if the line's product has become out of stock, we still keep it for the same reason mentioned above                        
                        if (childProductInfo.ProductStockLevel <= 0 && childProduct.ID != product.ID) continue;
                    }
                    childProductInfo.ChildProductAttributes = childProduct.ChildProductAttributes.Select(
                        x => new ProductVariationInfo
                        { 
                            Attribute = x.ProductVariation.AttributeType, 
                            Code = x.ProductVariation.Code, 
                            Name = x.ProductVariation.Name 
                        }).ToList();
                    
                    orderLine.ChildProductInfo.Add(childProductInfo);
                }
                childPageNum++;
            }
            while (childProductList?.HasNextPage ?? false);

            return orderLine;
        }

        /// <summary>
        /// Remove a sales order line
        /// </summary>
        /// <param name="httpClientFactory">HttpClientFactory for resolving the httpClient for client access</param>
        /// <param name="organizationID">Organization ID</param>
        /// <param name="salesOrderID">Sales order ID</param>
        /// <param name="salesOrderLineID">Sales order line ID</param>
        /// <returns>Status code</returns>
        /// <exception cref="Exception"></exception>
        [AllowAnonymous]
        [OrganizationNeeded(false)]
        public async Task<IActionResult> DeleteSalesOrderLine([FromServices] IHttpClientFactory httpClientFactory, [FromRoute] Guid organizationID, 
            Guid salesOrderID, Guid salesOrderLineID)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("appClient");
            var corprioClient = new APIClient(httpClient);

            // note: we must validate if the sales order line being deleted really belongs to the sales order used in the view (which cannot be manipulated by the user),
            // because it is possible that the user has changed the sales order line ID by editing the HTML
            SalesOrder salesOrder = await corprioClient.SalesOrderApi.Get(organizationID: organizationID, id: salesOrderID);
            if (salesOrder?.Lines?.FirstOrDefault(x => x.ID == salesOrderLineID) == null)
                throw new Exception("The sales order line ID is invalid");

            try
            {
                await corprioClient.SalesOrderApi.DeleteLine(organizationID: organizationID, lineID: salesOrderLineID);
            }
            catch (ApiExecutionException ex)
            {
                throw new Exception($"The sales order line could not be deleted. Details: {ex.Message}");
            }
            return StatusCode(200);
        }

        /// <summary>
        /// Update a sales order line
        /// </summary>
        /// <param name="httpClientFactory">HttpClientFactory for resolving the httpClient for client access</param>
        /// <param name="organizationID">Organization ID</param>
        /// <param name="salesOrderID">Sales order ID</param>
        /// <param name="salesOrderLineID">Sales order line ID</param>
        /// <param name="productID">Product ID</param>
        /// <param name="quantity">Quantity of product</param>
        /// <returns>Status code</returns>
        /// <exception cref="Exception"></exception>
        [AllowAnonymous]
        [OrganizationNeeded(false)]
        public async Task<OrderLine> EditSalesOrderLine([FromServices] IHttpClientFactory httpClientFactory, 
            [FromRoute] Guid organizationID, Guid salesOrderID, Guid salesOrderLineID, Guid productID, decimal quantity)
        {
            if (quantity <= 0) throw new Exception(Resources.SharedResource.ResourceManager.GetString("ErrMsg_QuantityNotPositive"));

            HttpClient httpClient = httpClientFactory.CreateClient("appClient");
            var corprioClient = new APIClient(httpClient);

            // note: we must validate if the sales order line being edited really belongs to the sales order used in the view (which cannot be manipulated by the user),
            // because it is possible that the user has changed the sales order line ID by editing the HTML
            SalesOrder salesOrder = await corprioClient.SalesOrderApi.Get(organizationID: organizationID, id: salesOrderID) 
                ?? throw new Exception("The sales order ID is invalid");
            SalesOrderLine salesOrderLine = salesOrder.Lines.FirstOrDefault(x => x.ID == salesOrderLineID) 
                ?? throw new Exception("The sales order line ID is invalid");
            Product product = await corprioClient.ProductApi.Get(organizationID: organizationID, id: productID)
                ?? throw new Exception(Resources.SharedResource.ResourceManager.GetString("ErrMsg_ProductNotFound"));

            PriceWithCurrency price = await corprioClient.SellingPriceApi.GetPriceForCustomer(
                organizationID: organizationID,
                productID: productID,
                customerID: salesOrder.CustomerID,
                quantity: new Global.Measure.Quantity { UOMCode = product.ListPrice_UOMCode, Value = quantity },
                currencyCode: salesOrder.CurrencyCode);
            // note: we update the price's properties because we need it to compute the net unit price
            price.Price.DiscountValue = salesOrderLine.DiscountValue;
            price.Price.DiscountType = salesOrderLine.DiscountType;
            int roundToDecimals = await corprioClient.OrganizationSettingApi.GetCurrencyDecimals(organizationID: organizationID, currencyCode: salesOrder.CurrencyCode);
            NumberRoundingMode priceRoundingMode = await corprioClient.OrganizationSettingApi.GetPriceRoundingMode(organizationID);            

            salesOrderLine.UOMCode = product.ListPrice_UOMCode;
            salesOrderLine.ProductID = productID;            
            salesOrderLine.UnitPrice = price.Price.Value ?? 0;
            salesOrderLine.NetUnitPrice = NumberHelper.Round(value: price.Price.Net ?? 0, decimals: roundToDecimals, roundingMode: priceRoundingMode);
            salesOrderLine.Qty = quantity;
            salesOrderLine.LineAmount = quantity * salesOrderLine.NetUnitPrice;

            try
            {
                await corprioClient.SalesOrderApi.UpdateLine(organizationID: organizationID, line: salesOrderLine);
            }
            catch (ApiExecutionException ex)
            {
                throw new Exception($"The sales order line could not be updated. Details: {ex.Message}");
            }

            ApplicationSetting settings = await applicationSettingService.GetSetting<ApplicationSetting>(organizationID)
                ?? throw new Exception(Resources.SharedResource.ResourceManager.GetString("ErrMsg_AppSettingNotFound"));            

            return await PrepareOrderLine(corprioClient: corprioClient, organizationID: organizationID, product: product, 
                line: salesOrderLine, warehouseID: (Guid)settings.WarehouseID);
        }

        /// <summary>
        /// Generate the body of order confirmation email
        /// </summary>
        /// <param name="model">Data model</param>        
        /// <returns>Email body</returns>
        private string OrderReceivedEmailBody(OrderConfirmationDataModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            //the email html is generated from the mjml file with https://mjml.io/try-it-live to produce responsive emails
            //orderDetailsEmailFile is a string of the html email content containing ${resourceName} for localization
            string orderDetailsEmailFile = System.IO.File.ReadAllText(@"Resources\Email\html\OrderDetailsEmail.html");
            //sub html content to append inside the orderDetailsEmailFile
            string deliveryMethodFile = System.IO.File.ReadAllText(@"Resources\Email\html\OrderDetailsEmail\DeliveryMethod.html");
            string deliveryTableFile = System.IO.File.ReadAllText(@"Resources\Email\html\OrderDetailsEmail\DeliveryTable.html");
            string productRowFile = System.IO.File.ReadAllText(@"Resources\Email\html\OrderDetailsEmail\ProductRow.html");
            string productRemarkFile = System.IO.File.ReadAllText(@"Resources\Email\html\OrderDetailsEmail\ProductRemark.html");
            string RemarkFile = System.IO.File.ReadAllText(@"Resources\Email\html\OrderDetailsEmail\Remark.html");

            //build string for bill phone numbers
            var billPhoneNumbers = new StringBuilder();
            foreach (PhoneNumber phoneNumber in model.BillPhoneNumbers)
            {
                billPhoneNumbers.Append($"<div>{phoneNumber.E123PhoneNumber()}</div>");
            }

            //build string for delivery address table (this will show if customer has input delivery address)
            string deliveryTable = "";
            if (!string.IsNullOrWhiteSpace(model.DeliveryAddress_CountryAlphaCode))
            {
                StringBuilder deliveryPhoneNumbers = new StringBuilder();
                foreach (PhoneNumber p in model.DeliveryPhoneNumbers)
                {
                    deliveryPhoneNumbers.Append($"<div>{p.E123PhoneNumber()}</div>");
                }

                deliveryTable = deliveryTableFile.ReplaceString(new Dictionary<string, string>()
                {
                    { "deliveryAddress", model.DeliveryAddress.Display() },
                    { "deliveryContact_DisplayName", model.DeliveryContact_DisplayName },
                    { "deliveryPhoneNumbers", deliveryPhoneNumbers.ToString() }
                });
            }
            //build string for delivery method (this will show if customer has chosen self-pickup)
            string deliveryMethod = (
                string.IsNullOrWhiteSpace(model.DeliveryAddress_CountryAlphaCode) && !string.IsNullOrWhiteSpace(model.DeliveryAddress_Line1)
                ) ? deliveryMethodFile.ReplaceString(new Dictionary<string, string>() { { "deliveryMethod", model.DeliveryAddress_Line1 } }) : "";

            //build string for product rows
            var productRows = new StringBuilder();
            int lineNum = 0;
            foreach (var item in model.Products)
            {
                string productRemark = string.IsNullOrWhiteSpace(item.Remark) ? "" : productRemarkFile.ReplaceString(new Dictionary<string, string>() { { "p-remark", item.Remark } });
                productRows.Append(
                    productRowFile.ReplaceString(new Dictionary<string, string>() {
                        { "p-lineNum", (++lineNum).ToString() },
                        { "p-imageUrl", string.IsNullOrWhiteSpace(item.ImageUrls[0]) ? "" : item.ImageUrls[0] },
                        { "p-name", item.Name },
                        { "p-code", item.Code },
                        { "*ProductRemark", productRemark },
                        { "p-qty", item.Qty.ToString("N0") },
                        { "p-uomCode", item.UOMCode },
                        { "p-price", new Global.Finance.Money(model.CurrencyCode, item.NetUnitPrice).ToString() }
                    })
                );
            }

            //build string for order remark
            string remark = string.IsNullOrWhiteSpace(model.Remark) ? "" : RemarkFile.ReplaceString(new Dictionary<string, string>() { { "remark", model.Remark } });

            string message = orderDetailsEmailFile.ReplaceString(new Dictionary<string, string>()
            {
                { "logoImageUrl", model.LogoImageUrl },
                { "thankYouMessage", model.ThankYouMessage },
                { "paymentLink", model.PaymentLink },
                { "paymentButtonText", Resources.SharedResource.ResourceManager.GetString("ProceedToPayment") },
                { "documentNum", model.DocumentNum },
                { "orderDate", model.OrderDate.ToString() },
                { "billName", model.BillName },
                { "billEmail", model.BillEmail },
                { "billPhoneNumbers", billPhoneNumbers.ToString()},
                { "*DeliveryMethod", deliveryMethod },
                { "*DeliveryTable", deliveryTable },
                { "*ProductRow", productRows.ToString() },
                { "amount", new Global.Finance.Money(model.CurrencyCode, model.Amount).ToString() },
                { "*Remark", remark }
            });

            return message.LocalizeString(Resources.SharedResource.ResourceManager);
        }

        /// <summary>
        /// Send out order arrival email to the merchant AND order confirmation email to the customer
        /// </summary>
        /// <param name="corprio">Client for executing API requests among Corprio projects</param>
        /// <param name="organizationID">Organization ID</param>
        /// <param name="orderID">Sales order ID</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task SendEmailsAsync(APIClient corprio, Guid organizationID, Guid orderID)
        {
            OrderConfirmationDataModel model = await corprio.SalesOrderApi.GetResult<OrderConfirmationDataModel>(organizationID: organizationID,
                    id: orderID,
                    selector: $@"new(ID as SalesOrderID,OrderDate,DocumentNum,CurrencyCode,Amount,DeliveryAddress_Line1,DeliveryAddress_Line2,DeliveryAddress_City,DeliveryAddress_State,DeliveryAddress_PostalCode,DeliveryAddress_CountryAlphaCode,
                                DeliveryContact_DisplayName,DeliveryPhoneNumbers,BillName,BillEmail,BillPhoneNumbers,Remark,
                                Lines.OrderBy(SortOrder).Select(new (ProductID,Product.Code,Product.Name,Remark,Qty,UnitPrice,NetUnitPrice,UOMCode,Product.ListPrice_QtyDecimals as SalesUOMDecimals,DiscountType,DiscountValue,Product.Image01.UrlKey as Image01UrlKey)) as Products
                                )");
            foreach (ProductViewModel product in model.Products)
            {
                product.ImageUrls[0] = corprio.ImageApi.Url(organizationID, product.Image01UrlKey, ImageSize.Thumbnail);
            }

            ApplicationSetting applicationSetting = await applicationSettingService.GetSetting<ApplicationSetting>(organizationID)
                ?? throw new Exception(Resources.SharedResource.ResourceManager.GetString("ErrMsg_AppSettingNotFound"));            
            model.ThankYouMessage = applicationSetting.ThankYouMessage;
            OrganizationCoreInfo coreInfo = await corprio.OrganizationApi.GetCoreInfo(organizationID);            
            model.LogoImageUrl = coreInfo?.LogoImageUrl;

            string paymentAppUrl = configuration["PaymentAppUrl"].ToString();
            string orgShortName = await corprio.OrganizationApi.GetShortName(organizationID);
            string AppBaseUrl = configuration["AppBaseUrl"].ToString();
            string paymentLink = paymentAppUrl + "/" + orgShortName + "/RecePayment/order/" + orderID + "?successUrl=" + AppBaseUrl + "/" + organizationID + "/thankyou&failUrl=" + AppBaseUrl + "/" + organizationID + "/paymentfailed";
            model.PaymentLink = paymentLink;

            string emailBody = OrderReceivedEmailBody(model);
            
            if (applicationSetting.SendConfirmationEmail)
            {
                try
                {
                    await corprio.OrganizationApi.SendEmail(
                        organizationID: organizationID,
                        emailMessage: new Core.EmailMessage()
                        {
                            ToEmails = new Core.EmailAddress[] { new Core.EmailAddress(address: model.BillEmail, name: model.BillName) },
                            Subject = applicationSetting.DefaultEmailSubject,
                            Body = emailBody
                        }
                    );
                }
                catch (ApiExecutionException ex)
                {
                    if (ex.ErrorCode == ErrorCode.SendEmailError)
                    {
                        //if failed to send email to customer, send email to merchant to notify that email is not sent
                        await corprio.OrganizationApi.SendEmail(
                            organizationID: organizationID,
                            emailMessage: new Core.EmailMessage()
                            {
                                ToEmails = applicationSetting.EmailToReceiveOrder.Split(',', ';').Select(s => new Core.EmailAddress(s)).ToArray(),
                                Subject = "[" + Resources.SharedResource.AppName + "] " + string.Format(Resources.SharedResource.ConfirmationEmailNotSent, model.DocumentNum),
                                Body = string.Format(Resources.SharedResource.EmailNotSentMessage, model.BillEmail, model.DocumentNum)

                            }
                        );
                    }
                }
            }            
            
            await corprio.OrganizationApi.SendEmail(
                organizationID: organizationID,
                emailMessage: new Core.EmailMessage()
                {
                    ToEmails = applicationSetting.EmailToReceiveOrder.Split(',', ';').Select(s => new Core.EmailAddress(s)).ToArray(),
                    Subject = "[" + Resources.SharedResource.AppName + "] " + Resources.SharedResource.YouReceivedNewOrder + " " + model.DocumentNum,
                    Body = emailBody
                }
            );
        }

        /// <summary>
        /// Void a sales order and add its order lines to the cart of the chat bot who created the order
        /// </summary>
        /// <param name="httpClientFactory">HttpClientFactory for resolving the httpClient for client access</param>
        /// <param name="organizationID">Organization ID</param>
        /// <param name="salesOrderID">Sales order ID</param>
        /// <returns>Status code</returns>
        /// <exception cref="Exception"></exception>
        [AllowAnonymous]
        [OrganizationNeeded(false)]
        public async Task<IActionResult> VoidAndRecall([FromServices] IHttpClientFactory httpClientFactory, [FromRoute] Guid organizationID, Guid salesOrderID)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("appClient");
            var corprioClient = new APIClient(httpClient);

            SalesOrder salesOrder = await corprioClient.SalesOrderApi.Get(organizationID: organizationID, id: salesOrderID) 
                ?? throw new Exception(Resources.SharedResource.ResourceManager.GetString("ErrMsg_SalesOrderNotFound"));

            DbFriendlyBot bot = db.MetaBotStatuses.FirstOrDefault(x => x.BuyerCorprioID == salesOrder.CustomerID && x.FacebookUser.OrganizationID == organizationID && x.FacebookUser.Dormant == false); ;            
            if (bot == null)
            {
                // note: the frontend must expect 410 = chat history not found
                return StatusCode(410, "The chatbot responsible for this sales order is not found.");
            }
            MetaBotStatus botStatus = bot.ReadyToWork();

            ApplicationSetting applicationSetting = await applicationSettingService.GetSetting<ApplicationSetting>(organizationID);            
            if (applicationSetting == null) return NotFound(Resources.SharedResource.ResourceManager.GetString("ErrMsg_AppSettingNotFound"));

            // note 1: the bot may already carry some items at the cart now but we don't clear the cart
            // note 2: there is no reason to update the topic that the bot is dealing with
            Product product;
            foreach (SalesOrderLine line in salesOrder.Lines)
            {                
                if (line.ProductID == applicationSetting.DeliveryChargeProductID) continue;  // line representing delivery charge is excluded from the cart

                product = await corprioClient.ProductApi.Get(organizationID: organizationID, id: line.ProductID) 
                    ?? throw new Exception(Resources.SharedResource.ResourceManager.GetString("ErrMsg_ProductNotFound"));
                
                botStatus.Cart.Add(new BotBasket 
                { 
                    DiscountType = line.DiscountType,
                    Discount = line.DiscountValue,
                    ProductID = line.ProductID,
                    Name = product.Name,
                    Price = line.UnitPrice,
                    Quantity = line.Qty,
                    UOMCode = line.UOMCode,
                });
            }
            db.MetaBotStatuses.Update(bot.ReadyToSave(botStatus));
            await db.SaveChangesAsync();

            await corprioClient.SalesOrderApi.Void(organizationID: organizationID, id: salesOrderID);
            return StatusCode(200);
        }

        /// <summary>
        /// Void a sales order
        /// </summary>
        /// <param name="httpClientFactory">HttpClientFactory for resolving the httpClient for client access</param>
        /// <param name="organizationID">Organization ID</param>
        /// <param name="salesOrderID">Sales order ID</param>
        /// <returns>Status code</returns>
        [AllowAnonymous]
        [OrganizationNeeded(false)]
        public async Task<IActionResult> VoidSalesOrder([FromServices] IHttpClientFactory httpClientFactory, [FromRoute] Guid organizationID, Guid salesOrderID)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("appClient");
            var corprioClient = new APIClient(httpClient);
            await corprioClient.SalesOrderApi.Void(organizationID: organizationID, id: salesOrderID);
            return StatusCode(200);
        }

        /// <summary>
        /// Triggered when the customer elects to proceed to payment
        /// </summary>
        /// <param name="httpClientFactory">HttpClientFactory for resolving the httpClient for client access</param>
        /// <param name="organizationID">Organization ID</param>
        /// <param name="data">Data submitted by the customer when they complete checkout</param>
        /// <returns>Status code</returns>
        /// <exception cref="Exception"></exception>
        [AllowAnonymous]
        [OrganizationNeeded(false)]
        public async Task<IActionResult> FinalizeSalesOrder([FromServices] IHttpClientFactory httpClientFactory, [FromRoute] Guid organizationID,
            [FromBody] CheckoutDataModel data)
        {                                                
            if (data == null) throw new Exception("No data was provided.");

            ApplicationSetting applicationSetting = await applicationSettingService.GetSetting<ApplicationSetting>(organizationID);            
            if (applicationSetting == null) return NotFound(Resources.SharedResource.ResourceManager.GetString("ErrMsg_AppSettingNotFound"));

            HttpClient httpClient = httpClientFactory.CreateClient("appClient");
            var corprioClient = new APIClient(httpClient);

            SalesOrder salesOrder = await corprioClient.SalesOrderApi.Get(organizationID: organizationID, id: data.SalesOrderID)
                ?? throw new Exception(Resources.SharedResource.ResourceManager.GetString("ErrMsg_SalesOrderNotFound"));
            if (salesOrder.IsVoided || (salesOrder.BilledStatus != null && salesOrder.BilledStatus.InvoicedQty > 0))
            {
                return StatusCode(400, "The sales order has been voided or paid and therefore cannot be edited.");
            }

            // this state is 'proper' in the sense that it is how it should look like if the customer has submitted the form for payment
            SalesOrderCheckoutState properCheckoutState = new()
            {
                BillPerson_FamilyName = data.BillPerson?.FamilyName,
                BillPerson_GivenName = data.BillPerson?.GivenName,
                ChosenDeliveryMethod = data.ChosenDeliveryMethod,
                IsPaymentClicked = true,
            };

            // check if the customer has submitted the form for payment previously
            salesOrder.EntityProperties ??= new List<EntityProperty>();
            EntityProperty checkoutEP = salesOrder.EntityProperties.FirstOrDefault(x => x.Name == EntityPropertyName);
            if (checkoutEP != null)
            {
                SalesOrderCheckoutState checkoutStateInEP = JsonConvert.DeserializeObject<SalesOrderCheckoutState>(checkoutEP.Value)!;
                if (checkoutStateInEP == null)
                {
                    // note: we don't throw error, but act as if this is the first time the customer submits the form for payment.
                    // this practice is acceptable because the order has been neither voided nor paid.
                    Log.Error($"Failed to deserialize the entity property value for checkout state of sales order {data.SalesOrderID}");
                    
                }                
                else if (checkoutStateInEP.IsPaymentClicked)
                {
                    return StatusCode(400, "A sales order cannot be edited while its payment is being processed.");
                }                
                checkoutEP.Value = System.Text.Json.JsonSerializer.Serialize(properCheckoutState);                
            }
            else
            {                
                salesOrder.EntityProperties.Add(new EntityProperty { Name = EntityPropertyName, Value = System.Text.Json.JsonSerializer.Serialize(properCheckoutState) });
            }

            // note: before updating the sales order with the data submitted by the user, we need to check again line by line
            // if any product is out of stock, because we can't trust the client-side validation
            Product product;
            List<StockTotalByProductWarehouse> stockLevels;
            decimal reservedStock;
            foreach (SalesOrderLine line in salesOrder.Lines)
            {
                if (line.BaseQty <= 0) return StatusCode(400, Resources.SharedResource.ResourceManager.GetString("ErrMsg_QuantityNotPositive"));
                
                product = await corprioClient.ProductApi.Get(organizationID: organizationID, id: line.ProductID);
                if (product == null) return NotFound(Resources.SharedResource.ResourceManager.GetString("ErrMsg_ProductNotFound"));
                
                if (product.Nature != ProductNature.Inventory) continue;

                stockLevels = await corprioClient.StockApi.GetCurrentStocks(organizationID: organizationID, productID: line.ProductID, warehouseID: applicationSetting.WarehouseID);
                if (!stockLevels.Any())
                {
                    return StatusCode(400, string.Format(Resources.SharedResource.ErrMsg_OutOfStock, product.Name));
                }
                
                reservedStock = await corprioClient.StockApi.GetReservedStock(organizationID: organizationID, productID: line.ProductID, warehouseID: applicationSetting.WarehouseID);
                if (stockLevels.First().BaseQty < reservedStock)  // note: it is ok if the two are equal
                {
                    return StatusCode(400, string.Format(Resources.SharedResource.ErrMsg_OutOfStock, product.Name));
                }
            }

            salesOrder.BillName = data.BillPerson.FormatName(await corprioClient.OrganizationSettingApi.GetPersonNameFormat(organizationID));
            salesOrder.BillPhoneNumbers ??= new List<PhoneNumber>();            
            if (!salesOrder.BillPhoneNumbers.Any(x => x.Equals(data.BillContactPhone)))
            {
                salesOrder.BillPhoneNumbers.Add(data.BillContactPhone);
            }
                        
            if (data.ChosenDeliveryMethod == DeliveryOption.SelfPickup && applicationSetting.SelfPickUp)
            {
                salesOrder.DeliveryAddress_Line1 = Resources.SharedResource.ResourceManager.GetString("SelfPickUp");
                salesOrder.DeliveryAddress_Line2 = null;
                salesOrder.DeliveryAddress_City = null;
                salesOrder.DeliveryAddress_State = null;
                salesOrder.DeliveryAddress_PostalCode = null;
                salesOrder.DeliveryAddress_CountryAlphaCode = null;
            }
            else if (data.ChosenDeliveryMethod == DeliveryOption.Shipping && applicationSetting.ShipToCustomer)
            {
                if (string.IsNullOrWhiteSpace(data.DeliveryAddress?.Line1)) throw new Exception("Delivery address cannot be blank.");                
                if (string.IsNullOrWhiteSpace(data.DeliveryAddress.CountryAlphaCode) || data.DeliveryAddress.CountryAlphaCode.Length != 2) throw new Exception("Country code must consist of two letters.");

                salesOrder.DeliveryContact_GivenName = data.DeliveryContact.GivenName;
                salesOrder.DeliveryContact_FamilyName = data.DeliveryContact.FamilyName;
                salesOrder.DeliveryAddress_Line1 = data.DeliveryAddress.Line1;
                salesOrder.DeliveryAddress_Line2 = data.DeliveryAddress.Line2;
                salesOrder.DeliveryAddress_City = data.DeliveryAddress.City;
                salesOrder.DeliveryAddress_State = data.DeliveryAddress.State;
                salesOrder.DeliveryAddress_PostalCode = data.DeliveryAddress.PostalCode;
                salesOrder.DeliveryAddress_CountryAlphaCode = data.DeliveryAddress.CountryAlphaCode;
                
                salesOrder.DeliveryPhoneNumbers ??= new List<PhoneNumber>();                
                if (!salesOrder.DeliveryPhoneNumbers.Any(x => x.Equals(data.DeliveryContactPhone)))
                {
                    salesOrder.DeliveryPhoneNumbers.Add(data.DeliveryContactPhone);
                }

                // note: if AND only if the customer's primary address appears empty, then we update it with the delivery address as well
                Customer customer = await corprioClient.CustomerApi.Get(organizationID: organizationID, id: salesOrder.CustomerID)
                    ?? throw new Exception("Customer of the sales order could not be found.");

                if (string.IsNullOrWhiteSpace(customer.BusinessPartner.PrimaryAddress_Line1))
                {
                    customer.BusinessPartner.PrimaryAddress_Line1 = data.DeliveryAddress.Line1;
                    customer.BusinessPartner.PrimaryAddress_Line2 = data.DeliveryAddress.Line2;
                    customer.BusinessPartner.PrimaryAddress_City = data.DeliveryAddress.City;
                    customer.BusinessPartner.PrimaryAddress_State = data.DeliveryAddress.State;
                    customer.BusinessPartner.PrimaryAddress_PostalCode = data.DeliveryAddress.PostalCode;
                    customer.BusinessPartner.PrimaryAddress_CountryAlphaCode = data.DeliveryAddress.CountryAlphaCode;
                }
                try
                {
                    await corprioClient.CustomerApi.Update(organizationID: organizationID, customer: customer);
                }
                catch (ApiExecutionException ex)
                {
                    // note: we don't throw error because updating the customer's address is not necessarily part of the transaction
                    Log.Error($"Failed to update the customer's primary address. {ex.Message}");
                }
            }
            else if (!applicationSetting.SelfPickUp && !applicationSetting.ShipToCustomer)
            {
                salesOrder.DeliveryAddress_Line1 = Resources.SharedResource.ResourceManager.GetString("DeliveryMethodTBD");
                salesOrder.DeliveryAddress_Line2 = null;
                salesOrder.DeliveryAddress_City = null;
                salesOrder.DeliveryAddress_State = null;
                salesOrder.DeliveryAddress_PostalCode = null;
                salesOrder.DeliveryAddress_CountryAlphaCode = null;
            }
            else
            {
                throw new Exception("Invalid delivery method was selected.");
            }

            try
            {
                // note: "updateLines" actually means "updateChildren", i.e., True means EP is also impacted
                await corprioClient.SalesOrderApi.Update(organizationID: organizationID, salesOrder: salesOrder, updateLines: true);
            }
            catch (ApiExecutionException ex)
            {
                throw new Exception($"The sales order could not be updated. Details: {ex.Message}");
            }
            
            // assumption: we include a line for delivery charge of $0 even when free shipping is provided
            // note: the order line is added after the sales order's EP is updated, lest this new line is removed
            AddSalesOrderLineModel deliveryChargeOrderLine = new()
            {
                ProductID = applicationSetting.DeliveryChargeProductID ?? throw new Exception("Delivery charge product ID is missing from application setting."),
                Qty = 1,
                SalesOrderID = data.SalesOrderID,
                UnitPrice = new Price
                {
                    Value = (applicationSetting.FreeShippingAmount == null || applicationSetting.FreeShippingAmount > salesOrder.Lines.Sum(x => x.NetUnitPrice * x.Qty))
                       ? applicationSetting.DeliveryCharge
                       : 0,
                }
            };
            try
            {                
                await corprioClient.SalesOrderApi.AddLine(organizationID: organizationID, line: deliveryChargeOrderLine);
            }
            catch (ApiExecutionException ex)
            {
                throw new Exception($"The sales order could not be updated. Details: {ex.Message}");
            }

            var paymentMethods = await corprioClient.CustomerPaymentMethodApi.GetList(organizationID, loadDataOptions: new LoadDataOptions { PageSize = 1 });
            if (!paymentMethods.Any()) return StatusCode(412, "The merchant has not set up payment method.");

            bool isSmtpSet = false;
            try
            {
                isSmtpSet = corprioClient.Execute<bool>(
                    request: new CorprioRestClient.ApiRequest($"/organization/{organizationID}/IsSMTPSet", System.Net.Http.HttpMethod.Get)).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (ApiExecutionException ex)
            {
                Log.Error($"Failed to test the latest SMTP setting of organization {organizationID}. {ex.Message}");
            }

            if (isSmtpSet)
            {
                try
                {
                    await SendEmailsAsync(corprio: corprioClient, organizationID: organizationID, orderID: data.SalesOrderID);
                }
                catch
                {
                    return StatusCode(500, "Error in sending sales order confirmation email.");
                }
            }

            return StatusCode(200);
        }

        /// <summary>
        /// Return a view to thank the customer once they complete payment
        /// </summary>
        /// <param name="httpClientFactory">HttpClientFactory for resolving the httpClient for client access</param>
        /// <param name="organizationID">Organization ID</param>
        /// <param name="trnID">Order ID</param>
        /// <param name="trnType">Transaction type. Supported values are order or invoice</param>
        /// <param name="amt">The received amount</param>
        /// <param name="payID">Payment ID returned by the payment provider, if any</param>
        /// <param name="payMeth">Name of the payment provider</param>
        /// <returns>View</returns>
        [AllowAnonymous]
        [HttpPost("/{organizationID:guid}/thankyou")]
        public async Task<IActionResult> ThankYou(
            [FromServices] IHttpClientFactory httpClientFactory,
            [FromRoute] Guid organizationID,
            [FromForm] string payMeth,
            [FromForm] string payID,
            [FromForm] string trnType,
            [FromForm] Guid trnID,
            [FromForm] Guid paytrnid,
            [FromForm] decimal amt,
            [FromForm] string hash
            )
        {
            //Cannot use ApiClient because this method is called by anonymous users
            var httpClient = httpClientFactory.CreateClient("appClient"); //create httpClient for client access authorization only
            APIClient corprio = new APIClient(httpClient);

            OrderConfirmationDataModel model;

            if (trnType == "order")
            {
                //validate hash
                string calculatedHash = await corprio.OrganizationApi.GetHash(organizationID, new KeyValuePair<string, string>[] {
                    new KeyValuePair<string, string>("paymeth", payMeth),
                    new KeyValuePair<string, string>("payid", payID),
                    new KeyValuePair<string, string>("trntype", trnType),
                    new KeyValuePair<string, string>("trnid", trnID.ToString()),
                    new KeyValuePair<string, string>("paytrnid", paytrnid.ToString()),
                    new KeyValuePair<string, string>("amt", amt.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture))
                });
                if (calculatedHash != hash)
                {
                    //hash not match
                    Log.Error($"Unsupported transaction type {trnType} is received");
                    model = new OrderConfirmationDataModel { Message = "Invalid hash" };
                }
                else
                {
                    //get order record to show details to the customer
                    model = await corprio.SalesOrderApi.GetResult<OrderConfirmationDataModel>(organizationID: organizationID,
                        id: trnID,
                        selector: $@"new(ID as SalesOrderID,OrderDate,DocumentNum,CurrencyCode,Amount,DeliveryAddress_Line1,DeliveryAddress_Line2,DeliveryAddress_City,DeliveryAddress_State,DeliveryAddress_PostalCode,DeliveryAddress_CountryAlphaCode,
                            DeliveryContact_DisplayName,DeliveryPhoneNumbers,BillName,BillEmail,BillPhoneNumbers,Remark,
                            Lines.OrderBy(SortOrder).Select(new (ProductID,Product.Code,Product.Name,Remark,Qty,UnitPrice,NetUnitPrice,UOMCode,Product.ListPrice_QtyDecimals as SalesUOMDecimals,DiscountType,DiscountValue,Product.Image01.UrlKey as Image01UrlKey)) as Products
                            )");
                    foreach (var p in model.Products)
                    {
                        p.ImageUrls[0] = corprio.ImageApi.Url(organizationID, p.Image01UrlKey, ImageSize.Normal);

                    }
                    
                    ApplicationSetting settings = await applicationSettingService.GetSetting<ApplicationSetting>(organizationID);
                    model.ThankYouMessage = settings?.ThankYouMessage;
                    model.LogoImageUrl = await corprio.OrganizationApi.GetApplicationImageUrlByKey(organizationID, Common.LogoImageKey, ImageSize.Original);
                }
            }
            else
            {
                Log.Error($"Unsupported transaction type {trnType} is received");
                model = new OrderConfirmationDataModel { Message = "Invalid transaction type" };
            }
            return View(model);
        }
    }    
}

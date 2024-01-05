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
using Corprio.CorprioRestClient;
using DevExpress.RichEdit.Export;
using Corprio.DataModel.Business;
using Corprio.Global.Geography;
using DevExpress.Charts.Model;
using Corprio.SocialWorker.Helpers;
using Corprio.DataModel.Business.Sales.ViewModel;

namespace Corprio.SocialWorker.Controllers
{
    public class CheckoutController : AspNetCore.XtraReportSite.Controllers.BaseController
    {
        private readonly ApplicationDbContext db;
        readonly IConfiguration configuration;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>        
        public CheckoutController(ApplicationDbContext context, IConfiguration configuration) : base()
        {
            db = context;
            this.configuration = configuration;
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
        public async Task<IActionResult> Checkout([FromServices] IHttpClientFactory httpClientFactory, [FromRoute] Guid organizationID, 
            [FromRoute] Guid salesOrderID, [FromQuery] string ui)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("appClient");
            var corprioClient = new APIClient(httpClient);

            SalesOrder salesOrder = await corprioClient.SalesOrderApi.Get(organizationID: organizationID, id: salesOrderID);
            if (salesOrder == null) return NotFound(Resources.SharedResource.ResourceManager.GetString("ErrMsg_SalesOrderNotFound"));

            Customer customer = await corprioClient.CustomerApi.Get(organizationID: organizationID, id: salesOrder.CustomerID);
            if (customer == null) return NotFound(Resources.SharedResource.ResourceManager.GetString("ErrMsg_CustomerNotFound"));

            ApplicationSetting applicationSetting = db.Settings.FirstOrDefault(x => x.OrganizationID == organizationID);
            if (applicationSetting == null) return NotFound(Resources.SharedResource.ResourceManager.GetString("ErrMsg_AppSettingNotFound"));

            OrganizationCoreInfo coreInfo = await corprioClient.OrganizationApi.GetCoreInfo(organizationID);
            if (coreInfo == null) return NotFound(Resources.SharedResource.ResourceManager.GetString("ErrMsg_OrganizationInfoNotFound"));

            CheckoutViewModel checkoutView = new()
            {
                BillPerson = new Global.Person.Person 
                { 
                    GivenName = customer.BusinessPartner.GivenName,
                    FamilyName = customer.BusinessPartner.FamilyName,
                },
                CurrencyCode = salesOrder.CurrencyCode,
                DefaultCountryCode = coreInfo.CountryCode,
                DocumentNum = salesOrder.DocumentNum,
                Language = string.IsNullOrWhiteSpace(ui) ? System.Threading.Thread.CurrentThread.CurrentCulture.Name : ui,
                Lines = new List<OrderLine>(),
                OrderDate = salesOrder.OrderDate,
                OrganizationID = organizationID,
                OrganizationShortName = coreInfo.ShortName,
                OrganizationEmailAddress = coreInfo.EmailAddress.Address,
                SalesOrderID = salesOrderID,
                AllowSelfPickUp = applicationSetting.SelfPickUp,
                SelfPickUpInstruction = applicationSetting.SelfPickUpInstruction,
                ProvideDelivery = applicationSetting.ShipToCustomer,
                DeliveryCharge = applicationSetting.DeliveryCharge,
                FreeShippingAmount = applicationSetting.FreeShippingAmount,
            };            

            if (!string.IsNullOrWhiteSpace(customer.BusinessPartner.PrimaryMobilePhoneNumber_SubscriberNumber))
            {
                checkoutView.ContactPhone.NumberType = Global.PhoneNumberType.Mobile;
                checkoutView.ContactPhone.SubscriberNumber = customer.BusinessPartner.PrimaryMobilePhoneNumber_SubscriberNumber;
                checkoutView.ContactPhone.NationalDestinationCode = customer.BusinessPartner.PrimaryMobilePhoneNumber_NationalDestinationCode;
                checkoutView.ContactPhone.CountryCallingCode = customer.BusinessPartner.PrimaryMobilePhoneNumber_CountryCallingCode;                
            }

            // note: the customer may have provided a delivery address different from their primary address (or chosen self-pickup),
            // so we should use the former if it isn't blank
            if (!string.IsNullOrWhiteSpace(salesOrder.DeliveryAddress_Line1))
            {
                checkoutView.DeliveryAddress_Line1 = salesOrder.DeliveryAddress_Line1;
                checkoutView.DeliveryAddress_Line2 = salesOrder.DeliveryAddress_Line2;
                checkoutView.DeliveryAddress_City = salesOrder.DeliveryAddress_City;
                checkoutView.DeliveryAddress_State = salesOrder.DeliveryAddress_State;
                checkoutView.DeliveryAddress_PostalCode = salesOrder.DeliveryAddress_PostalCode;
                checkoutView.DeliveryAddress_CountryAlphaCode = salesOrder.DeliveryAddress_CountryAlphaCode;
            }
            else
            {
                checkoutView.DeliveryAddress_Line1 = customer.BusinessPartner.PrimaryAddress_Line1;
                checkoutView.DeliveryAddress_Line2 = customer.BusinessPartner.PrimaryAddress_Line2;
                checkoutView.DeliveryAddress_City = customer.BusinessPartner.PrimaryAddress_City;
                checkoutView.DeliveryAddress_State = customer.BusinessPartner.PrimaryAddress_State;
                checkoutView.DeliveryAddress_PostalCode = customer.BusinessPartner.PrimaryAddress_PostalCode;
                checkoutView.DeliveryAddress_CountryAlphaCode = customer.BusinessPartner.PrimaryAddress_CountryAlphaCode;
            }
            
            Product product;
            OrderLine orderLine;
            string imageUrlKey;
            salesOrder.Lines = salesOrder.Lines.OrderBy(x => x.SortOrder).ToList();
            foreach (var line in salesOrder.Lines)
            {
                product = await corprioClient.ProductApi.Get(organizationID: organizationID, id: line.ProductID);
                if (product == null) return NotFound(Resources.SharedResource.ResourceManager.GetString("ErrMsg_ProductNotFound"));

                orderLine = new OrderLine
                {
                    SalesOrderLineID = line.ID,
                    ProductName = product.Name,
                    ProductDesc = product.Description,
                    NetUnitPrice = line.NetUnitPrice,
                    Quantity = line.Qty,
                    UOMCode = line.UOMCode,
                };
                                
                HashSet<Guid> imageIDs = UtilityHelper.ReturnImageUrls(product);
                if (imageIDs.Count > 0)
                {
                    // assumption: we don't care which image of the product is used
                    imageUrlKey = await corprioClient.ImageApi.UrlKey(organizationID: organizationID, id: imageIDs.Single());
                    orderLine.URL = corprioClient.ImageApi.Url(organizationID: organizationID, imageUrlKey: imageUrlKey);
                }

                checkoutView.Lines.Add(orderLine);
            }

            return View(viewName: "Index", model: checkoutView);
        }

        [AllowAnonymous]
        [OrganizationNeeded(false)]
        public async Task DeleteSalesOrderLine([FromServices] IHttpClientFactory httpClientFactory, [FromRoute] Guid organizationID, 
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
        }

        [AllowAnonymous]
        [OrganizationNeeded(false)]
        public async Task EditSalesOrderLine([FromServices] IHttpClientFactory httpClientFactory, [FromRoute] Guid organizationID,
            Guid salesOrderID, Guid salesOrderLineID, decimal quantity)
        {
            if (quantity <= 0) throw new Exception("Quantity must be a positive number.");

            HttpClient httpClient = httpClientFactory.CreateClient("appClient");
            var corprioClient = new APIClient(httpClient);

            // note: we must validate if the sales order line being deleted really belongs to the sales order used in the view (which cannot be manipulated by the user),
            // because it is possible that the user has changed the sales order line ID by editing the HTML
            SalesOrderLine salesOrderLine = await corprioClient.SalesOrderApi.GetLine(organizationID: organizationID, salesOrderLineID: salesOrderLineID);
            if (salesOrderLine == null || salesOrderLine.SalesOrderID != salesOrderID) throw new Exception("The sales order line ID is invalid");

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
        }

        /// <summary>
        /// Generate the body of the email about successful order
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

            ApplicationSetting settings = db.Settings.FirstOrDefault(x => x.OrganizationID == organizationID) 
                ?? throw new Exception(Resources.SharedResource.ResourceManager.GetString("ErrMsg_AppSettingNotFound"));

            model.ThankYouMessage = "<h3>Thank you for your order</h3>";
            OrganizationCoreInfo coreInfo = await corprio.OrganizationApi.GetCoreInfo(organizationID);            
            model.LogoImageUrl = coreInfo?.LogoImageUrl;

            string paymentAppUrl = configuration["PaymentAppUrl"].ToString();
            string orgShortName = await corprio.OrganizationApi.GetShortName(organizationID);
            string AppBaseUrl = configuration["AppBaseUrl"].ToString();
            string paymentLink = paymentAppUrl + "/" + orgShortName + "/RecePayment/order/" + orderID + "?successUrl=" + AppBaseUrl + "/" + organizationID + "/thankyou&failUrl=" + AppBaseUrl + "/" + organizationID + "/paymentfailed";
            model.PaymentLink = paymentLink;

            string emailBody = OrderReceivedEmailBody(model);
            
            if (settings.SendConfirmationEmail)
            {
                try
                {
                    await corprio.OrganizationApi.SendEmail(
                        organizationID: organizationID,
                        emailMessage: new Core.EmailMessage()
                        {
                            ToEmails = new Core.EmailAddress[] { new Core.EmailAddress(address: model.BillEmail, name: model.BillName) },
                            Subject = settings.DefaultEmailSubject,
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
                                ToEmails = settings.EmailToReceiveOrder.Split(',', ';').Select(s => new Core.EmailAddress(s)).ToArray(),
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
                    ToEmails = settings.EmailToReceiveOrder.Split(',', ';').Select(s => new Core.EmailAddress(s)).ToArray(),
                    Subject = "[" + Resources.SharedResource.AppName + "] " + Resources.SharedResource.YouReceivedNewOrder + " " + model.DocumentNum,
                    Body = emailBody
                }
            );
        }

        [AllowAnonymous]
        [OrganizationNeeded(false)]
        public async Task<IActionResult> FinalizeSalesOrder([FromServices] IHttpClientFactory httpClientFactory, [FromRoute] Guid organizationID,
            Guid salesOrderID, string addressString, string deliveryPreference)
        {
            ApplicationSetting applicationSetting = db.Settings.FirstOrDefault(x => x.OrganizationID == organizationID);
            if (applicationSetting == null) return NotFound(Resources.SharedResource.ResourceManager.GetString("ErrMsg_AppSettingNotFound"));
                        
            HttpClient httpClient = httpClientFactory.CreateClient("appClient");
            var corprioClient = new APIClient(httpClient);

            SalesOrder salesOrder = await corprioClient.SalesOrderApi.Get(organizationID: organizationID, id: salesOrderID) 
                ?? throw new Exception("The sales order could not be found.");

            if (applicationSetting.SelfPickUp && (!applicationSetting.ShipToCustomer || deliveryPreference == "1"))
            {
                salesOrder.DeliveryAddress_Line1 = Resources.SharedResource.ResourceManager.GetString("SelfPickUp");
                salesOrder.DeliveryAddress_Line2 = null;
                salesOrder.DeliveryAddress_City = null;
                salesOrder.DeliveryAddress_State = null;
                salesOrder.DeliveryAddress_PostalCode = null;
                salesOrder.DeliveryAddress_CountryAlphaCode = null;
            }
            else if (applicationSetting.ShipToCustomer && (!applicationSetting.SelfPickUp || deliveryPreference == "2"))
            {
                if (string.IsNullOrWhiteSpace(addressString)) throw new Exception("Delivery address cannot be blank.");
                string[] address = addressString.Split(CheckoutConstants.Separator);
                if (address.Length != 6) throw new Exception("Incomplete delivery address.");
                if (string.IsNullOrWhiteSpace(address[0])) throw new Exception("Line 1 cannot be blank.");
                if (!string.IsNullOrWhiteSpace(address[5]) && address[5].Length != 2) throw new Exception("Country code must consist of two letters.");

                salesOrder.DeliveryAddress_Line1 = address[0];
                salesOrder.DeliveryAddress_Line2 = address[1];
                salesOrder.DeliveryAddress_City = address[2];
                salesOrder.DeliveryAddress_State = address[3];
                salesOrder.DeliveryAddress_PostalCode = address[4];
                salesOrder.DeliveryAddress_CountryAlphaCode = address[5];

                // note: if the customer's primary address appears empty, then we update it with the delivery address as well
                Customer customer = await corprioClient.CustomerApi.Get(organizationID: organizationID, id: salesOrder.CustomerID)
                    ?? throw new Exception("Customer of the sales order could not be found.");                
                if (string.IsNullOrWhiteSpace(customer.BusinessPartner.PrimaryAddress_Line1) && string.IsNullOrWhiteSpace(customer.BusinessPartner.PrimaryAddress_Line2))
                {
                    customer.BusinessPartner.PrimaryAddress_Line1 = address[0];
                    customer.BusinessPartner.PrimaryAddress_Line2 = address[1];
                    customer.BusinessPartner.PrimaryAddress_City = address[2];
                    customer.BusinessPartner.PrimaryAddress_State = address[3];
                    customer.BusinessPartner.PrimaryAddress_PostalCode = address[4];
                    customer.BusinessPartner.PrimaryAddress_CountryAlphaCode = address[5];
                }

                // assumption: we include a line for delivery charge of $0 even when free shipping is provided
                AddSalesOrderLineModel deliveryChargeOrderLine = new()
                {
                    ProductID = applicationSetting.DeliveryChargeProductID ?? throw new Exception("Delivery charge product ID is missing from application setting."),
                    Qty = 1,
                    SalesOrderID = salesOrderID,
                    UnitPrice = new Price
                    {
                       Value = (applicationSetting.FreeShippingAmount == null || applicationSetting.FreeShippingAmount > salesOrder.Lines.Sum(x => x.NetUnitPrice * x.Qty)) 
                           ? applicationSetting.DeliveryCharge 
                           : 0,
                    }
                };
                await corprioClient.SalesOrderApi.AddLine(organizationID: organizationID, line: deliveryChargeOrderLine);

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
                await corprioClient.SalesOrderApi.Update(organizationID: organizationID, salesOrder: salesOrder, updateLines: false);
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
                    await SendEmailsAsync(corprio: corprioClient, organizationID: organizationID, orderID: salesOrderID);
                }
                catch
                {
                    return StatusCode(500, "Error in sending sales order confirmation email.");
                }
            }
            
            return StatusCode(200);
        }
    }    
}

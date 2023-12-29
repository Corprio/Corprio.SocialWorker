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

namespace Corprio.SocialWorker.Controllers
{
    public class CheckoutController : AspNetCore.XtraReportSite.Controllers.BaseController
    {
        /// <summary>
        /// Returns view for customers to perform checkout
        /// </summary>
        /// <param name="httpClientFactory">HttpClientFactory for resolving the httpClient for client access</param>
        /// <param name="organizationID">Organization ID</param>
        /// <param name="salesOrderID">Sales order ID</param>
        /// <param name="ui">Culture name for user interface in the format of languagecode2-country/regioncode2</param>
        /// <returns></returns>
        [AllowAnonymous]
        [OrganizationNeeded(false)]
        [HttpGet("/{organizationID:guid}/checkout/{salesOrderID:guid}")]
        public async Task<IActionResult> Checkout([FromServices] IHttpClientFactory httpClientFactory, [FromRoute] Guid organizationID, 
            [FromRoute] Guid salesOrderID, [FromQuery] string ui)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("appClient");
            var corprioClient = new APIClient(httpClient);

            SalesOrder salesOrder = await corprioClient.SalesOrderApi.Get(organizationID: organizationID, id: salesOrderID);
            if (salesOrder == null) return NotFound(Resources.SharedResource.ResourceManager.GetString("ErrMsg_SalesOrderID"));

            Customer customer = await corprioClient.CustomerApi.Get(organizationID: organizationID, id: salesOrder.CustomerID);
            if (customer == null) return NotFound(Resources.SharedResource.ResourceManager.GetString("ErrMsg_CustomerNotFound"));

            CheckoutViewModel checkoutView = new()
            {                
                DocumentNum = salesOrder.DocumentNum,
                Language = string.IsNullOrWhiteSpace(ui) ? System.Threading.Thread.CurrentThread.CurrentCulture.Name : ui,
                Lines = new List<OrderLine>(),
                OrderDate = salesOrder.OrderDate,
                OrganizationID = organizationID,
                SalesOrderID = salesOrderID
            };

            if (!string.IsNullOrWhiteSpace(salesOrder.DeliveryAddress_Line1) || !string.IsNullOrWhiteSpace(salesOrder.DeliveryAddress_Line2))
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
            foreach (var line in salesOrder.Lines)
            {
                product = await corprioClient.ProductApi.Get(organizationID: organizationID, id: line.ProductID);
                if (product == null) return NotFound(Resources.SharedResource.ResourceManager.GetString("ErrMsg_ProductNotFound"));

                checkoutView.Lines.Add(new OrderLine
                {
                    SalesOrderLineID = line.ID,
                    ProductName = product.Name,
                    Price = line.UnitPrice,
                    Quantity = line.Qty,
                    UOMCode = line.UOMCode,
                });
            }

            return View(viewName: "Index", model: checkoutView);
        }
        
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

        public async Task UpdateDeliveryAddress([FromServices] IHttpClientFactory httpClientFactory, [FromRoute] Guid organizationID,
            Guid salesOrderID, string addressString)
        {
            if (string.IsNullOrWhiteSpace(addressString)) throw new Exception("Delivery address cannot be blank.");            
            string[] address = addressString.Split(CheckoutConstants.Separator);
            if (address.Length != 6) throw new Exception("Incomplete delivery address.");
            if (string.IsNullOrWhiteSpace(address[0])) throw new Exception("Line 1 cannot be blank.");
            if (!string.IsNullOrWhiteSpace(address[5]) && address[5].Length != 2) throw new Exception("Country code must consist of two letters.");

            HttpClient httpClient = httpClientFactory.CreateClient("appClient");
            var corprioClient = new APIClient(httpClient);

            SalesOrder salesOrder = await corprioClient.SalesOrderApi.Get(organizationID: organizationID, id: salesOrderID) 
                ?? throw new Exception("The sales order could not be found.");
            salesOrder.DeliveryAddress_Line1 = address[0];
            salesOrder.DeliveryAddress_Line2 = address[1];
            salesOrder.DeliveryAddress_City = address[2];
            salesOrder.DeliveryAddress_State = address[3];
            salesOrder.DeliveryAddress_PostalCode = address[4];
            salesOrder.DeliveryAddress_CountryAlphaCode = address[5];

            try
            {
                await corprioClient.SalesOrderApi.Update(organizationID: organizationID, salesOrder: salesOrder, updateLines: false);
            }
            catch (ApiExecutionException ex)
            {
                throw new Exception($"The sales order could not be updated. Details: {ex.Message}");
            }

            Customer customer = await corprioClient.CustomerApi.Get(organizationID: organizationID, id: salesOrder.CustomerID) 
                ?? throw new Exception("Customer of the sales order could not be found.");

            // note: if the customer's primary address appears empty, then we update it with the delivery address as well
            if (string.IsNullOrWhiteSpace(customer.BusinessPartner.PrimaryAddress_Line1) && string.IsNullOrWhiteSpace(customer.BusinessPartner.PrimaryAddress_Line2))
            {
                customer.BusinessPartner.PrimaryAddress_Line1 = address[0];
                customer.BusinessPartner.PrimaryAddress_Line2 = address[1];
                customer.BusinessPartner.PrimaryAddress_City = address[2];
                customer.BusinessPartner.PrimaryAddress_State = address[3];
                customer.BusinessPartner.PrimaryAddress_PostalCode = address[4];
                customer.BusinessPartner.PrimaryAddress_CountryAlphaCode = address[5];
            }

            try
            {
                await corprioClient.CustomerApi.Update(organizationID: organizationID, customer: customer);
            }
            catch (ApiExecutionException ex)
            {
                // note: we don't throw error because updating the customer's address is not necessarily part of the transaction
                Log.Error($"The delivery address of sales order could be updated but the customer's primary address couldn't. {ex.Message}");
            }
        }
    }    
}

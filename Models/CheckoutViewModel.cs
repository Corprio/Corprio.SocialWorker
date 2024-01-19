using Corprio.Global.Geography;
using Corprio.Global.Person;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Corprio.SocialWorker.Models
{                
    /// <summary>
    /// Data that the buyer is required to submit before proceeding to payment
    /// </summary>
    public class CheckoutDataModel
    {        
        [Required]
        public Person BillPerson { get; set; }

        [Required]
        public PhoneNumber BillContactPhone { get; set; }

        [Required]
        public DeliveryOption ChosenDeliveryMethod { get; set; } = DeliveryOption.NoOption;

        public Address DeliveryAddress { get; set; }

        public Person DeliveryContact { get; set; }                
        
        public PhoneNumber DeliveryContactPhone { get; set; }

        public string Footer { get; set; }

        /// <summary>
        /// ID of the sales order whose checkout is being processed.
        /// </summary>
        public Guid SalesOrderID { get; set; }        
    }
    
    /// <summary>
    /// Model for rendering the checkout view
    /// </summary>
    public class CheckoutViewModel: CheckoutDataModel
    {
        /// <summary>
        /// True if the merchant allows self pick-up.
        /// </summary>
        public bool AllowSelfPickUp { get; set; }

        /// <summary>
        /// The currency code used in rendering prices and (sub)totals.
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// The country code that is used to determine the phone number's default calling code
        /// </summary>
        public string DefaultCountryCode { get; set; }

        /// <summary>
        /// Delivery charge defined in application setting.
        /// </summary>
        public decimal DeliveryCharge { get; set; }

        /// <summary>
        /// The sales order number for user's reference.
        /// </summary>
        public string DocumentNum { get; set; }

        /// <summary>
        /// Threshold of purchase value that triggers free shipping; null if the merchant does not offer free shipping.
        /// </summary>
        public decimal? FreeShippingAmount { get; set; }

        /// <summary>
        /// True if the order is voided or paid (i.e., the view should disallow voiding the order)
        /// </summary>
        public bool IsOrderVoidOrPaid { get; set; }

        /// <summary>
        /// True if the customer has proceeded to payment
        /// </summary>
        public bool IsPaymentClicked { get; set; } = false;

        /// <summary>
        /// The page's language.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Information for the sales order lines, and their associated products, of this sales order
        /// </summary>
        public List<OrderLine> Lines { get; set; }

        /// <summary>
        /// The aforementioned lines in string format, for frontend's JavaScript to consume
        /// </summary>
        public string OrderLineJsonString { get; set; }

        /// <summary>
        /// The sales order date for user's reference.
        /// </summary>
        public DateTimeOffset OrderDate { get; set; }
                
        /// <summary>
        /// ID of the merchant on whose behalf the checkout is being processed.
        /// </summary>
        public Guid OrganizationID { get; set; }

        /// <summary>
        /// The merchant's email address.
        /// </summary>
        public string OrganizationEmailAddress { get; set; }

        /// <summary>
        /// Name of the merchant.
        /// </summary>
        public string OrganizationShortName { get; set; }

        /// <summary>
        /// True if the merchant provides delivery.
        /// </summary>
        public bool ProvideDelivery { get; set; }
                
        /// <summary>
        /// Self pickup instruction defined in application setting, which can be blank.
        /// </summary>
        public string SelfPickUpInstruction { get; set; }

        /// <summary>
        /// True if the view is rendered for preview only and therefore the buttons shouldn't work
        /// </summary>
        public bool IsPreview { get; set; }
    }
    
    public class OrderLine
    {
        /// <summary>
        /// Selected information about the product's (peer) child products, excluding those that are disabled and/or out of stock
        /// </summary>
        public List<ChildProductInfo> ChildProductInfo { get; set; }
        
        /// <summary>
        /// True if the product cannot be sold if there is insufficient stock
        /// </summary>
        public bool DisallowOutOfStock { get; set; }

        /// <summary>
        /// Net unit price of the product included in this sales order line
        /// </summary>
        public decimal NetUnitPrice { get; set; }

        /// <summary>
        /// Description of the product included in this sales order line
        /// </summary>
        public string ProductDesc { get; set; }

        /// <summary>
        /// ID of the product included in this sales order line
        /// </summary>
        public Guid ProductID { get; set; }

        /// <summary>
        /// Name of the product included in this sales order line
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// Stock level (excluding reserved stock) of the product included in this sales order line
        /// </summary>
        public decimal ProductStockLevel { get; set; }

        /// <summary>
        /// Quantity of the product included in this sales order line
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// ID of the sales order line corresponding to this line
        /// </summary>
        public Guid SalesOrderLineID { get; set; }

        /// <summary>
        /// UOM code of the product included in this sales order line
        /// </summary>
        public string UOMCode { get; set; }

        /// <summary>
        /// Image URL of the product included in this sales order line
        /// </summary>
        public string URL { get; set; }
    }

    public class ChildProductInfo
    {
        /// <summary>
        /// Product ID
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// Selected information about the product's child product attributes
        /// </summary>
        public List<ProductVariationInfo> ChildProductAttributes { get; set; }

        /// <summary>
        /// True if the product cannot be sold if there is insufficient stock
        /// </summary>
        public bool DisallowOutOfStock { get; set; }

        /// <summary>
        /// Stock level (excluding reserved stock) of the product
        /// </summary>
        public decimal ProductStockLevel { get; set; }
    }

    /// <summary>
    /// Selected information of product variation
    /// </summary>
    public class ProductVariationInfo
    {
        /// <summary>
        /// For example: size.
        /// </summary>
        public string Attribute { get; set; }

        /// <summary>
        /// For example: large.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// For example: L.
        /// </summary>
        public string Code { get; set; }
    }

    /// <summary>
    /// Delivery methods
    /// IMPORTANT: the order of the following options must be in line with that defined on the client side
    /// </summary>
    public enum DeliveryOption
    {
        NoOption,  // it is possible that the merchant provides no delivery methods for the buyer to select
        SelfPickup,
        Shipping
    }
}

using Corprio.Global.Geography;
using Corprio.Global.Person;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Corprio.SocialWorker.Models
{
    public class CheckoutViewModel
    {
        /// <summary>
        /// The currency code used in rendering prices and (sub)totals.
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// The sales order number for user's reference.
        /// </summary>
        public string DocumentNum { get; set; }

        /// <summary>
        /// The sales order date for user's reference.
        /// </summary>
        public DateTimeOffset OrderDate { get; set; }

        /// <summary>
        /// ID of the sales order whose checkout is being processed.
        /// </summary>
        public Guid SalesOrderID { get; set; }

        /// <summary>
        /// ID of the merchant on whose behalf the checkout is being processed.
        /// </summary>
        public Guid OrganizationID { get; set; }

        /// <summary>
        /// Name of the merchant.
        /// </summary>
        public string OrganizationShortName { get; set; }

        /// <summary>
        /// The merchant's email address.
        /// </summary>
        public string OrganizationEmailAddress { get; set; }

        public Person BillPerson { get; set; }

        [Required]
        public PhoneNumber ContactPhone { get; set; }

        /// <summary>
        /// The country code that is shown in phone number's text box by default
        /// </summary>
        public string DefaultCountryCode { get; set; }

        public string DeliveryAddress_Line1 { get; set; }
        
        public string DeliveryAddress_Line2 { get; set; }

        public string DeliveryAddress_City { get; set; }

        public string DeliveryAddress_State { get; set; }

        public string DeliveryAddress_PostalCode { get; set; }
        
        public string DeliveryAddress_CountryAlphaCode { get; set; }

        /// <summary>
        /// The page's language.
        /// </summary>
        public string Language { get; set; }

        public List<OrderLine> Lines { get; set; }

        /// <summary>
        /// True if the merchant allows self pick-up.
        /// </summary>
        public bool AllowSelfPickUp { get; set; }

        /// <summary>
        /// Self pickup instruction defined in application setting, which can be blank.
        /// </summary>
        public string SelfPickUpInstruction { get; set; }

        /// <summary>
        /// True if the merchant provides delivery.
        /// </summary>
        public bool ProvideDelivery { get; set; }

        /// <summary>
        /// Delivery charge defined in application setting.
        /// </summary>
        public decimal DeliveryCharge { get; set; }

        /// <summary>
        /// Threshold of purchase value that triggers free shipping; null if the merchant does not offer free shipping.
        /// </summary>
        public decimal? FreeShippingAmount { get; set; }
    }

    public class OrderLine
    {
        public Guid SalesOrderLineID { get; set; }

        public string ProductName { get; set; }

        public string ProductDesc { get; set; }

        public decimal NetUnitPrice { get; set; }

        public decimal Quantity { get; set; }

        public string UOMCode { get; set; }

        public string URL { get; set; }
    }
    
    public class CheckoutConstants
    {
        public const string Separator = "%;sep;%";
    }    
}

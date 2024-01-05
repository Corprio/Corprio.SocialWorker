using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Corprio.Global.Geography;

namespace Corprio.SocialWorker.Models
{
    /// <summary>
    /// View model for thank you page
    /// </summary>
    public class OrderConfirmationDataModel
    {
        /// <summary>
        /// Error message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Sales order ID
        /// </summary>
        public Guid SalesOrderID { get; set; }

        /// <summary>
        /// Date of the sales order
        /// </summary>
        public DateTimeOffset OrderDate { get; set; }

        /// <summary>
        /// Sales order number
        /// </summary>
        public string DocumentNum { get; set; }

        public string CurrencyCode { get; set; }

        public decimal Amount { get; set; }

        public string DeliveryAddress_Line1 { get; set; }

        public string DeliveryAddress_Line2 { get; set; }

        public string DeliveryAddress_City { get; set; }
        public string DeliveryAddress_State { get; set; }
        public string DeliveryAddress_PostalCode { get; set; }

        public string DeliveryAddress_CountryAlphaCode { get; set; }

        public Address DeliveryAddress => new()
        {
            City = DeliveryAddress_City,
            CountryAlphaCode = DeliveryAddress_CountryAlphaCode,
            Line1 = DeliveryAddress_Line1,
            Line2 = DeliveryAddress_Line2,
            PostalCode = DeliveryAddress_PostalCode,
            State = DeliveryAddress_State
        };

        public string DeliveryContact_DisplayName { get; set; }

        public List<PhoneNumber> DeliveryPhoneNumbers { get; set; } = new();

        public string BillName { get; set; }

        public string BillEmail { get; set; }

        public List<PhoneNumber> BillPhoneNumbers { get; set; } = new();

        public List<ProductViewModel> Products = new();

        public string Remark { get; set; }                        

        public string ThankYouMessage { get; set; }

        public string LogoImageUrl { get; set; }

        public string PaymentLink { get; set; }

    }    
}

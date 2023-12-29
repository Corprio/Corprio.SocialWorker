using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Corprio.SocialWorker.Models
{
    public class CheckoutViewModel
    {
        public string DocumentNum { get; set; }

        public DateTimeOffset OrderDate { get; set; }

        public Guid SalesOrderID { get; set; }

        public Guid OrganizationID { get; set; }
        
        public string DeliveryAddress_Line1 { get; set; }
        
        public string DeliveryAddress_Line2 { get; set; }

        public string DeliveryAddress_City { get; set; }

        public string DeliveryAddress_State { get; set; }

        public string DeliveryAddress_PostalCode { get; set; }
        
        public string DeliveryAddress_CountryAlphaCode { get; set; }

        public string Language { get; set; }

        public List<OrderLine> Lines { get; set; }        
    }

    public class OrderLine
    {
        public Guid SalesOrderLineID { get; set; }

        public string ProductName { get; set; }

        public decimal Price { get; set; }

        public decimal Quantity { get; set; }

        public string UOMCode { get; set; }
    }
    
    public class CheckoutConstants
    {
        public const string Separator = "%;sep;%";
    }
}

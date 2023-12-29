using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Corprio.SocialWorker.Models
{
    public class ApplicationSetting
    {
        public Guid Id { get; set; }

        /// <summary>
        /// ID of Facebook user in Corprio
        /// </summary>
        public Guid MetaUserID { get; set; }
        
        /// <summary>
        /// Whether an email will be sent to the customer to confirm his order
        /// </summary>
        [Display(Name = "SendConfirmationEmail", Description = "SendConfirmationEmail_Description", ResourceType = typeof(Resources.SharedResource))]
        public bool SendConfirmationEmail { get; set; } = false;

        /// <summary>
        /// Default subject of email
        /// </summary>
        [Display(Name = "DefaultEmailSubject", Description = "DefaultEmailSubject_Description", ResourceType = typeof(Resources.SharedResource))]
        [StringLength(200)]
        public string DefaultEmailSubject { get; set; } = "Thank you for your order";

        public bool IsSmtpSet { get; set; }

        /// <summary>
        /// Email to receive order email
        /// </summary>
        [Display(Name = "EmailToReceiveOrder", Description = "EmailToReceiveOrder_Description", ResourceType = typeof(Resources.SharedResource))]
        [MaxLength(200)]
        [RegularExpression(@"^([a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+)(?:[,;]{1,1}([a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+))*$", ErrorMessageResourceName = "ErrMailsToReceiveOrder", ErrorMessageResourceType = typeof(Resources.SharedResource))]
        public string EmailToReceiveOrder { get; set; }

        /// <summary>
        /// Default warehouse for sales order
        /// </summary>
        [Display(Name = "WarehouseID", Description = "WarehouseID_Description", ResourceType = typeof(Resources.SharedResource))]
        [Required]
        public Guid? WarehouseID { get; set; }

        /// <summary>
        /// Set to true if the merchant allows customer to self pickup the products
        /// </summary>
        [Display(Name = "SelfPickUp", Description = "SelfPickUp_Desc", ResourceType = typeof(Resources.SharedResource))]
        public bool SelfPickUp { get; set; }

        /// <summary>
        /// Instructions to be shown to customer for how to self pickup the products
        /// </summary>
        [Display(Name = "SelfPickUpInstruction", Description = "SelfPickUpInstruction_Desc", ResourceType = typeof(Resources.SharedResource))]
        public string SelfPickUpInstruction { get; set; }

        /// <summary>
        /// Set to true if the merchant is shipping to customers
        /// </summary>
        [Display(Name = "ShipToCustomer", Description = "ShipToCustomer_Desc", ResourceType = typeof(Resources.SharedResource))]
        public bool ShipToCustomer { get; set; }

        /// <summary>
        /// Default delivery charge for catalogue
        /// </summary>
        [Display(Name = "DefaultDeliveryCharge", Description = "DefaultDeliveryCharge_Description", ResourceType = typeof(Resources.SharedResource))]
        [Range(0, 99999)]
        public decimal DeliveryCharge { get; set; }

        /// <summary>
        /// Product ID representing the delivery charge.  Required when DeliveryCharge > 0
        /// </summary>
        [Display(Name = "DeliveryChargeProductID", Description = "DeliveryChargeProductID_Description", ResourceType = typeof(Resources.SharedResource))]
        public Guid? DeliveryChargeProductID { get; set; }

        /// <summary>
        /// delivery charge is waived if total amount equals or more than this amount
        /// </summary>
        [Display(Name = "FreeShippingAmount", Description = "FreeShippingAmount_Description", ResourceType = typeof(Resources.SharedResource))]
        [Range(0, 9999999)]
        public decimal? FreeShippingAmount { get; set; }


    }
}

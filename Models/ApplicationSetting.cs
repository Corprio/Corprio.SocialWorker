using Corprio.DataModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Corprio.DataModel.Resources;
using Corprio.DataModel.Business.Products;
using Corprio.DataModel.Business;
using Corprio.SocialWorker.Dictionaries;
using Corprio.SocialWorker.Helpers;

namespace Corprio.SocialWorker.Models
{
    /// <summary>
    /// Setting for this particular application
    /// </summary>
    public class ApplicationSetting
    {
        /// <summary>
        /// Content appearing after shopping cart on checkout page
        /// </summary>
        [Display(Name = "Footer", Description = "Footer_Description", ResourceType = typeof(Resources.SharedResource))]
        //[StringLength(2000)]
        public string Footer { get; set; } = "<p style='text-align: center;'><span style='font-family: Verdana; font-size: 1em;'>Check out now!</span></p>";

        /// <summary>
        /// Default message of thank you page
        /// </summary>
        [Display(Name = "ThankYouMessage", Description = "ThankYouMessage_Description", ResourceType = typeof(Resources.SharedResource))]
        //[StringLength(5000)]
        [Required]
        public string ThankYouMessage { get; set; } = "<h3>Thank you for your order</h3>";

        /// <summary>
        /// Whether an email will be sent to the customer to confirm his order.
        /// </summary>
        [Display(Name = "SendConfirmationEmail", Description = "SendConfirmationEmail_Description", ResourceType = typeof(Resources.SharedResource))]
        public bool SendConfirmationEmail { get; set; } = false;

        /// <summary>
        /// Default subject of email.
        /// </summary>
        [Display(Name = "DefaultEmailSubject", Description = "DefaultEmailSubject_Description", ResourceType = typeof(Resources.SharedResource))]
        [StringLength(200)]
        public string DefaultEmailSubject { get; set; } = "Thank you for your order";

        /// <summary>
        /// True if the user has completed SMTP setting.
        /// </summary>
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

        /// <summary>
        /// Keyword for indicating purchase intention. This keyword will be assigned to the next FB/IG post.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MsgRequired")]
        [StringLength(10)]
        public string KeywordForShoppingIntention { get; set; }

        /// <summary>
        /// The template for posting products to social media
        /// </summary>
        public string ProductPostTemplate { get; set; }

        /// <summary>
        /// Generate the post for publishing a product to social media
        /// </summary>
        /// <param name="product">The product to be published</param>
        /// <param name="coreInfo">The organization's core information</param>
        /// <param name="publicPrice">The product's price for walk-in customers</param>
        /// <returns>The post to be made in social media</returns>
        public string ProductPostMessage(Product product, OrganizationCoreInfo coreInfo, PriceWithCurrency publicPrice)
        {
            if (product == null) return null;
            string message = !string.IsNullOrWhiteSpace(ProductPostTemplate)
                ? ProductPostTemplate
                : string.Join(TemplateComponent.Separator, DefaultTemplate.DefaultTempalte_Product);            

            message = message.Replace(TemplateComponent.LineBreak, "\n")
                .Replace(TemplateComponent.DefaultMessage, BabelFish.Vocab["HowToBuy"][UtilityHelper.NICAM(coreInfo)].Replace("{0}", product.Name).Replace("{1}", KeywordForShoppingIntention))
                .Replace(TemplateComponent.Keyword, KeywordForShoppingIntention)
                .Replace(TemplateComponent.ProductName, product.Name)
                .Replace(TemplateComponent.ProductCode, product.Code)
                .Replace(TemplateComponent.ProductDescription, product.Description)
                .Replace(TemplateComponent.ProductListPrice, $"{product.ListPrice_CurrencyCode}{product.ListPrice_Value:F2}")
                .Replace(TemplateComponent.ProductPublicPrice, $"{publicPrice?.CurrencyCode}{publicPrice?.Price?.Value?.ToString("F2")}")
                .Replace(TemplateComponent.Separator, "");
            return UtilityHelper.UncleanAndClean(userInput: message, onceIsOK: true);
        }

        /// <summary>
        /// The template for posting catalogues to social media
        /// </summary>
        public string CataloguePostTemplate { get; set; }

        /// <summary>
        /// Generate the post for publishing a catalogue to social media
        /// </summary>
        /// <param name="productList">The catalogue to be published</param>
        /// <param name="coreInfo">The organization's core information</param>
        /// <param name="goBuyClickUrl">The catalogue's URL</param>
        /// <returns>The post to be made in social media</returns>
        public string CataloguePostMessage(ProductList productList, OrganizationCoreInfo coreInfo, string goBuyClickUrl)
        {
            if (productList == null) return null;
            string message = !string.IsNullOrWhiteSpace(CataloguePostTemplate)
                ? CataloguePostTemplate
                : string.Join(TemplateComponent.Separator, DefaultTemplate.DefaultTempalte_Catalogue);
            message = message.Replace(TemplateComponent.LineBreak, "\n")
                .Replace(TemplateComponent.DefaultMessage, BabelFish.Vocab["VisitCatalogue"][UtilityHelper.NICAM(coreInfo)])
                .Replace(TemplateComponent.CatalogueName, productList.Name)
                .Replace(TemplateComponent.CatalogueCode, productList.Code)
                .Replace(TemplateComponent.CatalogueEndDate, productList.EndDate?.ToString("u"))
                .Replace(TemplateComponent.CatalogueStartDate, productList.StartDate.ToString("u"))
                .Replace(TemplateComponent.CatalogueUrl, $"{goBuyClickUrl}/Catalogue/{coreInfo.ShortName}/{productList.Code}")
                .Replace(TemplateComponent.Separator, "");
            return UtilityHelper.UncleanAndClean(userInput: message, onceIsOK: true);
        }

        public PromotionSetting Promotion {  get; set; } = new PromotionSetting();
    }

    public class PromotionSetting
    {        
        /// <summary>
        /// Expiry time of the promotion. Null means the promotion will never expire.
        /// </summary>
        public DateTimeOffset? ExpiryTime { get; set; }

        /// <summary>
        /// Passcode that the customer is required to input in order to obtain a discount
        /// </summary>
        public string Passcode { get; set; }

        /// <summary>
        /// For instance, 15 means 15% off; null means no promotion is ongoing.
        /// </summary>
        [Range(0, 100)]
        public decimal? PercentageOff { get; set; }
    }

    /// <summary>
    /// Default template for publishing products and catalogues to social media
    /// </summary>
    public class DefaultTemplate
    {
        public static readonly List<string> DefaultTempalte_Product = new()
        {
            TemplateComponent.ProductName,
            "@",
            TemplateComponent.ProductPublicPrice,
            TemplateComponent.LineBreak,
            TemplateComponent.ProductDescription,
            TemplateComponent.LineBreak,
            TemplateComponent.DefaultMessage
        };

        public static readonly List<string> DefaultTempalte_Catalogue = new()
        {
            TemplateComponent.CatalogueName,
            TemplateComponent.LineBreak,
            TemplateComponent.DefaultMessage,
            TemplateComponent.LineBreak,
            TemplateComponent.CatalogueUrl
        };
    }

    /// <summary>
    /// Definition of standard template component
    /// </summary>
    public class TemplateComponent
    {
        public const string LineBreak = "%lineBreak%";
        public const string Separator = "%;sep;%";
        public const string DefaultMessage = "%defaultMessage%";
        // note: do not use the description in product list because it includes HTML tags
        public const string CatalogueName = "%catName%";
        public const string CatalogueCode = "%catCode%";
        public const string CatalogueUrl = "%catLink%";
        public const string CatalogueEndDate = "%catEndDate%";
        public const string CatalogueStartDate = "%catStartDate%";
        public const string ProductName = "%productName%";
        public const string ProductCode = "%productCode%";
        public const string ProductDescription = "%productDescription%";
        public const string ProductListPrice = "%productListPrice%";
        public const string ProductPublicPrice = "%productPublicPrice%";
        public const string Keyword = "%keyWord%";
    }
}

using System;
using System.Collections.Generic;
using Corprio.DataModel.Business;
using Corprio.DataModel.Business.Products;
using Corprio.SocialWorker.Dictionaries;
using Corprio.SocialWorker.Helpers;
using Corprio.DataModel.Resources;
using System.ComponentModel.DataAnnotations;

namespace Corprio.SocialWorker.Models
{
    public class PostTemplate
    {
        [Key]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MsgRequired")]
        [Display(Name = "ID")]
        public Guid ID { get; set; }

        /// <summary>
        /// Organization ID in Corprio.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MsgRequired")]
        public Guid OrganizationID { get; set; }
        
        public MessageType MessageType { get; set; }

        public string TemplateString { get; set; }        

        public string ProductPostMessage(Product product, OrganizationCoreInfo coreInfo, string keyword)
        {
            if (product == null) return null;
            string message = !string.IsNullOrWhiteSpace(TemplateString) 
                ? TemplateString 
                : string.Join(TemplateComponent.Separator, DefaultTemplate.DefaultTempalte_Product);

            keyword = string.IsNullOrWhiteSpace(keyword) ? BabelFish.Vocab["DefaultKeyWordForShoppingIntention"][UtilityHelper.NICAM(coreInfo)] : keyword;

            message = message.Replace(TemplateComponent.LineBreak, "\n")                                
                .Replace(TemplateComponent.DefaultMessage, BabelFish.Vocab["HowToBuy"][UtilityHelper.NICAM(coreInfo)].Replace("{0}", product.Name).Replace("{1}", keyword))
                .Replace(TemplateComponent.Keyword, keyword)
                .Replace(TemplateComponent.ProductName, product.Name)
                .Replace(TemplateComponent.ProductCode, product.Code)
                .Replace(TemplateComponent.ProductDescription, product.Description)
                .Replace(TemplateComponent.ProductListPrice, $"{product.ListPrice_CurrencyCode}{product.ListPrice_Value:F2}")
                .Replace(TemplateComponent.Separator, "");            
            return UtilityHelper.UncleanAndClean(userInput: message, onceIsOK: true);
        }

        public string CataloguePostMessage(ProductList productList, OrganizationCoreInfo coreInfo, string goBuyClickUrl)
        {
            if (productList == null) return null;
            string message = !string.IsNullOrWhiteSpace(TemplateString)
                ? TemplateString
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
    }

    public enum MessageType
    {
        CataloguePost,
        ProductPost
    }

    public class DefaultTemplate
    {
        public static readonly List<string> DefaultTempalte_Product = new() 
        {  
            TemplateComponent.ProductName,
            "@",            
            TemplateComponent.ProductListPrice,
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
        public const string Keyword = "%keyWord%";
    }
}

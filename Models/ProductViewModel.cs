using Corprio.DataModel.Business.Products;
using System.ComponentModel.DataAnnotations;
using Corprio.DataModel.Resources;
using System.Collections.Generic;

namespace Corprio.SocialWorker.Models
{
    public class ProductViewModel : Product
    {        
        /// <summary>
        /// Master product entity
        /// </summary>
        [Display(Name = "MasterProduct", ResourceType = typeof(Resource))]
        public string MasterProductCode { get; set; }

        /// <summary>
        /// Product type entity
        /// </summary>
        public string ProductTypeName { get; set; }

        /// <summary>
        /// Brand
        /// </summary>
        [Display(Name = "Brand", Description = "BrandDesc", ResourceType = typeof(Resource))]
        public string BrandName { get; set; }

        /// <summary>
        /// Tags associated with the product
        /// </summary>
        [Display(Name = "Tags", Description = "Tags", ResourceType = typeof(Resource))]
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// remark of the sales order line written by the customer
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// Discount of list price
        /// </summary>
        public decimal DiscountValue { get; set; }

        /// <summary>
        /// Type of discount of list price
        /// </summary>
        public DataModel.DiscountType DiscountType { get; set; }

        /// <summary>
        /// List price
        /// </summary>
        public decimal? UnitPrice { get; set; }

        /// <summary>
        /// The property has its value directly retrieved from SalesOrder object
        /// </summary>
        public decimal NetUnitPrice { get; set; }

        /// <summary>
        /// Default quantity of product added to cart
        /// </summary>

        public decimal Qty { get; set; } = 0;

        /// <summary>
        /// UOM of price
        /// </summary>
        public string UOMCode { get; set; }

        /// <summary>
        /// Decimals used by the UOM
        /// </summary>
        /// <remarks>Not used now. Assume selling whole unit</remarks>
        public int SalesUOMDecimals { get; set; }

        public string Image01UrlKey { get; set; }
        public string Image02UrlKey { get; set; }
        public string Image03UrlKey { get; set; }
        public string Image04UrlKey { get; set; }
        public string Image05UrlKey { get; set; }
        public string Image06UrlKey { get; set; }
        public string Image07UrlKey { get; set; }
        public string Image08UrlKey { get; set; }


        /// <summary>
        /// Image urls
        /// </summary>
        /// 
        public List<string> ImageUrls { get; set; } = new List<string>(new string[8]);


        //for check inventory
        public decimal? InventoryLevel { get; set; }        

        public bool HasAllChildProducts { get; set; }

        public List<List<object>> VariationsMatches { get; set; }
    }
}

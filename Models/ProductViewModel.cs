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
    }
}

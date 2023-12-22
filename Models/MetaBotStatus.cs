using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Corprio.DataModel;
using Corprio.DataModel.Resources;
using Newtonsoft.Json;

namespace Corprio.SocialWorker.Models
{
    /// <summary>
    /// Chat bot status
    /// </summary>
    public class MetaBotStatus : DbFriendlyBot
    {                
        /// <summary>
        /// The product IDs and names that the bot 'remembers'.
        /// </summary>
        public List<KeyValuePair<Guid, string>> ProductMemory { get; set; }

        /// <summary>
        /// The product variations that the bot 'remembers', in the format of { attribute : { value, value } }. 
        /// They represent choices that the bot WILL offer to the user.
        /// </summary>
        public List<KeyValuePair<string, List<string>>> VariationMemory { get; set; }
        
        /// <summary>
        /// The attribute-value pairs that the bot 'remembers. 
        /// They represent the product variations that the user HAS selected.
        /// </summary>
        public List<KeyValuePair<string, string>> AttributeValueMemory { get; set; }
        
        /// <summary>
        /// Shopping cart of the user.
        /// </summary>
        public List<BotBasket> Cart { get; set; }
    }    

    /// <summary>
    /// A basket in the shopping cart. Each basket captures the price, quantity, etc. of 1 product.
    /// </summary>
    public class BotBasket
    {
        /// <summary>
        /// Basket ID
        /// </summary>
        [Key]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MsgRequired")]
        public Guid ID { get; set; }
        
        /// <summary>
        /// Corprio entity ID of the product.
        /// </summary>
        public Guid ProductID { get; set; }

        /// <summary>
        /// Name of the product.
        /// </summary>        
        [StringLength(300)]
        public string Name { get; set; }

        /// <summary>
        /// Quantity of the product.
        /// </summary>
        [Range(-Constant.QtyMaxValue, Constant.QtyMaxValue)]
        public decimal Quantity { get; set; }

        /// <summary>
        /// Price of the product.
        /// </summary>
        [Range(-Constant.MoneyMaxValue, Constant.MoneyMaxValue)]
        public decimal? Price { get; set; }

        /// <summary>
        /// UOM code of the product.
        /// </summary>
        [StringLength(Constant.UOMCodeMaxLength)]
        public string UOMCode { get; set; }
    }
}

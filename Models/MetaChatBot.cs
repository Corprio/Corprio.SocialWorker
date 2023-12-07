using System;
using System.Collections.Generic;
using Corprio.DataModel.Business.Partners;

namespace Corprio.SocialWorker.Models
{
    public class MetaChatBot
    {
        /// <summary>
        /// Id of the entity that is having a conversation with the bot
        /// </summary>
        public string Id { get; set; }
        
        public BotLanguage Lang { get; set; }

        public BotTopic ThinkingOf { get; set; } = BotTopic.Limbo;

        public bool ThinkingOfYN { get; set; }

        public bool ThinkingOfMC { get; set; }
        
        public List<KeyValuePair<Guid, string>> PrdMemory { get; set; } = new List<KeyValuePair<Guid, string>>();

        public List<KeyValuePair<string, List<string>>> VarMemory { get; set; } = new List<KeyValuePair<string, List<string>>>();

        public List<KeyValuePair<string, string>> AttValMemory { get; set; } = new List<KeyValuePair<string, string>>();

        public BotClient Customer { get; set; } = new BotClient();

        public List<BotBasket> Cart { get; set; } = new List<BotBasket>();
    }

    public class BotClient
    {
        public Guid? Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Phone_CC { get; set; }

        public string Phone_NDC { get; set; }

        public string Phone_Num { get; set; }
    }

    public class BotBasket
    {
        public Guid ProductID { get; set; }
        
        public string Name { get; set; }

        public decimal Quantity { get; set; }
    }

    public enum BotLanguage
    {        
        English,        
        SimplifiedChinese,
        TraditionalChinese
    }

    public enum BotTopic
    {
        Limbo,
        Product,
        ProductVariations,
        Quantity,
        Checkout,
        ClearCart,
        Basket,
        Name,
        Email,
        MobilePhone
    }    
}

using System.ComponentModel;

namespace Corprio.SocialWorker.Models
{
    /// <summary>
    /// Natural languages that the bot can handle and speak
    /// </summary>
    public enum BotLanguage
    {
        English,
        SimplifiedChinese,
        TraditionalChinese
    }

    /// <summary>
    /// Range of topics that the bot is supposed to handle
    /// </summary>
    public enum BotTopic
    {
        Limbo,
        NewCustomerYN,
        ProductOpen,
        ProductYN,
        ProductMC,
        ProductVariationMC,
        QuantityOpen,
        CheckoutYN,
        ClearCartYN,
        EmailOpen,
        EmailConfirmationOpen,
        PromotionOpen
    }

    /// <summary>
    /// Meta products
    /// </summary>
    public enum MetaProduct
    {        
        Facebook,        
        Instagram
    }    

    /// <summary>
    /// Selected languages that can be detected by Meta NLP
    /// </summary>
    public enum MetaLanguageModel
    {
        [Description("CHINESE")]
        Chinese,
        [Description("ENGLISH")]
        English
    }

    /// <summary>
    /// Types of media items that can be published to Instagram
    /// </summary>
    public enum IgMediaType
    {
        [Description("CAROUSEL")]
        Carousel,
        [Description("IMAGE")]
        Image,
        [Description("REELS")]
        Reels,
        [Description("STORIES")]
        Stories,
        [Description("VIDEO")]
        Video
    }
}

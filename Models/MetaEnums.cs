using System.ComponentModel;

namespace Corprio.SocialWorker.Models
{
    public enum BotLanguage
    {
        English,
        SimplifiedChinese,
        TraditionalChinese
    }

    public enum BotTopic
    {
        Limbo,
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

    public enum MetaProduct
    {        
        Facebook,        
        Instagram
    }
    
    public enum MetaTokenType
    {
        [Description("APP")]
        App,
        [Description("USER")]
        User,
        [Description("PAGE")]
        Page
    }

    public enum MetaLanguageModel
    {
        [Description("CHINESE")]
        Chinese,
        [Description("ENGLISH")]
        English
    }

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

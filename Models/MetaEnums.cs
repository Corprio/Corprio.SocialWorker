using System.ComponentModel;

namespace Corprio.SocialWorker.Models
{
    public enum Platform
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

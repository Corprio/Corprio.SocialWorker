using System.Collections.Generic;
using System;
using System.Linq;
using Corprio.DataModel.Business;
using Corprio.Global.Globalization;
using Corprio.DataModel.Business.Products;
using Corprio.SocialWorker.Models.Meta;

namespace Corprio.SocialWorker.Helpers
{
    public class UtilityHelper
    {
        /// <summary>
        /// Undo any sanitization done on the client-side first and - optionally - perform another sanitization.
        /// (Note: we need to undo any sanitization done on the client-side first to prevent things like $ampamp or &amplt;)
        /// </summary>
        /// <param name="userInput">Input that may or may not have gone through client-side sanitization</param>
        /// <param name="onceIsOK">True if no re-sanitization is required</param>
        /// <returns></returns>
        public static string UncleanAndClean(string userInput, bool onceIsOK = false)
        {
            if (string.IsNullOrWhiteSpace(userInput)) return string.Empty;
            
            userInput = userInput.Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\"").Replace("&#x27;", "'").Replace("&nbsp;", " ");
            if (!onceIsOK)
            {
                userInput = userInput.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&#x27;").Replace(" ", "&nbsp;");
            }                        
            return userInput;
        }
        
        /// <summary>
        /// Determine the appropriate language based on the organization's default language
        /// </summary>
        /// <param name="coreInfo">Organization core information</param>
        /// <returns>An enum of language</returns>
        public static BotLanguage NICAM(OrganizationCoreInfo coreInfo)
        {
            string defaultLanguage = coreInfo == null ? LocaleCode.English : coreInfo.DefaultLanguage;            
            switch (defaultLanguage)
            {
                case LocaleCode.Chinese_HongKong:
                case LocaleCode.Chinese_Taiwan:
                    return BotLanguage.TraditionalChinese;                    
                case LocaleCode.Chinese:
                case LocaleCode.Chinese_Simplified:
                    return BotLanguage.SimplifiedChinese;                    
                default:
                    return BotLanguage.English;                    
            }                                    
        }

        /// <summary>
        /// Extract non-null image IDs from a product
        /// </summary>
        /// <param name="product">An object of Product class</param>
        /// <returns>A set of image IDs</returns>
        public static HashSet<Guid> ReturnImageUrls(Product product)
        {
            HashSet<Guid> validIds = new();
            if (product == null) return validIds;

            List<Guid?> imageIDs = new() { product.Image01ID, product.Image02ID, product.Image03ID, product.Image04ID,
                product.Image05ID, product.Image06ID, product.Image07ID, product.Image08ID };
            foreach (Guid? imageID in imageIDs)
            {
                if (imageID.HasValue) validIds.Add(imageID.Value);
            }
            return validIds;
        }
    }
}

using System.Collections.Generic;
using System;
using System.Linq;
using Corprio.DataModel.Business;
using Corprio.Global.Globalization;
using Corprio.SocialWorker.Models;

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
        /// Extract an entity ID from dynamic query results
        /// </summary>
        /// <param name="dynamicQueryResults">Dynamic query results</param>
        /// <param name="key">Key that is expected to correspond to an entity ID</param>
        /// <returns>An entity ID</returns>
        public static Guid GetGuidFromDynamicQueryResult(List<dynamic> dynamicQueryResults, string key)
        {
            Guid entityId = Guid.Empty;
            if (dynamicQueryResults?.Any() ?? false)
            {
                foreach (var kvp in dynamicQueryResults[0])
                {
                    if (kvp.Key == key && Guid.TryParse(kvp.Value, out entityId)) break;
                }
            }
            return entityId;
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
    }
}

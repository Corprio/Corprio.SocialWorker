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
            BotLanguage lang;
            if (coreInfo.DefaultLanguage == LocaleCode.Chinese_HongKong || coreInfo.DefaultLanguage == LocaleCode.Chinese_Taiwan)
            {
                lang = BotLanguage.TraditionalChinese;
            }
            else if (coreInfo.DefaultLanguage == LocaleCode.Chinese || coreInfo.DefaultLanguage == LocaleCode.Chinese_Simplified)
            {
                lang = BotLanguage.SimplifiedChinese;
            }
            else
            {
                lang = BotLanguage.English;
            }
            return lang;
        }
    }
}

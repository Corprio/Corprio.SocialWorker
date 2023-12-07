using Corprio.SocialWorker.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog;
using System.Linq;
using System;
using Corprio.CorprioAPIClient;
using System.Text.RegularExpressions;
using Corprio.DataModel.Business.Products;
using Corprio.DataModel;
using Corprio.SocialWorker.Dictionaries;
using Corprio.Core;

namespace Corprio.SocialWorker.Helpers
{
    public class DomesticHelper
    {
        private readonly APIClient Client;
        private readonly Guid OrgID;
        private MetaProfileModel Profile;
        private MetaChatBot Bot;
        
        public DomesticHelper(APIClient client, Guid organizationID, MetaProfileModel metaProfile, string senderId, List<DetectedLocale> detectedLocales)
        {
            if (string.IsNullOrEmpty(senderId)) throw new ArgumentException("Sender Id cannot be blank.");
            
            Client = client;
            OrgID = organizationID;
            Profile = metaProfile;
            Bot = metaProfile.Bots.FirstOrDefault(x => x.Id == senderId);
            if (Bot == null)
            {
                metaProfile.Bots.Add(new MetaChatBot() { Id = senderId });
                Bot = metaProfile.Bots.Last();
            }
            Bot.Lang = DetectLanguage(detectedLocales);
        }

        private async Task UpdateProfile()
        {
            await Client.OrganizationApi.SaveApplicationSetting(organizationID: OrgID, value: System.Text.Json.JsonSerializer.Serialize(Profile));
        }

        private string ThusSpokeBabel(string key, List<string> placeholders = null)
        {
            if (!BabelFish.Vocab.ContainsKey(key))
            {
                Log.Error($"Babel Fish does not understand {key}.");
                return ThusSpokeBabel("Err_DefaultMsg");                
            }

            if (placeholders == null) return BabelFish.Vocab[key][Bot.Lang];

            string babelSpeech = BabelFish.Vocab[key][Bot.Lang];
            for (int i = 0; i < placeholders.Count; i++)
            {
                babelSpeech = babelSpeech.Replace($"{{{i}}}", placeholders[i]);
            }
            return babelSpeech;
        }

        private async Task<string> ProductVariationChoiceString()
        {
            KeyValuePair<string, List<string>> attributeValues = Bot.VarMemory.FirstOrDefault(x => x.Value.Any());            
            if (attributeValues.Key == null)
            {
                Log.Error("No product variation attribute has any values.");
                return await SoftReboot();
            }

            string choices = "";
            for (int i = 0; i < attributeValues.Value.Count; i++)
            {
                choices = $"{i} - {attributeValues.Value[i]}\n";
            }
            choices += ThusSpokeBabel("Hint_Cancel");
            return choices;
        }

        private string ProductChoiceString()
        {
            string choices = "";
            for (int i = 0; i < Bot.PrdMemory.Count; i++)
            {
                choices += $"{i + 1} - {Bot.PrdMemory[i].Value}\n";
            }
            choices += ThusSpokeBabel("NoneOfTheAbove");
            return choices;
        }

        private Tuple<bool, string> CheckBuyIntention(string input)
        {
            if (Bot.Lang == BotLanguage.English) input = input.ToLower();
            input = input.Replace("，", "").Replace("。", "").Replace("、", "")
                .Replace(",", " ").Replace(".", " ")
                .Replace("?", " ").Replace("!", " ");
            input = Regex.Replace(input, @"\s+", " ");
            var keywords = Bot.Lang == BotLanguage.English 
                ? new List<string>() { "buy ", "purchase ", "want ", "like " }
                : new List<string>() { "買", "买", "要", "訂購", "订购" };
            
            foreach (string word in keywords)
            {
                int index = input.IndexOf(word);
                if (index == -1) continue;
                string wanted = input.Substring(index + word.Length, input.Length - index - word.Length).Trim();
                return new Tuple<bool, string>(true, wanted);
            }
            return new Tuple<bool, string>(false, input);            
        }

        /// <summary>
        /// Convert query results into a list of key value pairs
        /// </summary>
        /// <param name="queryResults">List of dynamic objects resulting from database query</param>
        /// <returns>List of key value pairs</returns>
        public List<KeyValuePair<Guid, string>> ReturnQueryResults(dynamic[] queryResults)
        {
            List<KeyValuePair<Guid, string>> searchResults = new();
            if (queryResults == null || queryResults.Length == 0) return searchResults;

            foreach (dynamic result in queryResults)
            {
                Guid id = Guid.Empty;
                string name = null;
                foreach (var kvp in result)
                {
                    if (kvp.Key == "ID")
                        _ = Guid.TryParse(kvp.Value, out id);
                    else
                        name = kvp.Value;
                }
                
                if (!id.Equals(Guid.Empty) && !string.IsNullOrWhiteSpace(name))
                {
                    searchResults.Add(new KeyValuePair<Guid, string>(id, name));
                }                
            }
            return searchResults;
        }

        /// <summary>
        /// Search for products with similar code and name
        /// </summary>        
        /// <param name="name">Key word provided by user for the search</param>
        /// <returns>List of query results</returns>
        /// <exception cref="ArgumentNullException"></exception>
        private async Task<List<KeyValuePair<Guid, string>>> SearchProduct(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");
            name = name.Trim();
            
            PagedList<dynamic> queryResults = await Client.ProductApi.QueryPage(organizationID: OrgID,
                selector: "new (ID, Name)",
                loadDataOptions: new LoadDataOptions
                {
                    PageIndex = 0,
                    PageSize = 1,
                    RequireTotalCount = false,
                    Sort = new SortingInfo[] { new SortingInfo { Selector = "Code", Desc = false } },
                    Filter = new List<object> { new object[] {
                        new string[] { "Code", "=", name },
                        "and", new string[] { "Disabled", "=", "False" }
                    }   }
                });
            if (queryResults.Items.Length == 1)
            {
                return ReturnQueryResults(queryResults.Items);
            }

            queryResults = await Client.ProductApi.QueryPage(organizationID: OrgID,
                selector: "new (ID, Name)",
                loadDataOptions: new LoadDataOptions
                {
                    PageIndex = 0,
                    PageSize = 10,
                    RequireTotalCount = true,
                    Sort = new SortingInfo[] { new SortingInfo { Selector = "Name", Desc = false } },
                    Filter = new List<object> { new object[] {
                        new string[] { "Code", "contains", name },
                        "and", new string[] { "Disabled", "=", "False" },
                        "or", new string[] { "Name", "contains", name },
                        "and", new string[] { "Disabled", "=", "False" }
                    } },
                });
            return ReturnQueryResults(queryResults.Items);
        }        

        public static BotLanguage DetectLanguage(List<DetectedLocale> detectedLocales)
        {
            // note: when the user simply inputs a number, the detected locales may be an empty list
            if (!(detectedLocales?.Any() ?? false)) return BotLanguage.English;

            DetectedLocale locale = detectedLocales.Count == 1
                ? detectedLocales[0]
                : detectedLocales.FirstOrDefault(x => x.Confidence == detectedLocales.Max(y => y.Confidence));
            return locale.Locale == "zh_TW" 
                ? BotLanguage.TraditionalChinese 
                : (locale.Locale == "zh_CN" ? BotLanguage.SimplifiedChinese : BotLanguage.English);
        }

        private async Task<string> GetChildProduct(Guid masterProductId)
        {
            Guid[] childProductIds = await Client.ProductApi.GetVariantIDs(organizationID: OrgID, masterProductID: masterProductId, 
                attributeValues: Bot.AttValMemory.ToArray());
            if (childProductIds.Length == 0)
            {
                return await HandleProductNotFound();
            }
            
            Product product = await Client.ProductApi.Get(organizationID: OrgID, id: childProductIds[0]);
            if (product == null)
            {
                return await HandleProductNotFound();
            }

            return await AddProductToCart(product);
        }

        private async Task<string> UpdateAttributeValuesMemory(string attributeType, string name)
        {
            Bot.AttValMemory.Add(new KeyValuePair<string, string>(attributeType, name));
            KeyValuePair<string, List<string>> attributeValues = Bot.VarMemory.FirstOrDefault(x => x.Key == attributeType);
            attributeValues = new KeyValuePair<string, List<string>>(attributeType, new List<string>());
            await UpdateProfile();
            return await AskQuestion();
        }

        private async Task<string> AskQuestion()
        {
            switch (Bot.ThinkingOf)
            {
                case BotTopic.Limbo:
                    if (!(Bot.Cart?.Any() ?? false))
                    {
                        Bot.ThinkingOf = BotTopic.Product;                        
                    }                                        
                    else if (Bot.Cart.Any(x => x.Quantity == 0))
                    {
                        Bot.ThinkingOf = BotTopic.Quantity;                        
                    }
                    else
                    {
                        Bot.ThinkingOf = BotTopic.Checkout;
                    }
                    await UpdateProfile();
                    return await AskQuestion();
                
                case BotTopic.Product:
                    if (!(Bot.PrdMemory?.Any() ?? false)) return ThusSpokeBabel("AskProduct");

                    if (Bot.PrdMemory.Count == 1)
                    {
                        return ThusSpokeBabel(key: "AskOneProduct", placeholders: Bot.PrdMemory.Select(x => x.Value).ToList());
                    }
                    
                    return ThusSpokeBabel(key: "AskMultiProducts", placeholders: new List<string>() { ProductChoiceString() });
                
                case BotTopic.ProductVariations:
                    KeyValuePair<string, List<string>> attributeValues = Bot.VarMemory.FirstOrDefault(x => x.Value.Any());
                    if (attributeValues.Key == null) return await GetChildProduct(Bot.PrdMemory[0].Key);

                    if (attributeValues.Value.Count == 1)
                    {
                        return await UpdateAttributeValuesMemory(attributeType: attributeValues.Key, name: attributeValues.Value[0]);

                        //Bot.AttValMemory.Add(new KeyValuePair<string, string>(attributeValues.Key, attributeValues.Value[0]));
                        //attributeValues = new KeyValuePair<string, List<string>>(attributeValues.Key, new List<string>());
                        //await UpdateProfile();
                        //return await AskQuestion();
                    }

                    string choices = await ProductVariationChoiceString();
                    return ThusSpokeBabel(key: "AskProductVariation", placeholders: new List<string>() { attributeValues.Key, choices });

                case BotTopic.Quantity:
                    BotBasket basket = Bot.Cart.FirstOrDefault(x => x.Quantity == 0);
                    if (basket == null)
                    {
                        Log.Error("The Bot was expected one basket with zero quantity but found none.");
                        return await SoftReboot();
                    }
                    return ThusSpokeBabel(key: "AskQty", placeholders: new List<string>() { basket.Name } );

                default:
                    return ThusSpokeBabel("Err_DefaultMsg");
            }
        }

        private async Task<string> AddProductToCart(Product product)
        {
            Bot.Cart.Add(new BotBasket { ProductID = product.ID, Name = product.Name });
            Bot.PrdMemory = new List<KeyValuePair<Guid, string>>();
            Bot.ThinkingOf = BotTopic.Quantity;
            await UpdateProfile();
            return await AskQuestion();
        }

        private async Task<string> HandleProductNotFound()
        {
            Bot.ThinkingOf = BotTopic.Limbo;
            Bot.ThinkingOfMC = false;
            Bot.PrdMemory = new List<KeyValuePair<Guid, string>>();
            Bot.VarMemory = new List<KeyValuePair<string, List<string>>>();
            await UpdateProfile();
            return ThusSpokeBabel("Err_ProductNotFound");
        }        

        private async Task<string> HandleMC(string input)
        {
            if (!int.TryParse(input, out int num))
                return $"{ThusSpokeBabel("Err_NotUnderstand")}\n{await AskQuestion()}";

            switch (Bot.ThinkingOf)
            {
                case BotTopic.Product:
                    if (num > Bot.PrdMemory.Count || num < 0) return $"{ThusSpokeBabel("Err_NotUnderstand")}\n{await AskQuestion()}";
                    if (num == 0)
                    {
                        Bot.ThinkingOfMC = false;
                        Bot.PrdMemory = new List<KeyValuePair<Guid, string>>();
                        await UpdateProfile();
                        return await AskQuestion();
                    }
                    return await SelectProduct(Bot.PrdMemory[num - 1].Key);                    
                case BotTopic.ProductVariations:
                    KeyValuePair<string, List<string>> attributeValues = Bot.VarMemory.FirstOrDefault(x => x.Value.Any());
                    if (attributeValues.Key == null)
                    {
                        Log.Error("The bot was expecting at least one attribute with values, but there was none.");
                        return await SoftReboot();
                    }                    
                    
                    if (num > attributeValues.Value.Count || num < 1) 
                        return $"{ThusSpokeBabel("Err_NotUnderstand")}\n{await AskQuestion()}";                    
                    
                    return await UpdateAttributeValuesMemory(attributeType: attributeValues.Key, name: attributeValues.Value[num - 1]);

                default:
                    Log.Error($"The bot was expecting multiple choices but the topic was {Bot.ThinkingOf}");
                    return await SoftReboot();
            }
            
        }

        private async Task<string> SoftReboot(bool clearCustomer = false, bool clearCart = false)
        {
            Bot.ThinkingOf = BotTopic.Limbo;
            Bot.ThinkingOfYN = false;
            Bot.ThinkingOfMC = false;            
            Bot.PrdMemory = new List<KeyValuePair<Guid, string>>();
            Bot.VarMemory = new List<KeyValuePair<string, List<string>>>();
            Bot.AttValMemory = new List<KeyValuePair<string, string>>();
            if (clearCustomer) Bot.Customer = new BotClient();
            if (clearCart) Bot.Cart = new List<BotBasket>();
            await UpdateProfile();
            return ThusSpokeBabel("Err_DefaultMsg");
        }

        private async Task<string> SelectProduct(Guid productId)
        {
            Product product = await Client.ProductApi.Get(organizationID: OrgID, id: productId);
            if (product == null) return await HandleProductNotFound();

            if (!product.IsMasterProduct || (!product.Variations?.Any() ?? false))
                return await AddProductToCart(product);

            Bot.ThinkingOfMC = true;
            Bot.ThinkingOf = BotTopic.ProductVariations;
            Bot.VarMemory = new List<KeyValuePair<string, List<string>>>();
            foreach (ProductVariation variation in product.Variations)
            {
                var existingAttributeValues = Bot.VarMemory.FirstOrDefault(x => x.Key == variation.AttributeType);
                if (existingAttributeValues.Key == null)
                {
                    Bot.VarMemory.Add(new KeyValuePair<string, List<string>>(variation.AttributeType, new List<string>() { variation.Name }));
                }
                else
                {
                    existingAttributeValues.Value.Add(variation.Name);
                }
            }
            await UpdateProfile();
            return await AskQuestion();
        }

        private async Task<string> HandleYesOrNo(string input)
        {
            input = input.ToLower();
            if (!BabelFish.YesNo.ContainsKey(input)) 
                return $"{ThusSpokeBabel("Err_NotUnderstand")}\n{await AskQuestion()}";

            Bot.ThinkingOfYN = false;
            if (BabelFish.YesNo[input] == 2)
            {
                Bot.PrdMemory = new List<KeyValuePair<Guid, string>>();
                await UpdateProfile();
                return await AskQuestion();
            }

            switch (Bot.ThinkingOf)
            {
                case BotTopic.Product:
                    return await SelectProduct(Bot.PrdMemory[0].Key);
                case BotTopic.Checkout:
                    // TODO
                    return null;
                case BotTopic.ClearCart:
                    // TODO
                    return null;
                default:
                    Log.Error($"The bot was expecting answer to yes/no question but the topic was {Bot.ThinkingOf}");
                    return await SoftReboot();
            }                                    
        }

        private async Task<string> HandleCancel()
        {
            Bot.ThinkingOfYN = false;
            Bot.ThinkingOfMC = false;

            switch (Bot.ThinkingOf)
            {
                case BotTopic.Product:
                    Bot.PrdMemory = new List<KeyValuePair<Guid, string>>();
                    break;
                case BotTopic.ProductVariations:
                    Bot.PrdMemory = new List<KeyValuePair<Guid, string>>();
                    Bot.AttValMemory = new List<KeyValuePair<string, string>>();
                    Bot.VarMemory = new List<KeyValuePair<string, List<string>>>();
                    Bot.ThinkingOf = BotTopic.Product;
                    break;
                default:                    
                    break;                
            }
            await UpdateProfile();
            return await AskQuestion();
        }

        public async Task<string> ThinkBeforeSpeak(string input)
        {                        
            if (input.ToLower() == BabelFish.KillCode) return await HandleCancel();
            
            // TODO - pick up the chat based on:
            // (1) any product in cart with 0 qty in cart,
            // (2) if all products are ready for checkout (or cart clearing)

            if (Bot.ThinkingOfYN) return await HandleYesOrNo(input: input);

            if (Bot.ThinkingOfMC) return await HandleMC(input: input);

            switch (Bot.ThinkingOf)
            {
                case BotTopic.Limbo:
                    (bool buyIntention, string wanted) = CheckBuyIntention(input: input);
                    Bot.PrdMemory = await SearchProduct(buyIntention ? wanted : input);
                    if (Bot.PrdMemory.Count == 1)
                    {
                        Bot.ThinkingOf = BotTopic.Product;
                        Bot.ThinkingOfYN = true;
                        Bot.ThinkingOfMC = false;
                        await UpdateProfile();
                        return await AskQuestion();
                    }

                    if (Bot.PrdMemory.Count > 1)
                    {
                        Bot.ThinkingOf = BotTopic.Product;
                        Bot.ThinkingOfYN = false;
                        Bot.ThinkingOfMC = true;
                        await UpdateProfile();
                        return await AskQuestion();
                    }

                    return ThusSpokeBabel(key: "CannotFindProduct", new List<string>() { input });
                
                case BotTopic.Product:
                    return await AskQuestion();

                case BotTopic.Quantity:
                case BotTopic.Email:
                case BotTopic.Name:
                case BotTopic.MobilePhone:
                default:
                    // For DEV only
                    Bot.ThinkingOf = BotTopic.Limbo;
                    Bot.ThinkingOfYN = false;
                    Bot.ThinkingOfMC = false;
                    Bot.PrdMemory = new();
                    Bot.VarMemory = new();
                    Bot.AttValMemory = new();
                    await UpdateProfile();
                    return $"My code is not ready to handle {input}. The bot is reset now. ";
            }                        
        }      
    }
}

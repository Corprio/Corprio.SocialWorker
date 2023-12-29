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
using Corprio.DataModel.Business.Sales;
using Corprio.DataModel.Business;
using Corprio.Core.Exceptions;
using Corprio.DataModel.Business.Partners;
using Corprio.Core.Utility;
using Corprio.DataModel.Shared;
using Corprio.DataModel.Business.Sales.ViewModel;
using Microsoft.Extensions.Configuration;

namespace Corprio.SocialWorker.Helpers
{
    public class DomesticHelper
    {
        private readonly ApplicationDbContext db;
        readonly IConfiguration configuration;
        private readonly APIClient Client;
        private readonly Guid OrgID;
        private readonly string PageName;
        private readonly DbFriendlyBot Shell;
        private readonly MetaBotStatus Bot;

        // magic numbers
        private const int ChoiceLimit = 10;        
        private const string KillCode = "!c";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client">Client for Api requests among Corprio projects</param>
        /// <param name="organizationID">ID of the organization being represented by the chat bot</param>        
        /// <param name="botStatus">An object storing the bot's state</param>
        /// <param name="pageName">Name of the page being represented by the chat bot</param>
        /// <param name="detectedLocales">List of locales detected by Wit.ai</param>
        /// <exception cref="ArgumentException"></exception>
        public DomesticHelper(ApplicationDbContext context, IConfiguration configuration, APIClient client, Guid organizationID, DbFriendlyBot botStatus, string pageName,
            List<DetectedLocale> detectedLocales = null)
        {
            if (string.IsNullOrWhiteSpace(pageName)) throw new ArgumentException("Page name cannot be blank.");

            db = context;
            this.configuration = configuration;
            Client = client;
            OrgID = organizationID;
            PageName = pageName;
            Shell = botStatus;
            Bot = botStatus.ReadyToWork();
            Bot.Language = DetectLanguage(detectedLocales);
        }

        /// <summary>
        /// Save the bot's state into memory
        /// </summary>
        /// <returns></returns>
        private async Task Save()
        {            
            db.MetaBotStatuses.Update(Shell.ReadyToSave(Bot));            
            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to save the bot's status. {ex.Message}");
            }
        }

        /// <summary>
        /// Generate a message in a language that is most relevant to the detected locale
        /// </summary>
        /// <param name="key">Key for looking up the right phrases in the dictionary</param>
        /// <param name="placeholders">List of strings to be interpolated</param>
        /// <returns>A (part of a) message that will be 'spoken' by the bot</returns>
        public string ThusSpokeBabel(string key, List<string> placeholders = null)
        {
            if (!BabelFish.Vocab.ContainsKey(key))
            {
                Log.Error($"Babel Fish does not understand {key}.");
                return ThusSpokeBabel("Err_DefaultMsg");                
            }

            if (placeholders == null) return BabelFish.Vocab[key][Bot.Language];

            string babelSpeech = BabelFish.Vocab[key][Bot.Language];
            for (int i = 0; i < placeholders.Count; i++)
            {
                babelSpeech = babelSpeech.Replace($"{{{i}}}", placeholders[i]);
            }
            return babelSpeech;
        }

        /// <summary>
        /// Generate a string of cart items
        /// </summary>
        /// <param name="showPrice">True if prices are to be included</param>
        /// <param name="currencyCode">Currency code for the prices</param>
        /// <returns>A string of cart items</returns>
        private string CartItemString(bool showPrice, string currencyCode = null)
        {
            if (showPrice && string.IsNullOrWhiteSpace(currencyCode))
            {
                Log.Error("No currency was provided when the bot wanted to show prices.");
                showPrice = !showPrice;
            }
            
            string items = "";
            foreach (BotBasket basket in Bot.Cart)
            {
                items += showPrice 
                    ? $"{basket.Name} @{currencyCode}{basket.Price:F2} x {basket.Quantity:F0}\n"
                    : $"{basket.Name} x {basket.Quantity:F0}\n";
            }
            return items;
        }

        /// <summary>
        /// Generate a string of product variations
        /// </summary>
        /// <returns>A string of product variations</returns>
        private async Task<string> ProductVariationChoiceString()
        {
            KeyValuePair<string, List<string>> attributeValues = Bot.VariationMemory.FirstOrDefault(x => x.Value.Any());            
            if (attributeValues.Key == null)
            {
                Log.Error("No product variation attribute has any values.");
                return await SoftReboot();
            }

            string choices = "";
            for (int i = 0; i < attributeValues.Value.Count; i++)
            {
                choices += $"{i + 1} - {attributeValues.Value[i]}\n";
            }
            choices += ThusSpokeBabel("Hint_Cancel");
            return choices;
        }

        /// <summary>
        /// Generate a string of products
        /// </summary>
        /// <returns>A string of products</returns>
        private string ProductChoiceString()
        {
            string choices = "";
            int limit = Math.Min(Bot.ProductMemory.Count, ChoiceLimit);
            for (int i = 0; i < limit; i++)
            {
                choices += $"{i + 1} - {Bot.ProductMemory[i].Value}\n";
            }
            choices += ThusSpokeBabel("NoneOfTheAbove");
            if (Bot.ProductMemory.Count > ChoiceLimit)
            {                
                choices += "\n" + ThusSpokeBabel(key: "TooManyResults", placeholders: new List<string>() { ChoiceLimit.ToString() });
            }
            return choices;
        }

        /// <summary>
        /// Extract product name/code from a user input
        /// </summary>
        /// <param name="input">User input</param>
        /// <returns>(1) True if the user input includes words that indicate buying intention, (2) words that presumably represent a product's name/code</returns>
        private Tuple<bool, string> CheckBuyIntention(string input)
        {
            if (Bot.Language == BotLanguage.English) input = input.ToLower();
            input = input.Replace("，", "").Replace("。", "").Replace("、", "")
                .Replace(",", " ").Replace(".", " ")
                .Replace("?", " ").Replace("!", " ");
            input = Regex.Replace(input, @"\s+", " ");
            var keywords = Bot.Language == BotLanguage.English 
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
                    PageSize = ChoiceLimit + 1, // we need +1 so that the bot can detect if there are too many results
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

        /// <summary>
        /// Return a language that is most likely spoken by the person conversing with the bot
        /// </summary>
        /// <param name="detectedLocales">List of locales detected by Wit.ai</param>
        /// <returns>A language spoken by the bot</returns>
        public BotLanguage DetectLanguage(List<DetectedLocale> detectedLocales)
        {
            // note 1: when the user simply inputs a number, the detected locales may be an empty list
            // note 2: we should use whatever language the bot was using previously
            if (!(detectedLocales?.Any() ?? false)) return Bot.Language;

            DetectedLocale locale = detectedLocales.Count == 1
                ? detectedLocales[0]
                : detectedLocales.FirstOrDefault(x => x.Confidence == detectedLocales.Max(y => y.Confidence));
            return locale.Locale == "zh_TW" 
                ? BotLanguage.TraditionalChinese 
                : (locale.Locale == "zh_CN" ? BotLanguage.SimplifiedChinese : BotLanguage.English);
        }

        /// <summary>
        /// PLACEHOlDER
        /// </summary>
        /// <returns></returns>
        private async Task<string> AskPromotionQuestion()
        {
            bool promotion = await CheckPromotion();
            if (!promotion)
            {
                Log.Error("The bot was expecting an ongoing promotion but there was none.");
            }

            Log.Error("The bot does not know how to ask promotion question.");
            return ThusSpokeBabel("Err_DefaultMsg");
        }

        /// <summary>
        /// PLACEHOlDER
        /// </summary>
        /// <returns>True if the organization is doing any promotion that may affect the selling price</returns>
        private async Task<bool> CheckPromotion()
        {
            // TODO - check if the organization is doing any promotion that may affect the selling price
            return false;
        }

        /// <summary>
        /// Process checkout
        /// </summary>
        /// <returns></returns>
        private async Task<string> Checkout()
        {                        
            if (Bot.BuyerCorprioID == null)
            {
                Log.Error("The bot was trying to process checkout but there was no valid customer ID.");
                return await SoftReboot();
            }
            
            OrganizationCoreInfo coreInfo = await Client.OrganizationApi.GetCoreInfo(OrgID);
            if (coreInfo == null)
            {
                Log.Error($"Failed to obtain core information for {OrgID}");
                return ThusSpokeBabel("Err_DefaultMsg");
            }

            PriceWithCurrency price;
            foreach (BotBasket basket in Bot.Cart)
            {
                try
                {
                    price = await Client.SellingPriceApi.GetPriceForCustomer(
                        organizationID: OrgID,
                        productID: basket.ProductID,
                        customerID: (Guid)Bot.BuyerCorprioID,
                        quantity: new Global.Measure.Quantity { UOMCode = basket.UOMCode, Value = basket.Quantity },
                        currencyCode: coreInfo.CurrencyCode);
                }
                catch (ApiExecutionException ex)
                {
                    Log.Error($"Failed to obtain selling price for {basket.Name} {basket.ProductID}. {ex.Message}");
                    return await SoftReboot();
                }

                basket.Price = price.Price.Value;
            }

            bool promotion = await CheckPromotion();
            if (promotion)
            {
                Bot.ThinkingOf = BotTopic.PromotionOpen;
                await Save();
                return await AskQuestion();
            }
            
            string checkoutURL = await CreateSalesOrder();
            if (string.IsNullOrWhiteSpace(checkoutURL)) return await SoftReboot(clearCustomer: true, clearCart: true);

            string cartItemString = CartItemString(showPrice: true, currencyCode: coreInfo.CurrencyCode);
            Bot.Cart.Clear();
            Bot.ThinkingOf = BotTopic.Limbo;
            await Save();
            return ThusSpokeBabel(key: "ThankYou", placeholders: new List<string>() { cartItemString, checkoutURL });
        }

        /// <summary>
        /// Create sales order (including lines)
        /// </summary>
        /// <returns>URL to the created sales order</returns>
        private async Task<string> CreateSalesOrder()
        {
            if (!(Bot.Cart?.Any() ?? false))
            {
                Log.Error("The bot was trying to create a sales order but the cart was empty.");
                return null;
            }

            if (Bot.BuyerCorprioID == null)
            {
                Log.Error("The bot was trying to create a sales order but there was no valid customer ID.");
                return null;
            }
            Customer customer = await Client.CustomerApi.Get(organizationID: OrgID, id: (Guid)Bot.BuyerCorprioID);
            if (customer == null)
            {
                Log.Error("The bot was trying to create a sales order but no customer could be found.");
                return null;
            }

            OrganizationCoreInfo coreInfo = await Client.OrganizationApi.GetCoreInfo(OrgID);
            if (coreInfo == null)
            {
                Log.Error($"Failed to obtain core information for {OrgID}");
                return null;
            }

            List<dynamic> warehouses = await Client.WarehouseApi.Query(organizationID: OrgID, 
                dynamicQuery: new DynamicQuery() 
                { 
                    Selector = "new (ID)",
                    Where = "Code=@0 and Disabled=false",
                    WhereArguments = new object[] { Constant.DefaultWarehouseCode },
                    OrderBy = "Code",
                    Skip = 0,
                    Take = 1
                });
            if (!warehouses.Any())
            {
                Log.Error("The bot was trying to create a sales order but the default warehouse could not be found.");
                return null;
            }
            Guid defaultWarehouseID = Guid.Parse(warehouses[0].ID);            

            Guid orderId;
            try
            {
                orderId = await Client.SalesOrderApi.Add(organizationID: OrgID, salesOrder: new AddSalesOrderModel
                {
                    OrderDate = DateTimeOffset.Now,
                    CustomerID = (Guid)Bot.BuyerCorprioID,
                    CurrencyCode = coreInfo.CurrencyCode,
                    BillAddress_Name = customer.BusinessPartner.PrimaryAddress_Name,
                    BillAddress_Line1 = customer.BusinessPartner.PrimaryAddress_Line1,
                    BillAddress_Line2 = customer.BusinessPartner.PrimaryAddress_Line2,
                    BillAddress_City = customer.BusinessPartner.PrimaryAddress_City,
                    BillAddress_State = customer.BusinessPartner.PrimaryAddress_State,
                    BillAddress_PostalCode = customer.BusinessPartner.PrimaryAddress_PostalCode,
                    BillAddress_CountryAlphaCode = customer.BusinessPartner.PrimaryAddress_CountryAlphaCode,
                    BillAddress_Latitude = customer.BusinessPartner.PrimaryAddress_Latitude,
                    BillAddress_Longitude = customer.BusinessPartner.PrimaryAddress_Longitude,
                    DeliveryAddress_Name = customer.BusinessPartner.PrimaryAddress_Name,
                    DeliveryAddress_Line1 = customer.BusinessPartner.PrimaryAddress_Line1,
                    DeliveryAddress_Line2 = customer.BusinessPartner.PrimaryAddress_Line2,
                    DeliveryAddress_City = customer.BusinessPartner.PrimaryAddress_City,
                    DeliveryAddress_State = customer.BusinessPartner.PrimaryAddress_State,
                    DeliveryAddress_PostalCode = customer.BusinessPartner.PrimaryAddress_PostalCode,
                    DeliveryAddress_CountryAlphaCode = customer.BusinessPartner.PrimaryAddress_CountryAlphaCode,
                    DeliveryAddress_Latitude = customer.BusinessPartner.PrimaryAddress_Latitude,
                    DeliveryAddress_Longitude = customer.BusinessPartner.PrimaryAddress_Longitude,
                    BillPhoneNumbers = new List<Global.Geography.PhoneNumber> { new Global.Geography.PhoneNumber
                    {
                        NumberType = Global.PhoneNumberType.Mobile,
                        CountryCallingCode = customer.BusinessPartner.PrimaryMobilePhoneNumber_CountryCallingCode,
                        NationalDestinationCode = customer.BusinessPartner.PrimaryMobilePhoneNumber_NationalDestinationCode,
                        SubscriberNumber = customer.BusinessPartner.PrimaryMobilePhoneNumber_SubscriberNumber
                    } },
                    BillEmail = customer.BusinessPartner.PrimaryEmail,
                    WarehouseID = defaultWarehouseID
                });
            }
            catch (ApiExecutionException ex)
            {
                Log.Error($"The bot failed to create a sales order. {ex.Message}");
                return null;
            }

            foreach (BotBasket basket in Bot.Cart)
            {
                try
                {
                    await Client.SalesOrderApi.AddLine(organizationID: OrgID,
                        line: new AddSalesOrderLineModel
                        {
                            ProductID = basket.ProductID,
                            Qty = basket.Quantity,
                            SalesOrderID = orderId,
                            UnitPrice = new Price
                            {
                                Value = basket.Price,
                                UOMCode = basket.UOMCode,
                                DiscountType = basket.DiscountType,
                                DiscountValue = basket.Discount
                            }
                        });
                }
                catch (ApiExecutionException ex)
                {
                    Log.Error($"The bot failed to create sales order line for {basket.ProductID}. {ex.Message}");
                    return null;
                }
            }

            return $"{configuration["AppBaseUrl"]}/{OrgID}/checkout/{orderId}";
        }

        /// <summary>
        /// Identify the relevant child product based on attribute types and names and add it to the cart
        /// </summary>
        /// <param name="masterProductId">The child product's master product's ID</param>
        /// <returns></returns>
        private async Task<string> GetChildProduct(Guid masterProductId)
        {
            Guid[] childProductIds = await Client.ProductApi.GetVariantIDs(organizationID: OrgID, masterProductID: masterProductId, 
                attributeValues: Bot.AttributeValueMemory.ToArray());
            if (childProductIds.Length == 0) return await HandleProductNotFound();

            Product product = await Client.ProductApi.Get(organizationID: OrgID, id: childProductIds[0]);
            if (product == null) return await HandleProductNotFound();
            
            Bot.VariationMemory.Clear();
            Bot.AttributeValueMemory.Clear();
            return await AddProductToCart(product);
        }

        /// <summary>
        /// Update the bot's memory about attribute types and names
        /// </summary>
        /// <param name="attributeType">Attribute type of a product variation (e.g., size)</param>
        /// <param name="name">Name of a product variation (e.g., XL)</param>
        /// <returns></returns>
        private async Task<string> UpdateAttributeValuesMemory(string attributeType, string name)
        {
            Bot.AttributeValueMemory.Add(new KeyValuePair<string, string>(attributeType, name));
            KeyValuePair<string, List<string>> attributeValues = Bot.VariationMemory.FirstOrDefault(x => x.Key == attributeType);
            if (attributeValues.Key == null)
            {
                Log.Error($"{attributeType} could not be found in the bot's memory.");
                return await SoftReboot();
            }
            attributeValues.Value.Clear();
            await Save();
            return await AskQuestion();
        }

        /// <summary>
        /// Generate a question based what the bot is thinking about
        /// </summary>
        /// <returns></returns>
        private async Task<string> AskQuestion()
        {
            switch (Bot.ThinkingOf)
            {
                case BotTopic.Limbo:
                    if (!(Bot.Cart?.Any() ?? false))
                    {
                        Bot.ThinkingOf = BotTopic.ProductOpen;
                    }                                        
                    else if (Bot.Cart.Any(x => x.Quantity == 0))
                    {                        
                        Bot.ThinkingOf = BotTopic.QuantityOpen;
                    }
                    else
                    {                        
                        Bot.ThinkingOf = BotTopic.CheckoutYN;
                    }
                    await Save();
                    return await AskQuestion();
                
                case BotTopic.ProductOpen:
                    if (!(Bot.ProductMemory?.Any() ?? false))
                    {                        
                        await Save();
                        return ThusSpokeBabel("AskProduct");
                    }

                    Bot.ThinkingOf = Bot.ProductMemory.Count == 1 ? BotTopic.ProductYN : BotTopic.ProductMC;
                    await Save();
                    return await AskQuestion();

                case BotTopic.ProductYN:
                    return ThusSpokeBabel(key: "AskOneProduct", placeholders: Bot.ProductMemory.Select(x => x.Value).ToList());

                case BotTopic.ProductMC:
                    return ThusSpokeBabel(key: "AskMultiProducts", placeholders: new List<string>() { ProductChoiceString() });

                case BotTopic.ProductVariationMC:
                    KeyValuePair<string, List<string>> attributeValues = Bot.VariationMemory.FirstOrDefault(x => x.Value.Any());                    
                    if (attributeValues.Key == null)
                    {
                        if (!(Bot.ProductMemory?.Any() ?? false))
                        {
                            Log.Error("The bot should remember one product when the topic is ProductVariation but it did not.");
                            return await SoftReboot();
                        }
                        return await GetChildProduct(Bot.ProductMemory[0].Key);
                    }

                    if (attributeValues.Value.Count == 1)
                    {
                        return await UpdateAttributeValuesMemory(attributeType: attributeValues.Key, name: attributeValues.Value[0]);
                    }

                    string choices = await ProductVariationChoiceString();
                    return ThusSpokeBabel(key: "AskProductVariation", placeholders: new List<string>() { attributeValues.Key, choices });

                case BotTopic.QuantityOpen:
                    BotBasket basket = Bot.Cart.FirstOrDefault(x => x.Quantity == 0);
                    if (basket == null)
                    {
                        Log.Error("The bot was expecting one basket with zero quantity but found none.");
                        return await SoftReboot();
                    }
                    return ThusSpokeBabel(key: "AskQty", placeholders: new List<string>() { basket.Name } );

                case BotTopic.CheckoutYN:
                    if (!(Bot.Cart?.Any() ?? false))
                    {
                        Log.Error("The bot was going to prompt for checkout but the cart was empty.");
                        return await SoftReboot();
                    }
                    return ThusSpokeBabel("AskCheckout");

                case BotTopic.ClearCartYN:
                    return ThusSpokeBabel(key: "AskClearCart", placeholders: new List<string>() { CartItemString(showPrice: false) });

                case BotTopic.EmailOpen:
                    return ThusSpokeBabel("AskEmail");

                // PLACEHOLDER
                case BotTopic.PromotionOpen:
                    return await AskPromotionQuestion();

                default:
                    return ThusSpokeBabel("Err_DefaultMsg");
            }
        }

        /// <summary>
        /// Add a product to the shopping cart
        /// </summary>
        /// <param name="product">A product object</param>
        /// <returns></returns>
        private async Task<string> AddProductToCart(Product product)
        {
            Bot.Cart.Add(new BotBasket { ProductID = product.ID, Name = product.Name, UOMCode = product.ListPrice_UOMCode });
            Bot.ProductMemory.Clear();
            Bot.ThinkingOf = BotTopic.QuantityOpen;            
            await Save();
            return await AskQuestion();
        }

        /// <summary>
        /// React when no product is found
        /// </summary>
        /// <returns>An error message</returns>
        private async Task<string> HandleProductNotFound()
        {
            Bot.ThinkingOf = BotTopic.Limbo;
            Bot.ProductMemory.Clear();
            Bot.VariationMemory.Clear();
            Bot.AttributeValueMemory.Clear();
            await Save();
            return ThusSpokeBabel("Err_ProductNotFound");
        }        

        /// <summary>
        /// Handle answer to a multiple-choice question
        /// </summary>
        /// <param name="answer"></param>
        /// <returns></returns>
        private async Task<string> HandleMC(int answer)
        {            
            switch (Bot.ThinkingOf)
            {
                case BotTopic.ProductMC:
                    // note: the product memory may have more products than the choice limit
                    if (answer > Math.Min(Bot.ProductMemory.Count, ChoiceLimit) || answer < 0) 
                        return $"{ThusSpokeBabel("Err_NotUnderstand")}\n{await AskQuestion()}";

                    if (answer == 0)
                    {
                        Bot.ThinkingOf = BotTopic.ProductOpen;
                        Bot.ProductMemory.Clear();
                        await Save();
                        return await AskQuestion();
                    }
                    return await SelectProduct(Bot.ProductMemory[answer - 1].Key);
                case BotTopic.ProductVariationMC:
                    KeyValuePair<string, List<string>> attributeValues = Bot.VariationMemory.FirstOrDefault(x => x.Value.Any());
                    if (attributeValues.Key == null)
                    {
                        Log.Error("The bot was expecting at least one attribute with values, but there was none.");
                        return await SoftReboot();
                    }
                    
                    if (answer > attributeValues.Value.Count || answer < 1) 
                        return $"{ThusSpokeBabel("Err_NotUnderstand")}\n{await AskQuestion()}";                    
                    
                    return await UpdateAttributeValuesMemory(attributeType: attributeValues.Key, name: attributeValues.Value[answer - 1]);

                default:
                    // if this scenario is triggered, the bot is really messed up, so we should reset even the customer and cart
                    Log.Error($"The bot was expecting multiple choices but the topic was {Bot.ThinkingOf}");
                    return await SoftReboot(clearCustomer: true, clearCart: true);
            }            
        }

        /// <summary>
        /// Reset the parameters and memory of the bot
        /// </summary>
        /// <param name="clearCustomer">True if the bot needs to forget the customer's email and any OTP</param>
        /// <param name="clearCart">True if the shopping cart needs to be emptied</param>
        /// <returns></returns>
        private async Task<string> SoftReboot(bool clearCustomer = false, bool clearCart = false)
        {
            Bot.ThinkingOf = BotTopic.Limbo;
            Bot.ProductMemory.Clear();
            Bot.VariationMemory.Clear();
            Bot.AttributeValueMemory.Clear();
            if (clearCustomer)
            {
                Bot.BuyerEmail = null;
                Bot.OTP_Code = null;
                Bot.OTP_ExpiryTime = null;                
            }
            if (clearCart) Bot.Cart.Clear();
            await Save();
            return ThusSpokeBabel("Err_DefaultMsg");
        }

        /// <summary>
        /// React when a (child) product is selected
        /// </summary>
        /// <param name="productId">ID of the product being selected</param>
        /// <returns></returns>
        private async Task<string> SelectProduct(Guid productId)
        {
            Product product = await Client.ProductApi.Get(organizationID: OrgID, id: productId);
            if (product == null) return await HandleProductNotFound();

            if (!product.IsMasterProduct || (!product.Variations?.Any() ?? false))
                return await AddProductToCart(product);
            
            Bot.ThinkingOf = BotTopic.ProductVariationMC;
            Bot.ProductMemory.Clear();
            Bot.ProductMemory.Add(new KeyValuePair<Guid, string>(product.ID, product.Name)); // the bot needs to 'remember' the master product ID
            Bot.VariationMemory.Clear();
            foreach (ProductVariation variation in product.Variations)
            {
                var existingAttributeValues = Bot.VariationMemory.FirstOrDefault(x => x.Key == variation.AttributeType);
                if (existingAttributeValues.Key == null)
                {
                    Bot.VariationMemory.Add(new KeyValuePair<string, List<string>>(variation.AttributeType, new List<string>() { variation.Name }));
                }
                else
                {
                    existingAttributeValues.Value.Add(variation.Name);
                }
            }
            await Save();
            return await AskQuestion();
        }

        /// <summary>
        /// Trigger the sending of an one-time password that expires in 24 hours
        /// </summary>
        /// <returns></returns>
        private async Task<string> SendOTP()
        {
            if (string.IsNullOrWhiteSpace(Bot.BuyerEmail))
            {
                Log.Error("The bot did not have a valid email address in memory when it tried to send an OTP.");
                Bot.ThinkingOf = BotTopic.EmailOpen;
                await Save();
                return await AskQuestion();
            }
            
            var rand = new Random();
            string OTP = rand.Next(1000000).ToString("D6");
            var message = new EmailMessage
            {
                Subject = ThusSpokeBabel(key: "Email_subject", placeholders: new List<string>() { PageName  }),
                Body = ThusSpokeBabel(key: "Email_body", placeholders: new List<string>() { OTP }),
                ToEmails = new EmailAddress[] { new EmailAddress(Bot.BuyerEmail) }
            };
            try
            {
                await Client.OrganizationApi.SendEmail(organizationID: OrgID, emailMessage: message);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to send OTP to {Bot.BuyerEmail}. {ex.Message}");
                Bot.ThinkingOf = BotTopic.EmailOpen;
                Bot.BuyerEmail = null;
                await Save();
                return $"{ThusSpokeBabel("Err_UndeliveredOTP")}";
            }

            Bot.ThinkingOf = BotTopic.EmailConfirmationOpen;            
            Bot.OTP_Code = OTP;
            Bot.OTP_ExpiryTime = DateTimeOffset.UtcNow.AddDays(1);
            await Save();
            return ThusSpokeBabel("CodeSent");
        }

        /// <summary>
        /// Get a customer ID based on an email address
        /// </summary>
        /// <param name="emailAddress">Email address that supposedly belongs to a customer in Corprio</param>
        /// <returns></returns>
        private async Task<Tuple<bool, Guid>> GetCustomerIdByEmail(string emailAddress)
        {
            Guid customerId = Guid.Empty;
            
            List<dynamic> existingCustomers = await Client.CustomerApi.Query(
                organizationID: OrgID,
                selector: "new (ID)",
                where: "BusinessPartner.PrimaryEmail==@0",
                orderBy: "ID",
                whereArguments: new string[] { emailAddress },
                skip: 0,
                take: 1);
            if (existingCustomers.Any()) 
            {
                customerId = Guid.Parse(existingCustomers[0].ID);
                return new Tuple<bool, Guid>(true, customerId);
            }                        

            // note: we tentatively use the customer's email as the given name, because the latter cannot be blank
            string[] splits = emailAddress.Split("@");
            try
            {
                customerId = await Client.CustomerApi.AddNew(organizationID: OrgID, 
                    customer: new Customer()
                    {
                        BusinessPartner = new BusinessPartner()
                        {
                            PartnerType = BusinessPartnerType.Person,
                            GivenName = StringHelper.StringTruncate(splits[0], 100),
                            PrimaryEmail = emailAddress                        
                        },
                        Source = "Facebook/Instagram",
                        EntityProperties = new List<EntityProperty>() { new EntityProperty() { Name = BabelFish.CustomerEpName, Value = Bot.BuyerID } }
                    });
            }
            catch (ApiExecutionException ex)
            {
                Log.Error($"Failed to create a customer with email {emailAddress}. {ex.Message}");
            }
            return new Tuple<bool, Guid>(false, customerId);
        }

        /// <summary>
        /// Validate an email address
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        private async Task<string> ConfirmEmail(string emailAddress)
        {
            string regex = @"^[^@\s]+@[^@\s]+\.(com|net|org|gov)$";
            bool isValid = Regex.IsMatch(emailAddress, regex, RegexOptions.IgnoreCase);
            if (!isValid) return ThusSpokeBabel("Err_InvalidEmail");

            // note: we don't care if this email address - and, by extension, the customer ID - is connected to another FB/IG ID,
            // because we allow one customer to be associated with more than one social media account
            Bot.BuyerEmail = emailAddress;
            (bool isExistingCustomer, Guid customerId) = await GetCustomerIdByEmail(emailAddress);
            if (isExistingCustomer)
            {
                await Save();
                return await SendOTP();
            }

            if (customerId.Equals(Guid.Empty))
            {
                Log.Error($"Failed to retrieve/create a customer ID for email {Bot.BuyerEmail}");
                return await SoftReboot(clearCustomer: true);
            }

            Bot.BuyerCorprioID = customerId;
            await Save();
            return await Checkout();            
        }
        
        /// <summary>
        /// Update a customer's entity properties with a Facebook user ID
        /// </summary>
        /// <returns></returns>
        private async Task<bool> UpdateCustomerEp()
        {
            if (Bot.BuyerCorprioID == null)
            {
                Log.Error("The customer ID was null when the bot attempted to update the customer's entity properties.");
                return false;
            }

            Customer customer = await Client.CustomerApi.Get(organizationID: OrgID, id: (Guid)Bot.BuyerCorprioID);
            if (customer == null)
            {
                Log.Error($"Failed to retrieve customer {(Guid)Bot.BuyerCorprioID}.");
                return false;
            }

            // note: we allow 1 customer to be associated with more than 1 social media account, so we don't use AddUpdateEntityProperty(),
            // which may over-write any EP with the same name
            (customer.EntityProperties ??= new List<EntityProperty>()).Add(new EntityProperty() { Name = BabelFish.CustomerEpName, Value = Bot.BuyerID });
            try
            {
                await Client.CustomerApi.Update(organizationID: OrgID, customer: customer);
            }
            catch (ApiExecutionException ex)
            {
                Log.Error($"Failed to assign {Bot.BuyerID} to customer {Bot.BuyerCorprioID}. {ex.Message}");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Look for the customer ID of the person conversing with the bot
        /// </summary>
        /// <returns></returns>
        private async Task<string> ConfirmCustomer()
        {                                    
            List<dynamic> existingCustomers = await Client.CustomerApi.Query(
                organizationID: OrgID,
                selector: "new (ID)",
                where: "EntityProperties.Any(Name==@0 && Value==@1)",
                orderBy: "ID",
                whereArguments: new string[] { BabelFish.CustomerEpName, Bot.BuyerID },
                skip: 0,
                take: 1);            
            if (!existingCustomers.Any()) 
            {
                Bot.ThinkingOf = BotTopic.EmailOpen;
                await Save();
                return await AskQuestion();
            }

            Bot.BuyerCorprioID = Guid.Parse(existingCustomers[0].ID);
            await Save();
            return await Checkout();
        }

        /// <summary>
        /// Handle answer to a yes or no question
        /// </summary>
        /// <param name="yes">True if the answer is interpreted as a yes</param>
        /// <returns></returns>
        private async Task<string> HandleYesOrNo(bool yes)
        {                        
            switch (Bot.ThinkingOf)
            {
                case BotTopic.ProductYN:
                    if (!(Bot.ProductMemory?.Any() ?? false))
                    {
                        Log.Error($"The bot did not have any product in memory when handling topic {Bot.ThinkingOf}");
                        return await SoftReboot();
                    }
                    
                    if (yes) return await SelectProduct(Bot.ProductMemory[0].Key);

                    Bot.ProductMemory.Clear();
                    Bot.ThinkingOf = BotTopic.ProductOpen;
                    await Save();
                    return await AskQuestion();

                case BotTopic.CheckoutYN:
                    if (yes) return Bot.BuyerCorprioID == null ? await ConfirmCustomer() : await Checkout();

                    Bot.ThinkingOf = BotTopic.ProductOpen;
                    await Save();
                    return await AskQuestion();

                case BotTopic.ClearCartYN:
                    Bot.ProductMemory.Clear();
                    Bot.VariationMemory.Clear();
                    Bot.AttributeValueMemory.Clear();

                    if (yes)
                    {
                        Bot.Cart.Clear();                        
                        Bot.ThinkingOf = BotTopic.ProductOpen;
                        await Save();
                        return $"{ThusSpokeBabel("EmptyCart")}\n{await AskQuestion()}";
                    }

                    Bot.ThinkingOf = (Bot.Cart?.Any(x => x.Quantity == 0) ?? false) 
                        ? BotTopic.QuantityOpen 
                        : ((Bot.Cart?.Any() ?? false) ? BotTopic.CheckoutYN : BotTopic.ProductOpen);

                    await Save();
                    return await AskQuestion();

                default:
                    // if this scenario is triggered, the bot is really messed up, so we should reset even customer and the cart
                    Log.Error($"The bot was expecting an answer to yes/no question but the topic was {Bot.ThinkingOf}");
                    return await SoftReboot(clearCustomer: true, clearCart: true);
            }                                    
        }

        /// <summary>
        /// React when the user cancels the current operation
        /// </summary>
        /// <returns></returns>
        private async Task<string> HandleCancel()
        {            
            switch (Bot.ThinkingOf)
            {
                case BotTopic.ProductOpen:
                    Bot.ProductMemory.Clear();
                    break;

                case BotTopic.ProductYN:
                    Bot.ProductMemory.Clear();
                    Bot.ThinkingOf = BotTopic.ProductOpen;
                    break;

                case BotTopic.ProductMC:
                    Bot.ProductMemory.Clear();
                    Bot.VariationMemory.Clear();
                    Bot.ThinkingOf = BotTopic.ProductOpen;
                    break;

                case BotTopic.ProductVariationMC:
                    Bot.ProductMemory.Clear();
                    Bot.AttributeValueMemory.Clear();
                    Bot.VariationMemory.Clear();
                    Bot.ThinkingOf = BotTopic.ProductOpen;
                    break;

                case BotTopic.QuantityOpen:
                case BotTopic.CheckoutYN:
                case BotTopic.EmailOpen:
                    Bot.ThinkingOf = BotTopic.ClearCartYN;
                    break;

                case BotTopic.EmailConfirmationOpen:
                    Bot.BuyerEmail = null;
                    Bot.OTP_Code = null;
                    Bot.OTP_ExpiryTime = null;
                    Bot.ThinkingOf = BotTopic.ClearCartYN;
                    break;

                default:
                    return await AskQuestion();
            }
            await Save();
            return await AskQuestion();
        }

        /// <summary>
        /// PLACEHOLDER
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private async Task<string> HandlePromotion(string input)
        {
            // PLACEHOLDER - if there is promotion (e.g., the user can input a code to get a better price),
            // then we handle it before sending checkout link
            Log.Error("No promotion is implemented in the bot yet.");
            return await SoftReboot();
        }

        /// <summary>
        /// Generate a message as if the user has selected a product id
        /// </summary>
        /// <param name="productId">Product ID that the user has selected</param>
        /// <returns></returns>
        public async Task<string> ReachOut(Guid productId)
        {            
            Bot.ThinkingOf = BotTopic.ProductOpen;
            Bot.ProductMemory.Clear();
            Bot.VariationMemory.Clear();
            Bot.AttributeValueMemory.Clear();            
            Bot.BuyerEmail = null;
            Bot.OTP_Code = null;
            Bot.OTP_ExpiryTime = null;
            Bot.Cart.Clear();            
            await Save();
            return await SelectProduct(productId);
        }

        /// <summary>
        /// Handle input to the bot
        /// </summary>
        /// <param name="input">User's input</param>
        /// <returns></returns>
        public async Task<string> ThinkBeforeSpeak(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            input = input.Trim();
            if (input.ToLower() == KillCode) return await HandleCancel();
            
            switch (Bot.ThinkingOf)
            {
                case BotTopic.Limbo:
                case BotTopic.ProductOpen:
                    (bool buyIntention, string wanted) = CheckBuyIntention(input);
                    Bot.ProductMemory = await SearchProduct(buyIntention ? wanted : input);                    
                    
                    // try to search again because it is possible that the product code/name includes words that otherwise indicate buying intention
                    if (buyIntention && !(Bot.ProductMemory?.Any() ?? false)) 
                        Bot.ProductMemory = await SearchProduct(input);

                    if (Bot.ProductMemory?.Any() ?? false)
                    {
                        Bot.ThinkingOf = Bot.ProductMemory.Count == 1 ? BotTopic.ProductYN : BotTopic.ProductMC;
                        await Save();
                        return await AskQuestion();
                    }                    

                    return ThusSpokeBabel(key: "CannotFindProduct", new List<string>() { input });

                case BotTopic.ProductYN:
                case BotTopic.CheckoutYN:
                case BotTopic.ClearCartYN:
                    input = input.ToLower();
                    return BabelFish.YesNo.ContainsKey(input)
                        ? await HandleYesOrNo(BabelFish.YesNo[input] == 1)
                        : $"{ThusSpokeBabel("Err_NotUnderstand")}\n{await AskQuestion()}";

                case BotTopic.ProductMC:
                case BotTopic.ProductVariationMC:
                    return int.TryParse(input, out int num)
                    ? await HandleMC(num)
                    : $"{ThusSpokeBabel("Err_NotUnderstand")}\n{await AskQuestion()}";

                case BotTopic.QuantityOpen:
                    if (!decimal.TryParse(input, out decimal qty))
                        return $"{ThusSpokeBabel("Err_NotUnderstand")}\n{await AskQuestion()}";

                    if (qty <= 0)
                        return $"{ThusSpokeBabel("Err_NonPositiveInput")}\n{await AskQuestion()}";
                    
                    BotBasket basket = Bot.Cart.FirstOrDefault(x => x.Quantity == 0);
                    if (basket == null)
                    {
                        Log.Error("The bot was expecting one basket with zero quantity but found none.");
                        return await SoftReboot();
                    }
                    basket.Quantity = qty;

                    // if there are other baskets with zero quantity - which is unlikely, but theoritically possible - stay on quantity
                    if (Bot.Cart.FirstOrDefault(x => x.Quantity == 0) != null)
                    {
                        await Save();
                        return await AskQuestion();
                    }
                    
                    Bot.ThinkingOf = BotTopic.CheckoutYN;                    
                    await Save();
                    return $"{ThusSpokeBabel(key: "CartUpdated", placeholders: new List<string>() { basket.Quantity.ToString("F0"), basket.Name })}\n{await AskQuestion()}";

                case BotTopic.EmailOpen:
                    return await ConfirmEmail(input);

                case BotTopic.EmailConfirmationOpen:
                    if (string.IsNullOrWhiteSpace(Bot.OTP_Code))
                    {
                        Log.Error("The bot did not have an OTP in the memory for email validation.");
                        return await SoftReboot(clearCustomer: true);
                    }
                    
                    if (input != Bot.OTP_Code || DateTimeOffset.Now > Bot.OTP_ExpiryTime)
                        return $"{ThusSpokeBabel("Err_InvalidOTP")}\n{await SendOTP()}";

                    Bot.OTP_Code = null;
                    (_, Guid customerId) = await GetCustomerIdByEmail(Bot.BuyerEmail);
                    if (customerId == Guid.Empty)
                    {
                        Log.Error($"Failed to retrieve/create a customer ID for email {Bot.BuyerEmail}");
                        return await SoftReboot(clearCustomer: true);
                    }

                    Bot.BuyerCorprioID = customerId;
                    await Save();
                    await UpdateCustomerEp();  // we proceed even if the EP update fails
                    return await Checkout();
                    
                case BotTopic.PromotionOpen:
                    // PLACEHOlDER
                    return await HandlePromotion(input);

                default:
                    Log.Error($"Unexpected topic {Bot.ThinkingOf} for input {input}");
                    return await SoftReboot(clearCustomer: true, clearCart: true);
            }                        
        }      
    }
}

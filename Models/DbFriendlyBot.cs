﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Corprio.DataModel;
using Corprio.DataModel.Resources;
using Newtonsoft.Json;

namespace Corprio.SocialWorker.Models
{
    /// <summary>
    /// Chat bot status in a DB friendly format (i.e., all objects have been serialized).
    /// </summary>
    public class DbFriendlyBot : Entity
    {
        /// <summary>
        /// ID of the user that is having a conversation with the bot.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MsgRequired")]
        [StringLength(50, MinimumLength = 1)]
        public string BuyerID { get; set; }

        /// <summary>
        /// Entity ID of Facebook user (not the ID assigned by Facebook to its user) who owns the bot.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MsgRequired")]
        public Guid FacebookUserID { get; set; }

        /// <summary>
        /// Facebook user object.
        /// </summary>
        [ForeignKey("FacebookUserID")]
        public MetaUser FacebookUser { get; set; }

        /// <summary>
        /// Language in which the next message generated by the bot will be.
        /// </summary>
        public BotLanguage Language { get; set; }

        /// <summary>
        /// Topic that the bot is dealing with.
        /// </summary>
        public BotTopic ThinkingOf { get; set; } = BotTopic.Limbo;

        /// <summary>
        /// The product IDs and names that the bot 'remembers'.
        /// </summary>
        public string ProductMemoryString { get; set; }

        /// <summary>
        /// The product variations that the bot 'remembers', in the format of { attribute : { value, value } }. 
        /// They represent choices that the bot WILL offer to the user.
        /// </summary>
        public string VariationMemoryString { get; set; }

        /// <summary>
        /// The attribute-value pairs that the bot 'remembers. 
        /// They represent the product variations that the user HAS selected.
        /// </summary>
        public string AttributeValueMemoryString { get; set; }

        /// <summary>
        /// Shopping cart of the user.
        /// </summary>
        public string CartString { get; set; }        

        /// <summary>
        /// Corprio entity ID of the buyer.
        /// </summary>
        public Guid? BuyerCorprioID { get; set; }

        /// <summary>
        /// Email address provided by the user.
        /// </summary>
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [StringLength(256)]
        public string BuyerEmail { get; set; }

        /// <summary>
        /// Confirmation code that the user is required to input.
        /// </summary>
        [StringLength(6)]
        public string OTP_Code { get; set; }

        /// <summary>
        /// Expiry time of the confirmation code.
        /// </summary>
        public DateTimeOffset? OTP_ExpiryTime { get; set; }

        public MetaBotStatus ReadyToWork()
        {
            if (this == null) return null;
            var bot = new MetaBotStatus()
            {
                BuyerID = this.BuyerID,
                FacebookUserID = this.FacebookUserID,
                FacebookUser = this.FacebookUser,                
                Language = this.Language,
                ThinkingOf = this.ThinkingOf,                
                BuyerCorprioID = this.BuyerCorprioID,
                BuyerEmail = this.BuyerEmail,
                OTP_Code = this.OTP_Code,
                OTP_ExpiryTime = this.OTP_ExpiryTime,
            };
            if (!string.IsNullOrWhiteSpace(this.ProductMemoryString))
                bot.ProductMemory = JsonConvert.DeserializeObject<List<KeyValuePair<Guid, string>>>(this.ProductMemoryString)!;
            bot.ProductMemory ??= new List<KeyValuePair<Guid, string>>();
            if (!string.IsNullOrWhiteSpace(this.VariationMemoryString))
                bot.VariationMemory = JsonConvert.DeserializeObject<List<KeyValuePair<string, List<string>>>>(this.VariationMemoryString)!;
            bot.VariationMemory ??= new List<KeyValuePair<string, List<string>>>();
            if (!string.IsNullOrWhiteSpace(this.AttributeValueMemoryString))
                bot.AttributeValueMemory = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(this.AttributeValueMemoryString)!;
            bot.AttributeValueMemory ??= new List<KeyValuePair<string, string>>();
            if (!string.IsNullOrWhiteSpace(this.CartString))
                bot.Cart = JsonConvert.DeserializeObject<List<BotBasket>>(this.CartString)!;                                    
            bot.Cart ??= new List<BotBasket>();
            return bot;
        }

        public DbFriendlyBot ReadyToSave(MetaBotStatus bot)
        {
            if (bot == null) return this;
            this.BuyerID = bot.BuyerID;
            this.FacebookUserID = bot.FacebookUserID;
            this.FacebookUser = bot.FacebookUser;
            this.Language = bot.Language;
            this.ThinkingOf = bot.ThinkingOf;
            this.ProductMemoryString = System.Text.Json.JsonSerializer.Serialize(bot.ProductMemory);
            this.VariationMemoryString = System.Text.Json.JsonSerializer.Serialize(bot.VariationMemory);
            this.AttributeValueMemoryString = System.Text.Json.JsonSerializer.Serialize(bot.AttributeValueMemory);
            this.CartString = System.Text.Json.JsonSerializer.Serialize(bot.Cart);            
            this.BuyerCorprioID = bot.BuyerCorprioID;
            this.BuyerEmail = bot.BuyerEmail;
            this.OTP_Code = bot.OTP_Code;
            this.OTP_ExpiryTime = bot.OTP_ExpiryTime;
            return this;
        }
    }        
}

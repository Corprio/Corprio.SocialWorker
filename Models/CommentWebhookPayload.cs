using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Corprio.SocialWorker.Models
{
    /// <summary>
    /// Reference: https://developers.facebook.com/docs/graph-api/webhooks/reference/instagram/#comments
    /// Example from log:
    /// {"entry": [
    ///   {"id": "17841462460365475", "time": 1707268679, "changes": [
    ///     {"value": {"from": {"id": "24286888740958239", "username": "gengar8864"}, 
    ///                "media": {"id": "18021480532860936", "media_product_type": "FEED"}, 
    ///                "id": "17908760879912236", "text": "+1"}, 
    ///      "field": "comments"}
    ///      ]
    ///    }
    ///  ], 
    ///  "object": "instagram"}
    /// </summary>
    public class CommentWebhookPayload
    {
        [JsonProperty("object")]
        public string Object { get; set; }

        [JsonProperty("entry")]
        public List<CommentWebhookEntry> Entry { get; set; }
    }

    public class CommentWebhookEntry
    {
        /// <summary>
        /// Do not treat this ID as a unique identifier of webhook, as it can simply be the Instagram business account ID
        /// </summary>
        [JsonProperty("id")]
        public string WebhookEntryID { get; set; }

        [JsonProperty("time")]
        public double Time { get; set; }

        public DateTime FormatedTime() => DateTime.FromOADate(Time);

        [JsonProperty("changes")]
        public List<CommentWebhookChange> Changes { get; set; }
    }

    public class CommentWebhookChange
    {
        /// <summary>
        /// Name of the updated field
        /// </summary>
        [JsonProperty("field")]
        public string Field { get; set; }

        /// <summary>
        /// Value
        /// </summary>
        [JsonProperty("value")]
        public CommentWebhookChangeValue Value { get; set; }
    }

    public class CommentWebhookChangeValue
    {
        /// <summary>
        /// The id of the object
        /// </summary>
        [JsonProperty("id")]
        public string WebhookChangeID { get; set; }
        
        /// <summary>
        /// Instagram-scoped ID and username of the Instagram user who created the comment
        /// </summary>
        [JsonProperty("from")]
        public IgWebhookFrom From { get; set; }

        /// <summary>
        /// ID and product type of the IG Media the comment was created on
        /// </summary>
        [JsonProperty("media")]
        public CommentWebhookMedia Media { get; set; }

        /// <summary>
        /// Comment text
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; }
    }

    /// <summary>
    /// ID and product type of the IG Media the comment was created on
    /// </summary>
    public class CommentWebhookMedia
    {
        /// <summary>
        /// ID of the IG Media the comment was created on
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Product type of the IG Media the comment was created on
        /// </summary>
        [JsonProperty("media_product_type")]
        public string MediaProductType { get; set; }
    }

    /// <summary>
    /// Instagram-scoped ID and username of the Instagram user who created the comment
    /// </summary>
    public class IgWebhookFrom
    {
        /// <summary>
        /// Instagram-scoped ID
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Username of the Instagram user who created the comment
        /// </summary>
        [JsonProperty("username")]
        public string Username { get; set; }
    }
}

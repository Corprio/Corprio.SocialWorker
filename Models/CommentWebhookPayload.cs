using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Corprio.SocialWorker.Models
{
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
        /// Do not treat this ID as a unique identifier of webhook, as it can simply be the Facebook page's ID
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
        [JsonProperty("field")]
        public string Field { get; set; }

        [JsonProperty("value")]
        public CommentWebhookChangeValue Value { get; set; }
    }

    public class CommentWebhookChangeValue
    {
        [JsonProperty("id")]
        public string WebhookChangeID { get; set; }

        [JsonProperty("parent_id")]
        public string ParentId { get; set; }

        [JsonProperty("from")]
        public WebhookFrom From { get; set; }

        [JsonProperty("media")]
        public CommentWebhookMedia Media { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }

    public class CommentWebhookMedia
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("media_product_type")]
        public string MediaProductType { get; set; }
    }
}

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Corprio.SocialWorker.Models
{
    /// <summary>
    /// https://developers.facebook.com/docs/graph-api/webhooks/reference/page/#feed
    /// </summary>
    public class FeedWebhookPayload
    {
        [JsonProperty("object")]
        public string Object { get; set; }

        [JsonProperty("entry")]
        public List<FeedWebhookEntry> Entry { get; set; }
    }

    public class FeedWebhookEntry
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
        public List<FeedWebhookChange> Changes { get; set; }
    }

    public class FeedWebhookChange
    {
        [JsonProperty("field")]
        public string Field { get; set; }

        [JsonProperty("value")]
        public FeedWebhookChangeValue Value { get; set; }
    }

    public class FeedWebhookChangeValue
    {
        [JsonProperty("item")]
        public string Item { get; set; }

        [JsonProperty("post_id")]
        public string PostId { get; set; }

        [JsonProperty("verb")]
        public string Verb { get; set; }

        [JsonProperty("published")]
        public int Published { get; set; }

        [JsonProperty("created_time")]
        public double CreatedTime { get; set; }

        public DateTime FormatedTime() => DateTime.FromOADate(CreatedTime);

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("from")]
        public WebhookFrom From { get; set; }
    }
}

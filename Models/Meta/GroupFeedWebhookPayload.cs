using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Corprio.SocialWorker.Models.Meta
{
    public class GroupFeedWebhookPayload
    {
        [JsonProperty("object")]
        public string Object { get; set; }

        [JsonProperty("entry")]
        public List<GroupFeedWebhookEntry> Entry { get; set; }
    }

    public class GroupFeedWebhookEntry
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("time")]
        public double Time { get; set; }

        public DateTime FormatedTime() => DateTime.FromOADate(Time);

        [JsonProperty("messaging")]
        public List<GroupFeedWebhookMessaging> Messaging { get; set; }
    }

    public class GroupFeedWebhookMessaging
    {
        [JsonProperty("recipient")]
        public OneLinePayload Recipient { get; set; }

        [JsonProperty("from")]
        public FbWebhookFrom From { get; set; }

        [JsonProperty("group_id")]
        public string GroupId { get; set; }

        [JsonProperty("comment_id")]
        public string CommentId { get; set; }

        [JsonProperty("post_id")]
        public string PostId { get; set; }

        [JsonProperty("created_time")]
        public double CreatedTime { get; set; }

        public DateTime FormatedTime() => DateTime.FromOADate(CreatedTime);

        [JsonProperty("item")]
        public string Item { get; set; }

        [JsonProperty("verb")]
        public string Verb { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("field")]
        public string Field { get; set; }

        [JsonProperty("parent_id")]
        public string ParentId { get; set; }
    }
}

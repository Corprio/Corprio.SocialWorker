using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Corprio.SocialWorker.Models
{
    /// <summary>
    /// Reference: https://developers.facebook.com/docs/graph-api/webhooks/reference/page/#feed
    /// Example from log:
    /// {"entry": [
    ///   {"id": "206070979425408", "time": 1707211879, 
    ///     "changes": [
    ///     {"value": {"from": {"id": "24266381753005180", "name": "Wong Tsun-Lok Frank"}, 
    ///       "post": {"status_type": "added_photos", "is_published": true, "updated_time": "2024-02-06T09:31:16+0000", 
    ///         "permalink_url": "https://www.facebook.com/710090817985661/posts/pfbid02kicXeLy3yWi95UN9BdtvcWzqp7rxsjT8J7hSiF3MCkuoAuFdU1NAaE97Yso8Kky3l", 
    ///         "promotion_status": "inactive", "id": "206070979425408_712750021053074"}, 
    ///         "message": "+1", "post_id": "206070979425408_712750021053074", 
    ///         "comment_id": "712750021053074_1063824911537852", 
    ///         "created_time": 1707211876, "item": "comment", "parent_id": "206070979425408_712750021053074", "verb": "add"}, 
    ///       "field": "feed"}
    ///      ]
    ///     }
    ///    ], 
    ///  "object": "page"}
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
        /// <summary>
        /// Name of the updated field
        /// </summary>
        [JsonProperty("field")]
        public string Field { get; set; }

        /// <summary>
        /// The contents of the update
        /// </summary>
        [JsonProperty("value")]
        public FeedWebhookChangeValue Value { get; set; }
    }

    public class FeedWebhookChangeValue
    {
        /// <summary>
        /// The type of item:
        /// {album, address, comment, connection, coupon, event, experience, group, group_message, interest, 
        /// link, mention, milestone, note, page, picture, platform-story, photo, photo-album, post, profile, 
        /// question, rating, reaction, relationship-status, share, status, story, timeline cover, tag, video}
        /// </summary>
        [JsonProperty("item")]
        public string Item { get; set; }

        /// <summary>
        /// The post ID
        /// </summary>
        [JsonProperty("post_id")]
        public string PostId { get; set; }

        /// <summary>
        /// The type of action taken
        /// {add, block, edit, edited, delete, follow, hide, mute, remove, unblock, unhide, update}
        /// </summary>
        [JsonProperty("verb")]
        public string Verb { get; set; }
        
        [JsonProperty("created_time")]
        public double CreatedTime { get; set; }

        public DateTime FormatedTime() => DateTime.FromOADate(CreatedTime);

        /// <summary>
        /// The message that is part of the content
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        /// The sender information
        /// </summary>
        [JsonProperty("from")]
        public FbWebhookFrom From { get; set; }
    }

    /// <summary>
    /// The sender information
    /// </summary>
    public class FbWebhookFrom
    {
        /// <summary>
        /// The ID of the sender
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The name of the sender
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}

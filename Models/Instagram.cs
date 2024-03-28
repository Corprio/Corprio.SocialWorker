using System;
using Newtonsoft.Json;

namespace Corprio.SocialWorker.Models
{    
    /// <summary>
    /// Represents an Instagram album, photo, or video (uploaded video, live video, video created with the Instagram TV app, reel, or story).
    /// https://developers.facebook.com/docs/instagram-api/reference/ig-media
    /// </summary>
    public class IgMedia
    {
        /// <summary>
        /// Caption. Excludes album children. 
        /// The @ symbol is excluded, unless the app user can perform admin-equivalent tasks 
        /// on the Facebook Page connected to the Instagram account used to create the caption.
        /// </summary>
        [JsonProperty("caption")]
        public string Caption { get; set; }

        /// <summary>
        /// Media ID.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The URL for the media.
        /// The media_url field is omitted from responses if the media contains copyrighted material 
        /// or has been flagged for a copyright violation. Examples of copyrighted material can include audio on reels.
        /// </summary>
        [JsonProperty("media_url")]
        public string MediaUrl { get; set; }

        /// <summary>
        /// ISO 8601-formatted creation date in UTC (default is UTC ±00:00).
        /// </summary>
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Username of user who created the media.
        /// </summary>
        [JsonProperty("username")]
        public string Username { get; set; }
    }

    /// <summary>
    /// Represents a comment on an IG Media.
    /// https://developers.facebook.com/docs/instagram-api/reference/ig-comment
    /// </summary>
    public class IgComment
    {
        /// <summary>
        /// An object containing:
        /// * id — IGSID of the Instagram user who created the IG Comment.
        /// * username — Username of the Instagram user who created the IG Comment.
        /// </summary>
        [JsonProperty("from")]
        public IgUser From { get; set; }

        /// <summary>
        /// IG Comment ID.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// IG Comment text.
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; }
    }        

    /// <summary>
    /// IG comment on an IG comment
    /// https://developers.facebook.com/docs/instagram-api/reference/ig-comment/replies
    /// </summary>
    public class IgCommentReply
    {
        /// <summary>
        /// IG Comment ID.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// IG Comment text.
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>
        /// ISO 8601-formatted creation date in UTC (default is UTC ±00:00).
        /// </summary>
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Represents an Instagram Business Account or an Instagram Creator Account.
    /// https://developers.facebook.com/docs/instagram-api/reference/ig-user
    /// </summary>
    public class IgUser
    {
        /// <summary>
        /// App-scoped User ID.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }
    }
}

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Corprio.SocialWorker.Models
{
    /// <summary>
    /// Represents a Facebook Page.
    /// https://developers.facebook.com/docs/graph-api/reference/page/
    /// </summary>
    public class FbPage
    {
        /// <summary>
        /// The Page's access token. 
        /// Only returned if the User making the request has a role (other than Live Contributor) on the Page. 
        /// If your business requires two-factor authentication, the User must also be authenticated.
        /// </summary>
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        /// <summary>
        /// The Page's category. e.g. Product/Service, Computers/Technology. 
        /// Can be read with Page Public Content Access or Page Public Metadata Access.
        /// </summary>
        [JsonProperty("category")]
        public string Category { get; set; }

        /// <summary>
        /// The Page's sub-categories. This field will not return the parent category.
        /// https://developers.facebook.com/docs/graph-api/reference/page-category/
        /// </summary>
        [JsonProperty("category_list")]
        public List<FbPageCategory> CategoryList { get; set; }

        /// <summary>
        /// The ID representing a Facebook Page.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The name of the Page. [DEFAULT]
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("tasks")]
        public List<string> Tasks { get; set; }
    }

    /// <summary>
    /// The Page's sub-categories. This field will not return the parent category.
    /// https://developers.facebook.com/docs/graph-api/reference/page-category/
    /// </summary>
    public class FbPageCategory
    {
        /// <summary>
        /// The id of the category.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The name of the category.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    /// <summary>
    /// An individual post in a profile's feed. The profile could be a user, page, app, or group.
    /// https://developers.facebook.com/docs/graph-api/reference/post/
    /// </summary>
    public class FbPost
    {
        /// <summary>
        /// The time the post was published, expressed as UNIX timestamp. [DEFAULT]
        /// </summary>
        [JsonProperty("created_time")]
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// The message written in the post. [DEFAULT]
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        /// The post ID.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    /// <summary>
    /// A comment can be made on various types of content on Facebook. 
    /// Most Graph API nodes have a /comments edge that lists all the comments on that object. 
    /// The /{comment-id} node returns a single comment.
    /// https://developers.facebook.com/docs/graph-api/reference/v18.0/comment
    /// </summary>
    public class FbComment
    {
        /// <summary>
        /// The person that made this comment.
        /// </summary>
        [JsonProperty("from")]
        public FbUser From { get; set; }

        /// <summary>
        /// The comment ID.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The comment text.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }
    }

    /// <summary>
    /// Represents a Facebook user.
    /// https://developers.facebook.com/docs/graph-api/reference/user/
    /// </summary>
    public class FbUser
    {
        /// <summary>
        /// The app user's App-Scoped User ID. This ID is unique to the app and cannot be used by other apps.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The person's full name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}

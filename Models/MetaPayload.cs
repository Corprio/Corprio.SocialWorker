using System.Collections.Generic;
using Newtonsoft.Json;

namespace Corprio.SocialWorker.Models
{    
    public class OneLinePayload : MetaPayloadError
    {
        /// <summary>
        /// IMPORTANT: 
        /// it is NOT always possible to use this ID to query the object, as it may be a page-scoped ID, 
        /// which is assigned to a person the first time they send a message to a particular Page
        /// </summary>
        [JsonProperty("id")]
        public string MetaID { get; set; }
    }

    public class ActionResultPayload : MetaPayloadError
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
    }

    /// <summary>
    /// Payload from sending message out
    /// </summary>
    public class MessageFeedback : MetaPayloadError
    {
        [JsonProperty("recipient_id")]
        public string RecipientID { get; set; }

        [JsonProperty("message_id")]
        public string MessageID { get; set; }
    }

    /// <summary>
    /// Payload from querying media under an IG account
    /// </summary>
    public class IgMediaPayload : MetaPayloadError
    {
        [JsonProperty("data")]
        public List<IgMedia> Data { get; set; }
    }

    /// <summary>
    /// Payload from querying comments on an IG media
    /// </summary>
    public class IgMediaCommentPayload : MetaPayloadError
    {
        [JsonProperty("data")]
        public List<IgComment> Data { get; set; }
    }

    /// <summary>
    /// Payload from querying comments on an IG comment
    /// </summary>
    public class IgCommentReplyPayload : MetaPayloadError
    {
        [JsonProperty("data")]
        public List<IgCommentReply> Data { get; set; }
    }

    /// <summary>
    /// Payload from querying a FB page
    /// </summary>
    public class FbPagePayload : MetaPayloadError
    {
        /// <summary>
        /// Meta entity ID for a Facebook page
        /// </summary>
        [JsonProperty("id")]
        public string FacebookPageID { get; set; }

        /// <summary>
        /// Instagram account linked to page during Instagram business conversion flow
        /// </summary>
        [JsonProperty("instagram_business_account")]
        public IgUser InstagramBusinessAccount { get; set; }
    }

    /// <summary>
    /// Payload from querying the feeds on a FB page
    /// </summary>
    public class FbPageFeedPayload : MetaPayloadError
    {
        [JsonProperty("data")]
        public List<FbPost> Data { get; set; }
    }

    /// <summary>
    /// Payload from querying the comments on a FB post
    /// </summary>
    public class FbPostCommentPayload : MetaPayloadError
    {
        [JsonProperty("data")]
        public List<FbComment> Data { get; set; }

        [JsonProperty("paging")]
        public Pagination Paging { get; set; }
    }            
    
    public class MetaPayloadError
    {
        [JsonProperty("error")]
        public MetaErrorResponse Error { get; set; }
    }

    /// <summary>
    /// https://developers.facebook.com/docs/whatsapp/cloud-api/support/error-codes/
    /// </summary>
    public class MetaErrorResponse
    {
        /// <summary>
        /// Combination of the error code and its title. For example: (#130429) Rate limit hit.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        /// Error type.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Error code. We recommend that you build your app's error handling around error codes instead of subcodes or HTTP response status codes.
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; set; }

        /// <summary>
        /// Deprecated. Will not be returned in v16.0+ responses.
        /// Graph API subcode. 
        /// Not all responses will include a subcode, so we recommend that you build your error handling logic around code and details properties instead.
        /// </summary>
        [JsonProperty("error_subcode")]
        public string ErrorSubcode { get; set; }

        /// <summary>
        /// Trace ID you can include when contacting Direct Support. The ID may help us debug the error.
        /// </summary>
        [JsonProperty("fbtrace_id")]
        public string FbTraceId { get; set; }

        /// <summary>
        /// Method to generate a custom error message based on Meta's error
        /// </summary>
        /// <returns></returns>
        public string CustomErrorMessage()
        {
            return $"Code: {Code}; Type: {Type}; Message: {Message}.";
        }
    }

    /// <summary>
    /// Paginated results
    /// https://developers.facebook.com/docs/graph-api/results
    /// </summary>
    public class Pagination
    {
        [JsonProperty("cursors")]
        public PagingCursors Cursors { get; set; }

        /// <summary>
        /// The Graph API endpoint that will return the next page of data. 
        /// If not included, this is the last page of data. 
        /// Due to how pagination works with visibility and privacy, it is possible that a page may be empty but contain a next paging link. 
        /// Stop paging when the next link no longer appears.
        /// </summary>
        [JsonProperty("next ")]
        public string Next { get; set; }

        /// <summary>
        /// The Graph API endpoint that will return the previous page of data. 
        /// If not included, this is the first page of data.
        /// </summary>
        [JsonProperty("previous ")]
        public string Previous { get; set; }
    }

    public class PagingCursors
    {
        /// <summary>
        /// This is the cursor that points to the start of the page of data that has been returned.
        /// </summary>
        [JsonProperty("before")]
        public string Before { get; set; }

        /// <summary>
        /// This is the cursor that points to the end of the page of data that has been returned.
        /// </summary>
        [JsonProperty("after")]
        public string After { get; set; }
    }
}

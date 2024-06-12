using Newtonsoft.Json;

namespace Corprio.SocialWorker.Models.Line
{
    /// <summary>
    /// Reference: https://developers.line.biz/en/reference/messaging-api/#test-webhook-endpoint
    /// </summary>
    public class LineWebhookTestResponse
    {
        /// <summary>
        /// Result of the communication from the LINE Platform to the webhook URL.
        /// </summary>
        [JsonProperty("success")]
        public bool Success { get; set; }

        /// <summary>
        /// Reason for the response. 
        /// </summary>
        [JsonProperty("reason")]
        public string Reason { get; set; }

        /// <summary>
        /// Details of the response. 
        /// </summary>
        [JsonProperty("detail")]
        public string Detail { get; set; }
    }
}

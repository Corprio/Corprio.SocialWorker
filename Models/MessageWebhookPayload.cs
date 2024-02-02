using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Corprio.SocialWorker.Models
{
    public class MessageWebhookPayload
    {
        [JsonProperty("object")]
        public string Object { get; set; }

        [JsonProperty("entry")]
        public List<MessageWebhookEntry> Entry { get; set; }
    }

    public class MessageWebhookEntry
    {
        /// <summary>
        /// Do not treat this ID as a unique identifier of webhook, as it can simply be the Facebook page's ID
        /// </summary>
        [JsonProperty("id")]
        public string WebhookEntryID { get; set; }

        [JsonProperty("time")]
        public double Time { get; set; }

        public DateTime FormatedTime() => DateTime.FromOADate(Time);

        [JsonProperty("messaging")]
        public List<MessageWebhookMessaging> Messaging { get; set; }
    }

    public class MessageWebhookMessaging
    {
        [JsonProperty("sender")]
        public OneLinePayload Sender { get; set; }

        [JsonProperty("recipient")]
        public OneLinePayload Recipient { get; set; }

        [JsonProperty("timestamp")]
        public double Timestamp { get; set; }

        public DateTime FormatedTime() => DateTime.FromOADate(Timestamp);

        [JsonProperty("message")]
        public MessageWebhookMessage Message { get; set; }
    }

    public class MessageWebhookMessage
    {
        /// <summary>
        /// Text message ID
        /// </summary>
        [JsonProperty("mid")]
        public string Mid { get; set; }

        /// <summary>
        /// Text message
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>
        /// True if the webhook is an echo, i.e., repeating an OUTBOUND message
        /// </summary>
        [JsonProperty("is_echo")]
        public bool? IsEcho { get; set; }

        /// <summary>
        /// NLP-related properties provided by Wit.ai, which are available only if NLP has been turned on for the page
        /// </summary>
        [JsonProperty("nlp")]
        public NLPprops NLP { get; set; }
    }

    public class NLPprops
    {
        /// <summary>
        /// List of locales detected by Wit.ai
        /// </summary>
        [JsonProperty("detected_locales")]
        public List<DetectedLocale> DetectedLocales { get; set; }
    }

    public class DetectedLocale
    {
        [JsonProperty("locale")]
        public string Locale { get; set; }

        [JsonProperty("confidence")]
        public float Confidence { get; set; }
    }
}

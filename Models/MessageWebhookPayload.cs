using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Corprio.SocialWorker.Models
{
    /// <summary>
    /// Examples from log:
    /// {"object":"page",
    ///  "entry":[
    ///    {"id":"206070979425408","time":1707212090210, "messaging":[
    ///     {"sender":{"id":"24266381753005180"},"recipient":{"id":"206070979425408"},"timestamp":1707212089992,
    ///      "message":{"mid":"m_dUbr3n22bB5lu3yw12_U1AE_CMJRAw0ScbMqUb2EAOjmxmRMyKgip04DehusAun1K57iI4JUil3XkDzOcuZi7w",
    ///                 "text":"\u4e0d",
    ///                 "nlp":{"intents":[],"entities":{},"traits":{},"detected_locales":[{"locale":"zh_TW","confidence":0.5421}]}
    ///     }
    ///    ]
    ///   }
    ///  ]
    /// }    
    /// {"object":"instagram",
    ///  "entry":[
    ///    {"time":1707268409629,"id":"17841462460365475","messaging":[
    ///      {"sender":{"id":"24286888740958239"},"recipient":{"id":"17841462460365475"},"timestamp":1707268406396,
    ///       "message":{"mid":"aWdfZAG1faXRlbToxOklHTWVzc2FnZAUlEOjE3ODQxNDYyNDYwMzY1NDc1OjM0MDI4MjM2Njg0MTcxMDMwMTI0NDI1OTM4NDQzMzA2NDE5OTI0NTozMTQ5MzU0MzM1NzkyOTAyNzUxODIzNjIzMjU4MjY5Mjg2NAZDZD",
    ///                 "text":"!h"}
    ///       }
    ///      ]
    ///     }
    ///    ]
    ///   }
    /// </summary>
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
        /// True if a message was un-sent (Note: Text is null in this case)
        /// </summary>
        [JsonProperty("is_deleted")]
        public bool? IsDeleted { get; set; }

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

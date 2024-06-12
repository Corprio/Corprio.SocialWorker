using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Corprio.SocialWorker.Models.Meta
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
        /// Attachments of the message
        /// </summary>
        [JsonProperty("attachments")]
        public List<WebhookMessageAttachment> Attachments { get; set; }

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

    /// <summary>
    /// Reference: https://developers.facebook.com/docs/messenger-platform/instagram/features/story-mention/
    /// Example of story mention webhook:
    //    {
    //      object: 'instagram',
    //      entry: [
    //          {
    //              time: 1712740660167,
    //              id: '17841465042465777',
    //              messaging: [
    //                  {
    //                      sender: { id: '25071364142507508' },
    //                      recipient: { id: '17841465042465777' },
    //                      timestamp: 1712740659552,
    //                      message: {
    //                          mid: 'aWdfZAG1faXRlbToxOklHTWVzc2FnZAUlEOjE3ODQxNDY1MDQyNDY1Nzc3OjM0MDI4MjM2Njg0MTcxMDMwMTI0NDI3NjAxODIwMDg3MjI1NTA1OTozMTU5NDQ4ODYxMTQwMTMyMDUzNjE1NDI5MjUzNTQyNzA3MgZDZD',
    //                          attachments: [
    //                              {
    //                                  type: 'story_mention',
    //                                  payload: { url: 'https://lookaside.fbsbx.com/ig_messaging_cdn/?asset_id=18076257598480693&signature=Abye-Y37CDo2_N8AtgvClxzxqaegnCS1QEXlNjO3ZhHlTP8nIaFTsU8eNKQmx06QdzlmKY6VX8-wIawG67pMdGG2rhkM80pPINoxK3Gontv333WOhZWjoSFAM2z0uH8J43W4NsPX1Vm--jDisGXIyET7GfRnJ0WSDA8tE7-1sJVFKjgXYRKP6N-DhP7Wi-2r7622b8DS83M4nq9N7bEqu41xfTpsN1U' }
    //                              }
    //                          ]
    //                      }
    //                  }
    //              ]
    //          }
    //      ]
    //  }
    /// </summary>
    public class WebhookMessageAttachment
    {
        /// <summary>
        /// For example: 'story_mention'
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("payload")]
        public WebhookMessageAttachmentPayload Payload { get; set; }
    }

    public class WebhookMessageAttachmentPayload
    {
        [JsonProperty("url")]
        public string URL { get; set; }
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

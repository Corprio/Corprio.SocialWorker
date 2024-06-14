using DevExpress.Data.Linq.Helpers;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Corprio.SocialWorker.Models.Line
{
    /// <summary>
    /// Line message objects' shapes vary according to their types
    /// https://developers.line.biz/en/reference/messaging-api/#message-common-properties
    /// </summary>
    public interface ILineMessage
    {
        [JsonProperty("type")]
        public string Type { get; set; }
    }

    /// <summary>
    /// Shape of a text only message
    /// https://developers.line.biz/en/reference/messaging-api/#text-message
    /// </summary>
    public class LineTextMessage : ILineMessage
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "text";

        [JsonProperty("text")]
        public string Text { get; set; }
    }

    /// <summary>
    /// Shape of an image only message
    /// https://developers.line.biz/en/reference/messaging-api/#image-message
    /// </summary>
    public class LineImageMessage : ILineMessage
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "image";

        [JsonProperty("previewImageUrl")]
        public string PreviewImageUrl { get; set; }

        [JsonProperty("originalContentUrl")]
        public string OriginalContentUrl { get; set; }
    }

    /// <summary>
    /// Flex Messages are messages with a customizable layout. 
    /// You can customize the layout freely based on the specification for CSS Flexible Box (CSS Flexbox).
    /// Reference: https://developers.line.biz/en/reference/messaging-api/#flex-message
    /// </summary>
    public class LineFlexMessage : ILineMessage
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "flex";

        /// <summary>
        /// Alternative text. When a user receives a message, it will appear in the device's notifications, talk list, and quote messages as an alternative to the Flex Message.
        /// Max character limit: 400
        /// (Required)
        /// </summary>
        [JsonProperty("altText")]
        public string AltText { get; set; }

        /// <summary>
        /// Flex message container
        /// </summary>
        [JsonProperty("contents")]
        public ILineFlexMessageContent Contents { get; set; }
    }

    /// <summary>
    /// A 'bubble' container. 
    /// Note: there are two types of flex message containers, namely bubble and carousel.
    /// </summary>
    public class LineFlexMessageBubble : ILineFlexMessageContent
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "bubble";

        [JsonProperty("styles")]
        public LineFlexMessageBlockStyle Styles { get; set; }

        [JsonProperty("header")]
        public LineFlexMessageBox Header { get; set; }

        [JsonProperty("hero")]
        public LineFlexMessageImage Hero { get; set; }

        [JsonProperty("body")]
        public LineFlexMessageBox Body { get; set; }

        [JsonProperty("footer")]
        public LineFlexMessageBox Footer { get; set; }
    }

    public class LineFlexMessageBlockStyle
    {
        [JsonProperty("header")]
        public LineFlexMessageStyle Header { get; set; }

        [JsonProperty("body")]
        public LineFlexMessageStyle Body { get; set; }

        [JsonProperty("footer")]
        public LineFlexMessageStyle Footer { get; set; }
    }

    public class LineFlexMessageStyle
    {
        [JsonProperty("backgroundColor")]
        public string BackgroundColor { get; set; }
    }

    /// <summary>
    /// A 'box' component of flex message.
    /// Note: Component is a unit that composes a block. There are close to 10 types of compoents.
    /// </summary>
    public class LineFlexMessageBox : ILineFlexMessageContent
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "box";

        [JsonProperty("layout")]
        public string Layout { get; set; }

        [JsonProperty("contents")]
        public List<ILineFlexMessageContent> Contents { get; set; }
    }

    /// <summary>
    /// The content object within a flex message has varying shape, determined by its type
    /// https://developers.line.biz/en/docs/messaging-api/flex-message-elements/#box
    /// </summary>
    public interface ILineFlexMessageContent
    {
        [JsonProperty("type")]
        public string Type { get; set; }
    }

    /// <summary>
    /// A 'text' component of flex message.
    /// Note: Component is a unit that composes a block. There are close to 10 types of compoents.
    /// </summary>
    public class LineFlexMessageText : ILineFlexMessageContent
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "text";

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("wrap")]
        public bool Wrap { get; set; } = true;
    }

    /// <summary>
    /// An 'image' component of flex message.
    /// Note: Component is a unit that composes a block. There are close to 10 types of compoents.
    /// </summary>
    public class LineFlexMessageImage : ILineFlexMessageContent
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "image";

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }

        [JsonProperty("aspectRatio")]
        public string AspectRatio { get; set; } = "1:1";
    }
}

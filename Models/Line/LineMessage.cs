using Newtonsoft.Json;
using System.Collections.Generic;

namespace Corprio.SocialWorker.Models.Line
{
    public interface ILineMessage
    {
        [JsonProperty("type")]
        public string Type { get; set; }
    }
    
    public class LineTextMessage : ILineMessage
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "text";

        [JsonProperty("text")]
        public string Text { get; set; }
    }

    public class LineImageMessage : ILineMessage
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "image";

        [JsonProperty("previewImageUrl")]
        public string PreviewImageUrl { get; set; }

        [JsonProperty("originalContentUrl")]
        public string OriginalContentUrl { get; set; }
    }

    public class LineFlexMessage : ILineMessage
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "flex";

        [JsonProperty("altText")]
        public string AltText { get; set; }

        [JsonProperty("contents")]
        public ILineFlexMessageContent Contents { get; set; }
    }

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

    public class LineFlexMessageBox : ILineFlexMessageContent
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "box";

        [JsonProperty("layout")]
        public string Layout { get; set; }

        [JsonProperty("contents")]
        public List<ILineFlexMessageContent> Contents { get; set; }
    }

    public interface ILineFlexMessageContent
    {
        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class LineFlexMessageText : ILineFlexMessageContent
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "text";

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("wrap")]
        public bool Wrap { get; set; } = true;
    }

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

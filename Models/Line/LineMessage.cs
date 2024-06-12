using Newtonsoft.Json;

namespace Corprio.SocialWorker.Models.Line
{
    public class LineMessage
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("previewImageUrl")]
        public string PreviewImageUrl { get; set; }

        [JsonProperty("originalContentUrl")]
        public string OriginalContentUrl { get; set; }
    }
}

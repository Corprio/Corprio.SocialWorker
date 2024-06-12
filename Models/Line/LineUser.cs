using Newtonsoft.Json;
using System.Collections.Generic;

namespace Corprio.SocialWorker.Models.Line
{
    public class LineUser : LineErrorResponse
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("statusMessage")]
        public string StatusMessage { get; set; }

        [JsonProperty("pictureUrl")]
        public string PictureUrl { get; set; }
    }

    public class LineErrorResponse
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("details")]
        public List<LineErrorDetail> Details { get; set; }
    }

    public class LineErrorDetail
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("property")]
        public string Property { get; set; }
    }
}

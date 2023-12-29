using Newtonsoft.Json;

namespace Corprio.SocialWorker.Models
{            
    public class WebhookFrom
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }    
}

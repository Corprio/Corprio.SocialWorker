using Corprio.DataModel;
using Corprio.DataModel.Resources;
using System.ComponentModel.DataAnnotations;

namespace Corprio.SocialWorker.Models.Meta
{
    /// <summary>
    /// Feedwebhook that has been processed by WebhookController
    /// </summary>
    public class MetaFeedWebhook : Entity
    {
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MsgRequired")]
        [StringLength(50, MinimumLength = 1)]
        public string PostID { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MsgRequired")]
        [StringLength(50, MinimumLength = 1)]
        public string SenderID { get; set; }

        public double CreatedTime { get; set; }
    }
}

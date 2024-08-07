﻿using Corprio.DataModel;
using Corprio.DataModel.Resources;
using System.ComponentModel.DataAnnotations;

namespace Corprio.SocialWorker.Models.Meta
{
    /// <summary>
    /// Comment webhook that has been processed by WebhookController
    /// </summary>
    public class MetaCommentWebhook : Entity
    {
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MsgRequired")]
        [StringLength(50, MinimumLength = 1)]
        public string MediaItemID { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MsgRequired")]
        [StringLength(50, MinimumLength = 1)]
        public string WebhookChangeID { get; set; }
    }
}

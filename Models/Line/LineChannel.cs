using Corprio.DataModel;
using Corprio.DataModel.Resources;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

namespace Corprio.SocialWorker.Models.Line
{
    public class LineChannel : Entity
    {
        /// <summary>
        /// True if the user elected to disable the connection between this Line channel and Corprio
        /// </summary>
        public bool Dormant { get; set; }

        /// <summary>
        /// Channel ID
        /// </summary>
        //[Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MsgRequired")]
        [StringLength(20)]
        public string ChannelID { get; set; }

        /// <summary>
        /// Channel name
        /// </summary>
        [StringLength(20)]
        public string ChannelName { get; set; }

        /// <summary>
        /// Channel secret
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MsgRequired")]
        [StringLength(50, MinimumLength = 1)]
        public string ChannelSecret { get; set; }

        /// <summary>
        /// Long-lived access token
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MsgRequired")]
        [StringLength(2000, MinimumLength = 1)]
        public string ChannelToken { get; set; }

        /// <summary>
        /// Organization ID in Corprio.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MsgRequired")]
        public Guid OrganizationID { get; set; }

        /// <summary>
        /// Bots representing the FB pages and/or IG accounts that are owned by this Facebook user
        /// </summary>
        public List<DbFriendlyBot> Bots { get; set; } = [];
    }
}

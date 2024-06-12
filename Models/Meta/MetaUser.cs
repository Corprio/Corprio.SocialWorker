using Corprio.DataModel;
using Corprio.DataModel.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Corprio.SocialWorker.Models.Meta
{
    /// <summary>
    /// Facebook user
    /// </summary>
    public class MetaUser : Entity
    {
        /// <summary>
        /// True if the user elected to terminate the connection between an organization and Facebook account
        /// </summary>
        public bool Dormant { get; set; }

        /// <summary>
        /// User ID in Facebook.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MsgRequired")]
        [StringLength(50, MinimumLength = 1)]
        public string FacebookUserID { get; set; }

        /// <summary>
        /// Organization ID in Corprio.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MsgRequired")]
        public Guid OrganizationID { get; set; }

        /// <summary>
        /// User access token.
        /// (Note: Meta documentation states that the access token has no fixed or max size.
        /// Given that most tokens we received during tests were shorter than 300, 2000 as max length should be safe enough.
        /// Source: https://developers.facebook.com/docs/facebook-login/guides/access-tokens#size)
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MsgRequired")]
        [StringLength(2000, MinimumLength = 1)]
        public string Token { get; set; }

        /// <summary>
        /// Facebook pages on which the user has a role.
        /// </summary>        
        public List<MetaPage> Pages { get; set; } = [];

        /// <summary>
        /// Bots representing the FB pages and/or IG accounts that are owned by this Facebook user
        /// </summary>
        public List<DbFriendlyBot> Bots { get; set; } = [];
    }
}

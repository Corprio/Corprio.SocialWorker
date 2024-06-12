using Corprio.DataModel;
using Corprio.DataModel.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Corprio.SocialWorker.Models.Meta
{
    /// <summary>
    /// Facebook page
    /// </summary>
    public class MetaPage : Entity
    {
        /// <summary>
        /// Entity ID of Facebook user (not the ID assigned by Facebook to its user).
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MsgRequired")]
        public Guid FacebookUserID { get; set; }

        /// <summary>
        /// Facebook user object.
        /// </summary>
        [ForeignKey("FacebookUserID")]
        public MetaUser FacebookUser { get; set; }

        /// <summary>
        /// The ID representing a Facebook Page.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MsgRequired")]
        [StringLength(50, MinimumLength = 1)]
        public string PageId { get; set; }

        /// <summary>
        /// Page access token.
        /// (Note: Meta documentation states that the access token has no fixed or max size.
        /// Given that most tokens we received during were shorter than 300, 2000 as max length should be safe enough.
        /// Source: https://developers.facebook.com/docs/facebook-login/guides/access-tokens#size)
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MsgRequired")]
        [StringLength(2000, MinimumLength = 1)]
        public string Token { get; set; }

        /// <summary>
        /// The name of the Page.
        /// </summary>
        [StringLength(300)]
        public string Name { get; set; }

        /// <summary>
        /// Identifier of Instagram account linked to the page, if any, during Instagram business conversion flow.
        /// </summary>
        [StringLength(300)]
        public string InstagramID { get; set; }

        /// <summary>
        /// Posts that were made on the page or the IG account associated with the page.
        /// </summary>
        public List<MetaPost> Posts { get; set; } = [];

        /// <summary>
        /// Story mentions of the IG account associated with the page.
        /// </summary>
        public List<MetaMention> Mentions { get; set; } = [];
    }
}

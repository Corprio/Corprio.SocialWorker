using Corprio.DataModel;
using Corprio.DataModel.Resources;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Corprio.SocialWorker.Models
{        
    /// <summary>
    /// Facebook post
    /// </summary>
    public class MetaPost : Entity
    {
        /// <summary>
        /// Entity ID of Facebook page (not the ID assigned by Facebook to a page).
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MsgRequired")]
        public Guid FacebookPageID { get; set; }

        /// <summary>
        /// Facebook page object.
        /// </summary>
        [ForeignKey("FacebookPageID")]
        public MetaPage FacebookPage { get; set; }        

        /// <summary>
        /// The ID representing a post.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MsgRequired")]
        [StringLength(50, MinimumLength = 1)]
        public string PostId { get; set; }

        /// <summary>
        /// Meta product with which the post was made (e.g., Instagram).
        /// </summary>
        public MetaProduct PostedWith { get; set; }
    }
}

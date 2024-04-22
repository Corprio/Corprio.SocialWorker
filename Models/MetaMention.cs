using Corprio.DataModel;
using Corprio.DataModel.Resources;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Corprio.SocialWorker.Models
{
    /// <summary>
    /// Story mention
    /// </summary>
    public class MetaMention : Entity
    {
        /// <summary>
        /// Entity ID of Facebook page (not the ID assigned by Facebook to a page) that is mentioned or has its Instagram Business Account mentioned
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MsgRequired")]
        public Guid FacebookPageID { get; set; }

        /// <summary>
        /// Facebook page object.
        /// </summary>
        [ForeignKey("FacebookPageID")]
        public MetaPage FacebookPage { get; set; }

        /// <summary>
        /// Instagram ID of the person who creates the story mention.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MsgRequired")]
        public string CreatorID { get; set; }

        /// <summary>
        /// Instagram username of the person who creates the story mention.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MsgRequired")]
        public string CreatorName { get; set; }

        /// <summary>
        /// URL of rich media content shared by Instagram users. 
        /// The CDN URL is privacy-aware and will not return the media when the content has been deleted or has expired.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MsgRequired")]
        public string CDNUrl { get; set; }

        /// <summary>
        /// Name of FB page / IG account being mentioned
        /// </summary>
        public string Mentioned {  get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace Corprio.SocialWorker.Models
{    
    public class MetaUser
    {
        /// <summary>
        /// User ID in Facebook.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// User access token.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// IDs of pages on which the user has a role.
        /// </summary>
        public List<string> PageIds { get; set; } = new List<string>();
    }    

    public class MetaPost
    {
        public bool IsFbPost { get; set; }

        public string PageId { get; set; }
    }

    public class MetaPage
    {        
        /// <summary>
        /// The Page's access token.         
        /// </summary>        
        public string Token { get; set; }

        /// <summary>
        /// The name of the Page.
        /// </summary>        
        public string Name { get; set; }

        /// <summary>
        /// Identifier of Instagram account linked to the page, if any, during Instagram business conversion flow.
        /// </summary>
        public string IgId { get; set; }

        /// <summary>
        /// The ID representing a Facebook Page.
        /// </summary>
        public string PageId { get; set; }        
    }
}

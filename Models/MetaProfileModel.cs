using System.Collections.Generic;

namespace Corprio.SocialWorker.Models
{
    public class MetaProfileModel
    {
        /// <summary>
        /// User ID in Facebook
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// User access token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// List of pages on which the user has a role
        /// </summary>
        public List<MetaPage> Pages { get; set; } = new List<MetaPage>();

        public List<MetaChatBot> Bots { get; set; } = new List<MetaChatBot>();
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
        /// Identifier of Instagram account linked to the page, if any, during Instagram business conversion flow
        /// </summary>
        public string IgId { get; set; }

        /// <summary>
        /// The ID representing a Facebook Page.
        /// </summary>
        public string PageId { get; set; }        
    }
}

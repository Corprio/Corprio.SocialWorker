using Corprio.SocialWorker.Models;
using System.Threading.Tasks;
using System;
using System.Linq.Dynamic.Core;
using System.Linq;
using System.Collections.Generic;

namespace Corprio.SocialWorker.Helpers
{
    public class DbActionHelper
    {
        public static async Task<PostTemplate> GetTemplate(ApplicationDbContext db, Guid organizationID, MessageType messageType)
        {
            PostTemplate postTemplate = db.Templates.FirstOrDefault(x => x.OrganizationID == organizationID && x.MessageType == messageType);
            if (postTemplate == null)
            {
                List<string> defaultTemplate = messageType == MessageType.ProductPost 
                    ? DefaultTemplate.DefaultTempalte_Product 
                    : DefaultTemplate.DefaultTempalte_Catalogue;

                postTemplate = new PostTemplate()
                {
                    ID = Guid.NewGuid(),
                    OrganizationID = organizationID,
                    MessageType = messageType,
                    TemplateString = string.Join(TemplateComponent.Separator, defaultTemplate)
                };
                db.Templates.Add(postTemplate);
                await db.SaveChangesAsync();
            }
            return postTemplate;
        }
    }
}

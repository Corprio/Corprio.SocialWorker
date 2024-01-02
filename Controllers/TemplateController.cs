using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Corprio.SocialWorker.Models;
using System.Linq.Dynamic.Core;
using System.Linq;
using Corprio.SocialWorker.Helpers;
using Serilog;

namespace Corprio.SocialWorker.Controllers
{
    public class TemplateController : AspNetCore.XtraReportSite.Controllers.BaseController
    {
        private readonly ApplicationDbContext db;        

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        public TemplateController(ApplicationDbContext context) : base()
        {
            db = context;            
        }
                
        /// <summary>
        /// Convert a string into an enum that indicates the purpose of a template
        /// </summary>
        /// <param name="messageTypeString">A string that indicates the purpose of a template</param>
        /// <returns>Purpose of the template (e.g., product publication)</returns>
        /// <exception cref="ArgumentException"></exception>
        public MessageType TranslateType(string messageTypeString)
        {
            var messageType = messageTypeString switch
            {
                nameof(MessageType.CataloguePost) => MessageType.CataloguePost,
                nameof(MessageType.ProductPost) => MessageType.ProductPost,
                _ => throw new ArgumentException($"Invalid message type {messageTypeString}"),
            };
            return messageType;
        }

        /// <summary>
        /// Retrieve the template in string format
        /// </summary>
        /// <param name="organizationID">Organization ID</param>
        /// <param name="messageType">Purpose of the template (e.g., product publication)</param>
        /// <returns>Template in string format</returns>
        public async Task<string> GetTemplate([FromRoute] Guid organizationID, string messageType)
        {
            var postTemplate = await DbActionHelper.GetTemplate(db: db, organizationID: organizationID, messageType: TranslateType(messageType));
            return postTemplate?.TemplateString;
        }

        /// <summary>
        /// Retrieve the keyword that indicates shopping intention
        /// </summary>
        /// <param name="organizationID">Organization ID</param>
        /// <returns>Keyword that indicates shopping intention</returns>
        /// <exception cref="Exception"></exception>
        public string GetKeyword([FromRoute] Guid organizationID)
        {
            MetaUser metaUser = db.MetaUsers.FirstOrDefault(x => x.OrganizationID == organizationID)
                ?? throw new Exception($"Failed to find any Facebook-related information for organization {organizationID}");
            return metaUser.KeywordForShoppingIntention;
        }


        /// <summary>
        /// Save the template for publishing products/catalogues
        /// </summary>
        /// <param name="organizationID">Organization ID</param>
        /// <param name="templateString">Publication template in string format</param>
        /// <param name="messageType">Purpose of the template (e.g., product publication)</param>
        /// <param name="keyword">Keyword that indicates shopping intention</param>
        /// <returns>Status Code</returns>
        /// <exception cref="Exception"></exception>
        public async Task<IActionResult> SaveTemplate([FromRoute] Guid organizationID, string templateString, string messageType, 
            string keyword = null)
        {
            MessageType type = TranslateType(messageType);
            if (type == MessageType.ProductPost)
            {
                if (string.IsNullOrWhiteSpace(keyword)) return StatusCode(400, "Keyword for shopping intention cannot be blank.");

                keyword = UtilityHelper.UncleanAndClean(keyword.Trim());
                if (keyword.Length > 10) return StatusCode(400, "Keyword for shopping intention cannot be longer than 10 characters.");

                MetaUser metaUser = db.MetaUsers.FirstOrDefault(x => x.OrganizationID == organizationID)
                    ?? throw new Exception($"Failed to find any Facebook-related information for organization {organizationID}");
                if (metaUser.KeywordForShoppingIntention != keyword)
                {
                    metaUser.KeywordForShoppingIntention = keyword;
                    try
                    {
                        await db.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Failed to save changes to the key word for shopping intention. {ex.Message}");
                        return StatusCode(500, "Failed to save changes to the key word for shopping intention.");
                    }
                }
            }
                        
            PostTemplate postTemplate = await DbActionHelper.GetTemplate(db: db, organizationID: organizationID, messageType: type);
            postTemplate.TemplateString = UtilityHelper.UncleanAndClean(templateString);
            db.Update(postTemplate);
            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to save changes to {messageType} template. {ex.Message}");
                return StatusCode(500, $"Failed to save changes to {messageType} template.");
            }
            return StatusCode(200);
        }
    }
}
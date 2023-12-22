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
    public class SettingsController : AspNetCore.XtraReportSite.Controllers.BaseController
    {
        private readonly ApplicationDbContext db;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        public SettingsController(ApplicationDbContext context) : base()
        {
            db = context;
        }

        public override IActionResult Index([FromRoute] Guid organizationID)
        {
            return View(organizationID);
        }

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

        public async Task<string> GetTemplate([FromRoute] Guid organizationID, string messageType)
        {
            var postTemplate = await DbActionHelper.GetTemplate(db: db, organizationID: organizationID, messageType: TranslateType(messageType));
            return postTemplate?.TemplateString;
        }

        public string GetKeyword([FromRoute] Guid organizationID)
        {
            MetaUser metaUser = db.MetaUsers.FirstOrDefault(x => x.OrganizationID == organizationID)
                ?? throw new Exception($"Failed to find any Facebook-related information for organization {organizationID}");
            return metaUser.KeywordForShoppingIntention;
        }

        

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
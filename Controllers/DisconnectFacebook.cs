using Corprio.AspNetCore.Site.Controllers;
using Corprio.SocialWorker.Models;
using Serilog;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Corprio.SocialWorker.Controllers
{
    public class DisconnectFacebook : OrganizationBaseController
    {
        private readonly ApplicationDbContext db;

        public DisconnectFacebook(ApplicationDbContext context) : base() 
        {
            db = context;
        }

        public override IActionResult Index([FromRoute] Guid organizationID)
        {
            return base.Index(organizationID);
        }

        /// <summary>
        /// Disable the Meta profile(s) associated with an organization        
        /// </summary>
        /// <param name="organizationID">Organization ID</param>
        /// <returns>Status code</returns>
        public async Task<IActionResult> TerminateConnection([FromRoute] Guid organizationID)
        {
            // theoritically there should only be up to one non-dormant profile for an organization,
            // but we use Where() instead of FirstOrDefault() just to be on the safe side
            var activeMetaUsers = db.MetaUsers.Where(x => x.OrganizationID == organizationID && x.Dormant == false);
            if (!activeMetaUsers.Any()) return StatusCode(200);

            foreach (MetaUser metaUser in activeMetaUsers)
            {
                metaUser.Dormant = true;
                db.MetaUsers.Update(metaUser);
            }            
            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to put the Meta profile(s) of {organizationID} to sleep. {ex}.");
                throw;
            }
            return StatusCode(200);
        }
    }    
}

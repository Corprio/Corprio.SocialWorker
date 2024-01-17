using Corprio.AspNetCore.Site.Controllers;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Corprio.SocialWorker.Controllers
{
    public class ReconnectFacebook : OrganizationBaseController
    {
        public override IActionResult Index([FromRoute] Guid organizationID)
        {
            return base.Index(organizationID);
        }
    }    
}

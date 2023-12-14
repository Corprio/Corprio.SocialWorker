using System;
using Microsoft.AspNetCore.Mvc;
using Corprio.CorprioAPIClient;
using Corprio.AspNetCore.Site.Controllers;

namespace Corprio.SocialWorker.Controllers
{
    public class HomeController : HomeControllerBase
    {                        
        protected override IActionResult AuthenticatedLandingPage(APIClient corprio, Guid organizationID)
        {
            return RedirectToAction("Index", "ConnectFacebook", new { organizationID });
        }        
    }
}
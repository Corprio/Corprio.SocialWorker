using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.Net.Http;
using System.IO;
using System.Linq;
using Corprio.SocialWorker.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Serilog;
using Corprio.CorprioAPIClient;
using Corprio.DataModel.Business;
using DevExpress.Xpo.DB.Helpers;
using DevExpress.XtraPrinting.Shape.Native;
using Corprio.AspNetCore.Site.Controllers;

namespace Corprio.SocialWorker.Controllers
{
    public class HomeController : HomeControllerBase
    {                        
        protected override IActionResult AuthenticatedLandingPage(APIClient corprio, Guid organizationID)
        {
            return RedirectToAction("Index", "MetaApi", new { organizationID });
        }        
    }
}
using Microsoft.AspNetCore.Mvc;

namespace Corprio.SocialWorker.Controllers
{
    public class SettingsController : AspNetCore.XtraReportSite.Controllers.BaseController
    {
        public IActionResult Index() => View();
    }
}
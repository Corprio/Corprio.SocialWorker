using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Corprio.SocialWorker.Models;
using System.Linq;
using System.Net.Http;

namespace Corprio.SocialWorker.Controllers
{
    public class StoryMentionController : AspNetCore.XtraReportSite.Controllers.BaseController
    {
        private readonly ApplicationDbContext db;

        public StoryMentionController(ApplicationDbContext context) : base()
        {
            db = context;
        }

        public override IActionResult Index([FromRoute] Guid organizationID)
        {
            return base.Index(organizationID);
        }

        public IActionResult GetPage([FromQuery] string metaUserId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(metaUserId);            
            var mentions = db.MetaMentions.Where(m => m.FacebookPage.FacebookUser.FacebookUserID == metaUserId).ToList();                        
            return Json(mentions);
        }

        //public async Task<bool> TestCDN([FromServices] HttpClient httpClient, string url)
        //{
        //    var httpRequest = new HttpRequestMessage()
        //    {
        //        Method = HttpMethod.Get,
        //        RequestUri = new System.Uri(url),
        //    };

        //    var response = await httpClient.SendAsync(httpRequest);
        //    if (response.IsSuccessStatusCode)
        //    {
        //        string responseString = await response.Content.ReadAsStringAsync();
        //        return !(responseString?.Contains("Resource not available") ?? false);
        //    }
        //    return false;
        //}
    }
}

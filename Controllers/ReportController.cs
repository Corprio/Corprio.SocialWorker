using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Corprio.AspNetCore.XtraReportSite.Controllers;
using Corprio.AspNetCore.XtraReportSite;
using Corprio.AspNetCore.XtraReportSite.Services;
using Corprio.CorprioAPIClient;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using Corprio.DataModel;
using System.Linq;

namespace Corprio.SocialWorker.Controllers
{
    [Authorize]
    public class ReportController : ReportControllerBase
    {
        public ReportController(IReportStorageService reportStorageService,
            IReportDataService reportDataService,
            RequestCultureProvider requestCultureProvider,
            ILogger<ReportControllerBase> logger) : base(reportStorageService, reportDataService, requestCultureProvider, logger)
        {
        }

        
    }
}
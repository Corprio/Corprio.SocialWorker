using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Corprio.SocialWorker.Models;
using System.Linq.Dynamic.Core;
using System.Linq;
using Corprio.SocialWorker.Helpers;
using Serilog;
using Corprio.DataModel;
using Corprio.CorprioAPIClient;
using Corprio.Core.Exceptions;

namespace Corprio.SocialWorker.Controllers
{
    public class GetStartedController : AspNetCore.XtraReportSite.Controllers.BaseController
    {
        private readonly ApplicationDbContext db;
        readonly APIClient corprio;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        public GetStartedController(ApplicationDbContext context, APIClient corprio) : base()
        {
            db = context;
            this.corprio = corprio;
        }
        
        public async Task<IActionResult> Save([FromRoute] Guid organizationID, [FromForm] ApplicationSetting model)
        {
            model.OrganizationID = organizationID;
            ApplicationSetting setting = db.Settings.FirstOrDefault(x => x.OrganizationID == organizationID);
            bool newSetting = setting == null;
            Core.Utility.PropertyCopier.Copy(source: model, target: setting, ignoreNotUpdatable: false, copyKeyProperties: false, excludeProperties: null);

            if (newSetting)
            {
                db.Settings.Add(setting);
            }
            else
            {
                db.Settings.Update(setting);
            }
            await db.SaveChangesAsync();

            return StatusCode(200);
        }
        
        public override IActionResult Index([FromRoute] Guid organizationID)
        {
            ApplicationSetting applicationSetting = db.Settings.FirstOrDefault(x => x.OrganizationID == organizationID);
            bool firstVisit = applicationSetting == null;  
            bool updated = false;  // if true, then the setting needs to be saved
            applicationSetting ??= new()
            {
                ID = Guid.NewGuid(),
                OrganizationID = organizationID,
            };

            if (string.IsNullOrWhiteSpace(applicationSetting.EmailToReceiveOrder))
            {
                applicationSetting.EmailToReceiveOrder = User.Email();
                updated = true;
            }
            
            if (applicationSetting.DeliveryChargeProductID == null)
            {
                var productResult = corprio.ProductApi.Query(
                    organizationID: organizationID,
                    selector: "new(ID)",
                    where: "Code=@0 and Disabled=false",
                    whereArguments: new object[] { Constant.DefaultDeliveryChargeProductCode },
                    orderBy: "").ConfigureAwait(false).GetAwaiter().GetResult();
                if (productResult.Any())
                {
                    applicationSetting.DeliveryChargeProductID = Guid.Parse(productResult[0].ID);
                    updated = true;
                }
            }
            
            if (applicationSetting.WarehouseID == null)
            {
                var warehouseResult = corprio.WarehouseApi.Query(
                    organizationID: organizationID,
                    dynamicQuery: new DynamicQuery()
                    {
                        Selector = "new(ID)",
                        Where = "Code=@0 and Disabled=false",
                        WhereArguments = new object[] { Constant.DefaultWarehouseCode },
                        OrderBy = ""
                    }).ConfigureAwait(false).GetAwaiter().GetResult();
                if (warehouseResult.Any())
                {
                    applicationSetting.WarehouseID = Guid.Parse(warehouseResult[0].ID);
                    updated = true;
                }                
            }

            try
            {
                applicationSetting.IsSmtpSet = corprio.Execute<bool>(
                    request: new CorprioRestClient.ApiRequest($"/organization/{organizationID}/IsSMTPSet", System.Net.Http.HttpMethod.Get)).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (ApiExecutionException ex)
            {
                Log.Error($"Failed to test the latest SMTP setting of organization {organizationID}. {ex.Message}");
            }

            if (firstVisit)
            {
                db.Settings.Add(applicationSetting);
                db.SaveChangesAsync().ConfigureAwait(false).GetAwaiter();
            }
            else if (updated)
            {
                db.Settings.Update(applicationSetting);
                db.SaveChangesAsync().ConfigureAwait(false).GetAwaiter();
            }

            return View(applicationSetting);
        }
    }
}
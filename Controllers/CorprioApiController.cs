using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Corprio.DataModel;
using Corprio.Global.Geography;
using Corprio.CorprioAPIClient;
using System.Linq;

namespace Corprio.SocialWorker.Controllers
{
	public class CorprioApiController : AspNetCore.Site.Controllers.CorprioApiControllerBase
	{
		public CorprioApiController() : base()
		{
		}

        public async Task<JsonResult> GetCustomerDetails([FromServices] APIClient corprio, Guid organizationID,  Guid customerID)
        {
			var c = await corprio.CustomerApi.GetResult(
				organizationID: organizationID,
				id: customerID,
				selector: @"new(CurrencyCode,BusinessPartner)");
			return new JsonResult(c);
        }
    }
}
using System;
using Microsoft.AspNetCore.Mvc;
using Corprio.CorprioAPIClient;
using Corprio.AspNetCore.Site.Controllers;
using Corprio.SocialWorker.Models;
using Corprio.AspNetCore.Site.Services;
using Corprio.Core.Exceptions;
using Serilog;
using Corprio.DataModel;
using System.Linq;

namespace Corprio.SocialWorker.Controllers
{
    public class HomeController : HomeControllerBase
    {        
        readonly ApplicationSettingService applicationSettingService;
        readonly APIClient corprioClient;

        /// <summary>
        /// Constructor
        /// </summary>        
        /// <param name="client">Client for Api requests among Corprio projects</param>
        /// <param name="appSettingService">Application setting service</param>
        public HomeController(APIClient client, ApplicationSettingService appSettingService) : base()
        {            
            corprioClient = client;
            applicationSettingService = appSettingService;
        }

        protected override IActionResult AuthenticatedLandingPage(APIClient corprio, Guid organizationID)
        {            
            ApplicationSetting applicationSetting = applicationSettingService.GetSetting<ApplicationSetting>(organizationID).ConfigureAwait(false).GetAwaiter().GetResult();
            if (applicationSetting == null)
            {
                return RedirectToAction("Index", "GetStarted", new { organizationID });
            }
            
            // redirect the user to GetStarted if the setup for delivery method is incomplete
            if ((!applicationSetting.ShipToCustomer && !applicationSetting.SelfPickUp) 
                || (applicationSetting.SelfPickUp && string.IsNullOrWhiteSpace(applicationSetting.SelfPickUpInstruction))
                || (applicationSetting.ShipToCustomer && applicationSetting.DeliveryChargeProductID == null)
                )
            {
                return RedirectToAction("Index", "GetStarted", new { organizationID });
            }

            // redirect the user to GetStarted if SMTP is not set up for sending out confirmation email
            if (applicationSetting.SendConfirmationEmail)
            {
                try
                {
                    applicationSetting.IsSmtpSet = corprioClient.Execute<bool>(
                        request: new CorprioRestClient.ApiRequest($"/organization/{organizationID}/IsSMTPSet", System.Net.Http.HttpMethod.Get)).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                catch (ApiExecutionException ex)
                {
                    Log.Error($"Failed to test the latest SMTP setting of organization {organizationID}. {ex.Message}");
                }
                if (!applicationSetting.IsSmtpSet)
                {
                    return RedirectToAction("Index", "GetStarted", new { organizationID });
                }
            }

            // redirect the user to GetStarted if no payment method has been set up
            var paymentMethods = corprioClient.CustomerPaymentMethodApi.GetList(organizationID, loadDataOptions: new LoadDataOptions { PageSize = 1 }).ConfigureAwait(false).GetAwaiter().GetResult();
            if (!paymentMethods.Any())
            {
                return RedirectToAction("Index", "GetStarted", new { organizationID });
            }

            return RedirectToAction("Index", "ProductPublication", new { organizationID });
        }        
    }
}
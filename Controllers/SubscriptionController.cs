using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using Corprio.Core;
using Corprio.DataModel.Application;
using Corprio.DataModel.Business.ViewModel;
using Corprio.CorprioAPIClient;
using Serilog;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;

namespace Corprio.SocialWorker.Controllers
{
    [AllowAnonymous]
    public class SubscriptionController : ControllerBase
    {
        readonly IWebHostEnvironment hostingEnvironment;

        public SubscriptionController(IWebHostEnvironment hostingEnvironment) : base()
        {
            this.hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// Handle subscription webhook fired after new subscription of the application
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> New([FromServices] IEmailSender emailSender,
            [FromServices] IConfiguration configuration,
            string emailAddress,
            string organizationName,
            string languages,
            string invitationCode)
        {
            if (!HttpContext.Request.Headers.TryGetValue("secret", out Microsoft.Extensions.Primitives.StringValues secret)
                    || secret != configuration["SubscriptionWebHook:Secret"])
                return Unauthorized(); //missing or invalid secret

            //check if request is a verification test
            if (HttpContext.Request.Headers.TryGetValue("test", out Microsoft.Extensions.Primitives.StringValues isTest)
                && isTest.ToString().Equals("true", StringComparison.InvariantCultureIgnoreCase))
            {
                return Ok(configuration["SubscriptionWebHook:VerificationKey"]); //returns verification key given by Portal
            }

            if (string.IsNullOrWhiteSpace(emailAddress)) return BadRequest("Missing emailAddress");

            Dictionary<string, Models.EmailContentModel> emailContentTemplate;

            var templateJson = System.IO.File.ReadAllText(hostingEnvironment.ContentRootPath + "/Resources/NewSubscriptionEmail.json");
            if (templateJson == null) return BadRequest(@"File /Resources/NewSubscriptionEmail.json not found");

            emailContentTemplate = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, Models.EmailContentModel>>(templateJson);

            string locale = "en";
            if (!string.IsNullOrWhiteSpace(languages))
            {
                locale = languages.Split(',').FirstOrDefault();
            }

            try
            {
                string code = string.IsNullOrWhiteSpace(invitationCode) ? "default" : invitationCode;

                //send email of the subscriber
                EmailMessage mailMessage = new()
                {
                    Subject = emailContentTemplate[code].Subject.GetText(locale),
                    Body = emailContentTemplate[code].Body.GetText(locale),
                    FromEmail = new EmailAddress("noreply@corprio.com"),
                    ToEmails = new EmailAddress[] { new EmailAddress(emailAddress) }
                };                     
                await emailSender.SendEmailAsync(mailMessage);
                return Ok();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in Subscription/New");
                return BadRequest(ex.Message);
            }
        }
    }
}

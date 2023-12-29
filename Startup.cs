using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Corprio.AspNetCore.Site.Extensions;
using Corprio.AspNetCore.XtraReportSite;
using Corprio.Core;
using Microsoft.AspNetCore.Http;
using Corprio.CorprioRestClient;
using Corprio.SocialWorker.Models;
using Microsoft.EntityFrameworkCore;
using Corprio.AspNetCore.Site.Services;

namespace Corprio.SocialWorker
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        public void ConfigureServices(IServiceCollection services)
        {            
            services.AddCommonAppServices(Configuration);
            services.AddHttpClient();
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(
                Configuration.GetConnectionString("localDb")));

            services.AddSingleton<GlobalListService>();

            CorprioApiSetting corprioApiSetting = new()
            {
                ApiUrl = Configuration["CorprioApiSetting:ApiUrl"],
                ApiVersion = Configuration["CorprioApiSetting:ApiVersion"]
            };

            //add HTTP client for accessing API without user login
            services.AddClientAccessTokenHttpClient("webhookClient", "default", client => { client.ConfigureForCorprio(corprioApiSetting); });            
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // upgrade the stream so that it supports seeking and reading multiple times (essential for reading a http request as string)
            app.UseWhen(
                context => context.Request.Headers.ContainsKey("x-hub-signature-256"),
                appBuilder => appBuilder.Use(async (context, next) =>
                {
                    context.Request.EnableBuffering();
                    await next();
                })
                );

            app.UseCommonAppServices(env);            
        }
    }
}

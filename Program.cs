using System;
using System.IO;
using Azure.Identity;
using Corprio.SocialWorker.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Corprio.SocialWorker
{
    public class Program
    {
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .Build();
        public static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .CreateBootstrapLogger();

            try
            {
                Log.Information("Starting Corprio SocialWorker Service ...");

                var host = CreateHostBuilder(args).Build();

                //seed data
                Log.Information("Going to seed data");
                using var scope = host.Services.CreateScope();
                var services = scope.ServiceProvider;
                try
                {
                    Log.Information("Going to seed data");
                    using ApplicationDbContext dataContext = services.GetService<ApplicationDbContext>();
                    dataContext.Database.Migrate();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failure in seeding the local application database");
                    throw;
                }

                host.Run();
                Log.Information("Stopped cleanly");
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "SocialWorker terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    //If environment is Production/Sandbox, then read from Azure Key Fault (assume this application is hosted in Azure)
                    if (context.HostingEnvironment.IsProduction() || context.HostingEnvironment.EnvironmentName.ToLowerInvariant() == "sandbox")
                    {
                        var builtConfig = config.Build();
                        config.AddAzureKeyVault(new Uri($"https://{builtConfig["KeyVaultName"]}.vault.azure.net/"),
                            new DefaultAzureCredential());
                    }
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseDefaultServiceProvider(options => options.ValidateScopes = false); //required for using AspNetCore.Site services
                })
            .UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.Debug());
    }
}

using Corprio.AspNetCore.Site.Models;
using Corprio.AspNetCore.Site.Services;
using System;

namespace Corprio.SocialWorker.Services
{
    public class ProductTourService : IProductTourService
    {
        public ProductTour GetTour(string name, Guid? organizationID)
        {
            return name switch
            {
                "getstarted.index" => new ProductTour
                {
                    Steps = new[]
                         {
                             new DriveStep {
                                 Popover = new Popover{
                                     Title="Welcome",
                                     Description = "Welcome"
                                 }
                             },
                             new DriveStep {
                                 Element = "#sidebar",
                                 Popover = new Popover{
                                     Description = "heading"
                                 }
                             },
                             new DriveStep {
                                 Element = "#template-section",
                                 Popover = new Popover{
                                     Description = "template"
                                 }
                             }
                         }
                },
                "productpublication.index" => new ProductTour
                {
                    Steps = new[]
                         {
                             new DriveStep {
                                 Popover = new Popover{
                                     Title="Welcome",
                                     Description = "Welcome"
                                 }
                             },                             
                         }
                },
                _ => null,
            };
        }
    }
}

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
                            Popover = new Popover
                            {
                                Title = Resources.SharedResource.Tour_Welcome_Title,
                                Description = string.Format(Resources.SharedResource.Tour_Welcome_Desc, Resources.SharedResource.AppName),
                            }
                        },
                        new DriveStep {
                            Element = "#menu-item-GetStarted-0-0",
                            Popover = new Popover
                            {
                                Description = string.Format(Resources.SharedResource.Tour_GetStarted_Desc, Resources.SharedResource.AppName)
                            }
                        },
                        new DriveStep {
                            Element = "#menu-item-ProductPublication-0-1",
                            Popover = new Popover
                            {
                                Description = Resources.SharedResource.Tour_ProductPublication_Desc,
                            }
                        },
                        new DriveStep {
                            Element = "#menu-item-DisconnectFacebook-0-2",
                            Popover = new Popover
                            {
                                Description = Resources.SharedResource.Tour_DisconnectFacebook_Desc,
                            }
                        },
                        new DriveStep {
                            Element = "#fb-section",
                            Popover = new Popover
                            {
                                Description = string.Format(Resources.SharedResource.Tour_FbSection_Desc, Resources.SharedResource.AppName),
                            }
                        },
                        new DriveStep {
                            Element = "#som-section",
                            Popover = new Popover
                            {
                                Description = string.Format(Resources.SharedResource.Tour_SomSection_Desc, Resources.SharedResource.AppName)
                            }
                        },
                        new DriveStep {
                            Element = "#delivery-section",
                            Popover = new Popover
                            {
                                Description = Resources.SharedResource.Tour_DelliverySection_Desc,
                            }
                        },
                        new DriveStep {
                            Element = "#payment-section",
                            Popover = new Popover
                            {
                                Description = Resources.SharedResource.Tour_PaymentSection_Desc,
                            }
                        },
                        new DriveStep {
                            Element = "#checkout-section",
                            Popover = new Popover
                            {
                                Description = Resources.SharedResource.Tour_CheckoutSection_Desc,
                            }
                        },
                        new DriveStep {
                            Element = "#thankyou-section",
                            Popover = new Popover
                            {
                                Description = Resources.SharedResource.Tour_ThankyouSection_Desc,
                            }
                        },
                        new DriveStep {
                            Element = "#template-section",
                            Popover = new Popover
                            {
                                Description = Resources.SharedResource.Tour_TemplateSection_Desc,
                            }
                        },
                        new DriveStep {
                            Element = ".save-setting-btn",
                            Popover = new Popover
                            {
                                Description = string.Format(Resources.SharedResource.Tour_SaveButton_Desc, Resources.SharedResource.AppName),
                            }
                        },
                        new DriveStep {
                            Element = ".tourButton",
                            Popover = new Popover
                            {
                                Description = Resources.SharedResource.Tour_Rerun,
                            }
                        },
                    }
                },
                "productpublication.index" => new ProductTour
                {
                    Steps = new[]
                    {
                        new DriveStep {
                            Element = ".publish-btn",
                            Popover = new Popover
                            {
                                Description = Resources.SharedResource.Tour_PublishButton_Desc,
                            }
                        },
                    }
                },                
                _ => null,
            };
        }
    }
}

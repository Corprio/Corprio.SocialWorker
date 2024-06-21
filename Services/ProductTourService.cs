using Corprio.AspNetCore.Site.Models;
using Corprio.AspNetCore.Site.Services;
using System;

namespace Corprio.SocialWorker.Services
{
    public class ProductTourService : IProductTourService
    {                                        
        public ProductTour GetTour(string name, Guid? organizationID, bool isWideScreen)
        {                        
            return name switch
            {
                "getstarted.index" => new ProductTour
                {
                    Steps = [
                        new DriveStep
                        {
                            Popover = new Popover
                            {
                                Title = Resources.SharedResource.tour_title_Welcome,
                                Description = Resources.SharedResource.tour_desc_Welcome,
                            }
                        },
                        new DriveStep
                        {
                            Element = "#menu-item-GetStarted-0-0",
                            Popover = new Popover
                            {
                                Title = Resources.SharedResource.tour_title_GetStarted,
                                Description = Resources.SharedResource.tour_desc_GetStarted,
                            }
                        },
                        new DriveStep
                        {
                            Element = "#menu-item-ProductPublication-0-1",
                            Popover = new Popover
                            {
                                Title = Resources.SharedResource.tour_title_ProductPublication,
                                Description = Resources.SharedResource.tour_desc_ProductPublication,
                            }
                        },
                        new DriveStep
                        {
                            Element = "#menu-item-DisconnectFacebook-0-2",
                            Popover = new Popover
                            {
                                Title = Resources.SharedResource.tour_title_DisconnectFacebook,
                                Description = Resources.SharedResource.tour_desc_DisconnectFacebook,
                            }
                        },
                        new DriveStep
                        {
                            Element = "#fb-section",
                            Popover = new Popover
                            {
                                Title = Resources.SharedResource.tour_title_ConnectFacebook,
                                Description = Resources.SharedResource.tour_desc_ConnectFacebook,
                            }
                        },
                        new DriveStep
                        {
                            Element = "#line-section",
                            Popover = new Popover
                            {
                                Title = Resources.SharedResource.tour_title_ConnectLine,
                                Description = Resources.SharedResource.tour_desc_ConnectLine,
                            }
                        },
                        new DriveStep
                        {
                            Element = "#som-section",
                            Popover = new Popover
                            {
                                Title = Resources.SharedResource.tour_title_SomSection,
                                Description = Resources.SharedResource.tour_desc_SomSection,
                            }
                        },
                        new DriveStep
                        {
                            Element = "#delivery-section",
                            Popover = new Popover
                            {
                                Title = Resources.SharedResource.tour_title_DeliverySection,
                                Description = Resources.SharedResource.tour_desc_DeliverySection,
                            }
                        },
                        new DriveStep
                        {
                            Element = "#payment-section",
                            Popover = new Popover
                            {
                                Title = Resources.SharedResource.tour_title_PaymentSection,
                                Description = Resources.SharedResource.tour_desc_PaymentSection,
                            }
                        },
                        new DriveStep
                        {
                            Element = "#checkout-section",
                            Popover = new Popover
                            {
                                Title = Resources.SharedResource.tour_title_CheckoutSection,
                                Description = Resources.SharedResource.tour_desc_CheckoutSection,
                            }
                        },
                        new DriveStep
                        {
                            Element = "#thankyou-section",
                            Popover = new Popover
                            {
                                Title = Resources.SharedResource.tour_title_ThankyouSection,
                                Description = Resources.SharedResource.tour_desc_ThankyouSection,
                            }
                        },
                        new DriveStep
                        {
                            Element = "#broadcast-setting",
                            Popover = new Popover
                            {
                                Title = Resources.SharedResource.tour_title_BroadcastSetting,
                                Description = Resources.SharedResource.tour_desc_BroadcastSetting,
                            }
                        },
                        new DriveStep
                        {
                            Element = "#product-setting",
                            Popover = new Popover
                            {
                                Title = Resources.SharedResource.tour_title_ProductSetting,
                                Description = Resources.SharedResource.tour_desc_ProductSetting,
                            }
                        },
                        new DriveStep
                        {
                            Element = ".save-setting-btn",
                            Popover = new Popover
                            {
                                Title = Resources.SharedResource.tour_title_SaveButton,
                                Description = Resources.SharedResource.tour_desc_SaveButton,
                            }
                        },
                        DriveStep.QuickLaunchStep(),
                        DriveStep.SwitchCompanyStep(),
                        DriveStep.ManageAccountStep(),
                        DriveStep.ChangeLanguageStep(),
                        DriveStep.LastStep()
                        ]
                },
                "productpublication.index" => new ProductTour
                {
                    Steps = [
                        new DriveStep
                        {
                            Element = ".publish-col",
                            Popover = new Popover
                            {
                                Title = Resources.SharedResource.tour_title_SelectProduct,
                                Description = Resources.SharedResource.tour_desc_SelectProduct,
                            },
                        },
                        new DriveStep
                        {
                            Element = ".dx-icon-upload, .dx-icon-exportselected",
                            Popover = new Popover
                            {
                                Title = Resources.SharedResource.tour_title_PublishButton,
                                Description = Resources.SharedResource.tour_desc_PublishButton,
                            },
                        },
                        new DriveStep
                        {
                            Popover = new Popover
                            {
                                Title = Resources.SharedResource.tour_title_PublishProductDemo,
                                Description = Resources.SharedResource.tour_desc_PublishProductDemo,
                            },
                        },
                        new DriveStep
                        {
                            Popover = new Popover
                            {                                
                                Title = Resources.SharedResource.tour_title_PublishedInFacebook,
                                Description = Resources.SharedResource.tour_desc_PublishedInFacebook,
                            },
                        },
                        new DriveStep
                        {
                            Popover = new Popover
                            {                                
                                Title = Resources.SharedResource.tour_title_PublishedInInstagram,
                                Description = Resources.SharedResource.tour_desc_PublishedInInstagram,
                            },
                        },
                        new DriveStep
                        {
                            Popover = new Popover
                            {                                
                                Title = Resources.SharedResource.tour_title_PublishedInLine,
                                Description = Resources.SharedResource.tour_desc_PublishedInLine,
                            },
                        },
                        new DriveStep
                        {
                            Popover = new Popover
                            {
                                PopoverClass = "corprio-wide-popover",
                                Title = Resources.SharedResource.tour_title_HowToAddMyLineChannel,
                                Description = Resources.SharedResource.tour_desc_HowToAddMyLineChannel,
                            },
                        },
                        .. DriveStep.StandardGridIntro(isWideScreen: isWideScreen, hasLines: true),
                        DriveStep.LastStep()],                    
                },                
                _ => null,
            };
        }
    }
}

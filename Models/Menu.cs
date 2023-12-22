using Corprio.AspNetCore.Site.Models;
using System.Collections.Generic;

namespace Corprio.SocialWorker.Models
{
    public static class Menu
    {
        public static AppMenu StandardMenu => new AppMenu
        {
            Items = new List<AppMenuItem> {
                new AppMenuItem { 
                    ShowInBreadcrumb = false, Url = "/[organizationID]/ConnectFacebook", Controller = "MetaApi",
                    ID = "ConnectFacebook", Text = "ConnectFacebook", Icon="fa-regular fa-solid fa-arrow-right-arrow-left"
                },
                //new AppMenuItem {
                //    ShowInBreadcrumb = false, Url = "/[organizationID]/CataloguePublication", Controller = "CataloguePublication",
                //    ID = "CataloguePublication", Text = "PublishCatalogue", Icon="fa-regular fa-bag-shopping"
                //},
                new AppMenuItem {
                    ShowInBreadcrumb = false, Url = "/[organizationID]/ProductPublication", Controller = "ProductPublication",
                    ID = "ProductPublication", Text = "PublishProduct", Icon="fa-regular fa-bags-shopping"
                },
                new AppMenuItem {
                    ShowInBreadcrumb = false, Url = "/[organizationID]/Settings", Controller = "Settings",
                    ID = "Settings", Text = "Settings", Icon="fa-regular fa-gear"
                },
            }
        };
    }
}

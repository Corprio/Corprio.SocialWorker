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
                    ShowInBreadcrumb = false, Url = "/[organizationID]/GetStarted", Controller = "GetStarted",
                    ID = "GetStarted", Text = "GetStarted", Icon="fa-regular fa-gear"
                },
                new AppMenuItem {
                    ShowInBreadcrumb = false, Url = "/[organizationID]/ProductPublication", Controller = "ProductPublication",
                    ID = "ProductPublication", Text = "PublishProduct", Icon="fa-regular fa-bags-shopping"
                },
                //new AppMenuItem {
                //    ShowInBreadcrumb = false, Url = "/[organizationID]/CataloguePublication", Controller = "CataloguePublication",
                //    ID = "CataloguePublication", Text = "PublishCatalogue", Icon="fa-regular fa-bag-shopping"
                //},
                new AppMenuItem {
                    ShowInBreadcrumb = false, Url = "/[organizationID]/ReconnectFacebook", Controller = "ReconnectFacebook",
                    ID = "ReconnectFacebook", Text = "ReconnectFacebook", Icon="fa-regular fa-unlink"
                },
            }
        };
    }
}

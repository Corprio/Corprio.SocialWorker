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
                    ShowInBreadcrumb = false, Url = "/[organizationID]/Home", Controller = "Home",
                    ID = "Home", Text = "Home page", Icon="fa-regular fa-home"   
                },                             
            }
        };
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace sw_EnligateWeb
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{resource}.ashx/{*pathInfo}");

            //routes.MapRoute(
            //    name: "AddLeague",
            //    url: "Home/AddLeague/{league}",
            //    defaults: new { controller = "Home", action = "AddLeague"}
            //);

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
           
            routes.MapRoute(
                name: "signin-google",
                url: "signin-google",
                defaults: new { controller = "Account", action = "LoginExternalConfirmAsync" }
            );

            routes.MapRoute(
              name: "Chat",
              url: "{controller}/{action}/{id}",
              defaults: new { controller = "Admin", action = "ChatEnligate", id = UrlParameter.Optional }
          );
        }
    }
}

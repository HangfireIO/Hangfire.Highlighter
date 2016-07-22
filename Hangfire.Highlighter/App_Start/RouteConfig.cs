using System.Web.Mvc;
using System.Web.Routing;

namespace Hangfire.Highlighter
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("home", "", new { controller = "Home", action = "Index" });
            routes.MapRoute("create", "create", new { controller = "Home", action = "Create" });
        }
    }
}

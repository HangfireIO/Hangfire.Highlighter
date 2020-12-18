using System.Data.Entity;
using System.Web.Mvc;
using System.Web.Routing;
using Hangfire.Highlighter.Migrations;
using Hangfire.Highlighter.Models;
using Serilog;

namespace Hangfire.Highlighter
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            Database.SetInitializer(new MigrateDatabaseToLatestVersion<HighlighterDbContext, Configuration>());
        }

        protected void Application_End()
        {
            Log.CloseAndFlush();
        }
    }
}

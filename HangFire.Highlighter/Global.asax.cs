using System.Data.Entity;
using System.Web.Mvc;
using System.Web.Routing;
using HangFire.Highlighter.Migrations;
using HangFire.Highlighter.Models;

namespace HangFire.Highlighter
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            Database.SetInitializer(new MigrateDatabaseToLatestVersion<HighlighterDbContext, Configuration>());
        }

        protected void Application_BeginRequest()
        {
            StackExchange.Profiling.MiniProfiler.Start();
        }

        protected void Application_EndRequest()
        {
            StackExchange.Profiling.MiniProfiler.Stop();
        }
    }
}

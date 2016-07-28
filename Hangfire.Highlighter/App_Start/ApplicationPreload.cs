using System.Data.Entity;
using System.Web.Hosting;
using Hangfire.Highlighter.Migrations;
using Hangfire.Highlighter.Models;

namespace Hangfire.Highlighter
{
    public class ApplicationPreload : IProcessHostPreloadClient
    {
        public void Preload(string[] parameters)
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<HighlighterDbContext, Configuration>());
            HangfireAspNet.Use(Startup.GetHangfireConfiguration);
        }
    }
}
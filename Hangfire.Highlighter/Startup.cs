using Hangfire.Dashboard;
using Hangfire.Highlighter;
using Hangfire.Highlighter.Jobs;
using Microsoft.Owin;
using Owin;

[assembly:OwinStartup(typeof(Startup))]

namespace Hangfire.Highlighter
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();

            GlobalConfiguration.Configuration.UseSqlServerStorage("HighlighterDb");
            
            RecurringJob.AddOrUpdate<SnippetHighlighter>(
                "SnippetHighlighter.CleanUp",
                x => x.CleanUpAsync(), 
                Cron.Daily);

            var options = new DashboardOptions
            {
                Authorization = new IDashboardAuthorizationFilter[0]
            };
            
            app.UseHangfireDashboard("/hangfire", options);
            app.UseHangfireServer();
        }
    }
}
using System;
using System.Collections.Generic;
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
        public static IEnumerable<IDisposable> GetHangfireConfiguration()
        {
            GlobalConfiguration.Configuration.UseSqlServerStorage("HighlighterDb");

            yield return new BackgroundJobServer();
        }

        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();

            app.UseHangfireAspNet(GetHangfireConfiguration);
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new IDashboardAuthorizationFilter[0]
            });
            
            RecurringJob.AddOrUpdate<SnippetHighlighter>(
                "SnippetHighlighter.CleanUp",
                x => x.CleanUpAsync(), 
                Cron.Daily);
        }
    }
}
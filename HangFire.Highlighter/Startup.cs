using System;
using Hangfire.Highlighter;
using Hangfire.Highlighter.Jobs;
using Hangfire.SqlServer;
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

            GlobalConfiguration.Configuration.UseSqlServerStorage(
                "HighlighterDb",
                new SqlServerStorageOptions { QueuePollInterval = TimeSpan.FromSeconds(1) });

            RecurringJob.AddOrUpdate<SnippetHighlighter>(x => x.CleanUp(), "0 0 * * *");

            app.UseHangfireDashboard();
            app.UseHangfireServer();
        }
    }
}
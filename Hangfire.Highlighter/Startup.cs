using System;
using System.Collections.Generic;
using System.Transactions;
using Hangfire.Dashboard;
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
        public static IEnumerable<IDisposable> GetHangfireConfiguration()
        {
            GlobalConfiguration.Configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage("HighlighterDb", new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromSeconds(30),
                    QueuePollInterval = TimeSpan.Zero,
                    TransactionIsolationLevel = IsolationLevel.ReadCommitted,
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(1),
                    UsePageLocksOnDequeue = true,
                    DisableGlobalLocks = true
                });

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
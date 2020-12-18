using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using Hangfire.Dashboard;
using Hangfire.Highlighter;
using Hangfire.Highlighter.Jobs;
using Hangfire.SqlServer;
using Microsoft.Owin;
using Owin;
using Serilog;
using Serilog.Exceptions;

[assembly:OwinStartup(typeof(Startup))]

namespace Hangfire.Highlighter
{
    public class Startup
    {
        public static IEnumerable<IDisposable> GetHangfireConfiguration()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.WithProperty("App", "Hangfire.Highlighter")
                .Enrich.WithMachineName()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .Enrich.WithExceptionDetails()
                .WriteTo.Seq("https://logs.hangfire.io", apiKey: ConfigurationManager.AppSettings["SeqApiKey"])
                .MinimumLevel.Verbose()
                .CreateLogger();

            GlobalConfiguration.Configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSerilogLogProvider()
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage("HighlighterDb", new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromSeconds(30),
                    QueuePollInterval = TimeSpan.FromMilliseconds(450),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(1),
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true,
                    EnableHeavyMigrations = true
                });

            yield return new BackgroundJobServer(new BackgroundJobServerOptions
            {
                WorkerCount = 4,
                StopTimeout = TimeSpan.FromSeconds(5)
            });
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
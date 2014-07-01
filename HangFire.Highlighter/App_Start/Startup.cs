using HangFire.Highlighter;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Owin;
using Owin;

[assembly:OwinStartup(typeof(Startup))]

namespace HangFire.Highlighter
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();

            app.UseHangfire(config =>
            {
                config.UseAuthorizationFilters();

                config.UseSqlServerStorage("HighlighterDb");
                config.UseServer();
            });
        }
    }
}
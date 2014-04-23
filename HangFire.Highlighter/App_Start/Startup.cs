using HangFire.Highlighter;
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
        }
    }
}
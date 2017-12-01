using LibiadaWeb;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;

using Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace LibiadaWeb
{
    using AutoMapper;

    using LibiadaWeb.Tasks;

    /// <summary>
    /// The startup.
    /// </summary>
    public partial class Startup
    {
        /// <summary>
        /// The configuration.
        /// </summary>
        /// <param name="app">
        /// The app.
        /// </param>
        public void Configuration(IAppBuilder app)
        {
            Mapper.Initialize(cfg => cfg.CreateMap<TaskData, TaskData>());
            ConfigureAuth(app);

            app.MapSignalR();

            // Requiring auth for all signalR hubs
            GlobalHost.HubPipeline.RequireAuthentication();
        }
    }
}

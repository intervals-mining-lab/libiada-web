using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(LibiadaWeb.Startup))]
namespace LibiadaWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

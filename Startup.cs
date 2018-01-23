using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(sw_EnligateWeb.Startup))]
namespace sw_EnligateWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

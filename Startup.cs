using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Tellyt.Startup))]
namespace Tellyt
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

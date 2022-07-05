using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Occupancy.Startup))]
namespace Occupancy
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

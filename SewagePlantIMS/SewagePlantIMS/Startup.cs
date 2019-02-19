using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SewagePlantIMS.Startup))]
namespace SewagePlantIMS
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {

        }
    }
}

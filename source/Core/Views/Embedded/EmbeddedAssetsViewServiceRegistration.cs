using System.Collections.Generic;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Views.Embedded
{
    public class EmbeddedAssetsViewServiceRegistration : Registration<IViewService>
    {
        public EmbeddedAssetsViewServiceRegistration(EmbeddedAssetsViewServiceConfiguration config)
        {
            this.TypeFactory = () => new EmbeddedAssetsViewService(config);
        }
    }

    public class EmbeddedAssetsViewServiceConfiguration
    {
        public EmbeddedAssetsViewServiceConfiguration()
        {
            Stylesheets = new HashSet<string>();
            // adding default CSS here so hosting application can choose to remove it
            Stylesheets.Add("~/assets/styles.min.css");
            
            Scripts = new HashSet<string>();
        }
        
        public ICollection<string> Stylesheets { get; set; }
        public ICollection<string> Scripts { get; set; }
    }
}

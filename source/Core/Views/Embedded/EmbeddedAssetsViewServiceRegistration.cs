using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            Scripts = new HashSet<string>();
        }
        
        public ICollection<string> Stylesheets { get; set; }
        public ICollection<string> Scripts { get; set; }
    }
}

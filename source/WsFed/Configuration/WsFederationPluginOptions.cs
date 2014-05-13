using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.WsFed.Services;

namespace Thinktecture.IdentityServer.WsFed.Configuration
{
    public class WsFederationPluginOptions
    {
        public Dictionary<Type, Func<object>> Dependencies { get; set; }
        public Func<IRelyingPartyService> RelyingPartyService { get; set; }
    }
}

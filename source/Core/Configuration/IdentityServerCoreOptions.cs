using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Authentication;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core
{
    public class IdentityServerServiceFactory
    {
        public Func<ILogger> Logger { get; set; }
        public Func<IAuthenticationService> Authentication { get; set; }
    }

    public class IdentityServerCoreOptions
    {
        public IdentityServerServiceFactory Factory { get; set; }

        //public ILogger Logger { get; set; }
        //public ICoreConfiguration Configuration { get; set; }

        //public IAuthenticationService Authentication { get; set; }
        //public IProfileService Profile { get; set; }
    }

    public interface ICoreConfiguration
    {

    }
}

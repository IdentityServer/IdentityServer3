using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Authentication;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Connect.Services;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core
{
    public class IdentityServerServiceFactory
    {
        public Func<ILogger> Logger { get; set; }
        public Func<IUserService> UserService { get; set; }
        public Func<ICoreSettings> CoreSettings { get; set; }
        public Func<IAuthorizationCodeStore> AuthorizationCodeStore { get; set; }
        public Func<ITokenHandleStore> TokenHandleStore { get; set; }

        internal void Validate()
        {
            if (Logger == null) throw new InvalidOperationException("Logger not configured");
            if (UserService == null) throw new InvalidOperationException("UserService not configured");
            if (CoreSettings == null) throw new InvalidOperationException("CoreSettings not configured");
            if (AuthorizationCodeStore == null) throw new InvalidOperationException("AuthorizationCodeStore not configured");
            if (TokenHandleStore == null) throw new InvalidOperationException("TokenHandleStore not configured");
        }
    }

    public class IdentityServerCoreOptions
    {
        public IdentityServerServiceFactory Factory { get; set; }

        //public System.Security.Cryptography.X509Certificates.X509Certificate2 SigningCert { get; set; }

        //public ILogger Logger { get; set; }
        //public ICoreConfiguration Configuration { get; set; }

        //public IAuthenticationService Authentication { get; set; }
        //public IProfileService Profile { get; set; }
    }
}

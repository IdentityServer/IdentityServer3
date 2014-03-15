using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Connect.Services;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public class IdentityServerServiceFactory
    {
        // mandatory
        public Func<ILogger> Logger { get; set; }
        public Func<IUserService> UserService { get; set; }
        public Func<ICoreSettings> CoreSettings { get; set; }
        public Func<IAuthorizationCodeStore> AuthorizationCodeStore { get; set; }
        public Func<ITokenHandleStore> TokenHandleStore { get; set; }
        public Func<IConsentService> ConsentService { get; set; }
        
        // optional
        public Func<ITokenHandleStore> AssertionGrantValidator { get; set; }
        public Func<ITokenHandleStore> ClaimsProvider { get; set; }
        public Func<ICustomRequestValidator> CustomRequestValidator { get; set; }

        internal void Validate()
        {
            if (Logger == null) throw new InvalidOperationException("Logger not configured");
            if (UserService == null) throw new InvalidOperationException("UserService not configured");
            if (CoreSettings == null) throw new InvalidOperationException("CoreSettings not configured");
            if (AuthorizationCodeStore == null) throw new InvalidOperationException("AuthorizationCodeStore not configured");
            if (TokenHandleStore == null) throw new InvalidOperationException("TokenHandleStore not configured");
            if (ConsentService == null) throw new InvalidOperationException("ConsentService not configured");
        }
    }
}

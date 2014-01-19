using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Connect.TestServices;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.TestServices
{
    public class TestOptionsFactory
    {
        public static IdentityServerServiceFactory Create()
        {
            var codeStore = new TestAuthorizationCodeStore();
            var tokenStore = new TestTokenHandleStore();
            var core = new TestCoreSettings();
            var fact = new IdentityServerServiceFactory
            {
                Logger = () => new DebugLogger(),
                UserService = () => new TestUserService(),
                AuthorizationCodeStore = () => codeStore,
                TokenHandleStore = () => tokenStore,
                CoreSettings = () => core,
            };
            return fact;
        }
    }
}

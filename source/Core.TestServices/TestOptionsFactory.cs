using Thinktecture.IdentityServer.Core.Configuration;
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
            var consent = new TestConsentService();
            
            var fact = new IdentityServerServiceFactory
            {
                Logger = () => new DebugLogger(),
                UserService = () => new TestUserService(),
                AuthorizationCodeStore = () => codeStore,
                TokenHandleStore = () => tokenStore,
                CoreSettings = () => core,
                ConsentService = () => consent
            };

            return fact;
        }
    }
}

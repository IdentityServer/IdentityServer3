using Owin;
using SelfHost.Config;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Host.Config;
using Thinktecture.IdentityServer.WsFederation.Configuration;
using Thinktecture.IdentityServer.WsFederation.Services;


namespace SelfHost
{
    internal class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var factory = LocalTestFactory.Create(
                    issuerUri: "https://idsrv3.com",
                    siteName: "Thinktecture IdentityServer v3 - preview 1 (SelfHost)",
                    publicHostAddress: "http://localhost:3333");

            var opts = new IdentityServerCoreOptions
            {
                Factory = factory,
                ConfigurePlugins = ConfigurePlugins
            };

            appBuilder.UseIdentityServerCore(opts);
        }

        private void ConfigurePlugins(IAppBuilder pluginApp, IdentityServerCoreOptions coreOptions)
        {
            var wsfedOptions = new WsFederationPluginOptions
            {
                // todo - also signoutcleanup is broken right now
                LoginPageUrl = "http://localhost:3333/core/login",
                LogoutPageUrl = "http://localhost:3333/core/connect/logout",

                Factory = new WsFederationServiceFactory
                {
                    UserService = coreOptions.Factory.UserService,
                    CoreSettings = coreOptions.Factory.CoreSettings,
                    RelyingPartyService = () => new InMemoryRelyingPartyService(LocalTestRelyingParties.Get()),
                    WsFederationSettings = () => new LocalTestWsFederationSettings()
                },
            };

            pluginApp.UseWsFederationPlugin(wsfedOptions);
        }
    }
}
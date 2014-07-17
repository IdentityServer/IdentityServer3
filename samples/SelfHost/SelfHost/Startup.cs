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
            var factory = Factory.Create(
                    issuerUri: "https://idsrv3.com",
                    siteName: "Thinktecture IdentityServer v3 - preview 1 (SelfHost)");

            var options = new IdentityServerOptions
            {
                PublicHostName = "http://localhost:3333",
                Factory = factory,
                ConfigurePlugins = ConfigurePlugins
            };

            appBuilder.UseIdentityServer(options);
        }

        private void ConfigurePlugins(IAppBuilder pluginApp, IdentityServerOptions options)
        {
            var wsFedOptions = new WsFederationPluginOptions
            {
                Factory = new WsFederationServiceFactory
                {
                    UserService = options.Factory.UserService,
                    CoreSettings = options.Factory.CoreSettings,
                    RelyingPartyService = Registration.RegisterFactory<IRelyingPartyService>(() => new InMemoryRelyingPartyService(RelyingParties.Get())),
                    WsFederationSettings = Registration.RegisterFactory<WsFederationSettings>(() => new WsFedSettings())
                },
            };

            pluginApp.UseWsFederationPlugin(wsFedOptions);
        }
    }
}
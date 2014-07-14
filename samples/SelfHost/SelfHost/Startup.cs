using Owin;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Host.Config;


namespace SelfHost
{
    internal class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var factory = Factory.Create(
                    issuerUri: "https://idsrv3.com",
                    siteName: "Thinktecture IdentityServer v3 - preview 1 (SelfHost)",
                    publicHostAddress: "http://localhost:3333");

            var options = new IdentityServerOptions
            {
                Factory = factory,
                //ConfigurePlugins = ConfigurePlugins
            };

            appBuilder.UseIdentityServer(options);
        }

        // need to update wsfed nuget first
        //private void ConfigurePlugins(IAppBuilder pluginApp, IdentityServerCoreOptions coreOptions)
        //{
        //    var wsfedOptions = new WsFederationPluginOptions
        //    {
        //        // todo - also signoutcleanup is broken right now
        //        LoginPageUrl = "http://localhost:3333/core/login",
        //        LogoutPageUrl = "http://localhost:3333/core/connect/logout",

        //        Factory = new WsFederationServiceFactory
        //        {
        //            UserService = coreOptions.Factory.UserService,
        //            CoreSettings = coreOptions.Factory.CoreSettings,
        //            RelyingPartyService = () => new InMemoryRelyingPartyService(LocalTestRelyingParties.Get()),
        //            WsFederationSettings = () => new LocalTestWsFederationSettings()
        //        },
        //    };

        //    pluginApp.UseWsFederationPlugin(wsfedOptions);
        //}
    }
}
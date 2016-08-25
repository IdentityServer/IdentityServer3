using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services;
using Microsoft.Owin.Builder;
using Owin;

namespace IdentityServer3.Tests.TokenClients
{
    class TokenClientIdentityServer
    {
        public static AppBuilder Create()
        {
            AppBuilder app = new AppBuilder();

            var factory = new IdentityServerServiceFactory()
                            .UseInMemoryClients(Clients.Get())
                            .UseInMemoryScopes(Scopes.Get())
                            .UseInMemoryUsers(Users.Get());

            factory.CustomGrantValidators.Add(new Registration<ICustomGrantValidator, CustomGrantValidator>());
            factory.CustomGrantValidators.Add(new Registration<ICustomGrantValidator, CustomGrantValidator2>());

            app.UseIdentityServer(new IdentityServerOptions
            {
                EventsOptions = new EventsOptions
                {
                    RaiseErrorEvents = true,
                    RaiseFailureEvents = true,
                    RaiseInformationEvents = true,
                    RaiseSuccessEvents = true
                },

                IssuerUri = "https://idsrv3",
                SigningCertificate = TestCert.Load(),

                Factory = factory
            });

            return app;
        }
    }
}
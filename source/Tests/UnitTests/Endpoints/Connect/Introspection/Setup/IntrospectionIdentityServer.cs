using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services.InMemory;
using Microsoft.Owin.Builder;
using Owin;
using System.Collections.Generic;

namespace IdentityServer3.Tests.Endpoints.Connect.Introspection.Setup
{
    class IntrospectionIdentityServer
    {
        public static AppBuilder Create()
        {
            AppBuilder app = new AppBuilder();

            app.UseIdentityServer(new IdentityServerOptions
            {
                Factory = new IdentityServerServiceFactory()
                            .UseInMemoryClients(Clients.Get())
                            .UseInMemoryScopes(Scopes.Get())
                            .UseInMemoryUsers(new List<InMemoryUser>())
            });

            return app;
        }
    }
}

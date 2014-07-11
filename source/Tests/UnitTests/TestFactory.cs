using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Services.InMemory;

namespace Thinktecture.IdentityServer.Tests
{
    public class TestFactory
    {
        public static IdentityServerServiceFactory Create(
                    string issuerUri, string siteName, string publicHostAddress = "")
        {
            var settings = new TestSettings(issuerUri, siteName, publicHostAddress);
            var scopes = new InMemoryScopeService(TestScopes.Get());
            var clients = new InMemoryClientService(TestClients.Get());
            
            var fact = new IdentityServerServiceFactory
            {
                CoreSettings = Registration.RegisterFactory<CoreSettings>(() => settings),
                ScopeService = Registration.RegisterFactory<IScopeService>(() => scopes),
                ClientService = Registration.RegisterFactory<IClientService>(() => clients)
            };

            return fact;
        }
    }
}

using IdentityServer3.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityServer3.Tests.Endpoints.Connect.Introspection.Setup
{
    class Clients
    {
        public static IEnumerable<Client> Get()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "client1",
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },

                    Flow = Flows.ClientCredentials,
                    AllowAccessToAllScopes = true,
                    AccessTokenType = AccessTokenType.Reference
                },
                new Client
                {
                    ClientId = "client2",
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },

                    Flow = Flows.ClientCredentials,
                    AllowAccessToAllScopes = true,
                    AccessTokenType = AccessTokenType.Reference
                }
            };
        }
    }
}
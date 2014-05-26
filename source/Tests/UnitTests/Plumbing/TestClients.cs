using System;
using System.Collections.Generic;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Tests.Plumbing
{
    class TestClients
    {
        public static IEnumerable<Client> Get()
        {
            return new List<Client>
            {
                new Client
                    {
                        ClientName = "Code Client",
                        Enabled = true,
                        ClientId = "codeclient",
                        ClientSecret = "secret",
                        Flow = Flows.Code,
                        ApplicationType = ApplicationTypes.Web,
                        RequireConsent = false,
                    
                        RedirectUris = new List<Uri>
                        {
                            new Uri("https://server/cb"),
                        },

                        AuthorizationCodeLifetime = 60
                    },
                    new Client
                    {
                        ClientName = "Implicit Client",
                        Enabled = true,
                        ClientId = "implicitclient",
                        ClientSecret = "secret",
                        Flow = Flows.Implicit,
                        ApplicationType = ApplicationTypes.Native,
                        RequireConsent = false,
                    
                        RedirectUris = new List<Uri>
                        {
                            new Uri("oob://implicit/cb")
                        },
                    },
                    new Client
                    {
                        ClientName = "Code Client with Scope Restrictions",
                        Enabled = true,
                        ClientId = "codeclient_restricted",
                        ClientSecret = "secret",
                        Flow = Flows.Code,
                        ApplicationType = ApplicationTypes.Web,
                        RequireConsent = false,

                        ScopeRestrictions = new List<string>
                        {
                            "openid"
                        },
                    
                        RedirectUris = new List<Uri>
                        {
                            new Uri("https://server/cb"),
                        },
                    },
                    new Client
                    {
                        ClientName = "Client Credentials Client",
                        Enabled = true,
                        ClientId = "client",
                        ClientSecret = "secret",
                        Flow = Flows.ClientCredentials,
                        AccessTokenType = AccessTokenType.JWT
                    },
                    new Client
                    {
                        ClientName = "Client Credentials Client (restricted)",
                        Enabled = true,
                        ClientId = "client_restricted",
                        ClientSecret = "secret",
                        Flow = Flows.ClientCredentials,

                        ScopeRestrictions = new List<string>
                        {
                            "resource"
                        },       
                    },
                    new Client
                    {
                        ClientName = "Resource Owner Client",
                        Enabled = true,
                        ClientId = "roclient",
                        ClientSecret = "secret",
                        Flow = Flows.ResourceOwner,
                    },
                    new Client
                    {
                        ClientName = "Resource Owner Client (restricted)",
                        Enabled = true,
                        ClientId = "roclient_restricted",
                        ClientSecret = "secret",
                        Flow = Flows.ResourceOwner,

                        ScopeRestrictions = new List<string>
                        {
                            "resource"
                        },       
                    },
                    new Client
                    {
                        ClientName = "Assertion Flow Client",
                        Enabled = true,
                        ClientId = "assertionclient",
                        ClientSecret = "secret",
                        Flow = Flows.Assertion,
                    },
                    new Client
                    {
                        ClientName = "Disabled Client",
                        Enabled = false,
                        ClientId = "disabled",
                        ClientSecret = "invalid",
                        Flow = Flows.Assertion,
                    }
            };
        }
    }
}
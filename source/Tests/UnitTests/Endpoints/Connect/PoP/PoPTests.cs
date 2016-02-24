/*
 * Copyright 2014, 2015 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */


using IdentityServer3.Core.Models;
using IdentityServer3.Tests.Conformance;
using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using System.Net.Http;
using System.Net;
using System.Security.Cryptography;
using IdentityModel;
using Newtonsoft.Json;
using System.Text;

namespace IdentityServer3.Tests.Endpoints.Connect.PoP
{
    public class PoPTests : IdentityServerHostTest
    {
        const string Category = "Conformance.Basic.ClientAuthenticationTests";

        string client_id = "code_client";
        string redirect_uri = "https://code_client/callback";
        string client_secret = "secret";

        protected override void PreInit()
        {
            host.Scopes.Add(StandardScopes.OpenId);

            host.Clients.Add(new Client
            {
                Enabled = true,
                ClientId = client_id,
                ClientSecrets = new List<Secret>
                {
                    new Secret(client_secret.Sha256())
                },

                Flow = Flows.AuthorizationCode,
                AllowAccessToAllScopes = true,

                RequireConsent = false,
                RedirectUris = new List<string>
                {
                    redirect_uri
                }
            });
        }

        [Fact]
        [Trait("Category", Category)]
        public void Valid_Asymmetric_Key()
        {
            host.Login();

            var nonce = Guid.NewGuid().ToString();
            var query = host.RequestAuthorizationCode(client_id, redirect_uri, "openid", nonce);
            var code = query["code"];

            host.NewRequest();
            host.Client.SetBasicAuthentication(client_id, client_secret);

            var jwk = CreateJwk();
            var key = CreateJwkString(jwk);

            var result = host.PostForm(host.GetTokenUrl(),
                new
                {
                    grant_type = "authorization_code",
                    code,
                    redirect_uri,
                    token_type = "pop",
                    alg = "RS256",
                    key
                }
            );

            result.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Headers.CacheControl.NoCache.Should().BeTrue();
            result.Headers.CacheControl.NoStore.Should().BeTrue();

            var data = result.ReadJsonObject();
            data["token_type"].Should().NotBeNull();
            data["token_type"].ToString().Should().Be("pop");
            data["alg"].ToString().Should().NotBeNull();
            data["alg"].ToString().Should().Be("RS256");
            data["access_token"].Should().NotBeNull();
            data["expires_in"].Should().NotBeNull();
            data["id_token"].Should().NotBeNull();
        }

        private RSACryptoServiceProvider CreateProvider(int keySize = 2048)
        {
            var csp = new CspParameters
            { 
                Flags = CspProviderFlags.CreateEphemeralKey,
                KeyNumber = (int) KeyNumber.Signature
            };

            return new RSACryptoServiceProvider(keySize);
        }

        private PublicKeyJwk CreateJwk()
        {
            var prov = CreateProvider();
            var pubKey = prov.ExportParameters(false);

            var jwk = new PublicKeyJwk("key1")
            {
                n = Base64Url.Encode(pubKey.Modulus),
                e = Base64Url.Encode(pubKey.Exponent)
            };

            return jwk;
        }

        private string CreateJwkString(PublicKeyJwk jwk = null)
        {
            if (jwk == null) jwk = CreateJwk();
            
            var json = JsonConvert.SerializeObject(jwk);
            return Base64Url.Encode(Encoding.ASCII.GetBytes(json));
        }
    }
}
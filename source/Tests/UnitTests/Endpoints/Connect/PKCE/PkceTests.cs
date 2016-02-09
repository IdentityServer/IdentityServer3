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

using FluentAssertions;
using IdentityModel;
using IdentityServer3.Core;
using IdentityServer3.Core.Models;
using IdentityServer3.Tests.Conformance;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer3.Tests.Endpoints.Connect.PKCE
{
    public class PkceTests : IdentityServerHostTest
    {
        const string Category = "Conformance.PKCE";

        Client client;
        string client_id = "codewithproofkey_client";
        string redirect_uri = "https://code_client/callback";
        string code_verifier = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        string client_secret = "secret";
        string response_type = "code";

        protected override void PreInit()
        {
            host.Scopes.Add(StandardScopes.OpenId);
            host.Clients.Add(client = new Client
            {
                Enabled = true,
                ClientId = client_id,
                ClientSecrets = new List<Secret>
                {
                    new Secret(client_secret.Sha256())
                },

                Flow = Flows.AuthorizationCodeWithProofKey,
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
        public void Client_can_use_plain_code_challenge_method()
        {
            host.Login();

            var nonce = Guid.NewGuid().ToString();
            var code_challenge = code_verifier;
            var codeQuery = host.RequestAuthorizationCode(
                client_id,
                redirect_uri,
                Constants.StandardScopes.OpenId,
                nonce,
                code_challenge,
                Constants.CodeChallengeMethods.Plain);

            var code = codeQuery["code"];

            host.NewRequest();
            host.Client.DefaultRequestHeaders.Authorization = new BasicAuthenticationHeaderValue(client_id, client_secret);

            var result = host.PostForm(host.GetTokenUrl(),
                new
                {
                    grant_type = "authorization_code",
                    code,
                    redirect_uri,
                    code_verifier
                }
            );

            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var data = result.ReadJsonObject();
            data["token_type"].Should().NotBeNull();
            data["token_type"].ToString().Should().Be("Bearer");
            data["access_token"].Should().NotBeNull();
            data["expires_in"].Should().NotBeNull();
            data["id_token"].Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public void Client_can_use_sha256_code_challenge_method()
        {
            host.Login();

            var nonce = Guid.NewGuid().ToString();
            var code_challenge = Sha256OfCodeVerifier(code_verifier);
            var codeQuery = host.RequestAuthorizationCode(
                client_id,
                redirect_uri,
                Constants.StandardScopes.OpenId,
                nonce,
                code_challenge,
                Constants.CodeChallengeMethods.SHA_256);

            var code = codeQuery["code"];

            host.NewRequest();
            host.Client.DefaultRequestHeaders.Authorization = new BasicAuthenticationHeaderValue(client_id, client_secret);

            var result = host.PostForm(host.GetTokenUrl(),
                new
                {
                    grant_type = "authorization_code",
                    code,
                    redirect_uri,
                    code_verifier
                }
            );

            result.StatusCode.Should().Be(HttpStatusCode.OK);

            var data = result.ReadJsonObject();
            data["token_type"].Should().NotBeNull();
            data["token_type"].ToString().Should().Be("Bearer");
            data["access_token"].Should().NotBeNull();
            data["expires_in"].Should().NotBeNull();
            data["id_token"].Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Authorize_request_needs_code_challenge()
        {
            host.Login();

            var nonce = Guid.NewGuid().ToString();
            var authorizeUrl = host.GetAuthorizeUrl(
                client_id,
                redirect_uri,
                Constants.StandardScopes.OpenId,
                response_type,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString());

            var result = await host.Client.GetAsync(authorizeUrl);
            result.StatusCode.Should().Be(HttpStatusCode.Found);

            var query = result.Headers.Location.ParseHashFragment();
            query["error"].Should().Be(Constants.AuthorizeErrors.InvalidRequest);
            query["error_description"].Should().Be("code challenge required");
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Authorize_request_code_challenge_cannot_be_too_short()
        {
            host.Login();

            var nonce = Guid.NewGuid().ToString();
            var authorizeUrl = host.GetAuthorizeUrl(
                client_id,
                redirect_uri,
                Constants.StandardScopes.OpenId,
                response_type,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                "a");

            var result = await host.Client.GetAsync(authorizeUrl);
            result.StatusCode.Should().Be(HttpStatusCode.Found);

            var query = result.Headers.Location.ParseHashFragment();
            query["error"].Should().Be(Constants.AuthorizeErrors.InvalidRequest);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Authorize_request_code_challenge_cannot_be_too_long()
        {
            host.Login();

            var nonce = Guid.NewGuid().ToString();
            var authorizeUrl = host.GetAuthorizeUrl(
                client_id,
                redirect_uri,
                Constants.StandardScopes.OpenId,
                response_type,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                new string('a', host.Options.InputLengthRestrictions.CodeChallengeMaxLength + 1));

            var result = await host.Client.GetAsync(authorizeUrl);
            result.StatusCode.Should().Be(HttpStatusCode.Found);

            var query = result.Headers.Location.ParseHashFragment();
            query["error"].Should().Be(Constants.AuthorizeErrors.InvalidRequest);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Authorize_request_needs_supported_code_challenge_method()
        {
            host.Login();

            var nonce = Guid.NewGuid().ToString();
            var code_challenge = code_verifier;
            var authorizeUrl = host.GetAuthorizeUrl(
                client_id,
                redirect_uri,
                Constants.StandardScopes.OpenId,
                response_type,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                code_challenge,
                "unknown_code_challenge_method");

            var result = await host.Client.GetAsync(authorizeUrl);
            result.StatusCode.Should().Be(HttpStatusCode.Found);

            var query = result.Headers.Location.ParseHashFragment();
            query["error"].Should().Be(Constants.AuthorizeErrors.InvalidRequest);
            query["error_description"].Should().Be("transform algorithm not supported");
        }

        [Fact]
        [Trait("Category", Category)]
        public void Token_request_needs_code_verifier()
        {
            host.Login();

            var nonce = Guid.NewGuid().ToString();
            var code_challenge = code_verifier;
            var codeQuery = host.RequestAuthorizationCode(
                client_id,
                redirect_uri,
                Constants.StandardScopes.OpenId,
                nonce,
                code_challenge,
                Constants.CodeChallengeMethods.Plain);

            var code = codeQuery["code"];

            host.NewRequest();
            host.Client.DefaultRequestHeaders.Authorization = new BasicAuthenticationHeaderValue(client_id, client_secret);

            var result = host.PostForm(host.GetTokenUrl(),
                new
                {
                    grant_type = "authorization_code",
                    code,
                    redirect_uri
                }
            );

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var data = result.ReadJsonObject();
            data["error"].ToString().Should().Be(Constants.TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public void Token_request_code_verifier_cannot_be_too_short()
        {
            host.Login();

            var nonce = Guid.NewGuid().ToString();
            var code_challenge = code_verifier;
            var codeQuery = host.RequestAuthorizationCode(
                client_id,
                redirect_uri,
                Constants.StandardScopes.OpenId,
                nonce,
                code_challenge,
                Constants.CodeChallengeMethods.Plain);

            var code = codeQuery["code"];

            host.NewRequest();
            host.Client.DefaultRequestHeaders.Authorization = new BasicAuthenticationHeaderValue(client_id, client_secret);

            var result = host.PostForm(host.GetTokenUrl(),
                new
                {
                    grant_type = "authorization_code",
                    code,
                    redirect_uri,
                    code_verifier = "a"
                }
            );

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var data = result.ReadJsonObject();
            data["error"].ToString().Should().Be(Constants.TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public void Token_request_code_verifier_cannot_be_too_long()
        {
            host.Login();

            var nonce = Guid.NewGuid().ToString();
            var code_challenge = code_verifier;
            var codeQuery = host.RequestAuthorizationCode(
                client_id,
                redirect_uri,
                Constants.StandardScopes.OpenId,
                nonce,
                code_challenge,
                Constants.CodeChallengeMethods.Plain);

            var code = codeQuery["code"];

            host.NewRequest();
            host.Client.DefaultRequestHeaders.Authorization = new BasicAuthenticationHeaderValue(client_id, client_secret);

            var result = host.PostForm(host.GetTokenUrl(),
                new
                {
                    grant_type = "authorization_code",
                    code,
                    redirect_uri,
                    code_verifier = new string('a', host.Options.InputLengthRestrictions.CodeVerifierMaxLength + 1)
                }
            );

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var data = result.ReadJsonObject();
            data["error"].ToString().Should().Be(Constants.TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public void Token_request_code_verifier_must_match_with_code_chalenge()
        {
            host.Login();

            var nonce = Guid.NewGuid().ToString();
            var code_challenge = code_verifier;
            var codeQuery = host.RequestAuthorizationCode(
                client_id,
                redirect_uri,
                Constants.StandardScopes.OpenId,
                nonce,
                code_challenge,
                Constants.CodeChallengeMethods.Plain);

            var code = codeQuery["code"];

            host.NewRequest();
            host.Client.DefaultRequestHeaders.Authorization = new BasicAuthenticationHeaderValue(client_id, client_secret);

            var result = host.PostForm(host.GetTokenUrl(),
                new
                {
                    grant_type = "authorization_code",
                    code,
                    redirect_uri,
                    code_verifier = "mismatched_code_verifier"
                }
            );

            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var data = result.ReadJsonObject();
            data["error"].ToString().Should().Be(Constants.TokenErrors.InvalidGrant);
        }

        private static string Sha256OfCodeVerifier(string codeVerifier)
        {
            var codeVerifierBytes = Encoding.ASCII.GetBytes(codeVerifier);
            var hashedBytes = codeVerifierBytes.Sha256();
            var transformedCodeVerifier = Base64Url.Encode(hashedBytes);

            return transformedCodeVerifier;
        }
    }
}

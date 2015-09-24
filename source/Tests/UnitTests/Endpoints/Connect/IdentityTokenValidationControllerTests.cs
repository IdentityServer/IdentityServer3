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
using IdentityServer3.Core;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Validation;
using IdentityServer3.Tests.Endpoints;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer3.Tests.Connect.Endpoints
{
    public class IdentityTokenValidationControllerTests : IdSvrHostTestBase
    {
        const String Category = "Identity token validation endpoint";
        static readonly String TestUrl = Constants.RoutePaths.Oidc.IdentityTokenValidation;

        [Fact]
        [Trait("Category", Category)]
        public void GetIdTokenValidation_IdTokenValidationEndpointDisabled_ReturnsNotFound()
        {
            base.options.Endpoints.EnableIdentityTokenValidationEndpoint = false;
            var resp = Get();
            resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        [Trait("Category", Category)]
        public void GetIdTokenValidation_MissingTokenInQueryString_ReturnsBadRequest()
        {
            var resp = Get();
            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        [Trait("Category", Category)]
        public void PostIdTokenValidation_MissingToken_ReturnsBadRequest()
        {
            var form = new { };

            var resp = PostForm(TestUrl, form);
            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        [Trait("Category", Category)]
        public void GetIdTokenValidation_MissingClientIdInQueryString_ReturnsBadRequest()
        {
            var resp = Get(token: "token");
            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        [Trait("Category", Category)]
        public void PostIdTokenValidation_MissingClientId_ReturnsBadRequest()
        {
            var form = new
            {
                token = "token"
            };

            var resp = PostForm(TestUrl, form);
            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        [Trait("Category", Category)]
        public void GetIdTokenValidation_ValidIdToken_ReturnsClaims()
        {
            ConfigureIdentityServerOptions = x =>
            {
                x.Factory.Register(new Registration<TokenValidator, AlwaysValidIdentityTokenValidator>());
            };
            Init();

            var resp = Get("token", "client_id");
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
            var claims = resp.GetJson<IDictionary<String, String>>();

            claims.Should().NotBeNull();
            claims.Count.Should().Be(2);

            Action<KeyValuePair<String, String>, String, String> assertClaim = (claim, claimType, claimValue) =>
            {
                claim.Should().NotBeNull();
                claim.Key.Should().Be(claimType);
                claim.Value.Should().Be(claimValue);
            };

            assertClaim(claims.ElementAt(0), Constants.ClaimTypes.Subject, "unique_subject");
            assertClaim(claims.ElementAt(1), Constants.ClaimTypes.Name, "subject name");
        }

        [Fact]
        [Trait("Category", Category)]
        public void PostIdTokenValidation_ValidIdToken_ReturnsClaims()
        {
            ConfigureIdentityServerOptions = x =>
            {
                x.Factory.Register(new Registration<TokenValidator, AlwaysValidIdentityTokenValidator>());
            };
            Init();

            var form = new
            {
                token = "token",
                client_id = "client_id"
            };

            var resp = PostForm(TestUrl, form);
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
            var claims = resp.GetJson<IDictionary<String, String>>();

            claims.Should().NotBeNull();
            claims.Count.Should().Be(2);

            Action<KeyValuePair<String, String>, String, String> assertClaim = (claim, claimType, claimValue) =>
            {
                claim.Should().NotBeNull();
                claim.Key.Should().Be(claimType);
                claim.Value.Should().Be(claimValue);
            };

            assertClaim(claims.ElementAt(0), Constants.ClaimTypes.Subject, "unique_subject");
            assertClaim(claims.ElementAt(1), Constants.ClaimTypes.Name, "subject name");
        }

        [Fact]
        [Trait("Category", Category)]
        public void GetIdTokenValidation_InvalidIdToken_ReturnsBadRequest()
        {
            ConfigureIdentityServerOptions = x =>
            {
                x.Factory.Register(new Registration<TokenValidator, AlwaysInvalidIdentityTokenValidator>());
            };
            Init();

            var resp = Get("token", "client_id");
            var error = resp.GetJson<IDictionary<String, String>>(successExpected: false);

            error.Should().NotBeNull();
            error.Count.Should().Be(1);
            error.First().Key.Should().Be("Message");
            error.First().Value.Should().Be(Constants.ProtectedResourceErrors.InvalidToken);
        }

        [Fact]
        [Trait("Category", Category)]
        public void PostIdTokenValidation_InvalidIdToken_ReturnsBadRequest()
        {
            ConfigureIdentityServerOptions = x =>
            {
                x.Factory.Register(new Registration<TokenValidator, AlwaysInvalidIdentityTokenValidator>());
            };
            Init();

            var form = new
            {
                token = "token",
                client_id = "client_id"
            };

            var resp = PostForm(TestUrl, form);

            var error = resp.GetJson<IDictionary<String, String>>(successExpected: false);

            error.Should().NotBeNull();
            error.Count.Should().Be(1);
            error.First().Key.Should().Be("Message");
            error.First().Value.Should().Be(Constants.ProtectedResourceErrors.InvalidToken);
        }

        private HttpResponseMessage Get(String token = null, String clientId = null)
        {
            var parameters = new NameValueCollection();

            if (token.IsPresent())
            {
                parameters.Add("token", token);
            }

            if (clientId.IsPresent())
            {
                parameters.Add("client_id", clientId);
            }

            var url = TestUrl.AddQueryString(parameters.ToQueryString());
            return base.Get(url);
        }

        private class AlwaysValidIdentityTokenValidator : TokenValidator
        {
            public AlwaysValidIdentityTokenValidator(IdentityServerOptions options, IClientStore clients, ITokenHandleStore tokenHandles, ICustomTokenValidator customValidator)
                : base(options, clients, tokenHandles, customValidator)
            {
            }

            public override Task<TokenValidationResult> ValidateIdentityTokenAsync(string token, string clientId = null, bool validateLifetime = true)
            {
                var result = new TokenValidationResult
                {
                    IsError = false,
                    Claims = new[]
                    {
                        new Claim(Constants.ClaimTypes.Subject, "unique_subject"),
                        new Claim(Constants.ClaimTypes.Name, "subject name")
                    }
                };

                return Task.FromResult(result);
            }
        }

        private class AlwaysInvalidIdentityTokenValidator : TokenValidator
        {
            public AlwaysInvalidIdentityTokenValidator(IdentityServerOptions options, IClientStore clients, ITokenHandleStore tokenHandles, ICustomTokenValidator customValidator)
                : base(options, clients, tokenHandles, customValidator)
            {
            }

            public override Task<TokenValidationResult> ValidateIdentityTokenAsync(string token, string clientId = null, bool validateLifetime = true)
            {
                var result = new TokenValidationResult
                {
                    Error = Constants.ProtectedResourceErrors.InvalidToken
                };

                return Task.FromResult(result);
            }
        }
    }
}
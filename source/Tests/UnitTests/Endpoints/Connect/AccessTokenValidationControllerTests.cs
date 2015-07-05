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
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer3.Tests.Endpoints.Connect
{
    public class AccessTokenValidationControllerTests : IdSvrHostTestBase
    {
        const String Category = "Access token validation endpoint";
        static readonly String TestUrl = Constants.RoutePaths.Oidc.AccessTokenValidation;

        [Fact]
        [Trait("Category", Category)]
        public void GetAccessTokenValidation_AccessTokenValidationEndpointDisabled_ReturnsNotFound()
        {
            base.options.Endpoints.EnableAccessTokenValidationEndpoint = false;
            var resp = Get();
            resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        [Trait("Category", Category)]
        public void GetAccessTokenValidation_MissingToken_ReturnsBadRequest()
        {
            var resp = Get();
            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        [Trait("Category", Category)]
        public void PostAccessTokenValidation_MissingToken_ReturnsBadRequest()
        {
            var col = new NameValueCollection();
            var resp = PostForm(TestUrl, col);

            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        [Trait("Category", Category)]
        public void GetAccessTokenValidation_InvalidToken_ReturnsBadRequest()
        {
            ConfigureIdentityServerOptions = x =>
            {
                x.Factory.Register(new Registration<TokenValidator, AlwaysInvalidAccessTokenValidator>());
            };
            Init();

            var resp = Get("Dummy Token");
            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var error = resp.GetJson<IDictionary<String, String>>(successExpected: false);
            
            error.Should().NotBeNull();
            error.Count.Should().Be(1);
            error.First().Key.Should().Be("Message");
            error.First().Value.Should().Be(Constants.ProtectedResourceErrors.InvalidToken);
        }

        [Fact]
        [Trait("Category", Category)]
        public void PostAccessTokenValidation_InvalidToken_ReturnsBadRequest()
        {
            ConfigureIdentityServerOptions = x =>
            {
                x.Factory.Register(new Registration<TokenValidator, AlwaysInvalidAccessTokenValidator>());
            };
            Init();

            var form = new
            {
                token = "Dummy Token"
            };

            var resp = PostForm(TestUrl, form);
            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var error = resp.GetJson<IDictionary<String, String>>(successExpected: false);

            error.Should().NotBeNull();
            error.Count.Should().Be(1);
            error.First().Key.Should().Be("Message");
            error.First().Value.Should().Be(Constants.ProtectedResourceErrors.InvalidToken);
        }

        [Fact]
        [Trait("Category", Category)]
        public void GetAccessTokenValidation_ValidToken_ReturnsClaims()
        {
            ConfigureIdentityServerOptions = x =>
            {
                x.Factory.Register(new Registration<TokenValidator, AlwaysValidAccessTokenValidator>());
            };
            Init();

            var resp = Get("Dummy Token");
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
        public void PostAccessTokenValidation_ValidToken_ReturnsClaims()
        {
            ConfigureIdentityServerOptions = x =>
            {
                x.Factory.Register(new Registration<TokenValidator, AlwaysValidAccessTokenValidator>());
            };
            Init();

            var form = new
            {
                token = "Dummy Token"
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

        private new HttpResponseMessage Get(String token = null)
        {
            var parameters = new NameValueCollection();

            if (token.IsPresent())
            {
                parameters.Add("token", token);
            }

            var url = TestUrl.AddQueryString(parameters.ToQueryString());
            return base.Get(url);
        }

        private class AlwaysValidAccessTokenValidator : TokenValidator
        {
            public AlwaysValidAccessTokenValidator(IdentityServerOptions options, IClientStore clients, ITokenHandleStore tokenHandles, ICustomTokenValidator customValidator)
                : base(options, clients, tokenHandles, customValidator)
            {
            }

            public override Task<TokenValidationResult> ValidateAccessTokenAsync(string token, string expectedScope = null)
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

        private class AlwaysInvalidAccessTokenValidator : TokenValidator
        {
            public AlwaysInvalidAccessTokenValidator(IdentityServerOptions options, IClientStore clients, ITokenHandleStore tokenHandles, ICustomTokenValidator customValidator)
                : base(options, clients, tokenHandles, customValidator)
            {
            }

            public override Task<TokenValidationResult> ValidateAccessTokenAsync(string token, string expectedScope = null)
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

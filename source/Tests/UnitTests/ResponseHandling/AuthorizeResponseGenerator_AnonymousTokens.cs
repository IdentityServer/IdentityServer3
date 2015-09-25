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

namespace IdentityServer3.Tests.ResponseHandling
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using FluentAssertions;

    using IdentityModel;

    using IdentityServer3.Core.Configuration;
    using IdentityServer3.Core.Models;
    using IdentityServer3.Core.ResponseHandling;
    using IdentityServer3.Core.Services;
    using IdentityServer3.Core.Services.Default;
    using IdentityServer3.Core.Validation;

    using Moq;

    using Xunit;

    public class AuthorizeResponseGenerator_AnonymousTokens
    {
        private readonly IdentityServerOptions options;

        private readonly DefaultAnonymousClaimsProvider defaultAnonymousClaimsProvider;

        private readonly Mock<IClaimsProvider> claimsProvider;

        private readonly Mock<IEventService> eventService;

        private readonly Mock<IAuthorizationCodeStore> codeStore;

        private readonly Mock<IScopeStore> scopeStore;

        public AuthorizeResponseGenerator_AnonymousTokens()
        {
            this.options = new IdentityServerOptions { IssuerUri = "http://test.com", SigningCertificate = TestCert.Load() };
            this.defaultAnonymousClaimsProvider = new DefaultAnonymousClaimsProvider();

            claimsProvider = new Mock<IClaimsProvider>();
            eventService = new Mock<IEventService>();
            codeStore = new Mock<IAuthorizationCodeStore>();
            scopeStore = new Mock<IScopeStore>();
        }

        [Fact]
        public async Task When_Not_Authenticated_And_A_Request_For_Anon_Scope_Is_Made_For_AccessToken_And_IdToken_Then_An_Anonymous_Token_Is_Returned_For_Both()
        {
            var signingService = new DefaultTokenSigningService(this.options);

            var tokenService = new DefaultTokenService(this.options, claimsProvider.Object, new Mock<ITokenHandleStore>().Object, signingService, eventService.Object, defaultAnonymousClaimsProvider);
           
            var generator = new AuthorizeResponseGenerator(tokenService, codeStore.Object, eventService.Object);

            var request = new ValidatedAuthorizeRequest
                              {
                                  Subject = Principal.Anonymous,
                                  ClientId = "foo",
                                  RequestedScopes = new List<string> { "anon" },
                                  ResponseType = "token id_token",
                                  ValidatedScopes = new ScopeValidator(scopeStore.Object),
                                  IsOpenIdRequest = true,
                                  Client = new Client { AccessTokenType = AccessTokenType.Jwt, AccessTokenLifetime = int.MaxValue },
                                  Raw = new NameValueCollection()
                              };

            request.ValidatedScopes.ContainsAnonymousScope = true;

            var result = await generator.CreateImplicitFlowResponseAsync(request);

            result.AccessToken.Length.Should().BeGreaterThan(0);
            result.IdentityToken.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task When_Authenticated_And_A_Request_For_Anon_Scope_Is_Made_Then_Authenticated_Token_Is_Returned()
        {
            var signingService = new DefaultTokenSigningService(this.options);

            var tokenService = new DefaultTokenService(this.options, claimsProvider.Object, new Mock<ITokenHandleStore>().Object, signingService, eventService.Object, defaultAnonymousClaimsProvider);

            var generator = new AuthorizeResponseGenerator(tokenService, codeStore.Object, eventService.Object);

            var request = new ValidatedAuthorizeRequest
            {
                Subject = Principal.Create("site", new[]{ new Claim("firstname", "test"), new Claim("lastname", "user"), new Claim("amr", "site")   }),
                ClientId = "foo",
                RequestedScopes = new List<string> { "anon" },
                ResponseType = "token id_token",
                ValidatedScopes = new ScopeValidator(scopeStore.Object),
                IsOpenIdRequest = true,
                Client = new Client { AccessTokenType = AccessTokenType.Jwt, AccessTokenLifetime = int.MaxValue },
                Raw = new NameValueCollection()
            };

            request.ValidatedScopes.ContainsAnonymousScope = true;

            var result = await generator.CreateImplicitFlowResponseAsync(request);

            result.AccessToken.Length.Should().BeGreaterThan(0);
            result.IdentityToken.Length.Should().BeGreaterThan(0);
        }
    }
}
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
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.ResponseHandling;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.Default;
using IdentityServer3.Core.Validation;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer3.Tests.Connect.ResponseHandling
{
    using System.Collections.Specialized;

    public class AuthorizeResponseGenerator_AnonymousTokens
    {
        readonly IdentityServerOptions options;

        public AuthorizeResponseGenerator_AnonymousTokens()
        {
            options = new IdentityServerOptions() { IssuerUri = "http://test.com", SigningCertificate = TestCert.Load() };
        }

        [Fact]
        public async Task Anonymous_User_must_SignIn()
        {
            var claimsProvider = new Mock<IClaimsProvider>();
            var eventService = new Mock<IEventService>();

            var signingService = new DefaultTokenSigningService(options);

            var tokenService = new DefaultTokenService(options, claimsProvider.Object, new Mock<ITokenHandleStore>().Object, signingService, eventService.Object);
            var codeStore = new Mock<IAuthorizationCodeStore>();

            var scopeStore = new Mock<IScopeStore>();

            var generator = new AuthorizeResponseGenerator(tokenService, codeStore.Object, eventService.Object);

            var request = new ValidatedAuthorizeRequest
                              {
                                  Subject = Principal.Anonymous,
                                  ClientId = "foo",
                                  RequestedScopes = new List<string>() { "anon" },
                                  ResponseType = "token id_token",
                                  ValidatedScopes = new ScopeValidator(scopeStore.Object),
                                  IsOpenIdRequest = true,
                                  Client = new Client() { AccessTokenType = AccessTokenType.Jwt },
                                  Raw = new NameValueCollection()
                              };

            request.ValidatedScopes.ContainsAnonymousScope = true;



            var result = await generator.CreateImplicitFlowResponseAsync(request);

            result.AccessToken.Length.Should().BeGreaterThan(0);
            result.IdentityToken.Length.Should().BeGreaterThan(0);

        }


    }
}

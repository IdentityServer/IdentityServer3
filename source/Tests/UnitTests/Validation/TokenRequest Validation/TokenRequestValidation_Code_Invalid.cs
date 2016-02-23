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
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.Default;
using IdentityServer3.Core.Services.InMemory;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer3.Tests.Validation.TokenRequest
{

    public class TokenRequestValidation_Code_Invalid
    {
        IClientStore _clients = Factory.CreateClientStore();
        const string Category = "TokenRequest Validation - AuthorizationCode - Invalid";

        [Fact]
        [Trait("Category", Category)]
        public async Task Missing_AuthorizationCode()
        {
            var client = await _clients.FindClientByIdAsync("codeclient");
            var store = new InMemoryAuthorizationCodeStore();

            var code = new AuthorizationCode
            {
                Client = client,
                IsOpenId = true,
                RedirectUri = "https://server/cb",
            };

            await store.StoreAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Invalid_AuthorizationCode()
        {
            var client = await _clients.FindClientByIdAsync("codeclient");
            var store = new InMemoryAuthorizationCodeStore();

            var code = new AuthorizationCode
            {
                Client = client,
                IsOpenId = true,
                RedirectUri = "https://server/cb",
            };

            await store.StoreAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.Code, "invalid");
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task AuthorizationCodeTooLong()
        {
            var client = await _clients.FindClientByIdAsync("codeclient");
            var store = new InMemoryAuthorizationCodeStore();
            var options = new IdentityServerOptions();

            var code = new AuthorizationCode
            {
                Client = client,
                IsOpenId = true,
                RedirectUri = "https://server/cb",
            };

            await store.StoreAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: store);
            var longCode = "x".Repeat(options.InputLengthRestrictions.AuthorizationCode + 1);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.Code, longCode);
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task No_Scopes_for_AuthorizationCode()
        {
            var client = await _clients.FindClientByIdAsync("codeclient");
            var store = new InMemoryAuthorizationCodeStore();

            var code = new AuthorizationCode
            {
                Client = client,
                IsOpenId = true,
                RedirectUri = "https://server/cb",
            };

            await store.StoreAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.Code, "valid");
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            Constants.TokenErrors.InvalidRequest.Should().Be(result.Error);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Client_Not_Authorized_For_AuthorizationCode_Flow()
        {
            var client = await _clients.FindClientByIdAsync("implicitclient");
            var store = new InMemoryAuthorizationCodeStore();

            var code = new AuthorizationCode
            {
                Client = client,
                IsOpenId = true,
                RedirectUri = "https://server/cb",
            };

            await store.StoreAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.Code, "valid");
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.UnauthorizedClient);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Client_Trying_To_Request_Token_Using_Another_Clients_Code()
        {
            var client1 = await _clients.FindClientByIdAsync("codeclient");
            var client2 = await _clients.FindClientByIdAsync("codeclient_restricted");
            var store = new InMemoryAuthorizationCodeStore();

            var code = new AuthorizationCode
            {
                Client = client1,
                IsOpenId = true,
                RedirectUri = "https://server/cb",
            };

            await store.StoreAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.Code, "valid");
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client2);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Missing_RedirectUri()
        {
            var client = await _clients.FindClientByIdAsync("codeclient");
            var store = new InMemoryAuthorizationCodeStore();

            var code = new AuthorizationCode
            {
                Client = client,
                IsOpenId = true,
                RedirectUri = "https://server/cb",
            };

            await store.StoreAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.Code, "valid");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.UnauthorizedClient);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Different_RedirectUri_Between_Authorize_And_Token_Request()
        {
            var client = await _clients.FindClientByIdAsync("codeclient");
            var store = new InMemoryAuthorizationCodeStore();

            var code = new AuthorizationCode
            {
                Client = client,
                IsOpenId = true,
                RedirectUri = "https://server1/cb",
            };

            await store.StoreAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.Code, "valid");
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server2/cb");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.UnauthorizedClient);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Expired_AuthorizationCode()
        {
            var client = await _clients.FindClientByIdAsync("codeclient");
            var store = new InMemoryAuthorizationCodeStore();

            var code = new AuthorizationCode
            {
                Client = client,
                IsOpenId = true,
                RedirectUri = "https://server/cb",
                CreationTime = DateTimeOffset.UtcNow.AddSeconds(-100)
            };

            await store.StoreAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.Code, "valid");
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Reused_AuthorizationCode()
        {
            var client = await _clients.FindClientByIdAsync("codeclient");
            var store = new InMemoryAuthorizationCodeStore();

            var code = new AuthorizationCode
            {
                Subject = IdentityServerPrincipal.Create("123", "bob"),
                Client = client,
                IsOpenId = true,
                RedirectUri = "https://server/cb",
                RequestedScopes = new List<Scope>
                {
                    new Scope
                    {
                        Name = "openid"
                    }
                }
            };

            await store.StoreAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: store,
                customRequestValidator: new DefaultCustomRequestValidator());

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.Code, "valid");
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");

            // request first time
            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeFalse();

            // request second time
            validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: store,
                customRequestValidator: new DefaultCustomRequestValidator());
            
            result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Code_Request_with_disabled_User()
        {
            var client = await _clients.FindClientByIdAsync("codeclient");
            var store = new InMemoryAuthorizationCodeStore();

            var mock = new Mock<IUserService>();
            mock.Setup(u => u.IsActiveAsync(It.IsAny<IsActiveContext>())).Callback<IsActiveContext>(ctx =>
            {
                ctx.IsActive = false;
            }).Returns(Task.FromResult(0));

            var code = new AuthorizationCode
            {
                Client = client,
                Subject = IdentityServerPrincipal.Create("123", "bob"),
                RedirectUri = "https://server/cb",
                RequestedScopes = new List<Scope>
                {
                    new Scope
                    {
                        Name = "openid"
                    }
                }
            };

            await store.StoreAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: store,
                userService: mock.Object);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.Code, "valid");
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
        }

        [Theory]
        [InlineData("codewithproofkeyclient")]
        [InlineData("hybridwithproofkeyclient")]
        [Trait("Category", Category)]
        public async Task Code_Request_PKCE_Missing_CodeChallenge_In_AuthZCode(string clientId)
        {
            var client = await _clients.FindClientByIdAsync(clientId);
            var store = new InMemoryAuthorizationCodeStore();
            var options = new IdentityServerOptions();

            var code = new AuthorizationCode
            {
                Client = client,
                Subject = IdentityServerPrincipal.Create("123", "bob"),
                RedirectUri = "https://server/cb",
                RequestedScopes = new List<Scope>
                {
                    new Scope
                    {
                        Name = "openid"
                    }
                }
            };

            await store.StoreAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.Code, "valid");
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.TokenRequest.CodeVerifier, "x".Repeat(options.InputLengthRestrictions.CodeVerifierMinLength + 1));

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.InvalidGrant);
        }

        [Theory]
        [InlineData("codewithproofkeyclient")]
        [InlineData("hybridwithproofkeyclient")]
        [Trait("Category", Category)]
        public async Task Code_Request_PKCE_Missing_CodeChallengeMethod_In_AuthZCode(string clientId)
        {
            var client = await _clients.FindClientByIdAsync(clientId);
            var store = new InMemoryAuthorizationCodeStore();
            var options = new IdentityServerOptions();

            var code = new AuthorizationCode
            {
                Client = client,
                Subject = IdentityServerPrincipal.Create("123", "bob"),
                RedirectUri = "https://server/cb",
                CodeChallenge = "x".Repeat(options.InputLengthRestrictions.CodeChallengeMinLength + 1),
                RequestedScopes = new List<Scope>
                {
                    new Scope
                    {
                        Name = "openid"
                    }
                }
            };

            await store.StoreAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.Code, "valid");
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.TokenRequest.CodeVerifier, "x".Repeat(options.InputLengthRestrictions.CodeVerifierMinLength + 1));

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.InvalidGrant);
        }

        [Theory]
        [InlineData("codewithproofkeyclient")]
        [InlineData("hybridwithproofkeyclient")]
        [Trait("Category", Category)]
        public async Task Code_Request_PKCE_Missing_CodeVerifier(string clientId)
        {
            var client = await _clients.FindClientByIdAsync(clientId);
            var store = new InMemoryAuthorizationCodeStore();
            var options = new IdentityServerOptions();

            var code = new AuthorizationCode
            {
                Client = client,
                Subject = IdentityServerPrincipal.Create("123", "bob"),
                RedirectUri = "https://server/cb",
                CodeChallenge = "x".Repeat(options.InputLengthRestrictions.CodeChallengeMinLength + 1),
                CodeChallengeMethod = Constants.CodeChallengeMethods.Plain,
                RequestedScopes = new List<Scope>
                {
                    new Scope
                    {
                        Name = "openid"
                    }
                }
            };

            await store.StoreAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.Code, "valid");
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.InvalidGrant);
        }

        [Theory]
        [InlineData("codewithproofkeyclient")]
        [InlineData("hybridwithproofkeyclient")]
        [Trait("Category", Category)]
        public async Task Code_Request_PKCE_CodeVerifier_Too_Short(string clientId)
        {
            var client = await _clients.FindClientByIdAsync(clientId);
            var store = new InMemoryAuthorizationCodeStore();
            var options = new IdentityServerOptions();

            var code = new AuthorizationCode
            {
                Client = client,
                Subject = IdentityServerPrincipal.Create("123", "bob"),
                RedirectUri = "https://server/cb",
                CodeChallenge = "x".Repeat(options.InputLengthRestrictions.CodeChallengeMinLength + 1),
                CodeChallengeMethod = Constants.CodeChallengeMethods.Plain,
                RequestedScopes = new List<Scope>
                {
                    new Scope
                    {
                        Name = "openid"
                    }
                }
            };

            await store.StoreAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.Code, "valid");
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.TokenRequest.CodeVerifier, "x".Repeat(options.InputLengthRestrictions.CodeVerifierMinLength - 1));

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.InvalidGrant);
        }

        [Theory]
        [InlineData("codewithproofkeyclient")]
        [InlineData("hybridwithproofkeyclient")]
        [Trait("Category", Category)]
        public async Task Code_Request_PKCE_CodeVerifier_Too_Long(string clientId)
        {
            var client = await _clients.FindClientByIdAsync(clientId);
            var store = new InMemoryAuthorizationCodeStore();
            var options = new IdentityServerOptions();

            var code = new AuthorizationCode
            {
                Client = client,
                Subject = IdentityServerPrincipal.Create("123", "bob"),
                RedirectUri = "https://server/cb",
                CodeChallenge = "x".Repeat(options.InputLengthRestrictions.CodeChallengeMinLength + 1),
                CodeChallengeMethod = Constants.CodeChallengeMethods.Plain,
                RequestedScopes = new List<Scope>
                {
                    new Scope
                    {
                        Name = "openid"
                    }
                }
            };

            await store.StoreAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.Code, "valid");
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.TokenRequest.CodeVerifier, "x".Repeat(options.InputLengthRestrictions.CodeVerifierMaxLength + 1));

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.InvalidGrant);
        }

        [Theory]
        [InlineData("codewithproofkeyclient")]
        [InlineData("hybridwithproofkeyclient")]
        [Trait("Category", Category)]
        public async Task Code_Request_PKCE_Unsupported_CodeChallengeMethod_In_AuthZCode(string clientId)
        {
            var client = await _clients.FindClientByIdAsync(clientId);
            var store = new InMemoryAuthorizationCodeStore();
            var options = new IdentityServerOptions();

            var code = new AuthorizationCode
            {
                Client = client,
                Subject = IdentityServerPrincipal.Create("123", "bob"),
                RedirectUri = "https://server/cb",
                CodeChallenge = "x".Repeat(options.InputLengthRestrictions.CodeChallengeMinLength),
                CodeChallengeMethod = "Unknown",
                RequestedScopes = new List<Scope>
                {
                    new Scope
                    {
                        Name = "openid"
                    }
                }
            };

            await store.StoreAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.Code, "valid");
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.TokenRequest.CodeVerifier, "x".Repeat(options.InputLengthRestrictions.CodeVerifierMinLength));

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.InvalidGrant);
        }

        [Theory]
        [InlineData("codewithproofkeyclient")]
        [InlineData("hybridwithproofkeyclient")]
        [Trait("Category", Category)]
        public async Task Code_Request_PKCE_Transformed_CodeVerifier_Doesnt_Match_CodeChallenge_Plain(string clientId)
        {
            var client = await _clients.FindClientByIdAsync(clientId);
            var store = new InMemoryAuthorizationCodeStore();
            var options = new IdentityServerOptions();

            var code = new AuthorizationCode
            {
                Client = client,
                Subject = IdentityServerPrincipal.Create("123", "bob"),
                RedirectUri = "https://server/cb",
                CodeChallenge = "x".Repeat(options.InputLengthRestrictions.CodeChallengeMinLength),
                CodeChallengeMethod = Constants.CodeChallengeMethods.Plain,
                RequestedScopes = new List<Scope>
                {
                    new Scope
                    {
                        Name = "openid"
                    }
                }
            };

            await store.StoreAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.Code, "valid");
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.TokenRequest.CodeVerifier, "x".Repeat(options.InputLengthRestrictions.CodeVerifierMinLength) + "doesntmatch");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.InvalidGrant);
        }

        [Theory]
        [InlineData("codewithproofkeyclient")]
        [InlineData("hybridwithproofkeyclient")]
        [Trait("Category", Category)]
        public async Task Code_Request_PKCE_Transformed_CodeVerifier_Doesnt_Match_CodeChallenge_Sha256(string clientId)
        {
            var client = await _clients.FindClientByIdAsync(clientId);
            var store = new InMemoryAuthorizationCodeStore();
            var options = new IdentityServerOptions();

            var code = new AuthorizationCode
            {
                Client = client,
                Subject = IdentityServerPrincipal.Create("123", "bob"),
                RedirectUri = "https://server/cb",
                CodeChallenge = "x".Repeat(options.InputLengthRestrictions.CodeChallengeMinLength),
                CodeChallengeMethod = Constants.CodeChallengeMethods.SHA_256,
                RequestedScopes = new List<Scope>
                {
                    new Scope
                    {
                        Name = "openid"
                    }
                }
            };

            await store.StoreAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.Code, "valid");
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.TokenRequest.CodeVerifier, "x".Repeat(options.InputLengthRestrictions.CodeVerifierMinLength) + "doesntmatch");

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.InvalidGrant);
        }

        [Theory]
        [InlineData("codeclient")]
        [InlineData("hybridclient")]
        [Trait("Category", Category)]
        public async Task Code_Request_Contains_Code_Verifier_But_Client_Flow_Is_Not_PKCE(string clientId)
        {
            var client = await _clients.FindClientByIdAsync(clientId);
            var store = new InMemoryAuthorizationCodeStore();
            var options = new IdentityServerOptions();

            var code = new AuthorizationCode
            {
                Client = client,
                Subject = IdentityServerPrincipal.Create("123", "bob"),
                RedirectUri = "https://server/cb",
                RequestedScopes = new List<Scope>
                {
                    new Scope
                    {
                        Name = "openid"
                    }
                }
            };

            await store.StoreAsync("valid", code);

            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: store);

            var parameters = new NameValueCollection();
            parameters.Add(Constants.TokenRequest.GrantType, Constants.GrantTypes.AuthorizationCode);
            parameters.Add(Constants.TokenRequest.Code, "valid");
            parameters.Add(Constants.TokenRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.TokenRequest.CodeVerifier, "x".Repeat(options.InputLengthRestrictions.CodeVerifierMinLength));

            var result = await validator.ValidateRequestAsync(parameters, client);

            result.IsError.Should().BeTrue();
            result.Error.Should().Be(Constants.TokenErrors.InvalidGrant);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityServer3.Core;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.ResponseHandling;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Validation;
using Moq;
using Xunit;

namespace IdentityServer3.Tests.ResponseHandling
{
    public class TokenResponseGeneratorTests_CustomResponseParameters
    {
        private readonly Mock<ITokenService> tokenServiceMock;
        private readonly Mock<IRefreshTokenService> refreshTokenServiceMock;
        private readonly Mock<IScopeStore> scopeStoreMock;
        private readonly Mock<ICustomTokenResponseGenerator> customResponseGeneratorMock;
        private readonly TokenResponseGenerator tokenResponseGenerator;

        public TokenResponseGeneratorTests_CustomResponseParameters()
        {
            tokenServiceMock = new Mock<ITokenService>();
            tokenServiceMock.Setup(x => x.CreateAccessTokenAsync(It.IsAny<TokenCreationRequest>()))
                .ReturnsAsync(new Token());
            tokenServiceMock.Setup(x => x.CreateSecurityTokenAsync(It.IsAny<Token>()))
                .ReturnsAsync(string.Empty);

            refreshTokenServiceMock = new Mock<IRefreshTokenService>();
            scopeStoreMock = new Mock<IScopeStore>();
            customResponseGeneratorMock = new Mock<ICustomTokenResponseGenerator>();

            customResponseGeneratorMock.Setup(
                x => x.GenerateAsync(It.IsAny<ValidatedTokenRequest>(), It.IsAny<TokenResponse>()))
                .Returns((ValidatedTokenRequest vtr, TokenResponse tr) => Task.FromResult(tr));

            tokenResponseGenerator = new TokenResponseGenerator(
                tokenServiceMock.Object,
                refreshTokenServiceMock.Object,
                scopeStoreMock.Object,
                customResponseGeneratorMock.Object);
        }

        private ValidatedTokenRequest GetRequest()
        {
            return new ValidatedTokenRequest()
            {
                GrantType = Constants.GrantTypes.Password,
                ValidatedScopes = new ScopeValidator(scopeStoreMock.Object),
                Client = new Client()
                {
                    AccessTokenLifetime = 3600
                }
            };
        }

        [Fact]
        public async Task When_process_with_emtpy_custom_response_parameters_then_return_empty_custom_response_parameters()
        {
            var result = await tokenResponseGenerator.ProcessAsync(GetRequest(), new Dictionary<string, object>());
            Assert.Empty(result.Custom);
        }

        [Fact]
        public async Task When_process_with_null_custom_response_parameters_then_return_empty_custom_response_parameter()
        {
            var result = await tokenResponseGenerator.ProcessAsync(GetRequest(), null);
            Assert.Empty(result.Custom);
        }

        [Fact]
        public async Task When_process_with_not_empty_custom_response_parameters_then_return_not_empty_custom_response_parameters()
        {
            var result = await tokenResponseGenerator.ProcessAsync(GetRequest(),
                new Dictionary<string, object>()
                {
                    {"customKey1","customValue1" },
                    {"customKey2","customValue2" }
                });
            Assert.NotEmpty(result.Custom);
        }

        [Fact]
        public async Task When_process_with_not_empty_custom_response_parameters_then_return_given_custom_response_parameters()
        {
            var result = await tokenResponseGenerator.ProcessAsync(GetRequest(), new Dictionary<string, object>()
                {
                    {"customKey1","customValue1" },
                    {"customKey2","customValue2" }
                });

            Assert.Equal(2, result.Custom.Count);
            Assert.NotNull(result.Custom.Any(x => x.Key == "customKey1" && x.Value == "customValue1"));
            Assert.NotNull(result.Custom.Any(x => x.Key == "customKey2" && x.Value == "customValue2"));
        }
    }
}

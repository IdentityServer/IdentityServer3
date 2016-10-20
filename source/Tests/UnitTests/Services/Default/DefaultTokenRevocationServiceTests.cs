using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer3.Core;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.Default;
using IdentityServer3.Core.Validation;
using Moq;
using Xunit;
using Claim = System.Security.Claims.Claim;

namespace IdentityServer3.Tests.Services.Default
{
    public class DefaultTokenRevocationServiceTests
    {
        private readonly Mock<ITokenHandleStore> _tokenHandleStoreMock;
        private readonly Mock<IRefreshTokenStore> _refreshTokenStoreMock;
        private readonly DefaultTokenRevocationService _defaultTokenRevocationService;
        private const string SomeSubjectId = "some_subject_id";
        private const string SomeTokenHandle = "some_token";
        private static readonly Client SomeClient;
        private static readonly Token SomeToken;
        private static readonly RefreshToken SomeRefreshToken;

        private static readonly Client SomeOtherClient;

        static DefaultTokenRevocationServiceTests()
        {
            SomeClient = new Client
            {
                ClientId = "ClientId"
            };
            SomeToken = new Token
            {
                Client = SomeClient,
                Claims = new List<Claim> { new Claim(Constants.ClaimTypes.Subject, SomeSubjectId)}
            };
            SomeRefreshToken = new RefreshToken
            {
                AccessToken = SomeToken
            };
            
            SomeOtherClient = new Client
            {
                ClientId = "SomeOtherClientId"
            };
        }

        public DefaultTokenRevocationServiceTests()
        {
            _tokenHandleStoreMock = new Mock<ITokenHandleStore>();
            _refreshTokenStoreMock = new Mock<IRefreshTokenStore>();
            var eventServiceMock = new Mock<IEventService>();

            _defaultTokenRevocationService = new DefaultTokenRevocationService(
                _tokenHandleStoreMock.Object,
                _refreshTokenStoreMock.Object,
                eventServiceMock.Object);
        }

        [Theory]
        [InlineData(Constants.TokenTypeHints.AccessToken)]
        [InlineData(null)]
        [InlineData("unknownTokenType")]
        private async Task RevokeAsync_AccessTokenExists_RevokesAccessToken(string tokenTypeHint)
        {
            // Given
            var request = new TokenRevocationRequestValidationResult
            {
                TokenTypeHint = tokenTypeHint,
                Token = SomeTokenHandle,
                Client = SomeClient
            };

            _tokenHandleStoreMock.Setup(m => m.GetAsync(SomeTokenHandle)).Returns(Task.FromResult(SomeToken));

            // When
            var result = await _defaultTokenRevocationService.RevokeAsync(request);

            // Then
            VerifyAccessTokenIsRemoved(request.Token, Times.Once());
            VerifyAllTokensAreRevoked(SomeToken.SubjectId, SomeToken.ClientId, Times.Never());
            result.IsDone.Should().BeTrue();
            result.IsError.Should().BeFalse();
        }

        private void VerifyAccessTokenIsRemoved(string handle, Times times)
        {
            _tokenHandleStoreMock.Verify(m => m.RemoveAsync(handle), times);
        }

        private void VerifyAllTokensAreRevoked(string subjectId, string clientId, Times times)
        {
            _refreshTokenStoreMock.Verify(m => m.RevokeAsync(subjectId, clientId), times);
            _tokenHandleStoreMock.Verify(m => m.RevokeAsync(subjectId, clientId), times);
        }

        [Fact]
        private async Task RevokeAsync_TokenTypeAccessToken_NonexistingToken_ReturnsNotDone()
        {
            // Given
            var request = new TokenRevocationRequestValidationResult
            {
                TokenTypeHint = Constants.TokenTypeHints.AccessToken,
                Token = SomeTokenHandle,
                Client = SomeClient
            };
            _tokenHandleStoreMock.Setup(m => m.GetAsync(request.Token)).Returns(Task.FromResult<Token>(null));

            // When
            var result = await _defaultTokenRevocationService.RevokeAsync(request);

            // Then
            VerifyAccessTokenIsRemoved(request.Token, Times.Never());
            result.IsDone.Should().BeFalse();
            result.IsError.Should().BeFalse();
        }

        [Fact]
        private async Task RevokeAsync_TokenTypeAccessToken_WrongClient_ReturnsError()
        {
            // Given
            var request = new TokenRevocationRequestValidationResult
            {
                TokenTypeHint = Constants.TokenTypeHints.AccessToken,
                Token = SomeTokenHandle,
                Client = SomeOtherClient
            };

            _tokenHandleStoreMock.Setup(m => m.GetAsync(SomeTokenHandle)).Returns(Task.FromResult(SomeToken));

            // When
            var result = await _defaultTokenRevocationService.RevokeAsync(request);

            // Then
            VerifyAccessTokenIsRemoved(request.Token, Times.Never());
            VerifyAllTokensAreRevoked(SomeToken.SubjectId, SomeToken.ClientId, Times.Never());
            result.IsDone.Should().BeFalse();
            result.IsError.Should().BeTrue();
        }

        [Theory]
        [InlineData(Constants.TokenTypeHints.RefreshToken)]
        [InlineData(null)]
        [InlineData("unknownTokenType")]
        private async Task RevokeAsync_RefreshToken_RevokesAllTokensForSubjectAndClient(string tokenTypeHint)
        {
            // Given
            var request = new TokenRevocationRequestValidationResult
            {
                TokenTypeHint = tokenTypeHint,
                Token = SomeTokenHandle,
                Client = SomeClient
            };

            _tokenHandleStoreMock.Setup(m => m.GetAsync(request.Token)).Returns(Task.FromResult<Token>(null));
            _refreshTokenStoreMock.Setup(m => m.GetAsync(request.Token)).Returns(Task.FromResult(SomeRefreshToken));

            // When
            var result = await _defaultTokenRevocationService.RevokeAsync(request);

            // Then
            VerifyAllTokensAreRevoked(SomeToken.SubjectId, SomeToken.ClientId, Times.Once());
            result.IsDone.Should().BeTrue();
            result.IsError.Should().BeFalse();
        }

        [Fact]
        private async Task RevokeAsync_TokenTypeRefreshToken_NonexistingToken_ReturnsNotDone()
        {
            // Given
            var request = new TokenRevocationRequestValidationResult
            {
                TokenTypeHint = Constants.TokenTypeHints.RefreshToken,
                Token = SomeTokenHandle,
                Client = SomeClient
            };

            _refreshTokenStoreMock.Setup(m => m.GetAsync(request.Token)).Returns(Task.FromResult<RefreshToken>(null));

            // When
            var result = await _defaultTokenRevocationService.RevokeAsync(request);

            // Then
            VerifyAllTokensAreRevoked(SomeToken.SubjectId, SomeToken.ClientId, Times.Never());
            result.IsDone.Should().BeFalse();
            result.IsError.Should().BeFalse();
        }

        [Fact]
        private async Task RevokeAsync_TokenTypeRefreshToken_WrongClient_ReturnsError()
        {
            // Given
            var request = new TokenRevocationRequestValidationResult
            {
                TokenTypeHint = Constants.TokenTypeHints.RefreshToken,
                Token = SomeTokenHandle,
                Client = SomeOtherClient
            };

            _refreshTokenStoreMock.Setup(m => m.GetAsync(request.Token)).Returns(Task.FromResult(SomeRefreshToken));

            // When
            var result = await _defaultTokenRevocationService.RevokeAsync(request);

            // Then
            VerifyAllTokensAreRevoked(SomeToken.SubjectId, SomeToken.ClientId, Times.Never());
            result.IsDone.Should().BeFalse();
            result.IsError.Should().BeTrue();
        }
    }
}
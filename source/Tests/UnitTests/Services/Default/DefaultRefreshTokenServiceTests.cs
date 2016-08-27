using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.Default;
using Moq;
using System.Threading.Tasks;
using IdentityServer3.Core.Events;
using Xunit;

namespace IdentityServer3.Tests.Services.Default
{
    public class DefaultRefreshTokenServiceTests
    {
        private readonly Mock<IRefreshTokenStore> _refreshTokenStoreMock;
        private readonly Mock<IEventService> _eventServiceMock;
        private readonly DefaultRefreshTokenService _defaultRefreshTokenService;

        public DefaultRefreshTokenServiceTests()
        {
            _refreshTokenStoreMock = new Mock<IRefreshTokenStore>();
            _refreshTokenStoreMock.Setup(x => x.RemoveAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(0));

            _eventServiceMock = new Mock<IEventService>();
            _defaultRefreshTokenService = new DefaultRefreshTokenService(_refreshTokenStoreMock.Object, _eventServiceMock.Object);
        }

        [Fact]
        private async Task When_refreshing_reused_absolute_token_then_raise_event_with_two_similar_handlers()
        {
            // Given 
            var oldHandle = "old_handle";
            var client = new Client()
            {
                RefreshTokenUsage = TokenUsage.ReUse,
                RefreshTokenExpiration = TokenExpiration.Absolute
            };

            var refreshToken = new RefreshToken()
            {
                AccessToken = new Token("token_type")
                {
                    Client = new Client()
                }
            };

            // When
            await _defaultRefreshTokenService.UpdateRefreshTokenAsync(oldHandle, refreshToken, client);

            // Then
            _eventServiceMock.Verify(x => x.RaiseAsync<RefreshTokenRefreshDetails>(
                It.Is<Event<RefreshTokenRefreshDetails>>(p => p.Details.OldHandle == oldHandle && p.Details.NewHandle == oldHandle)), Times.Once);
        }

        [Fact]
        private async Task When_refreshing_onetimeonly_absolute_token_then_raise_event_with_two_different_handlers()
        {
            // Given 
            var oldHandle = "old_handle";
            var client = new Client()
            {
                RefreshTokenUsage = TokenUsage.OneTimeOnly,
                RefreshTokenExpiration = TokenExpiration.Absolute
            };

            var refreshToken = new RefreshToken()
            {
                AccessToken = new Token("token_type")
                {
                    Client = new Client()
                }
            };

            // When
            await _defaultRefreshTokenService.UpdateRefreshTokenAsync(oldHandle, refreshToken, client);

            // Then
            _eventServiceMock.Verify(x => x.RaiseAsync<RefreshTokenRefreshDetails>(
                It.Is<Event<RefreshTokenRefreshDetails>>(p => p.Details.OldHandle == oldHandle && p.Details.NewHandle != p.Details.OldHandle)), Times.Once);
        }
    }
}

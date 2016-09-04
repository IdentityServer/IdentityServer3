using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer3.Core.Events;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using Moq;
using Xunit;

namespace IdentityServer3.Tests.Events
{
    public class RefreshTokenRefreshDetailsTests
    {
        [Fact]
        public async Task When_claims_are_provided_then_create_event_with_given_claims()
        {
            // Given
            var oldHandle = "old_handle";
            var newHandle = "new_handle";

            var claimsPrincipalMock = new Mock<ClaimsPrincipal>();
            claimsPrincipalMock.Setup(x => x.Claims)
                .Returns(new List<Claim>()
                {
                    new Claim("claim_type_1", "claims_1_value"),
                    new Claim("claim_type_2", "claims_2_value")
                });

            var refreshToken = new RefreshToken()
            {
                AccessToken = new Token()
                {
                    Client = new Client()
                    {
                        ClientId = "client_id",
                        AccessTokenLifetime = 10
                    },
                },
                Subject = claimsPrincipalMock.Object
            };
            var eventServiceMock = new Mock<IEventService>();

            // When
            await eventServiceMock.Object.RaiseSuccessfulRefreshTokenRefreshEventAsync(oldHandle,
            newHandle,
            refreshToken);

            // Then
            eventServiceMock.Verify(
                x => x.RaiseAsync(
                    It.Is<Event<RefreshTokenRefreshDetails>>(rt
                    => rt.Details.Claims.ContainsKey("claim_type_1")
                    && rt.Details.Claims.ContainsKey("claim_type_2")
                    && rt.Details.Claims.Count == 2)));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IdentityServer3.Core.Events;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using Moq;
using Xunit;

namespace IdentityServer3.Tests.Events
{
    public class TokenRevokedDetailsTests
    {
        [Fact]
        public async Task When_claims_are_provided_then_create_event_with_given_claims()
        {
            // Given
            var handle = "handle";
            var subjectId = "subjectId";

            var claims = new List<Claim>()
            {
                new Claim("claim_type_1", "claims_1_value"),
                new Claim("claim_type_2", "claims_2_value")
            };

            var eventServiceMock = new Mock<IEventService>();

            // When
            await eventServiceMock.Object.RaiseTokenRevokedEventAsync(claims, subjectId, handle, "access_token");

            // Then
            eventServiceMock.Verify(
                x => x.RaiseAsync(
                    It.Is<Event<TokenRevokedDetails>>(rt
                    => rt.Details.Claims.ContainsKey("claim_type_1")
                    && rt.Details.Claims.ContainsKey("claim_type_2")
                    && rt.Details.Claims.Count == 2)));
        }
    }
}

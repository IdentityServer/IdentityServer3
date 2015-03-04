using FluentAssertions;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Configuration;
using Xunit;

namespace Thinktecture.IdentityServer.Tests.Configuration
{
    public class AuthenticationOptionsTests
    {
        [Fact]
        public void SigninMessageThreshold_Default_SameAsDefinedConstant()
        {
            new AuthenticationOptions()
                .SignInMessageThreshold
                .Should()
                .Be(Constants.SignInMessageThreshold);
        }
    }
}

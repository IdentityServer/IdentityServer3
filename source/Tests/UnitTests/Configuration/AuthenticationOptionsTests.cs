using FluentAssertions;
using IdentityServer3.Core;
using IdentityServer3.Core.Configuration;
using Xunit;

namespace IdentityServer3.Tests.Configuration
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

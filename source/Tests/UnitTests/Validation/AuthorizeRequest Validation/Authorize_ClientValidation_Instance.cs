using FluentAssertions;
using IdentityServer3.Core;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Validation;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer3.Tests.Validation.AuthorizeRequest_Validation
{
    public class Authorize_ClientValidation_Instance
    {
        IdentityServerOptions _options = TestIdentityServerOptions.Create();

        [Fact]
        [Trait("Category", "AuthorizeRequest Client Validation - IdToken")]
        public async Task Mixed_IdToken_Request()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "implicitclient:instance");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid resource");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "oob://implicit/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.IdToken);
            parameters.Add(Constants.AuthorizeRequest.Nonce, "abc");

            var validator = Factory.CreateAuthorizeRequestValidator();
            var result = await validator.ValidateAsync(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.User);
            result.Error.Should().Be(Constants.AuthorizeErrors.UnauthorizedClient);
        }
    }
}

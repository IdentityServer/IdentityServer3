using FluentAssertions;
using IdentityServer3.Core;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services.Default;
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
    public class Authorize_ClientInstanceValidation_Token
    {
        IdentityServerOptions _options = TestIdentityServerOptions.Create();

        [Fact]
        [Trait("Category", "AuthorizeRequest Client Validation - Token")]
        public async Task Mixed_Token_Request_Without_OpenId_Scope()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "implicitclient:instance");
            parameters.Add(Constants.AuthorizeRequest.Scope, "resource profile");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "oob://implicit/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Token);

            var validator = Factory.CreateAuthorizeRequestValidator(clientValidator: new DefaultInstanceClientValidator(
                    _options,
                    Factory.CreateClientStore(),
                    new DefaultRedirectUriValidator()
                ));
            var result = await validator.ValidateAsync(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.Client);
            result.Error.Should().Be(Constants.AuthorizeErrors.InvalidScope);
        }
    }
}

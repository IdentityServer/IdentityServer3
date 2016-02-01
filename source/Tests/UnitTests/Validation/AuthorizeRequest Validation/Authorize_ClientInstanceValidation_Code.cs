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
    public class Authorize_ClientInstanceValidation_Code
    {
        IdentityServerOptions _options = TestIdentityServerOptions.Create();

        [Fact]
        [Trait("Category", "AuthorizeRequest Client Validation - Code")]
        public async Task Code_Request_Unknown_Scope()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "codeclient:instance");
            parameters.Add(Constants.AuthorizeRequest.Scope, "unknown");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);

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

        [Fact]
        [Trait("Category", "AuthorizeRequest Client Validation - Code")]
        public async Task OpenId_Code_Request_Invalid_RedirectUri()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "codeclient:instance");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://invalid");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);

            var validator = Factory.CreateAuthorizeRequestValidator(clientValidator: new DefaultInstanceClientValidator(
                    _options,
                    Factory.CreateClientStore(),
                    new DefaultRedirectUriValidator()
                ));
            var result = await validator.ValidateAsync(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.User);
            result.Error.Should().Be(Constants.AuthorizeErrors.UnauthorizedClient);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Client Validation - Code")]
        public async Task OpenId_Code_Request_Invalid_IdToken_ResponseType()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "codeclient:instance");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.IdToken);
            parameters.Add(Constants.AuthorizeRequest.Nonce, "abc");

            var validator = Factory.CreateAuthorizeRequestValidator(clientValidator: new DefaultInstanceClientValidator(
                    _options,
                    Factory.CreateClientStore(),
                    new DefaultRedirectUriValidator()
                ));
            var result = await validator.ValidateAsync(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.User);
            result.Error.Should().Be(Constants.AuthorizeErrors.UnauthorizedClient);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Client Validation - Code")]
        public async Task OpenId_Code_Request_Invalid_IdTokenToken_ResponseType()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "codeclient:instance");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.IdTokenToken);
            parameters.Add(Constants.AuthorizeRequest.Nonce, "abc");

            var validator = Factory.CreateAuthorizeRequestValidator(clientValidator: new DefaultInstanceClientValidator(
                    _options,
                    Factory.CreateClientStore(),
                    new DefaultRedirectUriValidator()
                ));
            var result = await validator.ValidateAsync(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.User);
            result.Error.Should().Be(Constants.AuthorizeErrors.UnauthorizedClient);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Client Validation - Code")]
        public async Task OpenId_Code_Request_With_Unknown_Client()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "unknown:instance");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);

            var validator = Factory.CreateAuthorizeRequestValidator(clientValidator: new DefaultInstanceClientValidator(
                    _options,
                    Factory.CreateClientStore(),
                    new DefaultRedirectUriValidator()
                ));
            var result = await validator.ValidateAsync(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.User);
            result.Error.Should().Be(Constants.AuthorizeErrors.UnauthorizedClient);
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Client Validation - Code")]
        public async Task OpenId_Code_Request_With_Restricted_Scope()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "codeclient_restricted:instance");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid profile");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);

            var validator = Factory.CreateAuthorizeRequestValidator(clientValidator: new DefaultInstanceClientValidator(
                    _options,
                    Factory.CreateClientStore(),
                    new DefaultRedirectUriValidator()
                ));
            var result = await validator.ValidateAsync(parameters);

            result.IsError.Should().BeTrue();
            result.ErrorType.Should().Be(ErrorTypes.User);
            result.Error.Should().Be(Constants.AuthorizeErrors.UnauthorizedClient);
        }
    }
}

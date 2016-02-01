using FluentAssertions;
using IdentityServer3.Core;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services.Default;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer3.Tests.Validation.AuthorizeRequest_Validation
{
    public class Authorize_ClientInstanceValidation_Valid
    {
        IdentityServerOptions _options = TestIdentityServerOptions.Create();

        [Fact]
        [Trait("Category", "AuthorizeRequest Client Validation - Valid")]
        public async Task Valid_OpenId_Code_Request()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "codeclient:instance");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);

            var validator = Factory.CreateAuthorizeRequestValidator(clientValidator: new DefaultInstanceClientValidator(
                    _options,
                    Factory.CreateClientStore(),
                    new DefaultRedirectUriValidator()
                ));
            var result = await validator.ValidateAsync(parameters);

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Client Validation - Valid")]
        public async Task Valid_Resource_Code_Request()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "codeclient:instance");
            parameters.Add(Constants.AuthorizeRequest.Scope, "resource");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);

            var validator = Factory.CreateAuthorizeRequestValidator(clientValidator: new DefaultInstanceClientValidator(
                    _options,
                    Factory.CreateClientStore(),
                    new DefaultRedirectUriValidator()
                ));
            var result = await validator.ValidateAsync(parameters);

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Client Validation - Valid")]
        public async Task Valid_Mixed_Code_Request()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "codeclient:instance");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid resource");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);

            var validator = Factory.CreateAuthorizeRequestValidator(clientValidator: new DefaultInstanceClientValidator(
                    _options,
                    Factory.CreateClientStore(),
                    new DefaultRedirectUriValidator()
                ));
            var result = await validator.ValidateAsync(parameters);

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Client Validation - Valid")]
        public async Task Valid_Mixed_Code_Request_Multiple_Scopes()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "codeclient:instance");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid profile resource");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Code);

            var validator = Factory.CreateAuthorizeRequestValidator(clientValidator: new DefaultInstanceClientValidator(
                    _options,
                    Factory.CreateClientStore(),
                    new DefaultRedirectUriValidator()
                ));
            var result = await validator.ValidateAsync(parameters);

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Client Validation - Valid")]
        public async Task Valid_OpenId_IdTokenToken_Request()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "implicitclient:instance");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "oob://implicit/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.IdTokenToken);
            parameters.Add(Constants.AuthorizeRequest.Nonce, "abc");

            var validator = Factory.CreateAuthorizeRequestValidator(clientValidator: new DefaultInstanceClientValidator(
                    _options,
                    Factory.CreateClientStore(),
                    new DefaultRedirectUriValidator()
                ));
            var result = await validator.ValidateAsync(parameters);

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Client Validation - Valid")]
        public async Task Valid_Mixed_IdTokenToken_Request()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "implicitclient:instance");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid resource");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "oob://implicit/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.IdTokenToken);
            parameters.Add(Constants.AuthorizeRequest.Nonce, "abc");

            var validator = Factory.CreateAuthorizeRequestValidator(clientValidator: new DefaultInstanceClientValidator(
                    _options,
                    Factory.CreateClientStore(),
                    new DefaultRedirectUriValidator()
                ));
            var result = await validator.ValidateAsync(parameters);

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Client Validation - Valid")]
        public async Task Valid_Mixed_IdTokenToken_Request_Multiple_Scopes()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "implicitclient:instance");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid profile resource");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "oob://implicit/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.IdTokenToken);
            parameters.Add(Constants.AuthorizeRequest.Nonce, "abc");

            var validator = Factory.CreateAuthorizeRequestValidator(clientValidator: new DefaultInstanceClientValidator(
                    _options,
                    Factory.CreateClientStore(),
                    new DefaultRedirectUriValidator()
                ));
            var result = await validator.ValidateAsync(parameters);

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", "AuthorizeRequest Client Validation - Valid")]
        public async Task Valid_Resource_Token_Request()
        {
            var parameters = new NameValueCollection();
            parameters.Add(Constants.AuthorizeRequest.ClientId, "implicitclient:instance");
            parameters.Add(Constants.AuthorizeRequest.Scope, "resource");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "oob://implicit/cb");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, Constants.ResponseTypes.Token);

            var validator = Factory.CreateAuthorizeRequestValidator(clientValidator: new DefaultInstanceClientValidator(
                    _options,
                    Factory.CreateClientStore(),
                    new DefaultRedirectUriValidator()
                ));
            var result = await validator.ValidateAsync(parameters);

            result.IsError.Should().BeFalse();
        }
    }
}

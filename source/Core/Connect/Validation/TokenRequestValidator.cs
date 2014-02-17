using System;
using System.Collections.Specialized;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Connect.Services;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class TokenRequestValidator
    {
        private ICoreSettings _coreSettings;
        private ILogger _logger;
        private IAuthorizationCodeStore _authorizationCodes;

        private ValidatedTokenRequest _validatedRequest;

        public ValidatedTokenRequest ValidatedRequest
        {
            get
            {
                return _validatedRequest;
            }
        }

        public TokenRequestValidator(ICoreSettings coreSettings, ILogger logger, IAuthorizationCodeStore authorizationCodes)
        {
            _coreSettings = coreSettings;
            _logger = logger;
            _authorizationCodes = authorizationCodes;
        }

        public ValidationResult ValidateRequest(NameValueCollection parameters, ClaimsPrincipal client)
        {
            _validatedRequest = new ValidatedTokenRequest();

            /////////////////////////////////////////////
            // check client and credentials
            /////////////////////////////////////////////
            if (client == null || !client.Identity.IsAuthenticated)
            {
                _logger.Error("No client information present.");
                return Invalid(Constants.TokenErrors.InvalidClient);
            }

            var clientId = client.FindFirst(Constants.ClaimTypes.Id);
            if (clientId == null)
            {
                _logger.Error("No id claim present.");
                return Invalid(Constants.TokenErrors.InvalidClient);
            }

            var secret = client.FindFirst(Constants.ClaimTypes.Secret);
            if (secret == null)
            {
                _logger.Error("No secret claim present.");
                return Invalid(Constants.TokenErrors.InvalidClient);
            }

            var oidcClient = _coreSettings.FindClientById(clientId.Value);
            if (oidcClient == null)
            {
                _logger.ErrorFormat("Client not found in registry: {0}", clientId.Value);
                return Invalid(Constants.TokenErrors.InvalidClient);
            }

            if (oidcClient.ClientSecret != secret.Value)
            {
                _logger.ErrorFormat("Invalid client secret for: {0}", clientId.Value);
                return Invalid(Constants.TokenErrors.InvalidClient);
            }

            _logger.InformationFormat("Client found in registry: {0} / {1}", oidcClient.ClientId, oidcClient.ClientName);
            _validatedRequest.Client = oidcClient;

            /////////////////////////////////////////////
            // check grant type
            /////////////////////////////////////////////
            var grantType = parameters.Get(Constants.TokenRequest.GrantType);
            if (grantType.IsMissing())
            {
                _logger.Error("Grant type is missing.");
                return Invalid(Constants.TokenErrors.UnsupportedGrantType);
            }

            if (!Constants.SupportedGrantTypes.Contains(grantType))
            {
                _logger.ErrorFormat("Unsupported grant_type: {0}", grantType);
                return Invalid(Constants.TokenErrors.UnsupportedGrantType);
            }

            _logger.InformationFormat("Grant type: {0}", grantType);
            _validatedRequest.GrantType = grantType;

            switch (grantType)
            {
                case Constants.GrantTypes.AuthorizationCode:
                    return ValidateAuthorizationCodeRequest(parameters);
            }

            return Invalid(Constants.TokenErrors.UnsupportedGrantType);
        }

        private ValidationResult ValidateAuthorizationCodeRequest(NameValueCollection parameters)
        {
            /////////////////////////////////////////////
            // check if client is authorized for grant type
            /////////////////////////////////////////////
            if (_validatedRequest.Client.Flow != Flows.Code)
            {
                _logger.Error("Client not authorized for code flow");
                return Invalid(Constants.TokenErrors.UnauthorizedClient);
            }

            /////////////////////////////////////////////
            // validate authorization code
            /////////////////////////////////////////////
            var code = parameters.Get(Constants.TokenRequest.Code);
            if (code.IsMissing())
            {
                _logger.Error("Authorization code is missing.");
                return Invalid(Constants.TokenErrors.InvalidGrant);
            }

            var authZcode = _authorizationCodes.Get(code);
            if (authZcode == null)
            {
                _logger.ErrorFormat("Invalid authorization code: ", code);
                return Invalid(Constants.TokenErrors.InvalidGrant);
            }
            else
            {
                _logger.InformationFormat("Authorization code found: {0}", code);
            }

            _authorizationCodes.Remove(code);

            /////////////////////////////////////////////
            // validate client binding
            /////////////////////////////////////////////
            if (authZcode.ClientId != _validatedRequest.Client.ClientId)
            {
                _logger.ErrorFormat("Client {0} is trying to use a code from client {1}", _validatedRequest.Client.ClientId, authZcode.ClientId);
                return Invalid(Constants.TokenErrors.InvalidGrant);
            }

            /////////////////////////////////////////////
            // validate code expiration
            /////////////////////////////////////////////
            // todo: make configurable

            if (authZcode.CreationTime.HasExpired(60))
            {
                _logger.Error("Authorization code is expired");
                return Invalid(Constants.TokenErrors.InvalidGrant);
            }

            _validatedRequest.AuthorizationCode = authZcode;

            /////////////////////////////////////////////
            // validate redirect_uri
            /////////////////////////////////////////////
            var redirectUri = parameters.Get(Constants.TokenRequest.RedirectUri);
            if (redirectUri.IsMissing())
            {
                _logger.Error("Redirect URI is missing.");
                return Invalid(Constants.TokenErrors.UnauthorizedClient);
            }

            if (redirectUri != _validatedRequest.AuthorizationCode.RedirectUri.AbsoluteUri)
            {
                _logger.ErrorFormat("Invalid redirect_uri: ", redirectUri);
                return Invalid(Constants.TokenErrors.UnauthorizedClient);
            }

            return Valid();
        }

        private ValidationResult Valid()
        {
            return new ValidationResult
            {
                IsError = false
            };
        }

        private ValidationResult Invalid(string error)
        {
            return new ValidationResult
            {
                IsError = true,
                ErrorType = ErrorTypes.Client,
                Error = error
            };
        }
    }
}

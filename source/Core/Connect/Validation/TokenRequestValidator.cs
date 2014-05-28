/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Connect.Services;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Plumbing;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class TokenRequestValidator
    {
        private readonly CoreSettings _settings;
        private readonly ILogger _logger;
        private readonly IAuthorizationCodeStore _authorizationCodes;
        private readonly IUserService _users;
        private readonly IScopeService _scopes;
        private readonly IAssertionGrantValidator _assertionValidator;
        private readonly ICustomRequestValidator _customRequestValidator;

        private ValidatedTokenRequest _validatedRequest;
        
        public ValidatedTokenRequest ValidatedRequest
        {
            get
            {
                return _validatedRequest;
            }
        }

        public TokenRequestValidator(CoreSettings settings, ILogger logger, IAuthorizationCodeStore authorizationCodes, IUserService users, IScopeService scopes, IAssertionGrantValidator assertionValidator, ICustomRequestValidator customRequestValidator)
        {
            _settings = settings;
            _logger = logger;
            _authorizationCodes = authorizationCodes;
            _users = users;
            _scopes = scopes;
            _assertionValidator = assertionValidator;
            _customRequestValidator = customRequestValidator;
        }

        public async Task<ValidationResult> ValidateRequestAsync(NameValueCollection parameters, Client client)
        {
            _validatedRequest = new ValidatedTokenRequest();

            if (client == null)
            {
                throw new ArgumentNullException("client");
            }

            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            _validatedRequest.Raw = parameters;
            _validatedRequest.Client = client;
            _validatedRequest.Settings = _settings;

            /////////////////////////////////////////////
            // check grant type
            /////////////////////////////////////////////
            var grantType = parameters.Get(Constants.TokenRequest.GrantType);
            if (grantType.IsMissing())
            {
                _logger.Error("Grant type is missing.");
                return Invalid(Constants.TokenErrors.UnsupportedGrantType);
            }

            _logger.InformationFormat("Grant type: {0}", grantType);
            _validatedRequest.GrantType = grantType;

            switch (grantType)
            {
                case Constants.GrantTypes.AuthorizationCode:
                    return await RunValidationAsync(ValidateAuthorizationCodeRequestAsync, parameters);
                case Constants.GrantTypes.ClientCredentials:
                    return await RunValidationAsync(ValidateClientCredentialsRequestAsync, parameters);
                case Constants.GrantTypes.Password:
                    return await RunValidationAsync(ValidateResourceOwnerCredentialRequestAsync, parameters);
            }

            if (parameters.Get(Constants.TokenRequest.Assertion).IsPresent())
            {
                return await RunValidationAsync(ValidateAssertionRequestAsync, parameters);
            }

            _logger.ErrorFormat("Unsupported grant_type: {0}", grantType);
            return Invalid(Constants.TokenErrors.UnsupportedGrantType);
        }

        async Task<ValidationResult> RunValidationAsync(Func<NameValueCollection, Task<ValidationResult>> validationFunc, NameValueCollection parameters)
        {
            // run standard validation
            var result = await validationFunc(parameters);
            if (result.IsError)
            {
                return result;
            }

            // run custom validation
            return await _customRequestValidator.ValidateTokenRequestAsync(_validatedRequest, _users);
        }

        private async Task<ValidationResult> ValidateAuthorizationCodeRequestAsync(NameValueCollection parameters)
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

            var authZcode = await _authorizationCodes.GetAsync(code);
            if (authZcode == null)
            {
                _logger.ErrorFormat("Invalid authorization code: ", code);
                return Invalid(Constants.TokenErrors.InvalidGrant);
            }
            else
            {
                _logger.InformationFormat("Authorization code found: {0}", code);
            }

            await _authorizationCodes.RemoveAsync(code);

            /////////////////////////////////////////////
            // validate client binding
            /////////////////////////////////////////////
            if (authZcode.Client.ClientId!= _validatedRequest.Client.ClientId)
            {
                _logger.ErrorFormat("Client {0} is trying to use a code from client {1}", _validatedRequest.Client.ClientId, authZcode.Client.ClientId);
                return Invalid(Constants.TokenErrors.InvalidGrant);
            }

            /////////////////////////////////////////////
            // validate code expiration
            /////////////////////////////////////////////
            if (authZcode.CreationTime.HasExpired(_validatedRequest.Client.AuthorizationCodeLifetime))
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
                _logger.ErrorFormat("Invalid redirect_uri: {0}", redirectUri);
                return Invalid(Constants.TokenErrors.UnauthorizedClient);
            }

            return Valid();
        }

        private async Task<ValidationResult> ValidateClientCredentialsRequestAsync(NameValueCollection parameters)
        {
            /////////////////////////////////////////////
            // check if client is authorized for grant type
            /////////////////////////////////////////////
            if (_validatedRequest.Client.Flow != Flows.ClientCredentials)
            {
                _logger.Error("Client not authorized for client credentials flow");
                return Invalid(Constants.TokenErrors.UnauthorizedClient);
            }

            /////////////////////////////////////////////
            // check if client is allowed to request scopes
            /////////////////////////////////////////////
            if (! (await ValidateRequestedScopesAsync(parameters)))
            {
                _logger.Error("Invalid scopes.");
                return Invalid(Constants.TokenErrors.InvalidScope);
            }

            if (_validatedRequest.ValidatedScopes.ContainsOpenIdScopes)
            {
                _logger.Error("Client cannot request OpenID scopes in client credentials flow");
                return Invalid(Constants.TokenErrors.InvalidScope);
            }

            return Valid();
        }

        private async Task<ValidationResult> ValidateResourceOwnerCredentialRequestAsync(NameValueCollection parameters)
        {
            /////////////////////////////////////////////
            // check if client is authorized for grant type
            /////////////////////////////////////////////
            if (_validatedRequest.Client.Flow != Flows.ResourceOwner)
            {
                _logger.Error("Client not authorized for resource owner flow");
                return Invalid(Constants.TokenErrors.UnauthorizedClient);
            }

            /////////////////////////////////////////////
            // check if client is allowed to request scopes
            /////////////////////////////////////////////
            if (! (await ValidateRequestedScopesAsync(parameters)))
            {
                _logger.Error("Invalid scopes.");
                return Invalid(Constants.TokenErrors.InvalidScope);
            }

            /////////////////////////////////////////////
            // check resource owner credentials
            /////////////////////////////////////////////
            var userName = parameters.Get(Constants.TokenRequest.UserName);
            var password = parameters.Get(Constants.TokenRequest.Password);

            if (userName.IsMissing() || password.IsMissing())
            {
                return Invalid(Constants.TokenErrors.InvalidGrant);
            }

            var authnResult = await _users.AuthenticateLocalAsync(userName, password);
            if (authnResult != null)
            {
                _validatedRequest.UserName = userName;

                _validatedRequest.Subject = IdentityServerPrincipal.Create(
                    authnResult.Subject,
                    authnResult.Name,
                    Constants.AuthenticationMethods.Password,
                    Constants.BuiltInIdentityProvider);
            }
            else
            {
                return Invalid(Constants.TokenErrors.InvalidGrant);
            }

            return Valid();
        }

        private async Task<ValidationResult> ValidateAssertionRequestAsync(NameValueCollection parameters)
        {
            var assertion = parameters.Get(Constants.TokenRequest.Assertion);
            _validatedRequest.Assertion = assertion;

            /////////////////////////////////////////////
            // check if client is authorized for grant type
            /////////////////////////////////////////////
            if (_validatedRequest.Client.Flow != Flows.Assertion)
            {
                _logger.Error("Client not authorized for assertion flow");
                return Invalid(Constants.TokenErrors.UnauthorizedClient);
            }

            /////////////////////////////////////////////
            // check if client is allowed to request scopes
            /////////////////////////////////////////////
            if (!(await ValidateRequestedScopesAsync(parameters)))
            {
                _logger.Error("Invalid scopes.");
                return Invalid(Constants.TokenErrors.InvalidScope);
            }

            /////////////////////////////////////////////
            // validate assertion
            /////////////////////////////////////////////
            var principal = await _assertionValidator.ValidateAsync(_validatedRequest);
            if (principal == null)
            {
                _logger.Error("Invalid assertion.");
                return Invalid(Constants.TokenErrors.InvalidGrant);
            }

            _validatedRequest.Subject = principal;
            return Valid();
        }

        private async Task<bool> ValidateRequestedScopesAsync(NameValueCollection parameters)
        {
            var scopeValidator = new ScopeValidator(_logger);
            var requestedScopes = scopeValidator.ParseScopes(parameters.Get(Constants.TokenRequest.Scope));

            if (requestedScopes == null)
            {
                return false;
            }

            if (!scopeValidator.AreScopesAllowed(_validatedRequest.Client, requestedScopes))
            {
                return false;
            }
            
            if (!scopeValidator.AreScopesValid(requestedScopes, await _scopes.GetScopesAsync()))
            {
                return false;
            }

            _validatedRequest.Scopes = requestedScopes;
            _validatedRequest.ValidatedScopes = scopeValidator;
            return true;
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
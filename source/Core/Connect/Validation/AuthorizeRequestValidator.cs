/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Connect.Services;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class AuthorizeRequestValidator
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly ValidatedAuthorizeRequest _validatedRequest;
        private readonly CoreSettings _core;
        private readonly IScopeService _scopes;
        private readonly IClientService _clients;
        private readonly ICustomRequestValidator _customValidator;
        private readonly IUserService _users;

        public ValidatedAuthorizeRequest ValidatedRequest
        {
            get
            {
                return _validatedRequest;
            }
        }

        public AuthorizeRequestValidator(CoreSettings core, IScopeService scopes, IClientService clients, IUserService users, ICustomRequestValidator customValidator)
        {
            _core = core;
            _scopes = scopes;
            _clients = clients;
            _users = users;
            _customValidator = customValidator;

            _validatedRequest = new ValidatedAuthorizeRequest();
            _validatedRequest.CoreSettings = _core;
        }

        // basic protocol validation
        public ValidationResult ValidateProtocol(NameValueCollection parameters)
        {
            Logger.Info("Start protocol validation");

            if (parameters == null)
            {
                Logger.Error("Parameters are null.");
                throw new ArgumentNullException("parameters");
            }

            _validatedRequest.Raw = parameters;

            //////////////////////////////////////////////////////////
            // client_id must be present
            /////////////////////////////////////////////////////////
            var clientId = parameters.Get(Constants.AuthorizeRequest.ClientId);
            if (clientId.IsMissing())
            {
                Logger.Error("client_id is missing");
                return Invalid();
            }

            Logger.InfoFormat("client_id: {0}", clientId);
            _validatedRequest.ClientId = clientId;

            //////////////////////////////////////////////////////////
            // redirect_uri must be present, and a valid uri
            //////////////////////////////////////////////////////////
            var redirectUri = parameters.Get(Constants.AuthorizeRequest.RedirectUri);

            if (redirectUri.IsMissing())
            {
                Logger.Error("redirect_uri is missing");
                return Invalid();
            }

            Uri uri;
            if (!Uri.TryCreate(redirectUri, UriKind.Absolute, out uri))
            {
                Logger.ErrorFormat("invalid redirect_uri: {0}", redirectUri);
                return Invalid();
            }

            Logger.InfoFormat("redirect_uri: {0}", redirectUri);
            _validatedRequest.RedirectUri = new Uri(redirectUri);

            //////////////////////////////////////////////////////////
            // response_type must be present and supported
            //////////////////////////////////////////////////////////
            var responseType = parameters.Get(Constants.AuthorizeRequest.ResponseType);
            if (responseType.IsMissing())
            {
                Logger.Error("Missing response_type");
                return Invalid(ErrorTypes.Client, Constants.AuthorizeErrors.UnsupportedResponseType);
            }

            if (!Constants.SupportedResponseTypes.Contains(responseType))
            {
                Logger.ErrorFormat("Response type not supported: {0}", responseType);
                return Invalid(ErrorTypes.Client, Constants.AuthorizeErrors.UnsupportedResponseType);
            }

            Logger.InfoFormat("response_type: {0}", responseType);
            _validatedRequest.ResponseType = responseType;

            //////////////////////////////////////////////////////////
            // match response_type to flow
            //////////////////////////////////////////////////////////
            if (_validatedRequest.ResponseType == Constants.ResponseTypes.Code)
            {
                Logger.Info("Flow: code");
                _validatedRequest.Flow = Flows.Code;
                _validatedRequest.ResponseMode = Constants.ResponseModes.Query;
            }
            else if (_validatedRequest.ResponseType == Constants.ResponseTypes.Token ||
                     _validatedRequest.ResponseType == Constants.ResponseTypes.IdToken ||
                     _validatedRequest.ResponseType == Constants.ResponseTypes.IdTokenToken)
            {
                Logger.Info("Flow: implicit");
                _validatedRequest.Flow = Flows.Implicit;
                _validatedRequest.ResponseMode = Constants.ResponseModes.Fragment;
            }

            //////////////////////////////////////////////////////////
            // scope must be present
            //////////////////////////////////////////////////////////
            var scope = parameters.Get(Constants.AuthorizeRequest.Scope);
            if (scope.IsMissing())
            {
                Logger.Error("scope is missing");
                return Invalid(ErrorTypes.Client);
            }

            scope = scope.Trim();

            if (scope.Contains(Constants.StandardScopes.OpenId))
            {
                _validatedRequest.IsOpenIdRequest = true;
            }

            _validatedRequest.RequestedScopes = scope.Split(' ').Distinct().ToList();
            Logger.InfoFormat("scopes: {0}", scope);

            //////////////////////////////////////////////////////////
            // check scope vs response type plausability
            //////////////////////////////////////////////////////////

            // if response_type is code - all scope variations are allowed
            if (_validatedRequest.ResponseType != Constants.ResponseTypes.Code)
            {
                // openid requests require a requested id_token
                if (_validatedRequest.IsOpenIdRequest)
                {
                    if (!_validatedRequest.ResponseType.Contains(Constants.ResponseTypes.IdToken))
                    {
                        Logger.Error("Request contains openid scope, but response_type does not contain an identity token");
                        return Invalid(ErrorTypes.Client);
                    }
                }
                else
                {
                    // resource requests require a token response_type
                    if (_validatedRequest.ResponseType != Constants.ResponseTypes.Token)
                    {
                        Logger.Error("Request does not contain the openid scope, but response_type contains an identity token");
                        return Invalid(ErrorTypes.Client);
                    }
                }
            }

            //////////////////////////////////////////////////////////
            // check state
            //////////////////////////////////////////////////////////
            var state = parameters.Get(Constants.AuthorizeRequest.State);
            if (state.IsPresent())
            {
                Logger.InfoFormat("State: {0}", state);
                _validatedRequest.State = state;
            }
            else
            {
                Logger.Info("No state supplied");
            }

            //////////////////////////////////////////////////////////
            // check nonce
            //////////////////////////////////////////////////////////
            var nonce = parameters.Get(Constants.AuthorizeRequest.Nonce);
            if (nonce.IsPresent())
            {
                Logger.InfoFormat("Nonce: {0}", nonce);
                _validatedRequest.Nonce = nonce;
            }
            else
            {
                Logger.Info("No nonce supplied");

                if (_validatedRequest.Flow == Flows.Implicit)
                {
                    // only openid requests require nonce
                    if (_validatedRequest.IsOpenIdRequest)
                    {
                        Logger.Error("Nonce required for implicit flow with openid scope");
                        return Invalid(ErrorTypes.Client);
                    }
                }
            }

            //////////////////////////////////////////////////////////
            // check response_mode
            //////////////////////////////////////////////////////////
            var responseMode = parameters.Get(Constants.AuthorizeRequest.ResponseMode);
            if (responseMode.IsPresent())
            {
                if (Constants.SupportedResponseModes.Contains(responseMode))
                {
                    if (responseMode == Constants.ResponseModes.FormPost)
                    {
                        if (_validatedRequest.ResponseType != Constants.ResponseTypes.IdToken &&
                            _validatedRequest.ResponseType != Constants.ResponseTypes.IdTokenToken)
                        {
                            Logger.Error("Invalid response_type for response_mode");
                            return Invalid(ErrorTypes.Client, Constants.AuthorizeErrors.UnsupportedResponseType);
                    
                        }
                    }
                    
                    _validatedRequest.ResponseMode = responseMode;
                    Logger.InfoFormat("response_mode: {0}", responseMode);
                }
                else
                {
                    Logger.Info("Unsupported response_mode - ignored.");
                }
            }

            //////////////////////////////////////////////////////////
            // check prompt
            //////////////////////////////////////////////////////////
            var prompt = parameters.Get(Constants.AuthorizeRequest.Prompt);
            if (prompt.IsPresent())
            {
                Logger.InfoFormat("prompt: {0}", prompt);

                if (Constants.SupportedPromptModes.Contains(prompt))
                {
                    _validatedRequest.PromptMode = prompt;
                }
                else
                {
                    Logger.Info("Unsupported prompt mode - ignored.");
                }
            }

            //////////////////////////////////////////////////////////
            // check ui locales
            //////////////////////////////////////////////////////////
            var uilocales = parameters.Get(Constants.AuthorizeRequest.UiLocales);
            if (uilocales.IsPresent())
            {
                _validatedRequest.UiLocales = uilocales;
            }

            //////////////////////////////////////////////////////////
            // check max_age
            //////////////////////////////////////////////////////////
            var maxAge = parameters.Get(Constants.AuthorizeRequest.MaxAge);
            if (maxAge.IsPresent())
            {
                Logger.InfoFormat("max_age: {0}", maxAge);

                int seconds;
                if (int.TryParse(maxAge, out seconds))
                {
                    if (seconds >= 0)
                    {
                        _validatedRequest.MaxAge = seconds;
                    }
                    else
                    {
                        Logger.Error("Invalid max_age.");
                        return Invalid(ErrorTypes.Client);
                    }
                }
                else
                {
                    Logger.Error("Invalid max_age.");
                    return Invalid(ErrorTypes.Client);
                }
            }

            // todo: parse amr, acr

            Logger.Info("Protocol validation successful");
            return Valid();
        }

        public async Task<ValidationResult> ValidateClientAsync()
        {
            Logger.Info("Start client validation");

            if (_validatedRequest.ClientId.IsMissing())
            {
                throw new InvalidOperationException("ClientId is empty. Validate protocol first.");
            }

            //////////////////////////////////////////////////////////
            // check for valid client
            //////////////////////////////////////////////////////////
            var client = await _clients.FindClientByIdAsync(_validatedRequest.ClientId);
            if (client == null || client.Enabled == false)
            {
                Logger.ErrorFormat("Unknown client or not enabled: {0}", _validatedRequest.ClientId);
                return Invalid(ErrorTypes.User, Constants.AuthorizeErrors.UnauthorizedClient);
            }

            Logger.InfoFormat("Client found in registry: {0} / {1}", client.ClientId, client.ClientName);
            _validatedRequest.Client = client;

            //////////////////////////////////////////////////////////
            // check if redirect_uri is valid
            //////////////////////////////////////////////////////////
            if (!_validatedRequest.Client.RedirectUris.Contains(_validatedRequest.RedirectUri))
            {
                Logger.ErrorFormat("Invalid redirect_uri: {0}", _validatedRequest.RedirectUri);
                return Invalid(ErrorTypes.User, Constants.AuthorizeErrors.UnauthorizedClient);
            }

            //////////////////////////////////////////////////////////
            // check if flow is allowed for client
            //////////////////////////////////////////////////////////
            if (_validatedRequest.Flow != _validatedRequest.Client.Flow)
            {
                Logger.ErrorFormat("Invalid flow for client: {0}", _validatedRequest.Flow);
                return Invalid(ErrorTypes.User, Constants.AuthorizeErrors.UnauthorizedClient);
            }

            //////////////////////////////////////////////////////////
            // check scopes and scope restrictions
            //////////////////////////////////////////////////////////
            var scopeValidator = new ScopeValidator();

            if (!scopeValidator.AreScopesAllowed(_validatedRequest.Client, _validatedRequest.RequestedScopes))
            {
                return Invalid(ErrorTypes.User, Constants.AuthorizeErrors.UnauthorizedClient);
            }

            //////////////////////////////////////////////////////////
            // check if scopes are valid/supported and check for resource scopes
            //////////////////////////////////////////////////////////
            if (!scopeValidator.AreScopesValid(_validatedRequest.RequestedScopes, await _scopes.GetScopesAsync()))
            {
                return Invalid(ErrorTypes.Client, Constants.AuthorizeErrors.InvalidScope);
            }

            if (scopeValidator.ContainsOpenIdScopes && !_validatedRequest.IsOpenIdRequest)
            {
                Logger.Error("Identity related scope requests, but no openid scope");
                return Invalid(ErrorTypes.Client, Constants.AuthorizeErrors.InvalidScope);
            }

            if (scopeValidator.ContainsResourceScopes)
            {
                _validatedRequest.IsResourceRequest = true;
            }

            _validatedRequest.ValidatedScopes = scopeValidator;

            //////////////////////////////////////////////////////////
            // check id vs resource scopes and response types plausability
            //////////////////////////////////////////////////////////
            if (!scopeValidator.IsResponseTypeValid(_validatedRequest.ResponseType))
            {
                return Invalid(ErrorTypes.Client, Constants.AuthorizeErrors.InvalidScope);
            }

            var customResult = await _customValidator.ValidateAuthorizeRequestAsync(_validatedRequest, _users);

            if (customResult.IsError)
            {
                Logger.Error("Error in custom validation: " + customResult.Error);
            }

            Logger.Info("Client validation successful");
            return customResult;
        }

        private ValidationResult Invalid(ErrorTypes errorType = ErrorTypes.User, string error = Constants.AuthorizeErrors.InvalidRequest)
        {
            var result = new ValidationResult();

            result.IsError = true;
            result.Error = error;
            result.ErrorType = errorType;

            return result;
        }

        private ValidationResult Valid()
        {
            var result = new ValidationResult();
            result.IsError = false;

            return result;
        }
    }
}
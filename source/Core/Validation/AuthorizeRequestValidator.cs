/*
 * Copyright 2014, 2015 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#pragma warning disable 1591

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Configuration.Hosting;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Validation
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class AuthorizeRequestValidator
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly ValidatedAuthorizeRequest _validatedRequest;
        private readonly IdentityServerOptions _options;
        private readonly IClientStore _clients;
        private readonly ICustomRequestValidator _customValidator;
        private readonly IRedirectUriValidator _uriValidator;
        private readonly ScopeValidator _scopeValidator;
        private readonly SessionCookie _sessionCookie;

        public ValidatedAuthorizeRequest ValidatedRequest
        {
            get
            {
                return _validatedRequest;
            }
        }

        public AuthorizeRequestValidator(IdentityServerOptions options, IClientStore clients, ICustomRequestValidator customValidator, IRedirectUriValidator uriValidator, ScopeValidator scopeValidator, SessionCookie sessionCookie)
        {
            _options = options;
            _clients = clients;
            _customValidator = customValidator;
            _uriValidator = uriValidator;
            _scopeValidator = scopeValidator;
            _sessionCookie = sessionCookie;

            _validatedRequest = new ValidatedAuthorizeRequest
            {
                Options = _options, 
            };
        }

        // basic protocol validation
        public ValidationResult ValidateProtocol(NameValueCollection parameters)
        {
            Logger.Info("Start authorize request protocol validation");

            if (parameters == null)
            {
                Logger.Error("Parameters are null.");
                throw new ArgumentNullException("parameters");
            }

            _validatedRequest.Raw = parameters;

            //////////////////////////////////////////////////////////
            // check state
            //////////////////////////////////////////////////////////
            var state = parameters.Get(Constants.AuthorizeRequest.State);
            if (state.IsPresent())
            {
                _validatedRequest.State = state;
            }
            
            //////////////////////////////////////////////////////////
            // client_id must be present
            /////////////////////////////////////////////////////////
            var clientId = parameters.Get(Constants.AuthorizeRequest.ClientId);
            if (clientId.IsMissingOrTooLong(Constants.MaxClientIdLength))
            {
                LogError("client_id is missing or too long");
                return Invalid();
            }

            _validatedRequest.ClientId = clientId;


            //////////////////////////////////////////////////////////
            // redirect_uri must be present, and a valid uri
            //////////////////////////////////////////////////////////
            var redirectUri = parameters.Get(Constants.AuthorizeRequest.RedirectUri);

            if (redirectUri.IsMissingOrTooLong(Constants.MaxRedirectUriLength))
            {
                LogError("redirect_uri is missing or too long");
                return Invalid();
            }

            Uri uri;
            if (!Uri.TryCreate(redirectUri, UriKind.Absolute, out uri))
            {
                LogError("invalid redirect_uri: " + redirectUri);
                return Invalid();
            }

            _validatedRequest.RedirectUri = redirectUri;


            //////////////////////////////////////////////////////////
            // response_type must be present and supported
            //////////////////////////////////////////////////////////
            var responseType = parameters.Get(Constants.AuthorizeRequest.ResponseType);
            if (responseType.IsMissing())
            {
                LogError("Missing response_type");
                return Invalid(ErrorTypes.Client, Constants.AuthorizeErrors.UnsupportedResponseType);
            }

            if (!Constants.SupportedResponseTypes.Contains(responseType))
            {
                LogError("Response type not supported: " + responseType);
                return Invalid(ErrorTypes.Client, Constants.AuthorizeErrors.UnsupportedResponseType);
            }

            _validatedRequest.ResponseType = responseType;


            //////////////////////////////////////////////////////////
            // match response_type to flow
            //////////////////////////////////////////////////////////
            _validatedRequest.Flow = Constants.ResponseTypeToFlowMapping[_validatedRequest.ResponseType];
            

            //////////////////////////////////////////////////////////
            // check if flow is allowed at authorize endpoint
            //////////////////////////////////////////////////////////
            if (!Constants.AllowedFlowsForAuthorizeEndpoint.Contains(_validatedRequest.Flow))
            {
                LogError("Invalid flow");
                return Invalid();
            }

            //////////////////////////////////////////////////////////
            // check response_mode parameter and set response_mode
            //////////////////////////////////////////////////////////

            // set default response mode for flow first
            _validatedRequest.ResponseMode = Constants.AllowedResponseModesForFlow[_validatedRequest.Flow].First();
            
            // check if response_mode parameter is present and valid
            var responseMode = parameters.Get(Constants.AuthorizeRequest.ResponseMode);
            if (responseMode.IsPresent())
            {
                if (Constants.SupportedResponseModes.Contains(responseMode))
                {
                    if (Constants.AllowedResponseModesForFlow[_validatedRequest.Flow].Contains(responseMode))
                    {
                        _validatedRequest.ResponseMode = responseMode;
                    }
                    else
                    {
                        LogError("Invalid response_mode for flow: " + responseMode);
                        return Invalid(ErrorTypes.User, Constants.AuthorizeErrors.UnsupportedResponseType);
                    }
                }
                else
                {
                    LogError("Unsupported response_mode: " + responseMode);
                    return Invalid(ErrorTypes.User, Constants.AuthorizeErrors.UnsupportedResponseType);
                }
            }

            //////////////////////////////////////////////////////////
            // scope must be present
            //////////////////////////////////////////////////////////
            var scope = parameters.Get(Constants.AuthorizeRequest.Scope);
            if (scope.IsMissing())
            {
                LogError("scope is missing");
                return Invalid(ErrorTypes.Client);
            }

            if (scope.Length > Constants.MaxScopeLength)
            {
                LogError("scopes too long.");
                return Invalid(ErrorTypes.Client);
            }

            _validatedRequest.RequestedScopes = scope.FromSpaceSeparatedString().Distinct().ToList();
            
            if (_validatedRequest.RequestedScopes.Contains(Constants.StandardScopes.OpenId))
            {
                _validatedRequest.IsOpenIdRequest = true;
            }

            //////////////////////////////////////////////////////////
            // check scope vs response_type plausability
            //////////////////////////////////////////////////////////
            var requirement = Constants.ResponseTypeToScopeRequirement[_validatedRequest.ResponseType];
            if (requirement == Constants.ScopeRequirement.Identity ||
                requirement == Constants.ScopeRequirement.IdentityOnly)
            {
                if (_validatedRequest.IsOpenIdRequest == false)
                {
                    LogError("response_type requires the openid scope");
                    return Invalid(ErrorTypes.Client);
                }
            }


            //////////////////////////////////////////////////////////
            // check nonce
            //////////////////////////////////////////////////////////
            var nonce = parameters.Get(Constants.AuthorizeRequest.Nonce);
            if (nonce.IsPresent())
            {
                if (nonce.Length > Constants.MaxNonceLength)
                {
                    LogError("Nonce too long");
                    return Invalid(ErrorTypes.Client);
                }

                _validatedRequest.Nonce = nonce;
            }
            else
            {
                if (_validatedRequest.Flow == Flows.Implicit)
                {
                    // only openid requests require nonce
                    if (_validatedRequest.IsOpenIdRequest)
                    {
                        LogError("Nonce required for implicit flow with openid scope");
                        return Invalid(ErrorTypes.Client);
                    }
                }
            }

            //////////////////////////////////////////////////////////
            // check prompt
            //////////////////////////////////////////////////////////
            var prompt = parameters.Get(Constants.AuthorizeRequest.Prompt);
            if (prompt.IsPresent())
            {
                if (Constants.SupportedPromptModes.Contains(prompt))
                {
                    _validatedRequest.PromptMode = prompt;
                }
                else
                {
                    Logger.Info("Unsupported prompt mode - ignored: " + prompt);
                }
            }

            //////////////////////////////////////////////////////////
            // check ui locales
            //////////////////////////////////////////////////////////
            var uilocales = parameters.Get(Constants.AuthorizeRequest.UiLocales);
            if (uilocales.IsPresent())
            {
                if (uilocales.Length > Constants.MaxUiLocaleLength)
                {
                    LogError("UI locale too long");
                    return Invalid(ErrorTypes.Client);
                }

                _validatedRequest.UiLocales = uilocales;
            }

            //////////////////////////////////////////////////////////
            // check display
            //////////////////////////////////////////////////////////
            var display = parameters.Get(Constants.AuthorizeRequest.Display);
            if (display.IsPresent())
            {
                if (Constants.SupportedDisplayModes.Contains(display))
                {
                    _validatedRequest.DisplayMode = display;
                }

                Logger.Info("Unsupported display mode - ignored: " + display);
            }

            //////////////////////////////////////////////////////////
            // check max_age
            //////////////////////////////////////////////////////////
            var maxAge = parameters.Get(Constants.AuthorizeRequest.MaxAge);
            if (maxAge.IsPresent())
            {
                int seconds;
                if (int.TryParse(maxAge, out seconds))
                {
                    if (seconds >= 0)
                    {
                        _validatedRequest.MaxAge = seconds;
                    }
                    else
                    {
                        LogError("Invalid max_age.");
                        return Invalid(ErrorTypes.Client);
                    }
                }
                else
                {
                    LogError("Invalid max_age.");
                    return Invalid(ErrorTypes.Client);
                }
            }

            //////////////////////////////////////////////////////////
            // check login_hint
            //////////////////////////////////////////////////////////
            var loginHint = parameters.Get(Constants.AuthorizeRequest.LoginHint);
            if (loginHint.IsPresent())
            {
                if (loginHint.Length > Constants.MaxLoginHintLength)
                {
                    LogError("Login hint too long");
                    return Invalid(ErrorTypes.Client);
                }

                _validatedRequest.LoginHint = loginHint;
            }

            //////////////////////////////////////////////////////////
            // check acr_values
            //////////////////////////////////////////////////////////
            var acrValues = parameters.Get(Constants.AuthorizeRequest.AcrValues);
            if (acrValues.IsPresent())
            {
                if (acrValues.Length > Constants.MaxAcrValuesLength)
                {
                    LogError("Acr values too long");
                    return Invalid(ErrorTypes.Client);
                }

                _validatedRequest.AuthenticationContextReferenceClasses = acrValues.FromSpaceSeparatedString().Distinct().ToList();
            }

            //////////////////////////////////////////////////////////
            // check session cookie
            //////////////////////////////////////////////////////////
            if (_options.Endpoints.EnableCheckSessionEndpoint)
            {
                var sessionId = _sessionCookie.GetSessionId();
                if (sessionId.IsPresent())
                {
                    _validatedRequest.SessionId = sessionId;
                }
            }
            
            LogSuccess();
            return Valid();
        }

        public async Task<ValidationResult> ValidateClientAsync()
        {
            Logger.Info("Start authorize request client validation");

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
                LogError("Unknown client or not enabled: " + _validatedRequest.ClientId);
                return Invalid(ErrorTypes.User, Constants.AuthorizeErrors.UnauthorizedClient);
            }

            _validatedRequest.Client = client;

            //////////////////////////////////////////////////////////
            // check if redirect_uri is valid
            //////////////////////////////////////////////////////////
            if (await _uriValidator.IsRedirectUriValidAsync(_validatedRequest.RedirectUri, _validatedRequest.Client) == false)
            {
                LogError("Invalid redirect_uri: " + _validatedRequest.RedirectUri);
                return Invalid(ErrorTypes.User, Constants.AuthorizeErrors.UnauthorizedClient);
            }

            //////////////////////////////////////////////////////////
            // check if flow is allowed for client
            //////////////////////////////////////////////////////////
            if (_validatedRequest.Flow != _validatedRequest.Client.Flow)
            {
                LogError("Invalid flow for client: " + _validatedRequest.Flow);
                return Invalid(ErrorTypes.User, Constants.AuthorizeErrors.UnauthorizedClient);
            }

            //////////////////////////////////////////////////////////
            // check if scopes are valid/supported and check for resource scopes
            //////////////////////////////////////////////////////////
            if (await _scopeValidator.AreScopesValidAsync(_validatedRequest.RequestedScopes) == false)
            {
                return Invalid(ErrorTypes.Client, Constants.AuthorizeErrors.InvalidScope);
            }

            if (_scopeValidator.ContainsOpenIdScopes && !_validatedRequest.IsOpenIdRequest)
            {
                LogError("Identity related scope requests, but no openid scope");
                return Invalid(ErrorTypes.Client, Constants.AuthorizeErrors.InvalidScope);
            }

            if (_scopeValidator.ContainsResourceScopes)
            {
                _validatedRequest.IsResourceRequest = true;
            }

            //////////////////////////////////////////////////////////
            // check scopes and scope restrictions
            //////////////////////////////////////////////////////////
            if (!_scopeValidator.AreScopesAllowed(_validatedRequest.Client, _validatedRequest.RequestedScopes))
            {
                return Invalid(ErrorTypes.User, Constants.AuthorizeErrors.UnauthorizedClient);
            }

            _validatedRequest.ValidatedScopes = _scopeValidator;

            //////////////////////////////////////////////////////////
            // check id vs resource scopes and response types plausability
            //////////////////////////////////////////////////////////
            if (!_scopeValidator.IsResponseTypeValid(_validatedRequest.ResponseType))
            {
                return Invalid(ErrorTypes.Client, Constants.AuthorizeErrors.InvalidScope);
            }

            //////////////////////////////////////////////////////////
            // check if sessionId is available and if session management is enabled
            //////////////////////////////////////////////////////////
            if (_options.Endpoints.EnableCheckSessionEndpoint)
            {
                if (_validatedRequest.SessionId.IsMissing())
                {
                    Logger.Warn("Session management is enabled, but session id cookie is missing.");
                }
            }

            var customResult = await _customValidator.ValidateAuthorizeRequestAsync(_validatedRequest);

            if (customResult.IsError)
            {
                LogError("Error in custom validation: " + customResult.Error);
            }
            else
            {
                LogSuccess();
            }

            return customResult;
        }

        private ValidationResult Invalid(ErrorTypes errorType = ErrorTypes.User, string error = Constants.AuthorizeErrors.InvalidRequest)
        {
            var result = new ValidationResult
            {
                IsError = true, 
                Error = error, 
                ErrorType = errorType
            };

            return result;
        }

        private ValidationResult Valid()
        {
            var result = new ValidationResult
            {
                IsError = false
            };

            return result;
        }

        private void LogError(string message)
        {
            var validationLog = new AuthorizeRequestValidationLog(_validatedRequest);
            var json = LogSerializer.Serialize(validationLog);

            Logger.ErrorFormat("{0}\n {1}", message, json);
        }

        private void LogSuccess()
        {
            var validationLog = new AuthorizeRequestValidationLog(_validatedRequest);
            var json = LogSerializer.Serialize(validationLog);

            Logger.InfoFormat("{0}\n {1}", "Authorize request validation success", json);
        }
    }
}
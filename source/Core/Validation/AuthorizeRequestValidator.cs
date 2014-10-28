/*
 * Copyright 2014 Dominick Baier, Brock Allen
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

using Microsoft.Owin;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Validation
{
    public class AuthorizeRequestValidator
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly ValidatedAuthorizeRequest _validatedRequest;
        private readonly IdentityServerOptions _options;
        private readonly IScopeStore _scopes;
        private readonly IClientStore _clients;
        private readonly ICustomRequestValidator _customValidator;

        public ValidatedAuthorizeRequest ValidatedRequest
        {
            get
            {
                return _validatedRequest;
            }
        }

        public AuthorizeRequestValidator(IdentityServerOptions options, IScopeStore scopes, IClientStore clients, ICustomRequestValidator customValidator, IOwinContext context)
        {
            _options = options;
            _scopes = scopes;
            _clients = clients;
            _customValidator = customValidator;

            _validatedRequest = new ValidatedAuthorizeRequest
            {
                Options = _options, 
                Environment = context.Environment
            };
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
            _validatedRequest.Flow = Constants.ResponseTypeToFlowMapping[_validatedRequest.ResponseType];
            Logger.Info("Flow: " + _validatedRequest.Flow.ToString());


            //////////////////////////////////////////////////////////
            // check if flow is allowed at authorize endpoint
            //////////////////////////////////////////////////////////
            if (!Constants.AllowedFlowsForAuthorizeEndpoint.Contains(_validatedRequest.Flow))
            {
                return Invalid();
            }

            //////////////////////////////////////////////////////////
            // check response_mode parameter and set response_mode
            //////////////////////////////////////////////////////////
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
                        Logger.Info("Invalid response_mode for flow: " + responseMode);
                        return Invalid(ErrorTypes.Client, Constants.AuthorizeErrors.UnsupportedResponseType);
                    }
                }
                else
                {
                    Logger.InfoFormat("Unsupported response_mode: {0}", responseMode);
                    return Invalid(ErrorTypes.Client, Constants.AuthorizeErrors.UnsupportedResponseType);
                }
            }
            else
            {
                _validatedRequest.ResponseMode = Constants.AllowedResponseModesForFlow[_validatedRequest.Flow].First();
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

            _validatedRequest.RequestedScopes = scope.FromSpaceSeparatedString().Distinct().ToList();
            Logger.InfoFormat("scopes: {0}", scope);

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
                    Logger.Error("response_type requires the openid scope");
                    return Invalid(ErrorTypes.Client);
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

            //////////////////////////////////////////////////////////
            // check login_hint
            //////////////////////////////////////////////////////////
            var loginHint = parameters.Get(Constants.AuthorizeRequest.LoginHint);
            if (loginHint.IsPresent())
            {
                Logger.InfoFormat("login_hint: {0}", loginHint);
                _validatedRequest.LoginHint = loginHint;
            }

            //////////////////////////////////////////////////////////
            // check acr_values
            //////////////////////////////////////////////////////////
            var acrValues = parameters.Get(Constants.AuthorizeRequest.AcrValues);
            if (acrValues.IsPresent())
            {
                Logger.InfoFormat("acr_values: {0}", acrValues);
                _validatedRequest.AuthenticationContextReferenceClasses = acrValues.FromSpaceSeparatedString().Distinct().ToList();
            }

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

            var scopeValidator = new ScopeValidator();
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

            //////////////////////////////////////////////////////////
            // check scopes and scope restrictions
            //////////////////////////////////////////////////////////
            if (!scopeValidator.AreScopesAllowed(_validatedRequest.Client, _validatedRequest.RequestedScopes))
            {
                return Invalid(ErrorTypes.User, Constants.AuthorizeErrors.UnauthorizedClient);
            }

            _validatedRequest.ValidatedScopes = scopeValidator;

            //////////////////////////////////////////////////////////
            // check id vs resource scopes and response types plausability
            //////////////////////////////////////////////////////////
            if (!scopeValidator.IsResponseTypeValid(_validatedRequest.ResponseType))
            {
                return Invalid(ErrorTypes.Client, Constants.AuthorizeErrors.InvalidScope);
            }

            var customResult = await _customValidator.ValidateAuthorizeRequestAsync(_validatedRequest);

            if (customResult.IsError)
            {
                Logger.Error("Error in custom validation: " + customResult.Error);
            }

            Logger.Info("Client validation successful");
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
    }
}
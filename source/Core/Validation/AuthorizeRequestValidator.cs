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

using System;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityModel;
using Thinktecture.IdentityServer.Core.App_Packages.LibLog._2._0;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Configuration.Hosting;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Logging.Models;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Validation
{
    internal class AuthorizeRequestValidator
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly IdentityServerOptions _options;
        private readonly IClientStore _clients;
        private readonly ICustomRequestValidator _customValidator;
        private readonly IRedirectUriValidator _uriValidator;
        private readonly ScopeValidator _scopeValidator;
        private readonly SessionCookie _sessionCookie;

        public AuthorizeRequestValidator(IdentityServerOptions options, IClientStore clients, ICustomRequestValidator customValidator, IRedirectUriValidator uriValidator, ScopeValidator scopeValidator, SessionCookie sessionCookie)
        {
            _options = options;
            _clients = clients;
            _customValidator = customValidator;
            _uriValidator = uriValidator;
            _scopeValidator = scopeValidator;
            _sessionCookie = sessionCookie;
        }

        public async Task<AuthorizeRequestValidationResult> ValidateAsync(NameValueCollection parameters, ClaimsPrincipal subject = null)
        {
            Logger.Info("Start authorize request protocol validation");

            var request = new ValidatedAuthorizeRequest
            {
                Options = _options,
                Subject = subject ?? Principal.Anonymous
            };
            
            if (parameters == null)
            {
                Logger.Error("Parameters are null.");
                throw new ArgumentNullException("parameters");
            }

            request.Raw = parameters;

            // validate client_id and redirect_uri
            var clientResult = await ValidateClientAsync(request);
            if (clientResult.IsError)
            {
                return clientResult;
            }

            // state, response_type, response_mode
            var mandatoryResult = ValidateCoreParameters(request);
            if (mandatoryResult.IsError)
            {
                return mandatoryResult;
            }

            // scope, scope restrictions and plausability
            var scopeResult = await ValidateScopeAsync(request);
            if (scopeResult.IsError)
            {
                return scopeResult;
            }

            // nonce, prompt, acr_values, login_hint etc.
            var optionalResult = ValidateOptionalParameters(request);
            if (optionalResult.IsError)
            {
                return optionalResult;
            }

            // custom validator
            var customResult = await _customValidator.ValidateAuthorizeRequestAsync(request);

            if (customResult.IsError)
            {
                LogError("Error in custom validation: " + customResult.Error, request);
                return Invalid(request, customResult.ErrorType, customResult.Error);
            }

            LogSuccess(request);
            return Valid(request);
        }

        public async Task<AuthorizeRequestValidationResult> ValidateClientAsync(ValidatedAuthorizeRequest request)
        {
            //////////////////////////////////////////////////////////
            // client_id must be present
            /////////////////////////////////////////////////////////
            var clientId = request.Raw.Get(Constants.AuthorizeRequest.CLIENT_ID);
            if (clientId.IsMissingOrTooLong(Constants.MAX_CLIENT_ID_LENGTH))
            {
                LogError("client_id is missing or too long", request);
                return Invalid(request);
            }

            request.ClientId = clientId;


            //////////////////////////////////////////////////////////
            // redirect_uri must be present, and a valid uri
            //////////////////////////////////////////////////////////
            var redirectUri = request.Raw.Get(Constants.AuthorizeRequest.REDIRECT_URI);

            if (redirectUri.IsMissingOrTooLong(Constants.MAX_REDIRECT_URI_LENGTH))
            {
                LogError("redirect_uri is missing or too long", request);
                return Invalid(request);
            }

            Uri uri;
            if (!Uri.TryCreate(redirectUri, UriKind.Absolute, out uri))
            {
                LogError("invalid redirect_uri: " + redirectUri, request);
                return Invalid(request);
            }

            request.RedirectUri = redirectUri;


            //////////////////////////////////////////////////////////
            // check for valid client
            //////////////////////////////////////////////////////////
            var client = await _clients.FindClientByIdAsync(request.ClientId);
            if (client == null || client.Enabled == false)
            {
                LogError("Unknown client or not enabled: " + request.ClientId, request);
                return Invalid(request, ErrorTypes.USER, Constants.AuthorizeErrors.UNAUTHORIZED_CLIENT);
            }

            request.Client = client;

            //////////////////////////////////////////////////////////
            // check if redirect_uri is valid
            //////////////////////////////////////////////////////////
            if (await _uriValidator.IsRedirectUriValidAsync(request.RedirectUri, request.Client) == false)
            {
                LogError("Invalid redirect_uri: " + request.RedirectUri, request);
                return Invalid(request, ErrorTypes.USER, Constants.AuthorizeErrors.UNAUTHORIZED_CLIENT);
            }

            return Valid(request);
        }

        private AuthorizeRequestValidationResult ValidateCoreParameters(ValidatedAuthorizeRequest request)
        {
            //////////////////////////////////////////////////////////
            // check state
            //////////////////////////////////////////////////////////
            var state = request.Raw.Get(Constants.AuthorizeRequest.STATE);
            if (state.IsPresent())
            {
                request.State = state;
            }

            //////////////////////////////////////////////////////////
            // response_type must be present and supported
            //////////////////////////////////////////////////////////
            var responseType = request.Raw.Get(Constants.AuthorizeRequest.RESPONSE_TYPE);
            if (responseType.IsMissing())
            {
                LogError("Missing response_type", request);
                return Invalid(request, ErrorTypes.CLIENT, Constants.AuthorizeErrors.UNSUPPORTED_RESPONSE_TYPE);
            }

            if (!Constants.SupportedResponseTypes.Contains(responseType))
            {
                LogError("Response type not supported: " + responseType, request);
                return Invalid(request, ErrorTypes.CLIENT, Constants.AuthorizeErrors.UNSUPPORTED_RESPONSE_TYPE);
            }

            request.ResponseType = responseType;


            //////////////////////////////////////////////////////////
            // match response_type to flow
            //////////////////////////////////////////////////////////
            request.Flow = Constants.ResponseTypeToFlowMapping[request.ResponseType];


            //////////////////////////////////////////////////////////
            // check if flow is allowed at authorize endpoint
            //////////////////////////////////////////////////////////
            if (!Constants.AllowedFlowsForAuthorizeEndpoint.Contains(request.Flow))
            {
                LogError("Invalid flow", request);
                return Invalid(request);
            }

            //////////////////////////////////////////////////////////
            // check response_mode parameter and set response_mode
            //////////////////////////////////////////////////////////

            // set default response mode for flow first
            request.ResponseMode = Constants.AllowedResponseModesForFlow[request.Flow].First();

            // check if response_mode parameter is present and valid
            var responseMode = request.Raw.Get(Constants.AuthorizeRequest.RESPONSE_MODE);
            if (responseMode.IsPresent())
            {
                if (Constants.SupportedResponseModes.Contains(responseMode))
                {
                    if (Constants.AllowedResponseModesForFlow[request.Flow].Contains(responseMode))
                    {
                        request.ResponseMode = responseMode;
                    }
                    else
                    {
                        LogError("Invalid response_mode for flow: " + responseMode, request);
                        return Invalid(request, ErrorTypes.USER, Constants.AuthorizeErrors.UNSUPPORTED_RESPONSE_TYPE);
                    }
                }
                else
                {
                    LogError("Unsupported response_mode: " + responseMode, request);
                    return Invalid(request, ErrorTypes.USER, Constants.AuthorizeErrors.UNSUPPORTED_RESPONSE_TYPE);
                }
            }

            
            //////////////////////////////////////////////////////////
            // check if flow is allowed for client
            //////////////////////////////////////////////////////////
            if (request.Flow != request.Client.Flow)
            {
                LogError("Invalid flow for client: " + request.Flow, request);
                return Invalid(request, ErrorTypes.USER, Constants.AuthorizeErrors.UNAUTHORIZED_CLIENT);
            }

            return Valid(request);
        }

        private async Task<AuthorizeRequestValidationResult> ValidateScopeAsync(ValidatedAuthorizeRequest request)
        {
            //////////////////////////////////////////////////////////
            // scope must be present
            //////////////////////////////////////////////////////////
            var scope = request.Raw.Get(Constants.AuthorizeRequest.SCOPE);
            if (scope.IsMissing())
            {
                LogError("scope is missing", request);
                return Invalid(request, ErrorTypes.CLIENT);
            }

            if (scope.Length > Constants.MAX_SCOPE_LENGTH)
            {
                LogError("scopes too long.", request);
                return Invalid(request, ErrorTypes.CLIENT);
            }

            request.RequestedScopes = scope.FromSpaceSeparatedString().Distinct().ToList();

            if (request.RequestedScopes.Contains(Constants.StandardScopes.OPEN_ID))
            {
                request.IsOpenIdRequest = true;
            }

            //////////////////////////////////////////////////////////
            // check scope vs response_type plausability
            //////////////////////////////////////////////////////////
            var requirement = Constants.ResponseTypeToScopeRequirement[request.ResponseType];
            if (requirement == Constants.ScopeRequirement.IDENTITY ||
                requirement == Constants.ScopeRequirement.IDENTITY_ONLY)
            {
                if (request.IsOpenIdRequest == false)
                {
                    LogError("response_type requires the openid scope", request);
                    return Invalid(request, ErrorTypes.CLIENT);
                }
            }

            //////////////////////////////////////////////////////////
            // check if scopes are valid/supported and check for resource scopes
            //////////////////////////////////////////////////////////
            if (await _scopeValidator.AreScopesValidAsync(request.RequestedScopes) == false)
            {
                return Invalid(request, ErrorTypes.CLIENT, Constants.AuthorizeErrors.INVALID_SCOPE);
            }

            if (_scopeValidator.ContainsOpenIdScopes && !request.IsOpenIdRequest)
            {
                LogError("Identity related scope requests, but no openid scope", request);
                return Invalid(request, ErrorTypes.CLIENT, Constants.AuthorizeErrors.INVALID_SCOPE);
            }

            if (_scopeValidator.ContainsResourceScopes)
            {
                request.IsResourceRequest = true;
            }

            //////////////////////////////////////////////////////////
            // check scopes and scope restrictions
            //////////////////////////////////////////////////////////
            if (!_scopeValidator.AreScopesAllowed(request.Client, request.RequestedScopes))
            {
                return Invalid(request, ErrorTypes.USER, Constants.AuthorizeErrors.UNAUTHORIZED_CLIENT);
            }

            request.ValidatedScopes = _scopeValidator;

            //////////////////////////////////////////////////////////
            // check id vs resource scopes and response types plausability
            //////////////////////////////////////////////////////////
            if (!_scopeValidator.IsResponseTypeValid(request.ResponseType))
            {
                return Invalid(request, ErrorTypes.CLIENT, Constants.AuthorizeErrors.INVALID_SCOPE);
            }

            return Valid(request);
        }

        private AuthorizeRequestValidationResult ValidateOptionalParameters(ValidatedAuthorizeRequest request)
        {
            //////////////////////////////////////////////////////////
            // check nonce
            //////////////////////////////////////////////////////////
            var nonce = request.Raw.Get(Constants.AuthorizeRequest.NONCE);
            if (nonce.IsPresent())
            {
                if (nonce.Length > Constants.MAX_NONCE_LENGTH)
                {
                    LogError("Nonce too long", request);
                    return Invalid(request, ErrorTypes.CLIENT);
                }

                request.Nonce = nonce;
            }
            else
            {
                if (request.Flow == Flows.IMPLICIT)
                {
                    // only openid requests require nonce
                    if (request.IsOpenIdRequest)
                    {
                        LogError("Nonce required for implicit flow with openid scope", request);
                        return Invalid(request, ErrorTypes.CLIENT);
                    }
                }
            }


            //////////////////////////////////////////////////////////
            // check prompt
            //////////////////////////////////////////////////////////
            var prompt = request.Raw.Get(Constants.AuthorizeRequest.PROMPT);
            if (prompt.IsPresent())
            {
                if (Constants.SupportedPromptModes.Contains(prompt))
                {
                    request.PromptMode = prompt;
                }
                else
                {
                    Logger.Info("Unsupported prompt mode - ignored: " + prompt);
                }
            }

            //////////////////////////////////////////////////////////
            // check ui locales
            //////////////////////////////////////////////////////////
            var uilocales = request.Raw.Get(Constants.AuthorizeRequest.UI_LOCALES);
            if (uilocales.IsPresent())
            {
                if (uilocales.Length > Constants.MAX_UI_LOCALE_LENGTH)
                {
                    LogError("UI locale too long", request);
                    return Invalid(request, ErrorTypes.CLIENT);
                }

                request.UiLocales = uilocales;
            }

            //////////////////////////////////////////////////////////
            // check display
            //////////////////////////////////////////////////////////
            var display = request.Raw.Get(Constants.AuthorizeRequest.DISPLAY);
            if (display.IsPresent())
            {
                if (Constants.SupportedDisplayModes.Contains(display))
                {
                    request.DisplayMode = display;
                }

                Logger.Info("Unsupported display mode - ignored: " + display);
            }

            //////////////////////////////////////////////////////////
            // check max_age
            //////////////////////////////////////////////////////////
            var maxAge = request.Raw.Get(Constants.AuthorizeRequest.MAX_AGE);
            if (maxAge.IsPresent())
            {
                int seconds;
                if (int.TryParse(maxAge, out seconds))
                {
                    if (seconds >= 0)
                    {
                        request.MaxAge = seconds;
                    }
                    else
                    {
                        LogError("Invalid max_age.", request);
                        return Invalid(request, ErrorTypes.CLIENT);
                    }
                }
                else
                {
                    LogError("Invalid max_age.", request);
                    return Invalid(request, ErrorTypes.CLIENT);
                }
            }

            //////////////////////////////////////////////////////////
            // check login_hint
            //////////////////////////////////////////////////////////
            var loginHint = request.Raw.Get(Constants.AuthorizeRequest.LOGIN_HINT);
            if (loginHint.IsPresent())
            {
                if (loginHint.Length > Constants.MAX_LOGIN_HINT_LENGTH)
                {
                    LogError("Login hint too long", request);
                    return Invalid(request, ErrorTypes.CLIENT);
                }

                request.LoginHint = loginHint;
            }

            //////////////////////////////////////////////////////////
            // check acr_values
            //////////////////////////////////////////////////////////
            var acrValues = request.Raw.Get(Constants.AuthorizeRequest.ACR_VALUES);
            if (acrValues.IsPresent())
            {
                if (acrValues.Length > Constants.MAX_ACR_VALUES_LENGTH)
                {
                    LogError("Acr values too long", request);
                    return Invalid(request, ErrorTypes.CLIENT);
                }

                request.AuthenticationContextReferenceClasses = acrValues.FromSpaceSeparatedString().Distinct().ToList();
            }

            //////////////////////////////////////////////////////////
            // check session cookie
            //////////////////////////////////////////////////////////
            if (_options.Endpoints.EnableCheckSessionEndpoint)
            {
                var sessionId = _sessionCookie.GetSessionId();
                if (sessionId.IsPresent())
                {
                    request.SessionId = sessionId;
                }
                else
                {
                    LogError("Check session endpoint enabled, but SessionId is missing", request);
                }
            }

            return Valid(request);
        }

        private AuthorizeRequestValidationResult Invalid(ValidatedAuthorizeRequest request, ErrorTypes errorType = ErrorTypes.USER, string error = Constants.AuthorizeErrors.INVALID_REQUEST)
        {
            var result = new AuthorizeRequestValidationResult
            {
                IsError = true,
                Error = error,
                ErrorType = errorType,
                ValidatedRequest = request
            };

            return result;
        }

        private AuthorizeRequestValidationResult Valid(ValidatedAuthorizeRequest request)
        {
            var result = new AuthorizeRequestValidationResult
            {
                IsError = false,
                ValidatedRequest = request
            };

            return result;
        }

        private void LogError(string message, ValidatedAuthorizeRequest request)
        {
            var validationLog = new AuthorizeRequestValidationLog(request);
            var json = LogSerializer.Serialize(validationLog);

            Logger.ErrorFormat("{0}\n {1}", message, json);
        }

        private void LogSuccess(ValidatedAuthorizeRequest request)
        {
            var validationLog = new AuthorizeRequestValidationLog(request);
            var json = LogSerializer.Serialize(validationLog);

            Logger.InfoFormat("{0}\n {1}", "Authorize request validation success", json);
        }
    }
}
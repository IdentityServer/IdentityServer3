﻿/*
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

using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Services;

#pragma warning disable 1591

namespace Thinktecture.IdentityServer.Core.Validation
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class EndSessionRequestValidator
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly ValidatedEndSessionRequest _validatedRequest;
        private readonly TokenValidator _tokenValidator;
        private readonly IRedirectUriValidator _uriValidator;

        public ValidatedEndSessionRequest ValidatedRequest
        {
            get
            {
                return _validatedRequest;
            }
        }

        public EndSessionRequestValidator(IdentityServerOptions options, TokenValidator tokenValidator, IRedirectUriValidator uriValidator)
        {
            _tokenValidator = tokenValidator;
            _uriValidator = uriValidator;

            _validatedRequest = new ValidatedEndSessionRequest
            {
                Options = options,
            };
        }

        public async Task<ValidationResult> ValidateAsync(NameValueCollection parameters, ClaimsPrincipal subject)
        {
            Logger.Info("Start end session request validation");

            _validatedRequest.Raw = parameters;
            _validatedRequest.Subject = subject;

            if (!subject.Identity.IsAuthenticated)
            {
                Logger.Warn("User is anonymous. Ignoring end session parameters");
                return Invalid();
            }

            var idTokenHint = parameters.Get(Constants.EndSessionRequest.IdTokenHint);
            if (idTokenHint.IsPresent())
            {
                // validate id_token - no need to validate token life time
                var tokenValidationResult = await _tokenValidator.ValidateIdentityTokenAsync(idTokenHint, null, false);
                if (tokenValidationResult.IsError)
                {
                    LogError("Error validating id token hint.");
                    return Invalid();
                }

                _validatedRequest.Client = tokenValidationResult.Client;

                // validate sub claim against currently logged on user
                var subClaim = tokenValidationResult.Claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.Subject);
                if (subClaim != null)
                {
                    if (subject.GetSubjectId() != subClaim.Value)
                    {
                        LogError("Current user does not match identity token");
                        return Invalid();
                    }
                }

                var redirectUri = parameters.Get(Constants.EndSessionRequest.PostLogoutRedirectUri);
                if (redirectUri.IsPresent())
                {
                    _validatedRequest.PostLogOutUri = redirectUri;

                    if (await _uriValidator.IsPostLogoutRedirectUriValidAsync(redirectUri, _validatedRequest.Client) == false)
                    {
                        LogError("Invalid post logout URI");
                        return Invalid();
                    }

                    var state = parameters.Get(Constants.EndSessionRequest.State);
                    if (state.IsPresent())
                    {
                        _validatedRequest.State = state;
                    }
                }
            }

            LogSuccess();
            return Valid();
        }

        private ValidationResult Valid()
        {
            return new ValidationResult
            {
                IsError = false
            };
        }

        private ValidationResult Invalid()
        {
            return new ValidationResult
            {
                IsError = true,
                Error = "Invalid request"
            };
        }

        private void LogError(string message)
        {
            var log = new EndSessionRequestValidationLog(_validatedRequest);
            var json = LogSerializer.Serialize(log);
            
            Logger.ErrorFormat("{0}\n{1}", message, json);
        }

        private void LogSuccess()
        {
            var log = new EndSessionRequestValidationLog(_validatedRequest);
            var json = LogSerializer.Serialize(log);

            Logger.InfoFormat("{0}\n{1}", "End session request validation success", json);
        }
    }
}
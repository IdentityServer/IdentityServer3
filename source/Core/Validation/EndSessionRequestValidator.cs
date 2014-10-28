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
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Validation
{
    public class EndSessionRequestValidator
    {
        private readonly ValidatedEndSessionRequest _validatedRequest;
        private readonly TokenValidator _tokenValidator;

        public ValidatedEndSessionRequest ValidatedRequest
        {
            get
            {
                return _validatedRequest;
            }
        }

        public EndSessionRequestValidator(IdentityServerOptions options, IOwinContext context, TokenValidator tokenValidator, IClientStore clients)
        {
            _tokenValidator = tokenValidator;

            _validatedRequest = new ValidatedEndSessionRequest
            {
                Options = options,
                Environment = context.Environment
            };
        }

        public async Task<ValidationResult> ValidateAsync(NameValueCollection parameters, ClaimsPrincipal subject)
        {
            _validatedRequest.Raw = parameters;
            _validatedRequest.Subject = subject;

            if (!subject.Identity.IsAuthenticated)
            {
                return Invalid();
            }

            var idTokenHint = parameters.Get(Constants.EndSessionRequest.IdTokenHint);
            if (idTokenHint.IsPresent())
            {
                // validate id_token
                var tokenValidationResult = await _tokenValidator.ValidateIdentityTokenAsync(idTokenHint);
                if (tokenValidationResult.IsError)
                {
                    return Invalid();
                }

                _validatedRequest.Client = tokenValidationResult.Client;

                // validate sub claim against currently logged on user
                var subClaim = tokenValidationResult.Claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.Subject);
                if (subClaim != null)
                {
                    if (subject.GetSubjectId() != subClaim.Value)
                    {
                        return Invalid();
                    }
                }

                var redirectUri = parameters.Get(Constants.EndSessionRequest.PostLogoutRedirectUri);
                if (redirectUri.IsPresent())
                {
                    Uri uri;
                    if (Uri.TryCreate(redirectUri, UriKind.Absolute, out uri))
                    {
                        if (_validatedRequest.Client.PostLogoutRedirectUris.Contains(uri))
                        {
                            _validatedRequest.PostLogOutUri = uri;
                        }
                        else
                        {
                            return Invalid();
                        }
                    }
                    
                    var state = parameters.Get(Constants.EndSessionRequest.State);
                    if (state.IsPresent())
                    {
                        _validatedRequest.State = state;
                    }
                }
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

        private ValidationResult Invalid()
        {
            return new ValidationResult
            {
                IsError = true,
                Error = "Invalid request"
            };
        }
    }
}
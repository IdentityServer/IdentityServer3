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

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Resources;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Validation;

namespace Thinktecture.IdentityServer.Core.ResponseHandling
{
    public class AuthorizeInteractionResponseGenerator
    {
        private readonly SignInMessage _signIn;
        private readonly IConsentService _consent;
        private readonly IUserService _users;

        public AuthorizeInteractionResponseGenerator(IConsentService consent, IUserService users)
        {
            _signIn = new SignInMessage();
            _consent = consent;
            _users = users;
        }

        public async Task<LoginInteractionResponse> ProcessLoginAsync(ValidatedAuthorizeRequest request, ClaimsPrincipal user)
        {
            // let the login page know the client requesting authorization
            _signIn.ClientId = request.ClientId;

            // pass through display mode to signin service
            if (request.DisplayMode.IsPresent())
            {
                _signIn.DisplayMode = request.DisplayMode;
            }

            // pass through ui locales to signin service
            if (request.UiLocales.IsPresent())
            {
                _signIn.UiLocales = request.UiLocales;
            }

            // check login_hint - we only support idp: right now
            if (request.LoginHint.IsPresent())
            {
                if (request.LoginHint.StartsWith(Constants.LoginHints.HomeRealm))
                {
                    _signIn.IdP = request.LoginHint.Substring(Constants.LoginHints.HomeRealm.Length);
                }
                if (request.LoginHint.StartsWith(Constants.LoginHints.Tenant))
                {
                    _signIn.Tenant = request.LoginHint.Substring(Constants.LoginHints.Tenant.Length);
                }
            }

            // pass through acr values
            if (request.AuthenticationContextReferenceClasses.Any())
            {
                _signIn.AcrValues = request.AuthenticationContextReferenceClasses;
            }

            if (request.PromptMode == Constants.PromptModes.Login)
            {
                // remove prompt so when we redirect back in from login page
                // we won't think we need to force a prompt again
                request.Raw.Remove(Constants.AuthorizeRequest.Prompt);
                return new LoginInteractionResponse
                {
                    SignInMessage = _signIn
                };
            }

            // unauthenticated user
            if (user.Identity.IsAuthenticated    == false ||
                await _users.IsActiveAsync(user) == false)
            {
                // prompt=none means user must be signed in already
                if (request.PromptMode == Constants.PromptModes.None)
                {
                    return new LoginInteractionResponse
                    {
                        Error = new AuthorizeError
                        {
                            ErrorType = ErrorTypes.Client,
                            Error = Constants.AuthorizeErrors.LoginRequired,
                            ResponseMode = request.ResponseMode,
                            ErrorUri = request.RedirectUri,
                            State = request.State
                        }
                    };
                }

                return new LoginInteractionResponse
                {
                    SignInMessage = _signIn
                };
            }

            // check current idp
            var currentIdp = user.GetIdentityProvider();

            // check if idp login hint matches current provider
            if (_signIn.IdP.IsPresent())
            {
                if (_signIn.IdP != currentIdp)
                {
                    return new LoginInteractionResponse
                    {
                        SignInMessage = _signIn
                    };
                }
            }

            // check authentication freshness
            if (request.MaxAge.HasValue)
            {
                var authTime = user.GetAuthenticationTime();
                if (DateTime.UtcNow > authTime.AddSeconds(request.MaxAge.Value))
                {
                    return new LoginInteractionResponse
                    {
                        SignInMessage = _signIn
                    };
                }
            }

            return new LoginInteractionResponse();
        }

        public Task<LoginInteractionResponse> ProcessClientLoginAsync(ValidatedAuthorizeRequest request)
        {
            // check idp restrictions
            var currentIdp = request.Subject.GetIdentityProvider();
            if (request.Client.IdentityProviderRestrictions != null && request.Client.IdentityProviderRestrictions.Any())
            {
                if (!request.Client.IdentityProviderRestrictions.Contains(currentIdp))
                {
                    var response = new LoginInteractionResponse
                    {
                        SignInMessage = _signIn
                    };

                    return Task.FromResult(response);
                }
            }

            return Task.FromResult(new LoginInteractionResponse());
        }

        public async Task<ConsentInteractionResponse> ProcessConsentAsync(ValidatedAuthorizeRequest request, UserConsent consent = null)
        {
            if (request == null) throw new ArgumentNullException("request");

            if (request.PromptMode != null && 
                request.PromptMode != Constants.PromptModes.None &&
                request.PromptMode != Constants.PromptModes.Consent)
            {
                throw new ArgumentException("Invalid PromptMode");
            }

            var consentRequired = await _consent.RequiresConsentAsync(request.Client, request.Subject, request.RequestedScopes);

            if (consentRequired && request.PromptMode == Constants.PromptModes.None)
            {
                return new ConsentInteractionResponse
                {
                    Error = new AuthorizeError
                    {
                        ErrorType = ErrorTypes.Client,
                        Error = Constants.AuthorizeErrors.InteractionRequired,
                        ResponseMode = request.ResponseMode,
                        ErrorUri = request.RedirectUri,
                        State = request.State
                    }
                };
            }

            if (request.PromptMode == Constants.PromptModes.Consent || consentRequired)
            {
                var response = new ConsentInteractionResponse();

                // did user provide consent
                if (consent == null)
                {
                    // user was not yet shown conset screen
                    response.IsConsent = true;
                }
                else
                {
                    request.WasConsentShown = true;

                    // user was shown consent -- did they say yes or no
                    if (consent.WasConsentGranted == false)
                    {
                        // no need to show consent screen again
                        // build access denied error to return to client
                        response.Error = new AuthorizeError { 
                            ErrorType = ErrorTypes.Client,
                            Error = Constants.AuthorizeErrors.AccessDenied,
                            ResponseMode = request.ResponseMode,
                            ErrorUri = request.RedirectUri, 
                            State = request.State
                        };
                    }
                    else
                    {
                        // they said yes, set scopes they chose
                        request.ValidatedScopes.SetConsentedScopes(consent.ScopedConsented);

                        if (!request.ValidatedScopes.GrantedScopes.Any())
                        {
                            // they said yes, but didn't pick any scopes
                            // show consent again and provide error message
                            response.IsConsent = true;
                            response.ConsentError = Messages.MustSelectAtLeastOnePermission;
                        }
                        else if (request.Client.AllowRememberConsent)
                        {
                            // remember consent
                            var scopes = Enumerable.Empty<string>();
                            if (consent.RememberConsent)
                            {
                                // remember what user actually selected
                                scopes = request.ValidatedScopes.GrantedScopes.Select(x => x.Name);
                            }
                            
                            await _consent.UpdateConsentAsync(request.Client, request.Subject, scopes);
                        }
                    }
                }
                
                return response;
            }

            return new ConsentInteractionResponse();
        }
    }
}
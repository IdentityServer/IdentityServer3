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

using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Logging;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Resources;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Validation;
using IdentityServer3.Core.ViewModels;
using System;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace IdentityServer3.Core.ResponseHandling
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class AuthorizeInteractionResponseGenerator
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly SignInMessage _signIn;
        private readonly IdentityServerOptions _options;
        private readonly IConsentService _consent;
        private readonly IUserService _users;
        private readonly ILocalizationService _localizationService;

        public AuthorizeInteractionResponseGenerator(IdentityServerOptions options, IConsentService consent, IUserService users, ILocalizationService localizationService)
        {
            _signIn = new SignInMessage();
            _options = options;
            _consent = consent;
            _users = users;
            _localizationService = localizationService;
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

            // pass through login_hint
            if (request.LoginHint.IsPresent())
            {
                _signIn.LoginHint = request.LoginHint;
            }

            // process acr values
            var acrValues = request.AuthenticationContextReferenceClasses.Distinct().ToList();
            
            // look for well-known acr value -- idp
            var idp = acrValues.FirstOrDefault(x => x.StartsWith(Constants.KnownAcrValues.HomeRealm));
            if (idp.IsPresent())
            {
                _signIn.IdP = idp.Substring(Constants.KnownAcrValues.HomeRealm.Length);
                acrValues.Remove(idp);
            }

            // look for well-known acr value -- tenant
            var tenant = acrValues.FirstOrDefault(x => x.StartsWith(Constants.KnownAcrValues.Tenant));
            if (tenant.IsPresent())
            {
                _signIn.Tenant = tenant.Substring(Constants.KnownAcrValues.Tenant.Length);
                acrValues.Remove(tenant);
            }

            // pass through any remaining acr values
            if (acrValues.Any())
            {
                _signIn.AcrValues = acrValues;
            }

            if (request.PromptMode == Constants.PromptModes.Login)
            {
                // remove prompt so when we redirect back in from login page
                // we won't think we need to force a prompt again
                request.Raw.Remove(Constants.AuthorizeRequest.Prompt);

                Logger.Info("Redirecting to login page because of prompt=login");

                return new LoginInteractionResponse
                {
                    SignInMessage = _signIn
                };
            }

            // unauthenticated user
            var isAuthenticated = user.Identity.IsAuthenticated;
            if (!isAuthenticated) Logger.Info("User is not authenticated. Redirecting to login.");
            
            // user de-activated
            bool isActive = false;

            if (isAuthenticated)
            {
                var isActiveCtx = new IsActiveContext(user, request.Client);
                await _users.IsActiveAsync(isActiveCtx);
                
                isActive = isActiveCtx.IsActive; 
                if (!isActive) Logger.Info("User is not active. Redirecting to login.");
            }

            if (!isAuthenticated || !isActive)
            {
                // prompt=none means user must be signed in already
                if (request.PromptMode == Constants.PromptModes.None)
                {
                    Logger.Info("prompt=none was requested. But user is not authenticated.");

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
                    Logger.Info("Current IdP is not the requested IdP. Redirecting to login");
                    Logger.InfoFormat("Current: {0} -- Requested: {1}", currentIdp, _signIn.IdP);

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
                if (DateTimeOffsetHelper.UtcNow > authTime.AddSeconds(request.MaxAge.Value))
                {
                    Logger.Info("Requested MaxAge exceeded. Redirecting to login");

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

                    Logger.WarnFormat("User is logged in with idp: {0}, but idp not in client restriction list.", currentIdp); 
                    
                    return Task.FromResult(response);
                }
            }

            // check if idp is local and local logins are not allowed
            if (currentIdp == Constants.BuiltInIdentityProvider)
            {
                if (_options.AuthenticationOptions.EnableLocalLogin == false || 
                    request.Client.EnableLocalLogin == false)
                {
                    var response = new LoginInteractionResponse
                    {
                        SignInMessage = _signIn
                    };

                    Logger.Warn("User is logged in with local idp, but local logins not enabled.");
                    
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
                Logger.Info("Prompt=none requested, but consent is required.");

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
                            response.ConsentError = _localizationService.GetMessage(MessageIds.MustSelectAtLeastOnePermission);
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
/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Authentication;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class AuthorizeInteractionResponseGenerator
    {
        private SignInMessage _signIn;
        private CoreSettings _settings;

        private IConsentService _consent;

        public AuthorizeInteractionResponseGenerator(CoreSettings settings, IConsentService consent)
        {
            _signIn = new SignInMessage();

            _settings = settings;
            _consent = consent;
        }

        public InteractionResponse ProcessLogin(ValidatedAuthorizeRequest request, ClaimsPrincipal user)
        {
            // pass through display mode to signin service
            if (request.DisplayMode.IsPresent())
            {
                _signIn.DisplayMode = request.DisplayMode;
            }

            // pass through ui locales to signin service
            if (request.UiLocales.IsPresent())
            {
                _signIn.UILocales = request.UiLocales;
            }

            if (request.PromptMode == Constants.PromptModes.Login)
            {
                // remove prompt so when we redirect back in from login page
                // we won't think we need to force a prompt again
                request.Raw.Remove(Constants.AuthorizeRequest.Prompt);
                return new InteractionResponse
                {
                    IsLogin = true,
                    SignInMessage = _signIn
                };
            }

            // unauthenticated user
            if (!user.Identity.IsAuthenticated)
            {
                // prompt=none means user must be signed in already
                if (request.PromptMode == Constants.PromptModes.None)
                {
                    return new InteractionResponse
                    {
                        IsError = true,
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

                return new InteractionResponse
                {
                    IsLogin = true,
                    SignInMessage = _signIn
                };
            }

            // check authentication freshness
            if (request.MaxAge.HasValue)
            {
                var authTime = user.GetAuthenticationTime();
                if (DateTime.UtcNow > authTime.AddSeconds(request.MaxAge.Value))
                {
                    return new InteractionResponse
                    {
                        IsLogin = true,
                        SignInMessage = _signIn
                    };
                }
            }

            return new InteractionResponse();
        }

        public async Task<InteractionResponse> ProcessConsentAsync(ValidatedAuthorizeRequest request, UserConsent consent)
        {
            var consentRequired = await _consent.RequiresConsentAsync(request.Client, request.Subject, request.RequestedScopes);

            if (consentRequired && request.PromptMode == Constants.PromptModes.None)
            {
                return new InteractionResponse
                {
                    IsError = true,
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
                var response = new InteractionResponse();

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
                        response.IsError = true;
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
                            response.ConsentError = "Must select at least one permission.";
                        }
                        
                        if (request.Client.AllowRememberConsent)
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

            return new InteractionResponse();
        }
    }
}
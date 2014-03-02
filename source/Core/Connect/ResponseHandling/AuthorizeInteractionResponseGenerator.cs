using System;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Authentication;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class AuthorizeInteractionResponseGenerator
    {
        private SignInMessage _signIn;
        private ICoreSettings _core;

        public AuthorizeInteractionResponseGenerator(ICoreSettings core)
        {
            _signIn = new SignInMessage();
            _core = core;
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
                            Error = Constants.AuthorizeErrors.InteractionRequired,
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

            // prompt=login

            // clear the auth cookie
            // remove the prompt=login
            // redirect to login page

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

        public InteractionResponse ProcessConsent(ValidatedAuthorizeRequest request, ClaimsPrincipal user)
        {
            if (request.PromptMode == Constants.PromptModes.Consent ||
                _core.RequiresConsent(request.Client, user, request.RequestedScopes))
            {
                return new InteractionResponse
                {
                    IsConsent = true
                };
            }

            return new InteractionResponse();
        }
    }
}

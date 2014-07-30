/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using System.Collections.Specialized;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Authentication;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Hosting;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Views;

namespace Thinktecture.IdentityServer.Core.Connect
{
    [HostAuthentication(Constants.PrimaryAuthenticationType)]
    [SecurityHeaders]
    [NoCache]
    public class AuthorizeEndpointController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly IViewService _viewService;
        private readonly AuthorizeRequestValidator _validator;
        private readonly AuthorizeResponseGenerator _responseGenerator;
        private readonly AuthorizeInteractionResponseGenerator _interactionGenerator;
        private readonly IdentityServerOptions _options;

        public AuthorizeEndpointController(
            IViewService viewService,
            AuthorizeRequestValidator validator,
            AuthorizeResponseGenerator responseGenerator,
            AuthorizeInteractionResponseGenerator interactionGenerator,
            IdentityServerOptions options)
        {
            _viewService = viewService;
            _options = options;

            _responseGenerator = responseGenerator;
            _interactionGenerator = interactionGenerator;
            _validator = validator;
        }

        [Route(Constants.RoutePaths.Oidc.Authorize, Name = Constants.RouteNames.Oidc.Authorize)]
        public async Task<IHttpActionResult> Get(HttpRequestMessage request)
        {
            Logger.Info("Start authorize request");

            return await ProcessRequestAsync(request.RequestUri.ParseQueryString());
        }

        protected async Task<IHttpActionResult> ProcessRequestAsync(NameValueCollection parameters, UserConsent consent = null)
        {
            if (!_options.AuthorizeEndpoint.IsEnabled)
            {
                Logger.Warn("Endpoint is disabled. Aborting");
                return NotFound();
            }

            ///////////////////////////////////////////////////////////////
            // validate protocol parameters
            //////////////////////////////////////////////////////////////
            var result = _validator.ValidateProtocol(parameters);
            var request = _validator.ValidatedRequest;

            if (result.IsError)
            {
                return this.AuthorizeError(
                    result.ErrorType,
                    result.Error,
                    request.ResponseMode,
                    request.RedirectUri,
                    request.State);
            }

            var loginInteraction = _interactionGenerator.ProcessLogin(request, User as ClaimsPrincipal);

            if (loginInteraction.IsError)
            {
                return this.AuthorizeError(loginInteraction.Error);
            }
            if (loginInteraction.IsLogin)
            {
                return this.RedirectToLogin(loginInteraction.SignInMessage, request.Raw, _options);
            }

            // user must be authenticated at this point
            if (!User.Identity.IsAuthenticated)
            {
                throw new InvalidOperationException("User is not authenticated");
            }

            request.Subject = User as ClaimsPrincipal;

            ///////////////////////////////////////////////////////////////
            // validate client
            //////////////////////////////////////////////////////////////
            result = await _validator.ValidateClientAsync();

            if (result.IsError)
            {
                return this.AuthorizeError(
                    result.ErrorType,
                    result.Error,
                    request.ResponseMode,
                    request.RedirectUri,
                    request.State);
            }

            var consentInteraction = await _interactionGenerator.ProcessConsentAsync(request, consent);

            if (consentInteraction.IsError)
            {
                return this.AuthorizeError(consentInteraction.Error);
            }

            if (consentInteraction.IsConsent)
            {
                Logger.Info("Showing consent screen");
                return CreateConsentResult(request, consent, request.Raw, consentInteraction.ConsentError);
            }

            return await CreateAuthorizeResponseAsync(request);
        }

        [Route(Constants.RoutePaths.Oidc.Consent, Name = Constants.RouteNames.Oidc.Consent)]
        [HttpPost]
        public Task<IHttpActionResult> PostConsent(UserConsent model)
        {
            Logger.Info("Resuming from consent, restarting validation");
            return ProcessRequestAsync(Request.RequestUri.ParseQueryString(), model ?? new UserConsent());
        }

        [Route(Constants.RoutePaths.Oidc.SwitchUser, Name = Constants.RouteNames.Oidc.SwitchUser)]
        [HttpGet]
        public async Task<IHttpActionResult> LoginAsDifferentUser()
        {
            var parameters = Request.RequestUri.ParseQueryString();
            parameters[Constants.AuthorizeRequest.Prompt] = Constants.PromptModes.Login;
            return await ProcessRequestAsync(parameters);
        }

        private async Task<IHttpActionResult> CreateAuthorizeResponseAsync(ValidatedAuthorizeRequest request)
        {
            if (request.Flow == Flows.Implicit)
            {
                return await CreateImplicitFlowAuthorizeResponseAsync(request);
            }

            if (request.Flow == Flows.Code)
            {
                return await CreateCodeFlowAuthorizeResponseAsync(request);
            }

            Logger.Error("Unsupported flow. Aborting.");
            throw new InvalidOperationException("Unsupported flow");
        }

        private async Task<IHttpActionResult> CreateCodeFlowAuthorizeResponseAsync(ValidatedAuthorizeRequest request)
        {
            var response = await _responseGenerator.CreateCodeFlowResponseAsync(request, User as ClaimsPrincipal);
            return this.AuthorizeCodeResponse(response);
        }

        private async Task<IHttpActionResult> CreateImplicitFlowAuthorizeResponseAsync(ValidatedAuthorizeRequest request)
        {
            var response = await _responseGenerator.CreateImplicitFlowResponseAsync(request);

            // create form post response if responseMode is set form_post
            if (request.ResponseMode == Constants.ResponseModes.FormPost)
            {
                return this.AuthorizeImplicitFormPostResponse(response);
            }

            return this.AuthorizeImplicitFragmentResponse(response);
        }

        private IHttpActionResult CreateConsentResult(
            ValidatedAuthorizeRequest validatedRequest,
            UserConsent consent,
            NameValueCollection requestParameters,
            string errorMessage)
        {
            var env = Request.GetOwinEnvironment();
            var consentModel = new ConsentViewModel()
            {
                SiteName = _options.SiteName,
                SiteUrl = env.GetIdentityServerBaseUrl(),
                ErrorMessage = errorMessage,
                CurrentUser = User.GetName(),
                ClientName = validatedRequest.Client.ClientName,
                ClientUrl = validatedRequest.Client.ClientUri,
                ClientLogoUrl = validatedRequest.Client.LogoUri.AbsoluteUri,
                IdentityScopes = validatedRequest.GetIdentityScopes(),
                ApplicationScopes = validatedRequest.GetApplicationScopes(),
                AllowRememberConsent = validatedRequest.Client.AllowRememberConsent,
                RememberConsent = consent != null ? consent.RememberConsent : true,
                LoginWithDifferentAccountUrl = Url.Route(Constants.RouteNames.Oidc.SwitchUser, null) + "?" + requestParameters.ToQueryString(),
                LogoutUrl = Url.Route(Constants.RouteNames.Oidc.EndSession, null),
                ConsentUrl = Url.Route(Constants.RouteNames.Oidc.Consent, null) + "?" + requestParameters.ToQueryString()
            };
            return new ConsentActionResult(_viewService, env, consentModel);
        }

        IHttpActionResult RedirectToLogin(SignInMessage message, NameValueCollection parameters, IdentityServerOptions options)
        {
            message = message ?? new SignInMessage();

            var path = Url.Route(Constants.RouteNames.Oidc.Authorize, null) + "?" + parameters.ToQueryString();
            var url = new Uri(Request.RequestUri, path);
            message.ReturnUrl = url.AbsoluteUri;

            return new LoginResult(message, Request.GetOwinContext().Environment, _options.DataProtector);
        }

        IHttpActionResult AuthorizeError(ErrorTypes errorType, string error, string responseMode, Uri errorUri, string state)
        {
            return AuthorizeError(new AuthorizeError
            {
                ErrorType = errorType,
                Error = error,
                ResponseMode = responseMode,
                ErrorUri = errorUri,
                State = state
            });
        }

        IHttpActionResult AuthorizeError(AuthorizeError error)
        {
            if (error.ErrorType == ErrorTypes.User)
            {
                var env = Request.GetOwinEnvironment();
                var errorModel = new ErrorViewModel
                {
                    SiteName = _options.SiteName,
                    SiteUrl = env.GetIdentityServerBaseUrl(),
                    CurrentUser = User.GetName(),
                    ErrorMessage = error.Error
                };
                var errorResult = new ErrorActionResult(_viewService, env, errorModel);
                return errorResult;
            }
            else
            {
                string character;
                if (error.ResponseMode == Constants.ResponseModes.Query ||
                    error.ResponseMode == Constants.ResponseModes.FormPost)
                {
                    character = "?";
                }
                else
                {
                    character = "#";
                }

                var url = string.Format("{0}{1}error={2}", error.ErrorUri.AbsoluteUri, character, error.Error);

                if (error.State.IsPresent())
                {
                    url = string.Format("{0}&state={1}", url, error.State);
                }

                Logger.Info("Redirecting to: " + url);

                return Redirect(url);
            }
        }
    }
}
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

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.App_Packages.LibLog._2._0;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Configuration.Hosting;
using Thinktecture.IdentityServer.Core.Events.Base;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.ResponseHandling;
using Thinktecture.IdentityServer.Core.Results;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Validation;
using Thinktecture.IdentityServer.Core.ViewModels;

#pragma warning disable 1591

namespace Thinktecture.IdentityServer.Core.Endpoints.Connect
{
    /// <summary>
    /// OAuth2/OpenID Connect authorize endpoint
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [ErrorPageFilter]
    [HostAuthentication(Constants.PRIMARY_AUTHENTICATION_TYPE)]
    [SecurityHeaders]
    [NoCache]
    [PreventUnsupportedRequestMediaTypes(allowFormUrlEncoded: true)]
    internal class AuthorizeEndpointController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly IViewService _viewService;
        private readonly AuthorizeRequestValidator _validator;
        private readonly AuthorizeResponseGenerator _responseGenerator;
        private readonly AuthorizeInteractionResponseGenerator _interactionGenerator;
        private readonly IdentityServerOptions _options;
        private readonly ILocalizationService _localizationService;
        private readonly IEventService _events;
        private readonly AntiForgeryToken _antiForgeryToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizeEndpointController" /> class.
        /// </summary>
        /// <param name="viewService">The view service.</param>
        /// <param name="validator">The validator.</param>
        /// <param name="responseGenerator">The response generator.</param>
        /// <param name="interactionGenerator">The interaction generator.</param>
        /// <param name="options">The options.</param>
        /// <param name="localizationService">The localization service.</param>
        /// <param name="events">The event service.</param>
        /// <param name="antiForgeryToken">The anti forgery token.</param>
        public AuthorizeEndpointController(
            IViewService viewService,
            AuthorizeRequestValidator validator,
            AuthorizeResponseGenerator responseGenerator,
            AuthorizeInteractionResponseGenerator interactionGenerator,
            IdentityServerOptions options,
            ILocalizationService localizationService,
            IEventService events,
            AntiForgeryToken antiForgeryToken)
        {
            _viewService = viewService;
            _options = options;

            _responseGenerator = responseGenerator;
            _interactionGenerator = interactionGenerator;
            _validator = validator;
            _localizationService = localizationService;
            _events = events;
            _antiForgeryToken = antiForgeryToken;
        }

        /// <summary>
        /// GET
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        [Route(Constants.RoutePaths.Oidc.AUTHORIZE, Name = Constants.RouteNames.Oidc.AUTHORIZE)]
        public async Task<IHttpActionResult> Get(HttpRequestMessage request)
        {
            Logger.Info("Start authorize request");

            if (!_options.Endpoints.EnableAuthorizeEndpoint)
            {
                const string error = "Endpoint is disabled. Aborting";
                Logger.Warn(error);
                RaiseFailureEvent(error);

                return NotFound();
            }

            var response = await ProcessRequestAsync(request.RequestUri.ParseQueryString());

            Logger.Info("End authorize request");
            return response;
        }

        private async Task<IHttpActionResult> ProcessRequestAsync(NameValueCollection parameters, UserConsent consent = null)
        {
            // validate request
            var result = await _validator.ValidateAsync(parameters, User as ClaimsPrincipal);
            
            if (result.IsError)
            {
                return AuthorizeError(
                    result.ErrorType,
                    result.Error,
                    result.ValidatedRequest);
            }

            var request = result.ValidatedRequest;
            var loginInteraction = await _interactionGenerator.ProcessLoginAsync(request, User as ClaimsPrincipal);

            if (loginInteraction.IsError)
            {
                return AuthorizeError(
                    loginInteraction.Error.ErrorType,
                    loginInteraction.Error.Error,
                    request);
            }
            if (loginInteraction.IsLogin)
            {
                return RedirectToLogin(loginInteraction.SignInMessage, request.Raw);
            }

            // user must be authenticated at this point
            if (!User.Identity.IsAuthenticated)
            {
                throw new InvalidOperationException("User is not authenticated");
            }

            request.Subject = User as ClaimsPrincipal;

            // now that client configuration is loaded, we can do further validation
            loginInteraction = await _interactionGenerator.ProcessClientLoginAsync(request);
            if (loginInteraction.IsLogin)
            {
                return RedirectToLogin(loginInteraction.SignInMessage, request.Raw);
            }

            var consentInteraction = await _interactionGenerator.ProcessConsentAsync(request, consent);

            if (consentInteraction.IsError)
            {
                return AuthorizeError(
                    consentInteraction.Error.ErrorType,
                    consentInteraction.Error.Error,
                    request);
            }

            if (consentInteraction.IsConsent)
            {
                Logger.Info("Showing consent screen");
                return CreateConsentResult(request, consent, request.Raw, consentInteraction.ConsentError);
            }

            return await CreateAuthorizeResponseAsync(request);
        }

        [Route(Constants.RoutePaths.Oidc.CONSENT, Name = Constants.RouteNames.Oidc.CONSENT)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IHttpActionResult> PostConsent(UserConsent model)
        {
            Logger.Info("Resuming from consent, restarting validation");
            return ProcessRequestAsync(Request.RequestUri.ParseQueryString(), model ?? new UserConsent());
        }

        [Route(Constants.RoutePaths.Oidc.SWITCH_USER, Name = Constants.RouteNames.Oidc.SWITCH_USER)]
        [HttpGet]
        public async Task<IHttpActionResult> LoginAsDifferentUser()
        {
            var parameters = Request.RequestUri.ParseQueryString();
            parameters[Constants.AuthorizeRequest.PROMPT] = Constants.PromptModes.LOGIN;
            return await ProcessRequestAsync(parameters);
        }

        private async Task<IHttpActionResult> CreateAuthorizeResponseAsync(ValidatedAuthorizeRequest request)
        {
            var response = await _responseGenerator.CreateResponseAsync(request);

            if (request.ResponseMode == Constants.ResponseModes.QUERY ||
                request.ResponseMode == Constants.ResponseModes.FRAGMENT)
            {
                RaiseSuccessEvent();
                return new AuthorizeRedirectResult(response, _options);
            }

            if (request.ResponseMode == Constants.ResponseModes.FORM_POST)
            {
                RaiseSuccessEvent();
                return new AuthorizeFormPostResult(response, Request);
            }

            Logger.Error("Unsupported response mode. Aborting.");
            throw new InvalidOperationException("Unsupported response mode");
        }

        private IHttpActionResult CreateConsentResult(
            ValidatedAuthorizeRequest validatedRequest,
            UserConsent consent,
            NameValueCollection requestParameters,
            string errorMessage)
        {
            string loginWithDifferentAccountUrl = null;
            if (validatedRequest.HasIdpAcrValue() == false)
            {
                loginWithDifferentAccountUrl = Url.Route(Constants.RouteNames.Oidc.SWITCH_USER, null)
                    .AddQueryString(requestParameters.ToQueryString());
            }
            
            var env = Request.GetOwinEnvironment();
            var consentModel = new ConsentViewModel
            {
                RequestId = env.GetRequestId(),
                SiteName = _options.SiteName,
                SiteUrl = env.GetIdentityServerBaseUrl(),
                ErrorMessage = errorMessage,
                CurrentUser = env.GetCurrentUserDisplayName(),
                LogoutUrl = env.GetIdentityServerLogoutUrl(),
                ClientName = validatedRequest.Client.ClientName,
                ClientUrl = validatedRequest.Client.ClientUri,
                ClientLogoUrl = validatedRequest.Client.LogoUri,
                IdentityScopes = validatedRequest.GetIdentityScopes(_localizationService),
                ResourceScopes = validatedRequest.GetResourceScopes(_localizationService),
                AllowRememberConsent = validatedRequest.Client.AllowRememberConsent,
                RememberConsent = consent == null || consent.RememberConsent,
                LoginWithDifferentAccountUrl = loginWithDifferentAccountUrl,
                ConsentUrl = Url.Route(Constants.RouteNames.Oidc.CONSENT, null).AddQueryString(requestParameters.ToQueryString()),
                AntiForgery = _antiForgeryToken.GetAntiForgeryToken()
            };

            return new ConsentActionResult(_viewService, consentModel);
        }

        IHttpActionResult RedirectToLogin(SignInMessage message, NameValueCollection parameters)
        {
            message = message ?? new SignInMessage();

            var path = Url.Route(Constants.RouteNames.Oidc.AUTHORIZE, null).AddQueryString(parameters.ToQueryString());
            var host = new Uri(Request.GetOwinEnvironment().GetIdentityServerHost());
            var url = new Uri(host, path);
            message.ReturnUrl = url.AbsoluteUri;

            return new LoginResult(Request.GetOwinContext().Environment, message);
        }

        IHttpActionResult AuthorizeError(ErrorTypes errorType, string error, ValidatedAuthorizeRequest request)
        {
            RaiseFailureEvent(error);

            // show error message to user
            if (errorType == ErrorTypes.USER)
            {
                var env = Request.GetOwinEnvironment();
                var errorModel = new ErrorViewModel
                {
                    RequestId = env.GetRequestId(),
                    SiteName = _options.SiteName,
                    SiteUrl = env.GetIdentityServerBaseUrl(),
                    CurrentUser = env.GetCurrentUserDisplayName(),
                    LogoutUrl = env.GetIdentityServerLogoutUrl(),
                    ErrorMessage = LookupErrorMessage(error)
                };

                var errorResult = new ErrorActionResult(_viewService, errorModel);
                return errorResult;
            }

            // return error to client
            var response = new AuthorizeResponse
            {
                Request = request,

                IsError = true,
                Error = error,
                State = request.State,
                RedirectUri = request.RedirectUri
            };

            if (request.ResponseMode == Constants.ResponseModes.FORM_POST)
            {
                return new AuthorizeFormPostResult(response, Request);
            }
            return new AuthorizeRedirectResult(response, _options);
        }

        private void RaiseSuccessEvent()
        {
            _events.RaiseSuccessfulEndpointEvent(EventConstants.EndpointNames.AUTHORIZE);
        }

        private void RaiseFailureEvent(string error)
        {
            _events.RaiseFailureEndpointEvent(EventConstants.EndpointNames.AUTHORIZE, error);
        }

        private string LookupErrorMessage(string error)
        {
            var msg = _localizationService.GetMessage(error);
            if (msg.IsMissing())
            {
                msg = error;
            }
            return msg;
        }
    }
}
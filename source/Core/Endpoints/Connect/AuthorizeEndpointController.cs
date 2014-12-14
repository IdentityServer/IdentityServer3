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
using System.Collections.Specialized;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Configuration.Hosting;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.ResponseHandling;
using Thinktecture.IdentityServer.Core.Results;
using Thinktecture.IdentityServer.Core.Services;
using Thinktecture.IdentityServer.Core.Validation;
using Thinktecture.IdentityServer.Core.ViewModels;

namespace Thinktecture.IdentityServer.Core.Endpoints
{
    /// <summary>
    /// OAuth2/OpenID Connect authorize endpoint
    /// </summary>
    [ErrorPageFilter]
    [HostAuthentication(Constants.PrimaryAuthenticationType)]
    [SecurityHeaders]
    [NoCache]
    [PreventUnsupportedRequestMediaTypes(allowFormUrlEncoded: true)]
    public class AuthorizeEndpointController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly IViewService _viewService;
        private readonly AuthorizeRequestValidator _validator;
        private readonly AuthorizeResponseGenerator _responseGenerator;
        private readonly AuthorizeInteractionResponseGenerator _interactionGenerator;
        private readonly IdentityServerOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizeEndpointController"/> class.
        /// </summary>
        /// <param name="viewService">The view service.</param>
        /// <param name="validator">The validator.</param>
        /// <param name="responseGenerator">The response generator.</param>
        /// <param name="interactionGenerator">The interaction generator.</param>
        /// <param name="options">The options.</param>
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

        /// <summary>
        /// GET
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        [Route(Constants.RoutePaths.Oidc.Authorize, Name = Constants.RouteNames.Oidc.Authorize)]
        public async Task<IHttpActionResult> Get(HttpRequestMessage request)
        {
            Logger.Info("Start authorize request");

            return await ProcessRequestAsync(request.RequestUri.ParseQueryString());
        }

        private async Task<IHttpActionResult> ProcessRequestAsync(NameValueCollection parameters, UserConsent consent = null)
        {
            if (!_options.Endpoints.AuthorizeEndpoint.IsEnabled)
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
                    request);
            }

            var loginInteraction = await _interactionGenerator.ProcessLoginAsync(request, User as ClaimsPrincipal);

            if (loginInteraction.IsError)
            {
                return this.AuthorizeError(
                    loginInteraction.Error.ErrorType,
                    loginInteraction.Error.Error,
                    request);
            }
            if (loginInteraction.IsLogin)
            {
                return this.RedirectToLogin(loginInteraction.SignInMessage, request.Raw);
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
                    request);
            }

            // now that client configuration is loaded, we can do further validation
            loginInteraction = await _interactionGenerator.ProcessClientLoginAsync(request);
            if (loginInteraction.IsLogin)
            {
                return this.RedirectToLogin(loginInteraction.SignInMessage, request.Raw);
            }

            var consentInteraction = await _interactionGenerator.ProcessConsentAsync(request, consent);

            if (consentInteraction.IsError)
            {
                return this.AuthorizeError(
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

        [Route(Constants.RoutePaths.Oidc.Consent, Name = Constants.RouteNames.Oidc.Consent)]
        [HttpPost]
        [ValidateAntiForgeryToken]
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
            var response = await _responseGenerator.CreateResponseAsync(request);

            if (request.ResponseMode == Constants.ResponseModes.Query ||
                request.ResponseMode == Constants.ResponseModes.Fragment)
            {
                return new AuthorizeRedirectResult(response);
            }

            if (request.ResponseMode == Constants.ResponseModes.FormPost)
            {
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
            var env = Request.GetOwinEnvironment();
            var consentModel = new ConsentViewModel
            {
                SiteName = _options.SiteName,
                SiteUrl = env.GetIdentityServerBaseUrl(),
                ErrorMessage = errorMessage,
                CurrentUser = User.GetName(),
                ClientName = validatedRequest.Client.ClientName,
                ClientUrl = validatedRequest.Client.ClientUri,
                ClientLogoUrl = validatedRequest.Client.LogoUri != null ? validatedRequest.Client.LogoUri : null,
                IdentityScopes = validatedRequest.GetIdentityScopes(),
                ResourceScopes = validatedRequest.GetResourceScopes(),
                AllowRememberConsent = validatedRequest.Client.AllowRememberConsent,
                RememberConsent = consent != null ? consent.RememberConsent : true,
                LoginWithDifferentAccountUrl = Url.Route(Constants.RouteNames.Oidc.SwitchUser, null).AddQueryString(requestParameters.ToQueryString()),
                LogoutUrl = Url.Route(Constants.RouteNames.Oidc.EndSession, null),
                ConsentUrl = Url.Route(Constants.RouteNames.Oidc.Consent, null).AddQueryString(requestParameters.ToQueryString()),
                AntiForgery = AntiForgeryTokenValidator.GetAntiForgeryHiddenInput(Request.GetOwinEnvironment())
            };

            return new ConsentActionResult(_viewService, consentModel);
        }

        IHttpActionResult RedirectToLogin(SignInMessage message, NameValueCollection parameters)
        {
            message = message ?? new SignInMessage();

            var path = Url.Route(Constants.RouteNames.Oidc.Authorize, null).AddQueryString(parameters.ToQueryString());
            var host = new Uri(Request.GetOwinEnvironment().GetIdentityServerHost()); 
            var url = new Uri(host, path);
            message.ReturnUrl = url.AbsoluteUri;
            
            return new LoginResult(message, Request.GetOwinContext().Environment, _options);
        }

        IHttpActionResult AuthorizeError(ErrorTypes errorType, string error, ValidatedAuthorizeRequest request)
        {
            // show error message to user
            if (errorType == ErrorTypes.User)
            {
                var env = Request.GetOwinEnvironment();
                var username = User.Identity.IsAuthenticated ? User.GetName() : (string)null;

                var errorModel = new ErrorViewModel
                {
                    SiteName = _options.SiteName,
                    SiteUrl = env.GetIdentityServerBaseUrl(),
                    CurrentUser = username,
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

            if (request.ResponseMode == Constants.ResponseModes.FormPost)
            {
                return new AuthorizeFormPostResult(response, Request);
            }
            else
            {
                return new AuthorizeRedirectResult(response);
            }
           
        }

        private string LookupErrorMessage(string error)
        {
            var msg = Resources.Messages.ResourceManager.GetString(error);
            if (msg.IsMissing())
            {
                msg = error;
            }
            return msg;
        }
    }
}
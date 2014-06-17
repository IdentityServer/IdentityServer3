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
using Thinktecture.IdentityServer.Core.Assets;
using Thinktecture.IdentityServer.Core.Authentication;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Connect
{
    [RoutePrefix("connect")]
    [HostAuthentication("idsrv")]
    public class AuthorizeEndpointController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly CoreSettings _settings;

        private readonly AuthorizeRequestValidator _validator;
        private readonly AuthorizeResponseGenerator _responseGenerator;
        private readonly AuthorizeInteractionResponseGenerator _interactionGenerator;
        private readonly InternalConfiguration _internalConfiguration;
        
        public AuthorizeEndpointController(
            AuthorizeRequestValidator validator, 
            AuthorizeResponseGenerator responseGenerator, 
            AuthorizeInteractionResponseGenerator interactionGenerator, 
            CoreSettings settings,
            InternalConfiguration internalConfiguration)
        {
            _settings = settings;
            _internalConfiguration = internalConfiguration;
        
            _responseGenerator = responseGenerator;
            _interactionGenerator = interactionGenerator;
            _validator = validator;
        }

        [Route("authorize", Name="authorize")]
        public async Task<IHttpActionResult> Get(HttpRequestMessage request)
        {
            Logger.Info("Start authorize request");

            return await ProcessRequestAsync(request.RequestUri.ParseQueryString());
        }

        protected async Task<IHttpActionResult> ProcessRequestAsync(NameValueCollection parameters, UserConsent consent = null)
        {   
            if (!_settings.AuthorizeEndpoint.Enabled)
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

            var interaction = _interactionGenerator.ProcessLogin(request, User as ClaimsPrincipal);

            if (interaction.IsError)
            {
                return this.AuthorizeError(interaction.Error);
            }
            if (interaction.IsLogin)
            {
                return this.RedirectToLogin(interaction.SignInMessage, request.Raw, _settings);
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

            interaction = await _interactionGenerator.ProcessConsentAsync(request, consent);
            
            if (interaction.IsError)
            {
                return this.AuthorizeError(interaction.Error);
            }

            if (interaction.IsConsent)
            {
                Logger.Info("Showing consent screen");
                return CreateConsentResult(request, request.Raw, interaction.ConsentError);
            }

            return await CreateAuthorizeResponseAsync(request);
        }

        [Route("consent")]
        [HttpPost]
        public Task<IHttpActionResult> PostConsent(UserConsent model)
        {
            Logger.Info("Resuming from consent, restarting validation");
            return ProcessRequestAsync(Request.RequestUri.ParseQueryString(), model ?? new UserConsent());
        }

        [Route("switch", Name="switch")]
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
            NameValueCollection requestParameters, 
            string errorMessage)
        {
            var consentModel = new ConsentModel(validatedRequest, requestParameters);
            string name = User.GetName();
            
            return new EmbeddedHtmlResult(
                Request, 
                new LayoutModel
                {
                    Server = _settings.SiteName,
                    ErrorMessage = errorMessage,
                    Page = "consent",
                    Username = name,
                    SwitchUrl = Url.Route("switch", null) + "?" + requestParameters.ToQueryString(),
                    PageModel = consentModel
                });
        }

        IHttpActionResult RedirectToLogin(SignInMessage message, NameValueCollection parameters, CoreSettings settings)
        {
            message = message ?? new SignInMessage();

            var path = Url.Route("authorize", null) + "?" + parameters.ToQueryString();
            var url = new Uri(Request.RequestUri, path);
            message.ReturnUrl = url.AbsoluteUri;
            
            return new LoginResult(message, this.Request, settings, _internalConfiguration);
        }
    }
}
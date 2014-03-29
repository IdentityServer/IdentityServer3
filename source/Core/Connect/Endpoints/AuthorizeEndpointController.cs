/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Assets;
using Thinktecture.IdentityServer.Core.Authentication;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect
{
    [RoutePrefix("connect")]
    [HostAuthentication("idsrv")]
    public class AuthorizeEndpointController : ApiController
    {
        private ILogger _logger;

        private AuthorizeRequestValidator _validator;
        private AuthorizeResponseGenerator _responseGenerator;
        private AuthorizeInteractionResponseGenerator _interactionGenerator;
        IConsentService _consentService;
        private ICoreSettings _settings;

        public AuthorizeEndpointController(
            ILogger logger, 
            AuthorizeRequestValidator validator, 
            AuthorizeResponseGenerator responseGenerator, 
            AuthorizeInteractionResponseGenerator interactionGenerator, 
            IConsentService consentService,
            ICoreSettings settings)
        {
            _logger = logger;
            _settings = settings;

            _responseGenerator = responseGenerator;
            _interactionGenerator = interactionGenerator;
            _consentService = consentService;

            _validator = validator;
        }

        [Route("authorize", Name="authorize")]
        public async Task<IHttpActionResult> Get(HttpRequestMessage request)
        {
            return await ProcessRequestAsync(request.RequestUri.ParseQueryString());
        }

        protected virtual async Task<IHttpActionResult> ProcessRequestAsync(NameValueCollection parameters, UserConsent consent = null)
        {
            _logger.Start("OIDC authorize endpoint.");
            
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

            interaction = await _interactionGenerator.ProcessConsentAsync(request, User as ClaimsPrincipal, consent);
            
            if (interaction.IsError)
            {
                return this.AuthorizeError(interaction.Error);
            }

            if (interaction.IsConsent)
            {
                return CreateConsentResult(request, request.Raw, interaction.ConsentError);
            }

            return await CreateAuthorizeResponseAsync(request);
        }

        [Route("consent")]
        [HttpPost]
        public Task<IHttpActionResult> PostConsent(UserConsent model)
        {
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

            _logger.Error("Unsupported flow. Aborting.");
            throw new InvalidOperationException("Unsupported flow");
        }

        private async Task<IHttpActionResult> CreateCodeFlowAuthorizeResponseAsync(ValidatedAuthorizeRequest request)
        {
            var response = await _responseGenerator.CreateCodeFlowResponseAsync(request, User as ClaimsPrincipal);
            return this.AuthorizeCodeResponse(response);
        }

        private async Task<IHttpActionResult> CreateImplicitFlowAuthorizeResponseAsync(ValidatedAuthorizeRequest request)
        {
            var response = await _responseGenerator.CreateImplicitFlowResponseAsync(request, User as ClaimsPrincipal);

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
            var requestedScopes = validatedRequest.ValidatedScopes.RequestedScopes;
            var consentedScopeNames = validatedRequest.ValidatedScopes.GrantedScopes.Select(x => x.Name);

            var idScopes =
                from s in requestedScopes
                where s.IsOpenIdScope
                select new
                {
                    selected = consentedScopeNames.Contains(s.Name),
                    s.Name,
                    s.DisplayName,
                    s.Description,
                    s.Emphasize,
                    s.Required
                };
            var appScopes =
                from s in requestedScopes
                where !s.IsOpenIdScope
                select new
                {
                    selected = consentedScopeNames.Contains(s.Name),
                    s.Name,
                    s.DisplayName,
                    s.Description,
                    s.Emphasize,
                    s.Required
                };

            string name = User.Identity.IsAuthenticated ? User.GetName() : null;
            return new EmbeddedHtmlResult(
                Request, 
                new LayoutModel
                {
                    Title = validatedRequest.Client.ClientName,
                    ErrorMessage = errorMessage,
                    Page = "consent",
                    Name = name,
                    SwitchUrl = Url.Route("switch", null) + "?" + requestParameters.ToQueryString(),
                    PageModel = new
                    {
                        postUrl = "consent?" + requestParameters.ToQueryString(),
                        client = validatedRequest.Client.ClientName,
                        clientUrl = validatedRequest.Client.ClientUri,
                        clientLogo = validatedRequest.Client.LogoUri,
                        identityScopes = idScopes.ToArray(),
                        appScopes = appScopes.ToArray(),
                        allowRememberConsent = validatedRequest.Client.AllowRememberConsent
                    }
                });
        }

        IHttpActionResult RedirectToLogin(SignInMessage message, NameValueCollection parameters, ICoreSettings settings)
        {
            message = message ?? new SignInMessage();

            var path = Url.Route("authorize", null) + "?" + parameters.ToQueryString();
            var url = new Uri(Request.RequestUri, path);
            message.ReturnUrl = url.AbsoluteUri;
            
            return new LoginResult(message, this.Request, settings);
        }
    }
}
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

        [Route("authorize")]
        public async Task<IHttpActionResult> Get(HttpRequestMessage request)
        {
            return await ProcessRequest(request.RequestUri.ParseQueryString());
        }

        protected virtual async Task<IHttpActionResult> ProcessRequest(NameValueCollection parameters, UserConsent consent = null)
        {
            _logger.Start("OIDC authorize endpoint.");
            
            var signin = new SignInMessage();
            
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
                return this.Login(interaction.SignInMessage, _settings);
            }

            ///////////////////////////////////////////////////////////////
            // validate client
            //////////////////////////////////////////////////////////////
            result = _validator.ValidateClient();

            if (result.IsError)
            {
                return this.AuthorizeError(
                    result.ErrorType,
                    result.Error,
                    request.ResponseMode,
                    request.RedirectUri,
                    request.State);
            }

            interaction = _interactionGenerator.ProcessConsent(request, User as ClaimsPrincipal, consent);
            
            if (interaction.IsError)
            {
                return this.AuthorizeError(interaction.Error);
            }

            if (interaction.IsConsent)
            {
                return CreateConsentResult(request, parameters, interaction.ConsentError);
            }

            return CreateAuthorizeResponse(request);
        }

        [Route("consent")]
        [HttpPost]
        public Task<IHttpActionResult> PostConsent(UserConsent model)
        {
            return ProcessRequest(Request.RequestUri.ParseQueryString(), model ?? new UserConsent());
        }

        private IHttpActionResult CreateAuthorizeResponse(ValidatedAuthorizeRequest request)
        {
            if (request.Flow == Flows.Implicit)
            {
                return CreateImplicitFlowAuthorizeResponse(request);
            }

            if (request.Flow == Flows.Code)
            {
                return CreateCodeFlowAuthorizeResponse(request);
            }

            _logger.Error("Unsupported flow. Aborting.");
            throw new InvalidOperationException("Unsupported flow");
        }

        private IHttpActionResult CreateCodeFlowAuthorizeResponse(ValidatedAuthorizeRequest request)
        {
            var response = _responseGenerator.CreateCodeFlowResponse(request, User as ClaimsPrincipal);
            return this.AuthorizeCodeResponse(response);
        }

        private IHttpActionResult CreateImplicitFlowAuthorizeResponse(ValidatedAuthorizeRequest request)
        {
            var response = _responseGenerator.CreateImplicitFlowResponse(request, User as ClaimsPrincipal);

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
            
            return new EmbeddedHtmlResult(
                Request, 
                new LayoutModel
                {
                    Title = validatedRequest.Client.ClientName,
                    ErrorMessage = errorMessage,
                    Page = "consent",
                    Username = User.Identity.Name,
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
    }
}
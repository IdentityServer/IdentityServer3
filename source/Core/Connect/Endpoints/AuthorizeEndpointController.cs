using System;
using System.Collections.Generic;
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
        private ICoreSettings _settings;

        public AuthorizeEndpointController(ILogger logger, AuthorizeRequestValidator validator, AuthorizeResponseGenerator responseGenerator, AuthorizeInteractionResponseGenerator interactionGenerator, ICoreSettings settings)
        {
            _logger = logger;
            _settings = settings;

            _responseGenerator = responseGenerator;
            _interactionGenerator = interactionGenerator;

            _validator = validator;
        }

        [Route("authorize")]
        public async Task<IHttpActionResult> Get(HttpRequestMessage request)
        {
            return await ProcessRequest(request.RequestUri.ParseQueryString());
        }

        protected virtual async Task<IHttpActionResult> ProcessRequest(NameValueCollection parameters, ConsentInputModel consent = null)
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

            interaction = _interactionGenerator.ProcessConsent(request, User as ClaimsPrincipal);
            if (interaction.IsConsent)
            {
                var requestedScopes =
                        from s in _settings.GetScopes()
                        where request.Scopes.Contains(s.Name)
                        select s;

                string errorMessage = null;
                IEnumerable<string> consentedScopes = null;
                if (consent != null)
                {
                    if (consent.Button != "yes")
                    {
                        return this.AuthorizeError(ErrorTypes.Client, Constants.AuthorizeErrors.AccessDenied, request.ResponseMode, request.RedirectUri, request.State);
                    }

                    if (consent.Scopes != null)
                    {
                        consentedScopes = request.Scopes.Intersect(consent.Scopes);
                    }
                    else
                    {
                        consentedScopes = Enumerable.Empty<string>();
                    }

                    var requiredScopes = requestedScopes.Where(x => x.Required).Select(x=>x.Name);
                    consentedScopes = consentedScopes.Union(requiredScopes).Distinct();
                    
                    if (!consentedScopes.Any())
                    {
                        errorMessage = "Must select at least one permission";
                    }
                }

                if (consent == null || errorMessage != null)
                {
                    var idScopes =
                        from s in requestedScopes
                        where s.IsOpenIdScope
                        let claims = (from c in s.Claims ?? Enumerable.Empty<ScopeClaim>() select c.Description)
                        select new
                        {
                            selected = (consentedScopes != null ? consentedScopes.Contains(s.Name) : true),
                            s.Name,
                            s.Description,
                            s.Emphasize,
                            s.Required,
                            claims
                        };
                    var appScopes =
                        from s in requestedScopes
                        where !s.IsOpenIdScope
                        let claims = (from c in s.Claims ?? Enumerable.Empty<ScopeClaim>() select c.Description)
                        select new
                        {
                            selected = (consentedScopes != null ? consentedScopes.Contains(s.Name) : true),
                            s.Name,
                            s.Description,
                            s.Emphasize,
                            s.Required,
                            claims
                        };


                    return new EmbeddedHtmlResult(Request, new LayoutModel
                    {
                        Title = request.Client.ClientName,
                        ErrorMessage = errorMessage,
                        Page = "consent",
                        PageModel = new
                        {
                            postUrl = "consent?" + parameters.ToQueryString(),
                            client = request.Client.ClientName,
                            clientUrl = request.Client.ClientUri,
                            clientLogo = request.Client.LogoUri,
                            identityScopes = idScopes.ToArray(),
                            appScopes = appScopes.ToArray(),
                        }
                    });
                }

                request.Scopes = consentedScopes.ToList();
            }

            return CreateAuthorizeResponse(request);
        }

        [Route("consent")]
        [HttpPost]
        public Task<IHttpActionResult> PostConsent(ConsentInputModel model)
        {
            return ProcessRequest(Request.RequestUri.ParseQueryString(), model);
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
    }
}
using System;
using System.Collections.Specialized;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Authentication;
using Thinktecture.IdentityServer.Core.Protocols.Connect.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Protocols.Connect
{
    [RoutePrefix("connect/authorize")]
    [HostAuthentication("idsrv")]
    public class AuthorizeEndpointController : ApiController
    {
        private ILogger _logger;
        private OidcAuthorizeResponseGenerator _responseGenerator;
        private AuthorizeRequestValidator _validator;

        public AuthorizeEndpointController(ILogger logger, AuthorizeRequestValidator validator, OidcAuthorizeResponseGenerator responseGenerator)
        {
            _logger = logger;

            _responseGenerator = responseGenerator;
            _validator = validator;
        }

        [Route]
        public async Task<IHttpActionResult> Get(HttpRequestMessage request)
        {
            return await ProcessRequest(request.RequestUri.ParseQueryString());
        }

        [Route]
        public async Task<IHttpActionResult> Post(HttpRequestMessage request)
        {
            return await ProcessRequest(await request.Content.ReadAsFormDataAsync());
        }

        protected virtual async Task<IHttpActionResult> ProcessRequest(NameValueCollection parameters)
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

            // pass through display mode to signin service
            if (request.DisplayMode.IsPresent())
            {
                signin.DisplayMode = request.DisplayMode;
            }

            // pass through ui locales to signin service
            if (request.UiLocales.IsPresent())
            {
                signin.UILocales = request.UiLocales;
            }

            // unauthenticated user
            if (!User.Identity.IsAuthenticated)
            {
                // prompt=none means user must be signed in already
                if (request.PromptMode == Constants.PromptModes.None)
                {
                    return this.AuthorizeError(
                        ErrorTypes.Client,
                        Constants.AuthorizeErrors.InteractionRequired,
                        request.ResponseMode,
                        request.RedirectUri,
                        request.State);
                }

                return this.Login(signin);
            }

            // check authentication freshness
            if (request.MaxAge.HasValue)
            {
                var authTime = User.GetAuthenticationTime();
                if (DateTime.UtcNow > authTime.AddSeconds(request.MaxAge.Value))
                {
                    return this.Login(signin);
                }
            }

            // todo: prompt=login handling

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

            // todo: consent handling
            //if (request.PromptMode == Constants.PromptModes.Consent ||
            //    _services.Consent.RequiresConsent(request.Client, request.Scopes))
            //{
            //    // show consent page
            //    throw new NotImplementedException();
            //}

            return CreateAuthorizeResponse(request);
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
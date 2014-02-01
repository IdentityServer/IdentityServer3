using System.Collections.Specialized;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect
{
    [BasicAuthenticationFilter]
    [RoutePrefix("connect/token")]
    public class TokenEndpointController : ApiController
    {
        private ILogger _logger;
       
        private TokenRequestValidator _validator;
        private TokenResponseGenerator _generator;

        public TokenEndpointController(TokenRequestValidator validator, TokenResponseGenerator generator, ILogger logger)
        {
            _validator = validator;
            _generator = generator;

            _logger = logger;
        }

        [Route]
        public async Task<IHttpActionResult> Post(HttpRequestMessage request)
        {
            return await Process(await request.Content.ReadAsFormDataAsync());
        }

        private async Task<IHttpActionResult> Process(NameValueCollection parameters)
        {
            _logger.Start("OIDC token endpoint.");

            var result = _validator.ValidateRequest(parameters, User as ClaimsPrincipal);

            if (result.IsError)
            {
                return this.TokenErrorResponse(result.Error);
            }

            var response = _generator.Process(_validator.ValidatedRequest, User as ClaimsPrincipal);
            return this.TokenResponse(response);
        }
    }
}

using System.Collections.Specialized;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Protocols.Connect
{
    [BasicAuthenticationFilter]
    [RoutePrefix("connect/token")]
    public class OidcTokenEndpointController : ApiController
    {
        private ILogger _logger;
       
        private OidcTokenRequestValidator _validator;
        private OidcTokenResponseGenerator _generator;

        public OidcTokenEndpointController(OidcTokenRequestValidator validator, OidcTokenResponseGenerator generator, ILogger logger)
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

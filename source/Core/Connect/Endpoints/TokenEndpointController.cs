using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect
{
    //[ClientCredentialsFilter]
    [RoutePrefix("connect/token")]
    public class TokenEndpointController : ApiController
    {
        private ILogger _logger;
        private TokenResponseGenerator _generator;

        private TokenRequestValidator _requestValidator;
        private ClientValidator _clientValidator;
        
        public TokenEndpointController(TokenRequestValidator requestValidator, ClientValidator clientValidator, TokenResponseGenerator generator, ILogger logger)
        {
            _requestValidator = requestValidator;
            _clientValidator = clientValidator;

            _generator = generator;
            _logger = logger;
        }

        [Route]
        public async Task<IHttpActionResult> Post()
        {
            return await Process(await Request.Content.ReadAsFormDataAsync());
        }

        private Task<IHttpActionResult> Process(NameValueCollection parameters)
        {
            _logger.Start("OIDC token endpoint.");

            // validate client credentials on the wire
            var credential = _clientValidator.ValidateRequest(Request.Headers.Authorization, parameters);

            if (credential.IsMalformed || !credential.IsPresent)
            {
                _logger.Error("No or malformed client credential found.");
                return Task.FromResult(this.TokenErrorResponse(Constants.TokenErrors.InvalidClient));
            }

            // validate client against configuration store
            var client = _clientValidator.ValidateClient(credential);
            if (client == null)
            {
                _logger.Error("Invalid client credentials. Aborting.");
                return Task.FromResult(this.TokenErrorResponse(Constants.TokenErrors.InvalidClient));
            }

            // validate the token request
            var result = _requestValidator.ValidateRequest(parameters, client);

            if (result.IsError)
            {
                return Task.FromResult(this.TokenErrorResponse(result.Error));
            }

            // return response
            var response = _generator.Process(_requestValidator.ValidatedRequest);
            return Task.FromResult(this.TokenResponse(response));
        }
    }
}

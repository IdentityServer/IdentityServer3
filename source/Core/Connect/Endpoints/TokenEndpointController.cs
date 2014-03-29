using System.Collections.Specialized;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect
{
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
            return await ProcessAsync(await Request.Content.ReadAsFormDataAsync());
        }

        private async Task<IHttpActionResult> ProcessAsync(NameValueCollection parameters)
        {
            _logger.Start("OIDC token endpoint.");

            // validate client credentials and client
            var client = await ValidateClientAsync(parameters, Request.Headers.Authorization);
            if (client == null)
            {
                return this.TokenErrorResponse(Constants.TokenErrors.InvalidClient);
            }

            // validate the token request
            var result = await _requestValidator.ValidateRequestAsync(parameters, client);

            if (result.IsError)
            {
                return this.TokenErrorResponse(result.Error);
            }

            // return response
            var response = await _generator.ProcessAsync(_requestValidator.ValidatedRequest);
            return this.TokenResponse(response);
        }

        private async Task<Client> ValidateClientAsync(NameValueCollection parameters, AuthenticationHeaderValue header)
        {
            // validate client credentials on the wire
            var credential = _clientValidator.ValidateRequest(header, parameters);

            if (credential.IsMalformed || !credential.IsPresent)
            {
                _logger.Error("No or malformed client credential found.");
                return null;
            }

            // validate client against configuration store
            var client = await _clientValidator.ValidateClientAsync(credential);
            if (client == null)
            {
                _logger.Error("Invalid client credentials. Aborting.");
                return null;
            }

            return client;
        }
    }
}
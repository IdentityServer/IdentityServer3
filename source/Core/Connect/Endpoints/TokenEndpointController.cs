/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect
{
    [RoutePrefix("connect/token")]
    public class TokenEndpointController : ApiController
    {
        private readonly ILogger _logger;

        private readonly TokenResponseGenerator _generator;
        private readonly TokenRequestValidator _requestValidator;
        private readonly ClientValidator _clientValidator;
        
        public TokenEndpointController(TokenRequestValidator requestValidator, ClientValidator clientValidator, TokenResponseGenerator generator, ILogger logger)
        {
            _logger = logger;

            _requestValidator = requestValidator;
            _clientValidator = clientValidator;
            _generator = generator;
        }

        [Route]
        public async Task<IHttpActionResult> Post()
        {
            return await ProcessAsync(await Request.Content.ReadAsFormDataAsync());
        }

        public async Task<IHttpActionResult> ProcessAsync(NameValueCollection parameters)
        {
            _logger.Start("OIDC token endpoint.");

            // validate client credentials and client
            var client = await _clientValidator.ValidateClientAsync(parameters, Request.Headers.Authorization);
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
    }
}
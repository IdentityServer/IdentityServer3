/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Logging;

namespace Thinktecture.IdentityServer.Core.Connect
{
    [RoutePrefix("connect/token")]
    public class TokenEndpointController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly TokenResponseGenerator _generator;
        private readonly TokenRequestValidator _requestValidator;
        private readonly ClientValidator _clientValidator;
        private readonly CoreSettings _settings;
        
        public TokenEndpointController(CoreSettings settings, TokenRequestValidator requestValidator, ClientValidator clientValidator, TokenResponseGenerator generator)
        {
            _requestValidator = requestValidator;
            _clientValidator = clientValidator;
            _generator = generator;
            _settings = settings;
        }

        [Route]
        public async Task<IHttpActionResult> Post()
        {
            Logger.Info("Start token request");

            return await ProcessAsync(await Request.Content.ReadAsFormDataAsync());
        }

        public async Task<IHttpActionResult> ProcessAsync(NameValueCollection parameters)
        {
            if (!_settings.TokenEndpoint.Enabled)
            {
                Logger.Warn("Endpoint is disabled. Aborting");
                return NotFound();
            }

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
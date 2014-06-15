/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using Newtonsoft.Json;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Logging;

namespace Thinktecture.IdentityServer.Core.Connect
{
    [RoutePrefix("connect/accessTokenValidation")]
    public class AccessTokenValidationController : ApiController
    {
        private readonly TokenValidator _validator;
        private readonly ILog _logger;

        public AccessTokenValidationController(TokenValidator validator)
        {
            _validator = validator;
            _logger = LogProvider.GetCurrentClassLogger();
        }

        [Route]
        public async Task<IHttpActionResult> Get()
        {
            _logger.Info("Start access token validation request");

            var parameters = Request.RequestUri.ParseQueryString();

            var token = parameters.Get("token");
            if (token.IsMissing())
            {
                _logger.Error("token is missing.");
                return BadRequest("token is missing.");
            }

            var result = await _validator.ValidateAccessTokenAsync(token, parameters.Get("expectedScope"));
            
            if (result.IsError)
            {
                _logger.Info("Returning error: " + result.Error);
                return BadRequest(result.Error);
            }

            var response = result.Claims.Select(c => new { c.Type, c.Value });
            _logger.Debug(JsonConvert.SerializeObject(response, Formatting.Indented));

            _logger.Info("Returning access token claims");
            return Json(response);
        }
    }
}
/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using Newtonsoft.Json;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Logging;

namespace Thinktecture.IdentityServer.Core.Connect
{
    [RoutePrefix("connect/accessTokenValidation")]
    public class AccessTokenValidationController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly TokenValidator _validator;
        private readonly CoreSettings _settings;

        public AccessTokenValidationController(TokenValidator validator, CoreSettings settings)
        {
            _validator = validator;
            _settings = settings;
        }

        [Route]
        public async Task<IHttpActionResult> Get()
        {
            Logger.Info("Start access token validation request");

            if (!_settings.AccessTokenValidationEndpoint.Enabled)
            {
                Logger.Warn("Endpoint is disabled. Aborting");
                return NotFound();
            }

            var parameters = Request.RequestUri.ParseQueryString();

            var token = parameters.Get("token");
            if (token.IsMissing())
            {
                Logger.Error("token is missing.");
                return BadRequest("token is missing.");
            }

            var result = await _validator.ValidateAccessTokenAsync(token, parameters.Get("expectedScope"));
            
            if (result.IsError)
            {
                Logger.Info("Returning error: " + result.Error);
                return BadRequest(result.Error);
            }

            var response = result.Claims.Select(c => new { c.Type, c.Value });
            Logger.Debug(JsonConvert.SerializeObject(response, Formatting.Indented));

            Logger.Info("Returning access token claims");
            return Json(response);
        }
    }
}
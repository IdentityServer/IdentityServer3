/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Thinktecture.IdentityServer.Core.Connect
{
    [RoutePrefix("connect/accessTokenValidation")]
    public class AccessTokenValidationController : ApiController
    {
        private readonly TokenValidator _validator;

        public AccessTokenValidationController(TokenValidator validator)
        {
            _validator = validator;
        }

        [Route]
        public async Task<IHttpActionResult> Get()
        {
            var parameters = Request.RequestUri.ParseQueryString();

            var token = parameters.Get("token");
            if (token.IsMissing())
            {
                return BadRequest("token is missing.");
            }

            var result = await _validator.ValidateAccessTokenAsync(token, parameters.Get("expectedScope"));
            
            if (result.IsError)
            {
                return BadRequest(result.Error);
            }

            return Ok(result.Claims.Select(c => new { c.Type, c.Value }));
        }
    }
}
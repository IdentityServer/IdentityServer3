/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Connect.Results;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect
{
    [RoutePrefix("connect/userinfo")]
    public class UserInfoEndpointController : ApiController
    {
        private readonly UserInfoRequestValidator _validator;
        private readonly UserInfoResponseGenerator _generator;
        private readonly ILogger _logger;
        private readonly TokenValidator _tokenValidator;

        public UserInfoEndpointController(TokenValidator tokenValidator, UserInfoRequestValidator validator, UserInfoResponseGenerator generator, ILogger logger)
        {
            _validator = validator;

            _tokenValidator = tokenValidator;
            _generator = generator;

            _logger = logger;
        }

        [Route]
        public async Task<IHttpActionResult> Get(HttpRequestMessage request)
        {
            _logger.Start("OIDC userinfo endpoint.");

            var authorizationHeader = request.Headers.Authorization;

            if (authorizationHeader == null ||
                !authorizationHeader.Scheme.Equals(Constants.TokenTypes.Bearer) ||
                authorizationHeader.Parameter.IsMissing())
            {
                return Unauthorized();
            }

            var result = await _tokenValidator.ValidateAccessTokenAsync(
                authorizationHeader.Parameter, 
                Constants.StandardScopes.OpenId);

            if (result.IsError)
            {
                return BadRequest(Constants.UserInfoErrors.InvalidToken);
            }

            // pass scopes/claims to profile service
            var payload = await _generator.ProcessAsync(result.Claims);

            return new UserInfoResult(payload);
        }
    }
}
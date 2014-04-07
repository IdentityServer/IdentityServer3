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
using System.Linq;

namespace Thinktecture.IdentityServer.Core.Connect
{
    [RoutePrefix("connect/userinfo")]
    public class UserInfoEndpointController : ApiController
    {
        private readonly UserInfoResponseGenerator _generator;
        private readonly ILogger _logger;
        private readonly TokenValidator _tokenValidator;

        public UserInfoEndpointController(TokenValidator tokenValidator, UserInfoResponseGenerator generator, ILogger logger)
        {
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
                return Error(Constants.ProtectedResourceErrors.InvalidToken);
            }

            var result = await _tokenValidator.ValidateAccessTokenAsync(
                authorizationHeader.Parameter, 
                Constants.StandardScopes.OpenId);

            if (result.IsError)
            {
                return Error(result.Error);
            }

            // pass scopes/claims to profile service
            var subject = result.Claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.Subject).Value;
            var scopes = result.Claims.Where(c => c.Type == Constants.ClaimTypes.Scope).Select(c => c.Value);

            var payload = await _generator.ProcessAsync(subject, scopes);

            return new UserInfoResult(payload);
        }

        IHttpActionResult Error(string error, string description = null)
        {
            return new ProtectedResourceErrorResult(error, description);
        }
    }
}
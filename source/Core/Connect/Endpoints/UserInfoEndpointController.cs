/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Connect.Results;
using Thinktecture.IdentityServer.Core.Logging;

namespace Thinktecture.IdentityServer.Core.Connect
{
    [RoutePrefix("connect/userinfo")]
    public class UserInfoEndpointController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly UserInfoResponseGenerator _generator;
        private readonly TokenValidator _tokenValidator;
        private readonly CoreSettings _settings;

        public UserInfoEndpointController(CoreSettings settings, TokenValidator tokenValidator, UserInfoResponseGenerator generator)
        {
            _tokenValidator = tokenValidator;
            _generator = generator;
            _settings = settings;
        }

        [Route]
        public async Task<IHttpActionResult> Get(HttpRequestMessage request)
        {
            Logger.Info("Start userinfo request");

            if (!_settings.UserInfoEndpoint.Enabled)
            {
                Logger.Warn("Endpoint is disabled. Aborting");
                return NotFound();
            }

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
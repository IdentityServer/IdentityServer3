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
        private UserInfoRequestValidator _validator;
        private UserInfoResponseGenerator _generator;
        private ILogger _logger;

        public UserInfoEndpointController(UserInfoRequestValidator validator, UserInfoResponseGenerator generator, ILogger logger)
        {
            _validator = validator;
            _generator = generator;

            _logger = logger;
        }

        [Route]
        public async Task<IHttpActionResult> Get(HttpRequestMessage request)
        {
            _logger.Start("OIDC userinfo endpoint.");

            var result = await _validator.ValidateRequestAsync(request.Headers.Authorization);

            if (result.IsError)
            {
                throw new Exception();
            }

            // pass scopes/claims to profile service
            var payload = _generator.Process(_validator.ValidatedRequest);

            return new UserInfoResult(payload);
        }
    }
}

using System;
using System.Net.Http;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Connect.Results;

namespace Thinktecture.IdentityServer.Core.Connect
{
    [RoutePrefix("connect/userinfo")]
    public class UserInfoEndpointController : ApiController
    {
        private UserInfoRequestValidator _validator;
        private UserInfoResponseGenerator _generator;

        public UserInfoEndpointController(UserInfoRequestValidator validator, UserInfoResponseGenerator generator)
        {
            _validator = validator;
            _generator = generator;
        }

        [Route]
        public IHttpActionResult Get(HttpRequestMessage request)
        {
            var result = _validator.ValidateRequest(request.Headers.Authorization);

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

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect
{
    [RoutePrefix(".well-known/openid-configuration")]
    public class DiscoveryEndpointController : ApiController
    {
        private ICoreSettings _settings;

        public DiscoveryEndpointController(ICoreSettings settings)
        {
            _settings = settings;
        }

        [Route]
        public Dictionary<string, object> Get()
        {
            var doc = new Dictionary<string, object>();

            doc.Add("issuer", _settings.GetIssuerUri());
            doc.Add("jwks_uri", Request.GetRequestContext().VirtualPathRoot + "/connect/jwks");
            doc.Add("authorization_endpoint", Request.GetRequestContext().VirtualPathRoot + "/connect/authorize");
            doc.Add("token_endpoint", Request.GetRequestContext().VirtualPathRoot + "/connect/token");
            doc.Add("userinfo_endpoint", Request.GetRequestContext().VirtualPathRoot + "/connect/userinfo");
            doc.Add("scopes_supported", _settings.GetScopes().Select(s => s.Name));
            doc.Add("response_types_supported", Constants.SupportedResponseTypes);
            doc.Add("response_modes_supported", Constants.SupportedResponseModes);
            doc.Add("grant_types_supported", Constants.SupportedGrantTypes);
            doc.Add("subject_types_support", new string[] { "pairwise", "public" });
            doc.Add("id_token_signing_alg_values_supported", "RS256");

            return doc;
        }
    }
}
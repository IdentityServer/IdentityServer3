using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Thinktecture.IdentityModel;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect
{
    [RoutePrefix(".well-known")]
    public class DiscoveryEndpointController : ApiController
    {
        private ICoreSettings _settings;

        public DiscoveryEndpointController(ICoreSettings settings)
        {
            _settings = settings;
        }

        [Route("openid-configuration")]
        public Dictionary<string, object> GetConfiguration()
        {
            var doc = new Dictionary<string, object>();

            string host = _settings.GetPublicHost();
            if (host.IsMissing())
            {
                host = "https://" + Request.Headers.Host;
            }

            var baseUrl = new Uri(new Uri(host), Request.GetRequestContext().VirtualPathRoot).AbsoluteUri;
            if (!baseUrl.EndsWith("/")) baseUrl += "/";

            doc.Add("issuer", _settings.GetIssuerUri());
            doc.Add("jwks_uri", baseUrl + ".well-known/jwks");
            doc.Add("authorization_endpoint", baseUrl + "connect/authorize");
            doc.Add("token_endpoint", baseUrl + "connect/token");
            doc.Add("userinfo_endpoint", baseUrl + "connect/userinfo");
            doc.Add("scopes_supported", _settings.GetScopes().Select(s => s.Name));
            doc.Add("response_types_supported", Constants.SupportedResponseTypes);
            doc.Add("response_modes_supported", Constants.SupportedResponseModes);
            doc.Add("grant_types_supported", Constants.SupportedGrantTypes);
            doc.Add("subject_types_support", new string[] { "pairwise", "public" });
            doc.Add("id_token_signing_alg_values_supported", "RS256");

            return doc;
        }

        [Route("jwks")]
        public dynamic GetKeyData()
        {
            var cert = _settings.GetSigningCertificate();
            var cert64 = Convert.ToBase64String(cert.RawData);
            var thumbprint = Base64Url.Encode(cert.GetCertHash());

            var key = new
            {
                kty = "foo",
                use = "sig",
                kid = thumbprint,
                x5c = new string[] { cert64 }
            };

            return new { keys = new[] { key } };
        }
    }
}
using System;
using System.Linq;
using System.Web.Http;
using Thinktecture.IdentityModel;
using Thinktecture.IdentityServer.Core.Extensions;
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
        public dynamic GetConfiguration()
        {
            var baseUrl = Request.GetBaseUrl(_settings.GetPublicHost());

            return new
            {
                issuer = _settings.GetIssuerUri(),
                jwks_uri = baseUrl + ".well-known/jwks",
                authorization_endpoint = baseUrl + "connect/authorize",
                token_endpoint = baseUrl + "connect/token",
                userinfo_endpoint = baseUrl + "connect/userinfo",
                scopes_supported = _settings.GetScopes().Select(s => s.Name),
                response_types_supported = Constants.SupportedResponseTypes,
                response_modes_supported = Constants.SupportedResponseModes,
                grant_types_supported = Constants.SupportedGrantTypes,
                subject_types_support = new string[] { "pairwise", "public" },
                id_token_signing_alg_values_supported = "RS256"
            };
        }

        [Route("jwks")]
        public dynamic GetKeyData()
        {
            var cert = _settings.GetSigningCertificate();
            var cert64 = Convert.ToBase64String(cert.RawData);
            var thumbprint = Base64Url.Encode(cert.GetCertHash());

            var key = new
            {
                kty = "RSA",
                use = "sig",
                kid = thumbprint,
                x5c = new string[] { cert64 }
            };

            return new { keys = new[] { key } };
        }
    }
}
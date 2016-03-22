using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer3.Core.Configuration;
using Microsoft.IdentityModel.Protocols;

namespace IdentityServer3.Core.Services.Default
{
    /// <summary>
    /// Default signing key service based on IdentityServerOptions
    /// </summary>
    public class DefaultSigningKeyService : ISigningKeyService, ICertificateSigningKeyService
    {
        private readonly IdentityServerOptions _options;

        /// <summary>
        /// Initializes the services with identity server options
        /// </summary>
        /// <param name="options"></param>
        public DefaultSigningKeyService(IdentityServerOptions options)
        {
            _options = options;
        }

        /// <summary>
        /// Calculates the key id for a given x509 certificate
        /// </summary>
        /// <param name="certificate"></param>
        /// <returns>kid</returns>
        public Task<string> GetKidAsync(X509Certificate2 certificate)
        {
            return Task.FromResult(Base64Url.Encode(certificate.GetCertHash()));
        }

        /// <summary>
        /// Retrieves all public keys that can be used to validate tokens
        /// </summary>
        /// <returns>x509 certificates</returns>
        public Task<IEnumerable<JsonWebKey>> GetPublicKeysAsync()
        {
            return Task.FromResult(_options.PublicKeysForMetadata.Select(ToJsonWebKey));
        }

        /// <summary>
        /// Retrieves the primary signing key
        /// </summary>
        /// <returns>x509 certificate</returns>
        public Task<JsonWebKey> GetSigningKeyAsync()
        {
            return Task.FromResult(ToJsonWebKey(_options.SigningCertificate));
        }

        public Task<X509Certificate2> GetCertificate(JsonWebKey key)
        {
            if (Base64Url.Encode(_options.SigningCertificate.GetCertHash()).Equals(key.Kid))
            {
                return Task.FromResult(_options.SigningCertificate);
            }

            var cert =
                _options.PublicKeysForMetadata.FirstOrDefault(
                    publicCert => Base64Url.Encode(publicCert.GetCertHash()).Equals(key.Kid));
            return Task.FromResult(cert);
        }

        private JsonWebKey ToJsonWebKey(X509Certificate2 certificate)
        {
            var cert64 = Convert.ToBase64String(certificate.RawData);
            var thumbprint = Base64Url.Encode(certificate.GetCertHash());
            var key = certificate.PublicKey.Key as RSACryptoServiceProvider;
            var parameters = key.ExportParameters(false);
            var exponent = Base64Url.Encode(parameters.Exponent);
            var modulus = Base64Url.Encode(parameters.Modulus);

            var jsonWebKey = new JsonWebKey
            {
                Kty = "RSA",
                Use = "sig",
                Kid = Base64Url.Encode(certificate.GetCertHash()),
                X5t = thumbprint,
                E = exponent,
                N = modulus
            };

            jsonWebKey.X5c.Add(cert64);

            return jsonWebKey;
        }
    }
}
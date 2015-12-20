using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Services
{
    /// <summary>
    /// Service that deals with public and private keys used for token generation and metadata
    /// </summary>
    public interface ISigningKeyService
    {
        /// <summary>
        /// Retrieves the primary signing key
        /// </summary>
        /// <returns>Signing key</returns>
        Task<X509Certificate2> GetSigningKeyAsync();

        /// <summary>
        /// Retrieves all public keys that can be used to validate tokens
        /// </summary>
        /// <returns>Public keys</returns>
        Task<IEnumerable<X509Certificate2>> GetPublicKeysAsync();

        /// <summary>
        /// Calculates the key id for a given x509 certificate
        /// </summary>
        /// <param name="certificate"></param>
        /// <returns></returns>
        Task<string> GetKidAsync(X509Certificate2 certificate);
    }
}
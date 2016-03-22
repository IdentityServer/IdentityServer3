using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;

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
        Task<JsonWebKey> GetSigningKeyAsync();

        /// <summary>
        /// Retrieves all public keys that can be used to validate tokens
        /// </summary>
        /// <returns>Public keys</returns>
        Task<IEnumerable<JsonWebKey>> GetPublicKeysAsync();
    }
}
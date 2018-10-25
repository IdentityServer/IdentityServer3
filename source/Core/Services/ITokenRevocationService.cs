using System.Threading.Tasks;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Validation;

namespace IdentityServer3.Core.Services
{
    /// <summary>
    /// Revocation policy for RFC 7009 (http://tools.ietf.org/html/rfc7009)
    /// </summary>
    public interface ITokenRevocationService
    {
        /// <summary>
        /// Revokes token.
        /// </summary>
        /// <param name="requestResult">The token revocation requestResult</param>
        /// <returns></returns>
        Task<OperationResult> RevokeAsync(TokenRevocationRequestValidationResult requestResult);
    }
}
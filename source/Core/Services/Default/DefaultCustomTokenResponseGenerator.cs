using IdentityServer3.Core.Models;
using IdentityServer3.Core.Validation;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Services.Default
{
    /// <summary>
    /// nop custom token response generator
    /// </summary>
    public class DefaultCustomTokenResponseGenerator : ICustomTokenResponseGenerator
    {
        /// <summary>
        /// Custom response generation
        /// </summary>
        /// <param name="request">The validated request.</param>
        /// <param name="response">The standard token response.</param>
        /// <returns>The custom token response.</returns>
        public Task<TokenResponse> GenerateAsync(ValidatedTokenRequest request, TokenResponse response)
        {
            return Task.FromResult(response);
        }
    }
}
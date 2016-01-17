using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IdentityServer3.Core.Services
{
    /// <summary>
    /// Interface to allow validation of a user's authentication session.
    /// </summary>
    public interface IAuthenticationSessionValidator
    {
        /// <summary>
        /// This method is called whenever an authentication cookie is presented to IdentityServer for the logged in user.
        /// Return true to indicate the authentication cookie should be honored, false otherwise.
        /// </summary>
        /// <param name="subject">The user.</param>
        /// <returns>true if authentication session is valid, false otherwise.</returns>
        Task<bool> IsAuthenticationSessionValidAsync(ClaimsPrincipal subject);
    }
}

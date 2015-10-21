namespace IdentityServer3.Core.Configuration
{
    using System.Threading.Tasks;

    using Microsoft.Owin.Security;

    /// <summary>
    /// Providers the authentication session stores functions
    /// </summary>
    public interface IAuthenticationSessionStoreProvider
    {
        /// <summary>
        /// Provides the remove functionality of session store
        /// </summary>
        /// <param name="key">Session key</param>
        /// <returns>Async task</returns>
        Task RemoveAsync(string key);

        /// <summary>
        /// Provides the renew functionality of session store
        /// </summary>
        /// <param name="key">Session key</param>
        /// <param name="identity">Authentication ticket</param>
        /// <returns>Async task</returns>
        Task RenewAsync(string key, AuthenticationTicket identity);

        /// <summary>
        /// Provides the retrieve functionality of session store
        /// </summary>
        /// <param name="key">Session key</param>
        /// <returns>Async task with authentication ticket result</returns>
        Task<AuthenticationTicket> RetrieveAsync(string key);

        /// <summary>
        /// Provides the store functionality of session store
        /// </summary>
        /// <param name="ticket">Authentication ticket</param>
        /// <returns>Async task with session key</returns>
        Task<string> StoreAsync(AuthenticationTicket ticket);
    }
}
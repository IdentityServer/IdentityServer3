namespace IdentityServer3.Core.Configuration
{
    using System.Security.Claims;
    using System.Threading.Tasks;

    /// <summary>
    /// Providers the authentication session stores functions
    /// </summary>
    public interface IAuthenticationSessionStoreProvider
    {
        /// <summary>
        /// Provides the remove functionality of session store
        /// </summary>
        /// <param name="key">Session key</param>
        /// <returns>async Task</returns>
        Task RemoveAsync(string key);

        /// <summary>
        /// Provides the renew functionality of session store
        /// </summary>
        /// <param name="key">Session key</param>
        /// <param name="identity">Auth identity</param>
        /// <returns>async Task</returns>
        Task RenewAsync(string key, ClaimsIdentity identity);

        /// <summary>
        /// Provides the retrieve functionality of session store
        /// </summary>
        /// <param name="key">Session key</param>
        /// <returns>async Task with identity</returns>
        Task<ClaimsIdentity> RetrieveAsync(string key);

        /// <summary>
        /// Provides the store functionality of session store
        /// </summary>
        /// <param name="ticket">Auth ticket</param>
        /// <returns>async task with session key</returns>
        Task<string> StoreAsync(ClaimsIdentity ticket);
    }
}
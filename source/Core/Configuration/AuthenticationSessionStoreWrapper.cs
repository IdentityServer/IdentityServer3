namespace IdentityServer3.Core.Configuration
{
    using System.Threading.Tasks;

    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.Cookies;

    internal class AuthenticationSessionStoreWrapper : IAuthenticationSessionStore
    {
        private readonly IAuthenticationSessionStoreProvider provider;

        public AuthenticationSessionStoreWrapper(IAuthenticationSessionStoreProvider provider)
        {
            this.provider = provider;
        }

        public Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            return this.provider.StoreAsync(ticket);
        }

        public Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            return this.provider.RenewAsync(key, ticket);
        }

        public Task<AuthenticationTicket> RetrieveAsync(string key)
        {
            return this.provider.RetrieveAsync(key);
        }

        public Task RemoveAsync(string key)
        {
            return this.provider.RemoveAsync(key);
        }
    }
}
namespace IdentityServer3.Core.Configuration
{
    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.Cookies;
    using System.Threading.Tasks;

    internal class AuthenticationSessionStoreWrapper : IAuthenticationSessionStore
    {
        private readonly IAuthenticationSessionStoreProvider provider;

        public AuthenticationSessionStoreWrapper(IAuthenticationSessionStoreProvider provider)
        {
            this.provider = provider;
        }

        public Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            return this.provider.StoreAsync(new AuthenticationTicketModel(ticket));
        }

        public Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            return this.provider.RenewAsync(key, new AuthenticationTicketModel(ticket));
        }

        public async Task<AuthenticationTicket> RetrieveAsync(string key)
        {
            var ticket = await this.provider.RetrieveAsync(key);
            return ticket == null ? null : ticket.ToAuthenticationTicket();
        }

        public Task RemoveAsync(string key)
        {
            return this.provider.RemoveAsync(key);
        }
    }
}
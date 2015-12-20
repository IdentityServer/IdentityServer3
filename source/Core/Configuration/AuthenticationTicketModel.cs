namespace IdentityServer3.Core.Configuration
{
    using Microsoft.Owin.Security;
    using System.Collections.Generic;
    using System.Security.Claims;

    /// <summary>
    /// A model class represending an authentication ticket
    /// </summary>
    public class AuthenticationTicketModel
    {
        /// <summary>
        /// Instantiates an instance of authentication ticket
        /// </summary>
        public AuthenticationTicketModel(ClaimsIdentity identity, IDictionary<string, string> properties)
        {
            this.Identity = identity;
            this.Properties = properties;
        }

        internal AuthenticationTicketModel(AuthenticationTicket ticket)
            : this(ticket.Identity, ticket.Properties.Dictionary)
        {
        }

        /// <summary>
        /// The claims identity of the authentication ticket
        /// </summary>
        public ClaimsIdentity Identity { get; private set; }

        /// <summary>
        /// Authentication ticket properties
        /// </summary>
        public IDictionary<string, string> Properties { get; private set; }

        internal AuthenticationTicket ToAuthenticationTicket()
        {
            return new AuthenticationTicket(this.Identity, new AuthenticationProperties(this.Properties));
        }
    }
}
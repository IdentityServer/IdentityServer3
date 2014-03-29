using System;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Admin.Core;

namespace Thinktecture.IdentityServer.UserAdmin.Api.Configuration
{
    public class IdentityServerUserAdminConfiguration
    {
        public IdentityServerUserAdminConfiguration()
        {
            this.AdminRoleName = "UserManagerAdministrator";
        }

        public Func<IUserManager> UserManagerFactory { get; set; }

        public EmbeddedAuthentication EmbeddedAuthentication { get; set; }
        public ExternalAuthentication ExternalAuthentication { get; set; }

        public string AdminRoleName { get; set; }

        internal void Validate()
        {
            if (String.IsNullOrWhiteSpace(this.AdminRoleName))
            {
                throw new Exception("AdminRoleName is required.");
            }
            if (this.EmbeddedAuthentication == null && this.ExternalAuthentication == null)
            {
                throw new Exception("Neither EmbeddedAuthentication nor ExternalAuthentication was provided. One is required.");
            }
            if (this.EmbeddedAuthentication != null && this.ExternalAuthentication != null)
            {
                throw new Exception("Both EmbeddedAuthentication and EmbeddedAuthentication was provided. Only one is allowed.");
            }
        }
    }
    
    public class EmbeddedAuthentication
    {
        public string EmbeddedAdminUsername { get; set; }
        public string EmbeddedAdminPassword { get; set; }
    }
    
    public class ExternalAuthentication
    {
        public Func<ClaimsPrincipal, Task<ClaimsPrincipal>> ExternalClaimsTransformer { get; set; }
        public string OAuthAuthorizationEndpoint { get; set; }
        public string OAuthIssuer { get; set; }
        public string OAuthAudience { get; set; }
        public string OAuthSigningKey { get; set; }
        public X509Certificate2 OAuthSigningCertificate { get; set; }
    }
}

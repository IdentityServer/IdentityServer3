using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Thinktecture.IdentityModel;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Models
{
    public class Configuration
    {
        ISettingsService _settings;

        public Configuration(ISettingsService settings)
        {
            _settings = settings;
            _settings.Prefix = "oidc";
        }

        public string IssuerName
        {
            get 
            {
                return "https://idsrv3.com";
                //return _settings.Get("issuer_name");
            }
        }

        public int MaximumAuthorizationCodeLifetime 
        { 
            get
            {
                return 60;
            }
        }

        public X509Certificate2 SigningKey
        {
            get 
            {
                return X509.CurrentUser.My.SubjectDistinguishedName.Find("CN=Client").First();
            }
        }
    }
}

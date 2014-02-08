
namespace Thinktecture.IdentityServer.Core.Services
{
    public class InternalProtectionSettings
    {
        public string SigningKey { get; set; }
        public string Audience { get; set; }
        public string Issuer { get; set; }
        public int Ttl { get; set; }
    }
}


namespace Thinktecture.IdentityServer.Core.Connect.Models
{
    public class ScopeClaim
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool AlwaysIncludeInIdToken { get; set; }
    }
}

using System.Collections.Generic;

namespace Thinktecture.IdentityServer.Core.Connect.Models
{
    public class Scope
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Emphasize { get; set; }
        public bool IsOpenIdScope { get; set; }
        public IEnumerable<ScopeClaim> Claims { get; set; }
    }
}

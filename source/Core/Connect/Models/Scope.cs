using System.Collections.Generic;

namespace Thinktecture.IdentityServer.Core.Connect.Models
{
    public class Scope
    {
        public string Name { get; set; }
        public string Description { get; set; }
        
        // todo: do we allow non identity scopes for 1st release?
        public bool IsOpenIdScope { get; set; }
        
        public IEnumerable<ScopeClaim> Claims { get; set; }
    }
}

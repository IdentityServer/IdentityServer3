using System.Collections.Generic;

namespace Thinktecture.IdentityServer.Admin.Core
{
    public class QueryResult
    {
        public int Start { get; set; }
        public int Count { get; set; }
        public int Total { get; set; }
        public string Filter { get; set; }
        public IEnumerable<UserResult> Users { get; set; }
    }
}

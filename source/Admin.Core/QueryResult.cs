using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Admin.Core
{
    public class QueryResult
    {
        public int Start { get; set; }
        public int Count { get; set; }
        public int Total { get; set; }
        public IEnumerable<UserSummary> Users { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Admin.Core
{
    public enum ClaimDataType
    {
        String,
        Number,
        Boolean,
    }

    public class ClaimMetadata
    {
        public string ClaimType { get; set; }
        public ClaimDataType DataType { get; set; }
        public string DisplayName { get; set; }
        public bool Required { get; set; }
        public IEnumerable<string> AllowedValues { get; set; }
    }

    public class UserManagerMetadata
    {
        public string UniqueIdentitiferClaimType { get; set; }
        public IEnumerable<ClaimMetadata> Claims { get; set; }
    }
}

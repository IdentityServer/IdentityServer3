using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Connect.Models
{
    public class ClientCredential
    {
        public string ClientId { get; set; }
        public string Secret { get; set; }

        public bool IsMalformed { get; set; }
        public bool IsPresent { get; set; }
        public string Type { get; set; }
    }
}

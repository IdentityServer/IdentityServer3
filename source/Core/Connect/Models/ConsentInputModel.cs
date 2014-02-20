using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Connect.Models
{
    public class ConsentInputModel
    {
        public string Button { get; set; }
        public string[] Scopes { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public class CookieOptions
    {
        public string Prefix { get; set; }
        public TimeSpan ExpireTimeSpan { get; set; }
        public bool IsPersistent { get; set; }
    }
}

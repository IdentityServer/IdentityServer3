using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Thinktecture.IdentityServer.Core.Connect.Models
{
    public class Token
    {
        public string Audience { get; set; }
        public string Issuer { get; set; }
        public DateTime CreationTime { get; set; }
        public int Lifetime { get; set; }
        public string Type { get; set; }

        public List<Claim> Claims { get; set; }

        public Token()
        {
            CreationTime = DateTime.UtcNow;
        }
    }
}

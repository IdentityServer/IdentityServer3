using System;
using System.Collections.Generic;

namespace Thinktecture.IdentityServer.WsFed.Models
{
    public class RelyingParty
    {
        public string Realm { get; set; }
        public Uri ReplyUrl { get; set; }
        public string TokenType { get; set; }
        public int TokenLifeTime { get; set; }
        public IEnumerable<string> Claims { get; set; }
    }
}
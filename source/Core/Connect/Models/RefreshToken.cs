using System;

namespace Thinktecture.IdentityServer.Core.Connect.Models
{
    public class RefreshToken
    {
        public string Handle { get; set; }
        public string ClientId { get; set; }
        public DateTime CreationTime { get; set; }
        public int LifeTime { get; set; }

        public Token AccessToken { get; set; }
    }
}
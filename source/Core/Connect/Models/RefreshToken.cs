using System;

namespace Thinktecture.IdentityServer.Core.Connect.Models
{
    public class RefreshToken
    {
        public string ClientId { get; set; }
        public DateTime CreationTime { get; set; }
        public int LifeTime { get; set; }

        public Token AccessToken { get; set; }
    }
}
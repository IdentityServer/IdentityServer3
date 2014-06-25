/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Thinktecture.IdentityServer.WsFederation.Models
{
    public class RelyingParty
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }

        public string Realm { get; set; }
        public string ReplyUrl { get; set; }
        public string TokenType { get; set; }
        public int TokenLifeTime { get; set; }
        public X509Certificate2 EncryptingCertificate { get; set; }
        public Dictionary<string, string> ClaimMappings { get; set; }
    }
}
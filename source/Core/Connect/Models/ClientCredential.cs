/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

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

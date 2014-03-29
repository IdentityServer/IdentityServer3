/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;

namespace Thinktecture.IdentityServer.Core.Connect.Models
{
    public class AuthorizeResponse
    {
        public Uri RedirectUri { get; set; }
        public string IdentityToken { get; set; }
        public string AccessToken { get; set; }
        public int AccessTokenLifetime { get; set; }
        public string Code { get; set; }
        public string State { get; set; }
        public string Scope { get; set; }
    }
}

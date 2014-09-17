/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using Thinktecture.IdentityServer.Core.Authentication;

namespace Thinktecture.IdentityServer.Core.Connect.Models
{
    public class EndSessionResponse
    {
        public bool IsRedirect { get { return RedirectUri != null; } }
        public bool IsLogout { get { return LogoutMessage != null; } }
        public Uri RedirectUri { get; set; }
        public LogOutMessage LogoutMessage { get; set; }
    }
}

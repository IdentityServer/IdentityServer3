/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using Thinktecture.IdentityServer.Core.Authentication;

namespace Thinktecture.IdentityServer.Core.Connect.Models
{
    public class InteractionResponse
    {
        public bool IsError { get; set; }
        public bool IsLogin { get; set; }
        public bool IsConsent { get; set; }

        public AuthorizeError Error { get; set; }
        public string ConsentError { get; set; }
        public SignInMessage SignInMessage { get; set; }
    }
}

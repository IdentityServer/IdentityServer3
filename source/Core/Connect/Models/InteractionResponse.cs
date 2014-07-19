/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using Thinktecture.IdentityServer.Core.Authentication;

namespace Thinktecture.IdentityServer.Core.Connect.Models
{
    public abstract class InteractionResponse
    {
        public bool IsError { get { return Error != null; } }
        public AuthorizeError Error { get; set; }
    }

    public class LoginInteractionResponse : InteractionResponse
    {
        public bool IsLogin { get { return SignInMessage != null; } }
        public SignInMessage SignInMessage { get; set; }
    }
    
    public class ConsentInteractionResponse : InteractionResponse
    {
        public bool IsConsent { get; set; }
        public string ConsentError { get; set; }
    }
}

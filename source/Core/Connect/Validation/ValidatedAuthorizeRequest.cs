/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class ValidatedAuthorizeRequest
    {
        public NameValueCollection Raw { get; set; }
        public ClaimsPrincipal Subject { get; set; }

        public string ResponseType { get; set; }
        public string ResponseMode { get; set; }
        public Flows Flow { get; set; }
        public CoreSettings CoreSettings { get; set; }
        public ScopeValidator ValidatedScopes { get; set; }

        public string ClientId { get; set; }
        public Client Client { get; set; }
        public Uri RedirectUri { get; set; }
        public List<string> RequestedScopes { get; set; }
        public bool WasConsentShown { get; set; }
        public string State { get; set; }
        public string UiLocales { get; set; }
        public bool IsOpenIdRequest { get; set; }
        public bool IsResourceRequest { get; set; }
        
        public string Nonce { get; set; }
        public List<string> AuthenticationContextClasses { get; set; }
        public List<string> AuthenticationMethods { get; set; }
        public string DisplayMode { get; set; }
        public string PromptMode { get; set; }
        public int? MaxAge { get; set; }

        public bool AccessTokenRequested
        {
            get
            {
                return (ResponseType == Constants.ResponseTypes.IdTokenToken ||
                        ResponseType == Constants.ResponseTypes.Code);
            }
        }

        public ValidatedAuthorizeRequest()
        {
            RequestedScopes = new List<string>();
            AuthenticationContextClasses = new List<string>();
            AuthenticationMethods = new List<string>();
        }
    }
}
using System;
using System.Collections.Generic;
using Thinktecture.IdentityServer.Core.Protocols.Connect.Models;

namespace Thinktecture.IdentityServer.Core.Protocols.Connect
{
    public class ValidatedAuthorizeRequest
    {
        public string ResponseType { get; set; }
        public string ResponseMode { get; set; }
        public Flows Flow { get; set; }
        public Configuration Configuration { get; set; }

        public string ClientId { get; set; }
        public Client Client { get; set; }
        public Uri RedirectUri { get; set; }
        public List<string> Scopes { get; set; }
        public string State { get; set; }
        public string UiLocales { get; set; }
        
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
            Scopes = new List<string>();
            AuthenticationContextClasses = new List<string>();
            AuthenticationMethods = new List<string>();
        }
    }
}
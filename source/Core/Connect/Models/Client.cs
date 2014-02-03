using System;
using System.Collections.Generic;

namespace Thinktecture.IdentityServer.Core.Connect.Models
{
    public class Client
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string ClientName { get; set; }
        public string ClientUri { get; set; }
        public Uri LogoUri { get; set; }
        public ApplicationTypes ApplicationType { get; set; }
        public bool RequireConsent { get; set; }

        public Flows Flow { get; set; }
        //public bool AllowOpenId { get; set; }
        public List<Uri> RedirectUris { get; set; }
        
        public int IdentityTokenLifetime { get; set; }
        public int AccessTokenLifetime { get; set; }
        public int RefreshTokenLifetime { get; set; }

        public List<string> ScopeRestrictions { get; set; }

        public SubjectTypes SubjectType { get; set; }
        public Uri SectorIdentifierUri { get; set; }
        public SigningKeyTypes IdentityTokenSigningKeyType { get; set; }
        
        public bool RequireSignedAuthorizeRequest { get; set; }

        // todo: makes sense? must be always signed by def. x509 cert
        // how does userInfo endpoint distinguish?
        public AccessTokenType AccessTokenType { get; set; }
    }

    public enum Flows
    {
        Code,
        Implicit,
        Hybrid
    }

    public enum SubjectTypes
    {
        Global, 
        PPID
    };

    public enum ApplicationTypes
    {
        Web, 
        Native
    };

    public enum SigningKeyTypes
    {
        Default, 
        ClientSecret
    };

    public enum AccessTokenType
    {
        JWT, 
        Reference
    }
}

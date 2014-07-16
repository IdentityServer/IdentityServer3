/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

namespace Thinktecture.IdentityServer.Core.Models
{
    public enum Flows
    {
        Code,
        Implicit,
        ClientCredentials,
        ResourceOwner,
        Assertion
    }

    public enum SubjectTypes
    {
        Global,
        Ppid
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
        Jwt,
        Reference
    }

    public enum TokenUsage
    {
        ReUse,
        OneTimeOnly
    }

    public enum TokenExpiration
    {
        Sliding,
        Absolute
    }
}

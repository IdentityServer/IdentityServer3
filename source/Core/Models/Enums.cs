/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

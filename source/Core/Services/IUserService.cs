/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Authentication;

namespace Thinktecture.IdentityServer.Core.Models
{
    public interface IUserService
    {
        Task<AuthenticateResult> AuthenticateLocalAsync(string username, string password);
        Task<ExternalAuthenticateResult> AuthenticateExternalAsync(string subject, ExternalIdentity externalUser);
        Task<IEnumerable<Claim>> GetProfileDataAsync(string subject, IEnumerable<string> requestedClaimTypes = null);
    }

    

    

}

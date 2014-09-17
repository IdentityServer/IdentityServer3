/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Authentication;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Connect
{
    public class EndSessionResponseGenerator
    {
        public EndSessionResponse ProcessRequest(ValidatedEndSessionRequest request, ClaimsPrincipal subject)
        {
            if (request.PostLogoutRedirectUri != null)
            {
                if (!subject.Identity.IsAuthenticated)
                    return new EndSessionResponse { RedirectUri = request.PostLogoutRedirectUri };

                var subjectClaim = request.Claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.Subject);
                if (subjectClaim != null && subject.HasClaim(Constants.ClaimTypes.Subject, subjectClaim.Value))
                    return new EndSessionResponse { LogoutMessage = new LogOutMessage { ReturnUrl = request.PostLogoutRedirectUri.AbsoluteUri } };
            }
            return new EndSessionResponse();
        }
    }
}
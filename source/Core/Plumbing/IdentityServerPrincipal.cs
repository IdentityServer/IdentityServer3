/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using System.Collections.Generic;
using System.Security.Claims;
using Thinktecture.IdentityModel.Extensions;

namespace Thinktecture.IdentityServer.Core.Plumbing
{
    public static class IdentityServerPrincipal
    {
        public static ClaimsPrincipal Create(
            string subject,
            string name, 
            string authenticationMethod, 
            string idp, 
            string authenticationType = Constants.PrimaryAuthenticationType,
            long authenticationTime = 0)
        {
            if (String.IsNullOrWhiteSpace(subject)) throw new ArgumentNullException("subject");
            if (String.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");
            if (String.IsNullOrWhiteSpace(authenticationMethod)) throw new ArgumentNullException("authenticationMethod");
            if (String.IsNullOrWhiteSpace(idp)) throw new ArgumentNullException("idp");
            if (String.IsNullOrWhiteSpace(authenticationType)) throw new ArgumentNullException("authenticationType");

            if (authenticationTime <= 0) authenticationTime = DateTime.UtcNow.ToEpochTime();

            var claims = new List<Claim>
            {
                new Claim(Constants.ClaimTypes.Subject, subject),
                new Claim(Constants.ClaimTypes.Name, name),
                new Claim(Constants.ClaimTypes.AuthenticationMethod, authenticationMethod),
                new Claim(Constants.ClaimTypes.IdentityProvider, idp),
                new Claim(Constants.ClaimTypes.AuthenticationTime, authenticationTime.ToString(), ClaimValueTypes.Integer)
            };

            var id = new ClaimsIdentity(claims, authenticationType);
            return new ClaimsPrincipal(id);
        }
    }
}

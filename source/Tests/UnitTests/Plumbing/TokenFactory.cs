/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.Collections.Generic;
using System.Security.Claims;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Connect.Models;

namespace UnitTests.Plumbing
{
    static class TokenFactory
    {
        public static Token CreateAccessToken()
        {
            var token = new Token(Constants.TokenTypes.AccessToken)
            {
                Audience = "audience",
                Issuer = "issuer",
                Lifetime = 60,

                Claims = new List<Claim> 
                {
                    new Claim("client_id", "client"),
                    new Claim("scope", "openid")
                }
            };

            return token;
        }

        public static Token CreateIdentityToken()
        {
            var token = new Token(Constants.TokenTypes.IdentityToken)
            {
                Audience = "client",
                Issuer = "issuer",
                Lifetime = 60,

                Claims = new List<Claim> 
                {
                    new Claim("sub", "subject")
                }
            };

            return token;
        }
    }
}
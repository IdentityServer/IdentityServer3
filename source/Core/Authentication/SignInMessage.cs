/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.ServiceModel.Security.Tokens;
using Thinktecture.IdentityModel.Tokens;
using Thinktecture.IdentityServer.Core.Configuration;

namespace Thinktecture.IdentityServer.Core.Authentication
{
    public class SignInMessage
    {
        public string ReturnUrl { get; set; }
        public string IdP { get; set; }

        // internal use
        public DateTime ValidTo { get; set; }

        // not implemented
        public string DisplayMode { get; set; }
        public string UILocales { get; set; }
        //public string LoginHint { get; set; }
        //public string AuthenticationMethod { get; set; }

        public string Protect(int ttl, IDataProtector protector)
        {
            ValidTo = DateTime.UtcNow.AddSeconds(ttl);

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            var json = JsonConvert.SerializeObject(this, settings);
            return protector.Protect(json, "signinmessage");
        }

        public static SignInMessage Unprotect(string data, IDataProtector protector)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            var json = protector.Unprotect(data, "signinmessage");
            var message = JsonConvert.DeserializeObject<SignInMessage>(json);

            if (DateTime.UtcNow > message.ValidTo)
            {
                throw new Exception("SignInMessage expired.");
            }

            return message;
        }

        //public string ToJwt(string issuer, string audience, string key, int ttl)
        //{
        //    var claims = new List<Claim>();

        //    if (ReturnUrl.IsPresent())
        //    {
        //        claims.Add(new Claim("returnUrl", ReturnUrl));
        //    }

        //    if (IdP.IsPresent())
        //    {
        //        claims.Add(new Claim("idp", IdP));
        //    }

        //    var token = new JwtSecurityToken(
        //        issuer,
        //        audience,
        //        claims,
        //        new Lifetime(DateTime.UtcNow, DateTime.UtcNow.AddSeconds(ttl)),
        //        new HmacSigningCredentials(key));

        //    return new JwtSecurityTokenHandler().WriteToken(token);
        //}

        //public static SignInMessage FromJwt(string jwt, string issuer, string audience, string key)
        //{
        //    var message = new SignInMessage();
        //    var handler = new JwtSecurityTokenHandler();
        //    var parameters = new TokenValidationParameters
        //    {
        //        AllowedAudience = audience,
        //        ValidIssuer = issuer,
        //        SigningToken = new BinarySecretSecurityToken(Convert.FromBase64String(key))
        //    };

        //    var principal = handler.ValidateToken(jwt, parameters);

        //    var returnUrlClaim = principal.FindFirst("returnUrl");
        //    if (returnUrlClaim != null)
        //    {
        //        message.ReturnUrl = returnUrlClaim.Value;
        //    }

        //    var idpClaim = principal.FindFirst("idp");
        //    if (idpClaim != null)
        //    {
        //        message.IdP = idpClaim.Value;
        //    }

        //    return message;
        //}
    }
}
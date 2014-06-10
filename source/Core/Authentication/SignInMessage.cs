/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using Newtonsoft.Json;
using System;
using Thinktecture.IdentityServer.Core.Configuration;

namespace Thinktecture.IdentityServer.Core.Authentication
{
    public class SignInMessage
    {
        public string ReturnUrl { get; set; }
        public string IdP { get; set; }
        public string DisplayMode { get; set; }
        public string UILocales { get; set; }

        // internal use
        public DateTime ValidTo { get; set; }

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
    }
}
/*
 * Copyright 2014 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;

namespace Thinktecture.IdentityServer.Core.Authentication
{
    public class SignInMessage
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        public string ReturnUrl { get; set; }
        public string IdP { get; set; }
        public string DisplayMode { get; set; }
        public string UiLocales { get; set; }
        public IEnumerable<string> AcrValues { get; set; }

        // internal use
        public DateTime ValidTo { get; set; }

        
        public string Protect(int ttl, IDataProtector protector)
        {
            ValidTo = DateTime.UtcNow.AddSeconds(ttl);

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
            };

            var json = JsonConvert.SerializeObject(this, settings);
            Logger.DebugFormat("Protecting signin message: {0}", json);

            return protector.Protect(json, "signinmessage");
        }

        public static SignInMessage Unprotect(string data, IDataProtector protector)
        {
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
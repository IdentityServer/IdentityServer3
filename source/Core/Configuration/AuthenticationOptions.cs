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

using Owin;
using System;
using System.Collections.Generic;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public class AuthenticationOptions
    {
        public AuthenticationOptions()
        {
            EnableLocalLogin = true;
            CookieOptions = new CookieOptions();
        }

        public bool EnableLocalLogin { get; set; }
        public CookieOptions CookieOptions { get; set; }
        public IEnumerable<LoginPageLink> LoginPageLinks { get; set; }
        public bool DisableSignOutPrompt { get; set; }
        public Action<IAppBuilder, string> IdentityProviders { get; set; }

    }

    public class LoginPageLink
    {
        public string Text { get; set; }
        public string Href { get; set; }
    }
}
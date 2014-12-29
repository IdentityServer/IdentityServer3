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

using System.Collections.Generic;
using Thinktecture.IdentityServer.Core.Configuration;

namespace Thinktecture.IdentityServer.Core.ViewModels
{
    public class LoginViewModel : ErrorViewModel
    {
        // LoginUrl might be null if local logins are disabled
        public string LoginUrl { get; set; }
        public AntiForgeryHiddenInputViewModel AntiForgery { get; set; }
        public bool AllowRememberMe { get; set; }
        public bool RememberMe { get; set; }
        public string LogoutUrl { get; set; }
        public string Username { get; set; }
        public IEnumerable<LoginPageLink> ExternalProviders { get; set; }
        public IEnumerable<LoginPageLink> AdditionalLinks { get; set; }
    }
}

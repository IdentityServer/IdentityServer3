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

namespace Thinktecture.IdentityServer.Core.ViewModels
{
    public class ConsentViewModel : ErrorViewModel
    {
        public string LoginWithDifferentAccountUrl { get; set; }
        public string LogoutUrl { get; set; }
        public string ConsentUrl { get; set; }
        public AntiForgeryHiddenInputViewModel AntiForgery { get; set; }
        public string ClientName { get; set; }
        public string ClientUrl { get; set; }
        public string ClientLogoUrl { get; set; }
        public bool AllowRememberConsent { get; set; }
        public bool RememberConsent { get; set; }
        public IEnumerable<ConsentScopeViewModel> IdentityScopes { get; set; }
        public IEnumerable<ConsentScopeViewModel> ResourceScopes { get; set; }
    }
}

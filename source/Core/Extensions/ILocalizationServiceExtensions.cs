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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Extensions
{
    public static class ILocalizationServiceExtensions
    {
        public static string GetMessage(this ILocalizationService localization, string id)
        {
            if (localization == null) throw new ArgumentNullException("localization");

            return localization.GetString(Thinktecture.IdentityServer.Core.Constants.LocalizationCategories.Messages, id);
        }

        public static string GetEvent(this ILocalizationService localization, string id)
        {
            if (localization == null) throw new ArgumentNullException("localization");

            return localization.GetString(Thinktecture.IdentityServer.Core.Constants.LocalizationCategories.Events, id);
        }
        
        public static string GetScopeDisplayName(this ILocalizationService localization, string scope)
        {
            if (localization == null) throw new ArgumentNullException("localization");
            
            return localization.GetString(Thinktecture.IdentityServer.Core.Constants.LocalizationCategories.Scopes, scope + Constants.ScopeDisplayNameSuffix);
        }
        
        public static string GetScopeDescription(this ILocalizationService localization, string scope)
        {
            if (localization == null) throw new ArgumentNullException("localization");
            
            return localization.GetString(Thinktecture.IdentityServer.Core.Constants.LocalizationCategories.Scopes, scope + Constants.ScopeDescriptionSuffix);
        }
    }
}

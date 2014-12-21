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

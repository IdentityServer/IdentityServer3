﻿using System.Collections.Generic;
using System.Linq;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Resources;
using Thinktecture.IdentityServer.Core.Services.Default;
using Xunit;
using Xunit.Sdk;

namespace Thinktecture.IdentityServer.Tests.Services.Default
{
    public class DefaultLocalizationServiceTests
    {
        [Fact]
        public void GetString_ForAllMessageIds_ReturnsValues()
        {
            AssertTranslationExists(GetAllMessageIds(), Constants.LocalizationCategories.MESSAGES);    
        }

        [Fact]
        public void GetString_ForAllEventIds_ReturnsValues()
        {
            AssertTranslationExists(GetAllEventIds(), Constants.LocalizationCategories.EVENTS);
        }

        [Fact]
        public void GetString_ForAllScopeIds_ReturnsValues()
        {
            AssertTranslationExists(GetAllScopeIds(), Constants.LocalizationCategories.SCOPES);
        }

        private static void AssertTranslationExists(IEnumerable<string> ids, string category)
        {
            var service = new DefaultLocalizationService();
            var notFoundLocalizations = new List<string>();

            foreach (var id in ids)
            {
                var localizedString = service.GetString(category, id);
                if (string.IsNullOrEmpty(localizedString))
                {
                    var errormsg = string.Format("Could not find localization of Id '{0}' ", id);
                    notFoundLocalizations.Add(errormsg);
                }
                else
                {
                    Assert.NotEqual("", localizedString.Trim());
                }
            }
            if (notFoundLocalizations.Any())
            {
                var concated = notFoundLocalizations.Aggregate((x, y) => x + ", " + y);
                throw new AssertActualExpectedException("Some translation", "NOTHING!", concated);
            }
        }

        public static IEnumerable<string> GetAllMessageIds()
        {
            return typeof(MessageIds).GetFields().Select(m => m.GetRawConstantValue().ToString());
        }

        public static IEnumerable<string> GetAllEventIds()
        {
            return typeof(EventIds).GetFields().Select(m => m.GetRawConstantValue().ToString());
        }

        public static IEnumerable<string> GetAllScopeIds()
        {
            return typeof(ScopeIds).GetFields().Select(m => m.GetRawConstantValue().ToString());
        }
    }
}

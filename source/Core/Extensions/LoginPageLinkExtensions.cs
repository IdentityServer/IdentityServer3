/*
 * Copyright 2014, 2015 Dominick Baier, Brock Allen
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

using IdentityServer3.Core.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace IdentityServer3.Core.Extensions
{
    internal static class LoginPageLinkExtensions
    {
        internal static IEnumerable<LoginPageLink> Render(this IEnumerable<LoginPageLink> links, string baseUrl, string signinId)
        {
            if (links == null || !links.Any()) return null;

            var result = new List<LoginPageLink>();
            foreach (var link in links)
            {
                var url = link.Href;
                if (url.StartsWith("~/"))
                {
                    url = url.Substring(2);
                    url = baseUrl + url;
                }

                url = url.AddQueryString("signin=" + signinId);

                result.Add(new LoginPageLink
                {
                    Type = link.Type,
                    Text = link.Text,
                    Href = url
                });
            }
            return result;
        }
    }
}

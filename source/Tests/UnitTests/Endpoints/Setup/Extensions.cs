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

using FluentAssertions;
using IdentityServer3.Core.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Web;

namespace IdentityServer3.Tests.Endpoints
{
    static class Extensions
    {
        public static void SetCookies(this HttpClient client, IEnumerable<string> cookies)
        {
            foreach (var c in cookies)
            {
                if (c.LooksLikeACookieDeletion())
                {
                    client.RemoveCookieByName(c);
                }
                else
                {
                    client.DefaultRequestHeaders.Add("Cookie", c);
                }
            }
        }
        public static void SetCookies(this HttpClient client, IEnumerable<CookieState> cookies)
        {
            client.SetCookies(cookies.Select(c => c.ToString()));
        }

        public static IEnumerable<CookieState> GetCookies(this HttpResponseMessage resp)
        {
            IEnumerable<string> values;
            if (resp.Headers.TryGetValues("Set-Cookie", out values))
            {
                var cookies = new List<CookieState>();
                foreach (var value in values)
                {
                    CookieHeaderValue cookie;
                    if (CookieHeaderValue.TryParse(value, out cookie))
                    {
                        cookies.AddRange(cookie.Cookies);
                    }
                }
                return cookies;
            }
            return Enumerable.Empty<CookieState>();
        }

        public static IEnumerable<string> GetRawCookies(this HttpResponseMessage resp)
        {
            IEnumerable<string> values;
            if (resp.Headers.TryGetValues("Set-Cookie", out values))
            {
                return values;
            }
            return Enumerable.Empty<string>();
        }

        public static void AssertCookie(this HttpResponseMessage resp, string name)
        {
            var cookies = resp.GetCookies();
            var cookie = cookies.SingleOrDefault(x => x.Name == name);
            cookie.Should().NotBeNull();
        }

        public static void AssertPage(this HttpResponseMessage resp, string name)
        {
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
            resp.Content.Headers.ContentType.MediaType.Should().Be("text/html");
            var html = resp.Content.ReadAsStringAsync().Result;

            var match = Regex.Match(html, "<div class='container page-(.*)' ng-cloak>");
            match.Groups[1].Value.Should().Be(name);
        }

        static T GetModel<T>(string html)
        {
            var match = "<script id='modelJson' type='application/json'>";
            var start = html.IndexOf(match);
            var end = html.IndexOf("</script>", start);
            var content = html.Substring(start + match.Length, end - start - match.Length);
            var json = HttpUtility.HtmlDecode(content);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static T GetModel<T>(this HttpResponseMessage resp)
        {
            resp.IsSuccessStatusCode.Should().BeTrue();
            var html = resp.Content.ReadAsStringAsync().Result;
            return GetModel<T>(html);
        }

        public static T GetJson<T>(this HttpResponseMessage resp, Boolean successExpected = true)
        {
            if (successExpected)
            {
                resp.IsSuccessStatusCode.Should().BeTrue();
            }

            var json = resp.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<T>(json);
        }

        private static void RemoveCookieByName(this HttpClient client, string cookieString)
        {
            Conformance.Extensions.RemoveCookie(client, cookieString.Split('=').First());
        }

        private static bool LooksLikeACookieDeletion(this string cookieString)
        {
            if (!cookieString.Contains("expires"))
            {
                return false;
            }

            var parts = cookieString
                .Split(';')
                .Select(s => s.Trim())
                .ToDictionary(s => s.Split('=').First(), s => s.Split('=').Last());

            DateTime expiry;
            if (DateTime.TryParse(parts["expires"], out expiry))
            {
                return parts.First().Value == "." && expiry < DateTimeHelper.UtcNow;
            }

            return false;
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Thinktecture.IdentityServer.Tests
{
    static class Extensions
    {
        public static void SetCookies(this HttpClient client, IEnumerable<CookieState> cookies)
        {
            foreach (var c in cookies)
            {
                client.DefaultRequestHeaders.Add("Cookie", c.ToString());
            }
        }

        public static IEnumerable<CookieState> GetCookies(this HttpResponseMessage resp)
        {
            IEnumerable<string> values;
            if (resp.Headers.TryGetValues("Set-Cookie", out values))
            {
                List<CookieState> cookies = new List<CookieState>();
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
            Assert.IsNotNull(cookie);
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Tests
{
    static class Extensions
    {
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

        public static void AssertCookie(this HttpResponseMessage resp, string name)
        {
            var cookies = resp.GetCookies();
            var cookie = cookies.SingleOrDefault(x => x.Name == name);
            Assert.IsNotNull(cookie);
        }
    }
}

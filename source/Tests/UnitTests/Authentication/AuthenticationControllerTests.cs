using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Net.Http;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Authentication;
using System.Net;
using System.Text.RegularExpressions;
using Thinktecture.IdentityServer.Core.Assets;
using Newtonsoft.Json;

namespace Thinktecture.IdentityServer.Tests.Authentication
{
    [TestClass]
    public class AuthenticationControllerTests : IdSvrHostTestBase
    {
        LayoutModel GetLayoutModel(string html)
        {
            var match = Regex.Match(html, "<script id='layoutModelJson' type='application/json'>(.|\n)*?</script>");
            match = Regex.Match(match.Value, "{(.)*}");
            return JsonConvert.DeserializeObject<LayoutModel>(match.Value);
        }

        void AssertPage(HttpResponseMessage resp, string name)
        {
            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
            Assert.AreEqual("text/html", resp.Content.Headers.ContentType.MediaType);
            var html = resp.Content.ReadAsStringAsync().Result;
            var layout = GetLayoutModel(html);
            Assert.AreEqual(name, layout.Page);
        }

        private HttpResponseMessage GetLoginPage(SignInMessage msg = null)
        {
            msg = msg ?? new SignInMessage();
            var val = msg.Protect(60000, protector);
            var resp = Get(Constants.RoutePaths.Login + "?message=" + val);
            var setCookies = resp.Headers.GetValues("Set-Cookie");
            Assert.AreEqual(1, setCookies.Count());
            var cookie = setCookies.First();
            Assert.IsNotNull(cookie);
            StringAssert.Contains(cookie, "idsrv.login.message");
            client.DefaultRequestHeaders.Add("Cookie", cookie);
            return resp;
        }

        [TestMethod]
        public void GetLogin_WithSignInMessage_ReturnsLoginPage()
        {
            var msg = new SignInMessage();
            var val = msg.Protect(60000, protector);
            var resp = Get(Constants.RoutePaths.Login + "?message=" + val);
            AssertPage(resp, "login");
        }

        [TestMethod]
        public void GetLogin_WithSignInMessage_IssuesMessageCookie()
        {
            GetLoginPage();
        }

        [TestMethod]
        public void GetLogin_SignInMessageHasIdentityProvider_RedirectsToExternalProviderLogin()
        {
            var msg = new SignInMessage();
            msg.IdP = "Google";
            var resp = GetLoginPage(msg);

            Assert.AreEqual(HttpStatusCode.Found, resp.StatusCode);
            var expected = new Uri(Url(Constants.RoutePaths.LoginExternal));
            Assert.AreEqual(expected.AbsolutePath, resp.Headers.Location.AbsolutePath);
            StringAssert.Contains(resp.Headers.Location.Query, "provider=Google");
        }

        [TestMethod]
        public void GetLogin_NoSignInMessage_ReturnErrorPage()
        {
            var resp = Get(Constants.RoutePaths.Login);
            AssertPage(resp, "error");
        }

        [TestMethod]
        public void GetExternalLogin_ValidProvider_RedirectsToProvider()
        {
            var msg = new SignInMessage();
            msg.IdP = "Google";
            var resp1 = GetLoginPage(msg);

            var resp2 = client.GetAsync(resp1.Headers.Location.AbsoluteUri).Result;
            Assert.AreEqual(HttpStatusCode.Found, resp2.StatusCode);
            Assert.IsTrue(resp2.Headers.Location.AbsoluteUri.StartsWith("https://www.google.com"));
        }
    }
}

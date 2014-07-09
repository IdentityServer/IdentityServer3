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
using System.Net.Http.Headers;
using Thinktecture.IdentityServer.Core.Resources;
using Moq;
using System.Threading.Tasks;

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
        LayoutModel GetLayoutModel(HttpResponseMessage resp)
        {
            var html = resp.Content.ReadAsStringAsync().Result;
            return GetLayoutModel(html);
        }

        void AssertPage(HttpResponseMessage resp, string name)
        {
            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
            Assert.AreEqual("text/html", resp.Content.Headers.ContentType.MediaType);
            var layout = GetLayoutModel(resp);
            Assert.AreEqual(name, layout.Page);
        }

        private HttpResponseMessage GetLoginPage(SignInMessage msg = null)
        {
            msg = msg ?? new SignInMessage() { ReturnUrl = Url("authorize") };
            
            var val = msg.Protect(60000, protector);
            var resp = Get(Constants.RoutePaths.Login + "?message=" + val);
            resp.AssertCookie(AuthenticationController.LoginRequestMessageCookieName);
            
            foreach(var c in resp.GetCookies())
            {
                client.DefaultRequestHeaders.Add("Cookie", c.ToString());
            }

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

        [TestMethod]
        public void PostLogin_ValidCredentials_IssuesAuthenticationCookie()
        {
            GetLoginPage();
            var resp = Post(Constants.RoutePaths.Login, new LoginCredentials { Username = "alice", Password = "alice" });
            resp.AssertCookie(Constants.PrimaryAuthenticationType);
        }

        [TestMethod]
        public void PostLogin_ValidCredentials_RedirectsBackToAuthorization()
        {
            GetLoginPage();
            var resp = Post(Constants.RoutePaths.Login, new LoginCredentials { Username = "alice", Password = "alice" });
            Assert.AreEqual(HttpStatusCode.Found, resp.StatusCode);
            Assert.AreEqual(resp.Headers.Location, Url("authorize"));
        }

        [TestMethod]
        public void PostLogin_NoModel_ShowErrorPage()
        {
            GetLoginPage();
            var resp = Post(Constants.RoutePaths.Login, (LoginCredentials)null);
            AssertPage(resp, "login");
            var model = GetLayoutModel(resp);
            Assert.AreEqual(model.ErrorMessage, Messages.InvalidUsernameOrPassword);
        }

        [TestMethod]
        public void PostLogin_InvalidUsername_ShowErrorPage()
        {
            GetLoginPage();
            var resp = Post(Constants.RoutePaths.Login, new LoginCredentials { Username = "bad", Password = "alice" });
            AssertPage(resp, "login");
            var model = GetLayoutModel(resp);
            Assert.AreEqual(model.ErrorMessage, Messages.InvalidUsernameOrPassword);
        }
        [TestMethod]
        public void PostLogin_InvalidPassword_ShowErrorPage()
        {
            GetLoginPage();
            var resp = Post(Constants.RoutePaths.Login, new LoginCredentials { Username = "alice", Password = "bad" });
            AssertPage(resp, "login");
            var model = GetLayoutModel(resp);
            Assert.AreEqual(model.ErrorMessage, Messages.InvalidUsernameOrPassword);
        }
        [TestMethod]
        public void PostLogin_UserServiceReturnsError_ShowErrorPage()
        {
            mockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new AuthenticateResult("bad stuff")));
                
            GetLoginPage();
            var resp = Post(Constants.RoutePaths.Login, new LoginCredentials { Username = "alice", Password = "alice" });
            AssertPage(resp, "login");
            var model = GetLayoutModel(resp);
            Assert.AreEqual(model.ErrorMessage, "bad stuff");
        }
    }
}

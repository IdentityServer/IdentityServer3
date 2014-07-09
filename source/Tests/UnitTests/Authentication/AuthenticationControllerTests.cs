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
            client.SetCookies(resp.GetCookies());
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
        public void GetExternalLogin_InalidProvider_ReturnsUnauthorized()
        {
            var msg = new SignInMessage();
            msg.IdP = "Foo";
            var resp1 = GetLoginPage(msg);

            var resp2 = client.GetAsync(resp1.Headers.Location.AbsoluteUri).Result;
            Assert.AreEqual(HttpStatusCode.Unauthorized, resp2.StatusCode);
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
        [TestMethod]
        public void PostLogin_UserServiceReturnsNull_ShowErrorPage()
        {
            mockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult((AuthenticateResult)null));

            GetLoginPage();
            var resp = Post(Constants.RoutePaths.Login, new LoginCredentials { Username = "alice", Password = "alice" });
            AssertPage(resp, "login");
            var model = GetLayoutModel(resp);
            Assert.AreEqual(model.ErrorMessage, Messages.InvalidUsernameOrPassword);
        }
        [TestMethod]
        public void PostLogin_UserServiceReturnsParialLogin_IssuesPartialLoginCookie()
        {
            mockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new AuthenticateResult("/foo", "tempsub", "tempname")));

            GetLoginPage();
            var resp = Post(Constants.RoutePaths.Login, new LoginCredentials { Username = "alice", Password = "alice" });
            resp.AssertCookie(Constants.PartialSignInAuthenticationType);
        }
        [TestMethod]
        public void PostLogin_UserServiceReturnsParialLogin_IssuesRedirect()
        {
            mockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new AuthenticateResult("/foo", "tempsub", "tempname")));

            GetLoginPage();
            var resp = Post(Constants.RoutePaths.Login, new LoginCredentials { Username = "alice", Password = "alice" });
            Assert.AreEqual(HttpStatusCode.Found, resp.StatusCode);
            Assert.AreEqual(Url("foo"), resp.Headers.Location.AbsoluteUri);
        }

        [TestMethod]
        public void ResumeLoginFromRedirect_WithPartialCookie_IssuesFullLoginCookie()
        {
            mockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new AuthenticateResult("/foo", "tempsub", "tempname")));

            GetLoginPage();
            var resp1 = Post(Constants.RoutePaths.Login, new LoginCredentials { Username = "alice", Password = "alice" });
            client.SetCookies(resp1.GetCookies());
            var resp2 = Get(Constants.RoutePaths.ResumeLoginFromRedirect);
            resp2.AssertCookie(Constants.PrimaryAuthenticationType);
        }
        [TestMethod]
        public void ResumeLoginFromRedirect_WithPartialCookie_IssuesRedirectToAuthorizationPage()
        {
            mockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new AuthenticateResult("/foo", "tempsub", "tempname")));

            GetLoginPage();
            var resp1 = Post(Constants.RoutePaths.Login, new LoginCredentials { Username = "alice", Password = "alice" });
            client.SetCookies(resp1.GetCookies());
            var resp2 = Get(Constants.RoutePaths.ResumeLoginFromRedirect);
            Assert.AreEqual(HttpStatusCode.Found, resp2.StatusCode);
            Assert.AreEqual(Url("authorize"), resp2.Headers.Location.AbsoluteUri);
        }

        [TestMethod]
        public void ResumeLoginFromRedirect_WithoutPartialCookie_RedirectsToLogin()
        {
            mockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new AuthenticateResult("/foo", "tempsub", "tempname")));

            GetLoginPage();
            var resp1 = Post(Constants.RoutePaths.Login, new LoginCredentials { Username = "alice", Password = "alice" });
            var resp2 = Get(Constants.RoutePaths.ResumeLoginFromRedirect);
            Assert.AreEqual(HttpStatusCode.Found, resp2.StatusCode);
            Assert.AreEqual(Url(Constants.RoutePaths.Login), resp2.Headers.Location.AbsoluteUri);
        }
        [TestMethod]
        public void ResumeLoginFromRedirect_WithoutSignInMessageCookie_ShowsErrorPage()
        {
            mockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new AuthenticateResult("/foo", "tempsub", "tempname")));

            GetLoginPage();
            var resp1 = Post(Constants.RoutePaths.Login, new LoginCredentials { Username = "alice", Password = "alice" });
            client.DefaultRequestHeaders.Remove("Cookie");
            client.SetCookies(resp1.GetCookies());
            var resp2 = Get(Constants.RoutePaths.ResumeLoginFromRedirect);
            AssertPage(resp2, "error");
        }

        [TestMethod]
        public void Logout_ShowsLogoutPage()
        {
            var resp = Get(Constants.RoutePaths.Logout);
            AssertPage(resp, "logout");
        }

        [TestMethod]
        public void Logout_RemovesCookies()
        {
            var resp = Get(Constants.RoutePaths.Logout);
            var cookies = resp.Headers.GetValues("Set-Cookie");
            Assert.AreEqual(4, cookies.Count());
            // GetCookies will not return values for cookies that are expired/revoked
            Assert.AreEqual(0, resp.GetCookies().Count());
        }
    }
}

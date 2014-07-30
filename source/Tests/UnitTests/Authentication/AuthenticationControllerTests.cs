using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Authentication;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Resources;
using Thinktecture.IdentityServer.Core.Views;

namespace Thinktecture.IdentityServer.Tests.Authentication
{
    [TestClass]
    public class AuthenticationControllerTests : IdSvrHostTestBase
    {
        public ClaimsIdentity SignInIdentity { get; set; }

        protected override void Postprocess(Microsoft.Owin.IOwinContext ctx)
        {
            if (SignInIdentity != null)
            {
                ctx.Authentication.SignIn(SignInIdentity);
                SignInIdentity = null;
            }
        }

        T GetModel<T>(string html)
        {
            var match = Regex.Match(html, "<script id='modelJson' type='application/json'>(.|\n)*?</script>");
            match = Regex.Match(match.Value, "{(.)*}");
            return JsonConvert.DeserializeObject<T>(match.Value);
        }
        T GetModel<T>(HttpResponseMessage resp)
        {
            var html = resp.Content.ReadAsStringAsync().Result;
            return GetModel<T>(html);
        }

        void AssertPage(HttpResponseMessage resp, string name)
        {
            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
            Assert.AreEqual("text/html", resp.Content.Headers.ContentType.MediaType);
            var html = resp.Content.ReadAsStringAsync().Result;
            var match = Regex.Match(html, "<ng-include src=\"'/assets/app\\.(.*)\\.html'\"></ng-include>");
            Assert.AreEqual(name, match.Groups[1].Value);
        }

        private HttpResponseMessage GetLoginPage(SignInMessage msg = null)
        {
            msg = msg ?? new SignInMessage() { ReturnUrl = Url("authorize") };
            
            var val = msg.Protect(60000, protector);
            var resp = Get(Constants.RoutePaths.Login + "?message=" + val);
            resp.AssertCookie(AuthenticationController.SignInMessageCookieName);
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
        public void PostToLogin_ValidCredentials_IssuesAuthenticationCookie()
        {
            GetLoginPage();
            var resp = Post(Constants.RoutePaths.Login, new LoginCredentials { Username = "alice", Password = "alice" });
            resp.AssertCookie(Constants.PrimaryAuthenticationType);
        }

        [TestMethod]
        public void PostToLogin_ValidCredentials_RedirectsBackToAuthorization()
        {
            GetLoginPage();
            var resp = Post(Constants.RoutePaths.Login, new LoginCredentials { Username = "alice", Password = "alice" });
            Assert.AreEqual(HttpStatusCode.Found, resp.StatusCode);
            Assert.AreEqual(resp.Headers.Location, Url("authorize"));
        }

        [TestMethod]
        public void PostToLogin_NoModel_ShowErrorPage()
        {
            GetLoginPage();
            var resp = Post(Constants.RoutePaths.Login, (LoginCredentials)null);
            AssertPage(resp, "login");
            var model = GetModel<LoginViewModel>(resp);
            Assert.AreEqual(model.ErrorMessage, Messages.InvalidUsernameOrPassword);
        }

        [TestMethod]
        public void PostToLogin_InvalidUsername_ShowErrorPage()
        {
            GetLoginPage();
            var resp = Post(Constants.RoutePaths.Login, new LoginCredentials { Username = "bad", Password = "alice" });
            AssertPage(resp, "login");
            var model = GetModel<LoginViewModel>(resp);
            Assert.AreEqual(model.ErrorMessage, Messages.InvalidUsernameOrPassword);
        }

        [TestMethod]
        public void PostToLogin_InvalidPassword_ShowErrorPage()
        {
            GetLoginPage();
            var resp = Post(Constants.RoutePaths.Login, new LoginCredentials { Username = "alice", Password = "bad" });
            AssertPage(resp, "login");
            var model = GetModel<LoginViewModel>(resp);
            Assert.AreEqual(model.ErrorMessage, Messages.InvalidUsernameOrPassword);
        }

        [TestMethod]
        public void PostToLogin_UserServiceReturnsError_ShowErrorPage()
        {
            mockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new AuthenticateResult("bad stuff")));

            GetLoginPage();
            var resp = Post(Constants.RoutePaths.Login, new LoginCredentials { Username = "alice", Password = "alice" });
            AssertPage(resp, "login");
            var model = GetModel<LoginViewModel>(resp);
            Assert.AreEqual(model.ErrorMessage, "bad stuff");
        }

        [TestMethod]
        public void PostToLogin_UserServiceReturnsNull_ShowErrorPage()
        {
            mockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult((AuthenticateResult)null));

            GetLoginPage();
            var resp = Post(Constants.RoutePaths.Login, new LoginCredentials { Username = "alice", Password = "alice" });
            AssertPage(resp, "login");
            var model = GetModel<LoginViewModel>(resp);
            Assert.AreEqual(model.ErrorMessage, Messages.InvalidUsernameOrPassword);
        }

        [TestMethod]
        public void PostToLogin_UserServiceReturnsParialLogin_IssuesPartialLoginCookie()
        {
            mockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new AuthenticateResult("/foo", "tempsub", "tempname")));

            GetLoginPage();
            var resp = Post(Constants.RoutePaths.Login, new LoginCredentials { Username = "alice", Password = "alice" });
            resp.AssertCookie(Constants.PartialSignInAuthenticationType);
        }

        [TestMethod]
        public void PostToLogin_UserServiceReturnsParialLogin_IssuesRedirect()
        {
            mockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new AuthenticateResult("/foo", "tempsub", "tempname")));

            GetLoginPage();
            var resp = Post(Constants.RoutePaths.Login, new LoginCredentials { Username = "alice", Password = "alice" });
            Assert.AreEqual(HttpStatusCode.Found, resp.StatusCode);
            Assert.AreEqual(Url("foo"), resp.Headers.Location.AbsoluteUri);
        }

        [TestMethod]
        public void PostToLogin_CookieOptionsIsPersistentIsFalse_DoesNotIssuePersistentCookie()
        {
            this.options.CookieOptions.IsPersistent = false;
            GetLoginPage();
            var resp = Post(Constants.RoutePaths.Login, new LoginCredentials { Username = "alice", Password = "alice" });
            var cookies = resp.GetRawCookies();
            var cookie = cookies.Single(x => x.StartsWith(Constants.PrimaryAuthenticationType + "="));
            Assert.IsFalse(cookie.Contains("expires="));
        }

        [TestMethod]
        public void PostToLogin_CookieOptionsIsPersistentIsTrue_IssuesPersistentCookie()
        {
            this.options.CookieOptions.IsPersistent = true;
            GetLoginPage();
            var resp = Post(Constants.RoutePaths.Login, new LoginCredentials { Username = "alice", Password = "alice" });
            var cookies = resp.GetRawCookies();
            var cookie = cookies.Single(x => x.StartsWith(Constants.PrimaryAuthenticationType + "="));
            Assert.IsTrue(cookie.Contains("expires="));
        }

        [TestMethod]
        public void PostToLogin_CookieOptionsIsPersistentIsTrueButResponseIsPartialLogin_DoesNotIssuePersistentCookie()
        {
            mockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new AuthenticateResult("/foo", "tempsub", "tempname")));
            
            this.options.CookieOptions.IsPersistent = true;
            GetLoginPage();
            var resp = Post(Constants.RoutePaths.Login, new LoginCredentials { Username = "alice", Password = "alice" });
            var cookies = resp.GetRawCookies();
            var cookie = cookies.Single(x => x.StartsWith(Constants.PartialSignInAuthenticationType + "="));
            Assert.IsFalse(cookie.Contains("expires="));
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
        public void Logout_ShowsLogoutPromptPage()
        {
            var resp = Get(Constants.RoutePaths.Logout);
            AssertPage(resp, "logout");
        }

        [TestMethod]
        public void PostToLogout_RemovesCookies()
        {
            var resp = Post(Constants.RoutePaths.Logout, (string)null);
            var cookies = resp.Headers.GetValues("Set-Cookie");
            Assert.AreEqual(4, cookies.Count());
            // GetCookies will not return values for cookies that are expired/revoked
            Assert.AreEqual(0, resp.GetCookies().Count());
        }
        
        [TestMethod]
        public void PostToLogout_EmitsLogoutUrlsForProtocolIframes()
        {
            this.options.ProtocolLogoutUrls.Add("/foo/signout");
            var resp = Post(Constants.RoutePaths.Logout, (string)null);
            var model = GetModel<LoggedOutViewModel>(resp);
            var signOutUrls = model.IFrameUrls.ToArray();
            Assert.AreEqual(2, signOutUrls.Length);
            CollectionAssert.Contains(signOutUrls, Url(Constants.RoutePaths.Oidc.EndSessionCallback));
            CollectionAssert.Contains(signOutUrls, Url("/foo/signout"));
        }

        [TestMethod]
        public void LoginExternalCallback_WithoutExternalCookie_RendersLoginPageWithError()
        {
            var msg = new SignInMessage();
            msg.IdP = "Google";
            var resp1 = GetLoginPage(msg);
            var resp2 = client.GetAsync(resp1.Headers.Location.AbsoluteUri).Result;
            var resp3 = Get(Constants.RoutePaths.LoginExternalCallback);
            AssertPage(resp3, "login");
            var model = GetModel<LoginViewModel>(resp3);
            Assert.AreEqual(Messages.NoMatchingExternalAccount, model.ErrorMessage);
        }

        [TestMethod]
        public void LoginExternalCallback_WithNoClaims_RendersLoginPageWithError()
        {
            var msg = new SignInMessage();
            msg.IdP = "Google";
            var resp1 = GetLoginPage(msg);

            SignInIdentity = new ClaimsIdentity(Constants.ExternalAuthenticationType);
            var resp2 = client.GetAsync(resp1.Headers.Location.AbsoluteUri).Result;
            client.SetCookies(resp2.GetCookies());

            var resp3 = Get(Constants.RoutePaths.LoginExternalCallback);
            AssertPage(resp3, "login");
            var model = GetModel<LoginViewModel>(resp3);
            Assert.AreEqual(Messages.NoMatchingExternalAccount, model.ErrorMessage);
        }
        
        [TestMethod]
        public void LoginExternalCallback_WithoutSubjectOrNameIdClaims_RendersLoginPageWithError()
        {
            var msg = new SignInMessage();
            msg.IdP = "Google";
            var resp1 = GetLoginPage(msg);

            SignInIdentity = new ClaimsIdentity(new Claim[]{new Claim("foo", "bar")}, Constants.ExternalAuthenticationType);
            var resp2 = client.GetAsync(resp1.Headers.Location.AbsoluteUri).Result;
            client.SetCookies(resp2.GetCookies());
            
            var resp3 = Get(Constants.RoutePaths.LoginExternalCallback);
            AssertPage(resp3, "login");
            var model = GetModel<LoginViewModel>(resp3);
            Assert.AreEqual(Messages.NoMatchingExternalAccount, model.ErrorMessage);
        }

        [TestMethod]
        public void LoginExternalCallback_WithValidSubjectClaim_IssuesAuthenticationCookie()
        {
            var msg = new SignInMessage();
            msg.IdP = "Google";
            msg.ReturnUrl = Url("authorize");
            var resp1 = GetLoginPage(msg);

            var sub = new Claim(Constants.ClaimTypes.Subject, "123", ClaimValueTypes.String, "Google");
            SignInIdentity = new ClaimsIdentity(new Claim[] { sub }, Constants.ExternalAuthenticationType);
            var resp2 = client.GetAsync(resp1.Headers.Location.AbsoluteUri).Result;
            client.SetCookies(resp2.GetCookies());

            var resp3 = Get(Constants.RoutePaths.LoginExternalCallback);
            resp3.AssertCookie(Constants.PrimaryAuthenticationType);
        }

        [TestMethod]
        public void LoginExternalCallback_WithValidNameIDClaim_IssuesAuthenticationCookie()
        {
            var msg = new SignInMessage();
            msg.IdP = "Google";
            msg.ReturnUrl = Url("authorize");
            var resp1 = GetLoginPage(msg);

            var sub = new Claim(ClaimTypes.NameIdentifier, "123", ClaimValueTypes.String, "Google");
            SignInIdentity = new ClaimsIdentity(new Claim[] { sub }, Constants.ExternalAuthenticationType);
            var resp2 = client.GetAsync(resp1.Headers.Location.AbsoluteUri).Result;
            client.SetCookies(resp2.GetCookies());

            var resp3 = Get(Constants.RoutePaths.LoginExternalCallback);
            resp3.AssertCookie(Constants.PrimaryAuthenticationType);
        }
        
        [TestMethod]
        public void LoginExternalCallback_WithValidSubjectClaim_RedirectsToAuthorizeEndpoint()
        {
            var msg = new SignInMessage();
            msg.IdP = "Google";
            msg.ReturnUrl = Url("authorize");
            var resp1 = GetLoginPage(msg);

            var sub = new Claim(Constants.ClaimTypes.Subject, "123", ClaimValueTypes.String, "Google");
            SignInIdentity = new ClaimsIdentity(new Claim[] { sub }, Constants.ExternalAuthenticationType);
            var resp2 = client.GetAsync(resp1.Headers.Location.AbsoluteUri).Result;
            client.SetCookies(resp2.GetCookies());

            var resp3 = Get(Constants.RoutePaths.LoginExternalCallback);
            Assert.AreEqual(HttpStatusCode.Found, resp3.StatusCode);
            Assert.AreEqual(Url("authorize"), resp3.Headers.Location.AbsoluteUri);
        }

        [TestMethod]
        public void LoginExternalCallback_UserServiceReturnsError_ShowsError()
        {
            mockUserService.Setup(x => x.AuthenticateExternalAsync(It.IsAny<string>(), It.IsAny<ExternalIdentity>()))
                .Returns(Task.FromResult(new ExternalAuthenticateResult("foo bad")));
            
            var msg = new SignInMessage();
            msg.IdP = "Google";
            msg.ReturnUrl = Url("authorize");
            var resp1 = GetLoginPage(msg);

            var sub = new Claim(Constants.ClaimTypes.Subject, "123", ClaimValueTypes.String, "Google");
            SignInIdentity = new ClaimsIdentity(new Claim[] { sub }, Constants.ExternalAuthenticationType);
            var resp2 = client.GetAsync(resp1.Headers.Location.AbsoluteUri).Result;
            client.SetCookies(resp2.GetCookies());

            var resp3 = Get(Constants.RoutePaths.LoginExternalCallback);
            AssertPage(resp3, "login");
            var model = GetModel<LoginViewModel>(resp3);
            Assert.AreEqual("foo bad", model.ErrorMessage);
        }

        [TestMethod]
        public void LoginExternalCallback_UserServiceReturnsNull_ShowError()
        {
            mockUserService.Setup(x => x.AuthenticateExternalAsync(It.IsAny<string>(), It.IsAny<ExternalIdentity>()))
                .Returns(Task.FromResult((ExternalAuthenticateResult)null));

            var msg = new SignInMessage();
            msg.IdP = "Google";
            msg.ReturnUrl = Url("authorize");
            var resp1 = GetLoginPage(msg);

            var sub = new Claim(Constants.ClaimTypes.Subject, "123", ClaimValueTypes.String, "Google");
            SignInIdentity = new ClaimsIdentity(new Claim[] { sub }, Constants.ExternalAuthenticationType);
            var resp2 = client.GetAsync(resp1.Headers.Location.AbsoluteUri).Result;
            client.SetCookies(resp2.GetCookies());

            var resp3 = Get(Constants.RoutePaths.LoginExternalCallback);
            AssertPage(resp3, "login");
            var model = GetModel<LoginViewModel>(resp3);
            Assert.AreEqual(Messages.NoMatchingExternalAccount, model.ErrorMessage);
        }

        [TestMethod]
        public void LoginExternalCallback_UserIsAnonymous_NoSubjectIsPassedToUserService()
        {
            var msg = new SignInMessage();
            msg.IdP = "Google";
            msg.ReturnUrl = Url("authorize");
            var resp1 = GetLoginPage(msg);

            var sub = new Claim(Constants.ClaimTypes.Subject, "123", ClaimValueTypes.String, "Google");
            SignInIdentity = new ClaimsIdentity(new Claim[] { sub }, Constants.ExternalAuthenticationType);
            var resp2 = client.GetAsync(resp1.Headers.Location.AbsoluteUri).Result;
            client.SetCookies(resp2.GetCookies());

            Get(Constants.RoutePaths.LoginExternalCallback);

            mockUserService.Verify(x => x.AuthenticateExternalAsync(null, It.IsAny<ExternalIdentity>()));
        }

        [TestMethod]
        public void LoginExternalCallback_UserIsAlreadyLoggedIn_SubjectIsPassedToUserService()
        {
            var msg = new SignInMessage();
            msg.IdP = "Google";
            msg.ReturnUrl = Url("authorize");

            var userSub = new Claim(Constants.ClaimTypes.Subject, "818727", ClaimValueTypes.String, Constants.BuiltInIdentityProvider);
            SignInIdentity = new ClaimsIdentity(new Claim[] { userSub }, Constants.PrimaryAuthenticationType);
            var resp1 = GetLoginPage(msg);

            var sub = new Claim(Constants.ClaimTypes.Subject, "123", ClaimValueTypes.String, "Google");
            SignInIdentity = new ClaimsIdentity(new Claim[] { sub }, Constants.ExternalAuthenticationType);
            var resp2 = client.GetAsync(resp1.Headers.Location.AbsoluteUri).Result;
            client.SetCookies(resp2.GetCookies());

            Get(Constants.RoutePaths.LoginExternalCallback);

            mockUserService.Verify(x => x.AuthenticateExternalAsync("818727", It.IsAny<ExternalIdentity>()));
        }
    }
}

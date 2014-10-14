/*
 * Copyright 2014 Dominick Baier, Brock Allen
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
using Microsoft.Owin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Authentication;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Plumbing;
using Thinktecture.IdentityServer.Core.Resources;
using Thinktecture.IdentityServer.Core.Views;

namespace Thinktecture.IdentityServer.Tests.Authentication
{
    [TestClass]
    public class AuthenticationControllerTests : IdSvrHostTestBase
    {
        public ClaimsIdentity SignInIdentity { get; set; }
        public string SignInId { get; set; }

        protected override void Postprocess(Microsoft.Owin.IOwinContext ctx)
        {
            if (SignInIdentity != null)
            {
                var props = new Microsoft.Owin.Security.AuthenticationProperties();
                props.Dictionary.Add("signin", SignInId);
                ctx.Authentication.SignIn(props, SignInIdentity);
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

        private string WriteMessageToCookie<T>(T msg)
            where T : class
        {
            var headers = new Dictionary<string, string[]>();
            var env = new Dictionary<string, object>()
            {
                {"owin.RequestScheme", "https"},
                {"owin.ResponseHeaders", headers}
            };

            var ctx = new OwinContext(env);
            var signInCookie = new MessageCookie<T>(ctx, this.options);
            var id = signInCookie.Write(msg);

            client.SetCookies(headers["Set-Cookie"]);
            
            return id;
        }

        private HttpResponseMessage GetLoginPage(SignInMessage msg = null)
        {
            msg = msg ?? new SignInMessage() { ReturnUrl = Url("authorize") };
            if (msg.ClientId == null) msg.ClientId = TestClients.Get().First().ClientId;

            SignInId = WriteMessageToCookie(msg);

            var resp = Get(Constants.RoutePaths.Login + "?signin=" + SignInId);
            return resp;
        }

        private string GetLoginUrl()
        {
            return Constants.RoutePaths.Login + "?signin=" + SignInId;
        }

        private string GetResumeUrlFromPartialSignInCookie(HttpResponseMessage resp)
        {
            var cookie = resp.GetCookies().Where(x=>x.Name == Constants.PartialSignInAuthenticationType).Single();
            var ticket = ticketFormatter.Unprotect(cookie.Value);
            var urlClaim = ticket.Identity.Claims.Single(x => x.Type == Constants.ClaimTypes.PartialLoginReturnUrl);
            return urlClaim.Value;
        }

        [TestMethod]
        public void GetLogin_WithSignInMessage_ReturnsLoginPage()
        {
            var resp = GetLoginPage();
            AssertPage(resp, "login");
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
        public void GetLogin_NoSignInMessage_ReturnNotFound()
        {
            var resp = Get(Constants.RoutePaths.Login);
            Assert.AreEqual(404, (int)resp.StatusCode);
        }

        [TestMethod]
        public void GetExternalLogin_ValidProvider_RedirectsToProvider()
        {
            var msg = new SignInMessage();
            msg.IdP = "Google";
            var resp1 = GetLoginPage(msg);

            var resp2 = client.GetAsync(resp1.Headers.Location.AbsoluteUri).Result;
            Assert.AreEqual(HttpStatusCode.Found, resp2.StatusCode);
            Assert.IsTrue(resp2.Headers.Location.AbsoluteUri.StartsWith("https://accounts.google.com"));
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
            var resp = Post(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            resp.AssertCookie(Constants.PrimaryAuthenticationType);
        }

        [TestMethod]
        public void PostToLogin_ValidCredentials_RedirectsBackToAuthorization()
        {
            GetLoginPage();
            var resp = Post(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            Assert.AreEqual(HttpStatusCode.Found, resp.StatusCode);
            Assert.AreEqual(resp.Headers.Location, Url("authorize"));
        }

        [TestMethod]
        public void PostToLogin_NoModel_ShowErrorPage()
        {
            GetLoginPage();
            var resp = Post(GetLoginUrl(), (LoginCredentials)null);
            AssertPage(resp, "login");
            var model = GetModel<LoginViewModel>(resp);
            Assert.AreEqual(model.ErrorMessage, Messages.InvalidUsernameOrPassword);
        }

        [TestMethod]
        public void PostToLogin_InvalidUsername_ShowErrorPage()
        {
            GetLoginPage();
            var resp = Post(GetLoginUrl(), new LoginCredentials { Username = "bad", Password = "alice" });
            AssertPage(resp, "login");
            var model = GetModel<LoginViewModel>(resp);
            Assert.AreEqual(model.ErrorMessage, Messages.InvalidUsernameOrPassword);
        }

        [TestMethod]
        public void PostToLogin_InvalidPassword_ShowErrorPage()
        {
            GetLoginPage();
            var resp = Post(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "bad" });
            AssertPage(resp, "login");
            var model = GetModel<LoginViewModel>(resp);
            Assert.AreEqual(model.ErrorMessage, Messages.InvalidUsernameOrPassword);
        }

        [TestMethod]
        public void PostToLogin_UserServiceReturnsError_ShowErrorPage()
        {
            mockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SignInMessage>()))
                .Returns(Task.FromResult(new AuthenticateResult("bad stuff")));

            GetLoginPage();
            var resp = Post(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            AssertPage(resp, "login");
            var model = GetModel<LoginViewModel>(resp);
            Assert.AreEqual(model.ErrorMessage, "bad stuff");
        }

        [TestMethod]
        public void PostToLogin_UserServiceReturnsNull_ShowErrorPage()
        {
            mockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SignInMessage>()))
                .Returns(Task.FromResult((AuthenticateResult)null));

            GetLoginPage();
            var resp = Post(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            AssertPage(resp, "login");
            var model = GetModel<LoginViewModel>(resp);
            Assert.AreEqual(model.ErrorMessage, Messages.InvalidUsernameOrPassword);
        }

        [TestMethod]
        public void PostToLogin_UserServiceReturnsParialLogin_IssuesPartialLoginCookie()
        {
            mockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SignInMessage>()))
                .Returns(Task.FromResult(new AuthenticateResult("/foo", IdentityServerPrincipal.Create("tempsub", "tempname"))));

            GetLoginPage();
            var resp = Post(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            resp.AssertCookie(Constants.PartialSignInAuthenticationType);
        }

        [TestMethod]
        public void PostToLogin_UserServiceReturnsParialLogin_IssuesRedirect()
        {
            mockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SignInMessage>()))
                .Returns(Task.FromResult(new AuthenticateResult("/foo", IdentityServerPrincipal.Create("tempsub", "tempname"))));

            GetLoginPage();
            var resp = Post(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            Assert.AreEqual(HttpStatusCode.Found, resp.StatusCode);
            Assert.AreEqual(Url("foo"), resp.Headers.Location.AbsoluteUri);
        }

        [TestMethod]
        public void PostToLogin_CookieOptions_AllowRememberMeIsFalse_IsPersistentIsFalse_DoesNotIssuePersistentCookie()
        {
            this.options.AuthenticationOptions.CookieOptions.AllowRememberMe = false;
            this.options.AuthenticationOptions.CookieOptions.IsPersistent = false;
            GetLoginPage();
            var resp = Post(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            var cookies = resp.GetRawCookies();
            var cookie = cookies.Single(x => x.StartsWith(Constants.PrimaryAuthenticationType + "="));
            Assert.IsFalse(cookie.Contains("expires="));
        }

        [TestMethod]
        public void PostToLogin_CookieOptions_AllowRememberMeIsFalse_IsPersistentIsTrue_IssuesPersistentCookie()
        {
            this.options.AuthenticationOptions.CookieOptions.AllowRememberMe = false;
            this.options.AuthenticationOptions.CookieOptions.IsPersistent = true;
            GetLoginPage();
            var resp = Post(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            var cookies = resp.GetRawCookies();
            var cookie = cookies.Single(x => x.StartsWith(Constants.PrimaryAuthenticationType + "="));
            Assert.IsTrue(cookie.Contains("expires="));
        }

        [TestMethod]
        public void PostToLogin_CookieOptions_AllowRememberMeIsTrue_IsPersistentIsTrue_DoNotCheckRememberMe_DoeNotIssuePersistentCookie()
        {
            this.options.AuthenticationOptions.CookieOptions.AllowRememberMe = true;
            this.options.AuthenticationOptions.CookieOptions.IsPersistent = true;
            GetLoginPage();
            var resp = Post(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            var cookies = resp.GetRawCookies();
            var cookie = cookies.Single(x => x.StartsWith(Constants.PrimaryAuthenticationType + "="));
            Assert.IsFalse(cookie.Contains("expires="));
        }
        [TestMethod]
        public void PostToLogin_CookieOptions_AllowRememberMeIsTrue_IsPersistentIsTrue_CheckRememberMe_IssuesPersistentCookie()
        {
            this.options.AuthenticationOptions.CookieOptions.AllowRememberMe = true;
            this.options.AuthenticationOptions.CookieOptions.IsPersistent = true;
            GetLoginPage();
            var resp = Post(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice", RememberMe = true });
            var cookies = resp.GetRawCookies();
            var cookie = cookies.Single(x => x.StartsWith(Constants.PrimaryAuthenticationType + "="));
            Assert.IsTrue(cookie.Contains("expires="));
        }

        [TestMethod]
        public void PostToLogin_CookieOptionsIsPersistentIsTrueButResponseIsPartialLogin_DoesNotIssuePersistentCookie()
        {
            mockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SignInMessage>()))
                .Returns(Task.FromResult(new AuthenticateResult("/foo", IdentityServerPrincipal.Create("tempsub", "tempname"))));
            
            this.options.AuthenticationOptions.CookieOptions.IsPersistent = true;
            GetLoginPage();
            var resp = Post(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            var cookies = resp.GetRawCookies();
            var cookie = cookies.Single(x => x.StartsWith(Constants.PartialSignInAuthenticationType + "="));
            Assert.IsFalse(cookie.Contains("expires="));
        }

        [TestMethod]
        public void ResumeLoginFromRedirect_WithPartialCookie_IssuesFullLoginCookie()
        {
            mockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SignInMessage>()))
                .Returns(Task.FromResult(new AuthenticateResult("/foo", IdentityServerPrincipal.Create("tempsub", "tempname"))));

            GetLoginPage();
            var resp1 = Post(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            client.SetCookies(resp1.GetCookies());
            var resp2 = Get(GetResumeUrlFromPartialSignInCookie(resp1));
            resp2.AssertCookie(Constants.PrimaryAuthenticationType);
        }

        [TestMethod]
        public void ResumeLoginFromRedirect_WithPartialCookie_IssuesRedirectToAuthorizationPage()
        {
            mockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SignInMessage>()))
                .Returns(Task.FromResult(new AuthenticateResult("/foo", IdentityServerPrincipal.Create("tempsub", "tempname"))));

            GetLoginPage();
            var resp1 = Post(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            client.SetCookies(resp1.GetCookies());

            var resp2 = Get(GetResumeUrlFromPartialSignInCookie(resp1));
            Assert.AreEqual(HttpStatusCode.Found, resp2.StatusCode);
            Assert.AreEqual(Url("authorize"), resp2.Headers.Location.AbsoluteUri);
        }

        [TestMethod]
        public void ResumeLoginFromRedirect_WithoutPartialCookie_ShowsError()
        {
            mockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SignInMessage>()))
                .Returns(Task.FromResult(new AuthenticateResult("/foo", IdentityServerPrincipal.Create("tempsub", "tempname"))));

            GetLoginPage();
            var resp1 = Post(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            var resp2 = Get(GetResumeUrlFromPartialSignInCookie(resp1));
            AssertPage(resp2, "error");
        }

        [TestMethod]
        public void Logout_ShowsLogoutPromptPage()
        {
            var resp = Get(Constants.RoutePaths.Logout);
            AssertPage(resp, "logout");
        }
        
        [TestMethod]
        public void Logout_DisableSignOutPrompt_SkipsLogoutPromptPage()
        {
            this.options.AuthenticationOptions.DisableSignOutPrompt = true;
            var resp = Get(Constants.RoutePaths.Logout);
            AssertPage(resp, "loggedOut");
        }

        [TestMethod]
        public void PostToLogout_RemovesCookies()
        {
            var resp = Post(Constants.RoutePaths.Logout, (string)null);
            var cookies = resp.Headers.GetValues("Set-Cookie");
            Assert.AreEqual(3, cookies.Count());
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
        public void LoginExternalCallback_WithoutExternalCookie_RendersErrorPage()
        {
            var msg = new SignInMessage();
            msg.IdP = "Google";
            var resp1 = GetLoginPage(msg);
            var resp2 = client.GetAsync(resp1.Headers.Location.AbsoluteUri).Result;
            var resp3 = Get(Constants.RoutePaths.LoginExternalCallback);
            AssertPage(resp3, "error");
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
            mockUserService.Setup(x => x.AuthenticateExternalAsync(It.IsAny<ExternalIdentity>()))
                .Returns(Task.FromResult(new AuthenticateResult("foo bad")));
            
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
            mockUserService.Setup(x => x.AuthenticateExternalAsync(It.IsAny<ExternalIdentity>()))
                .Returns(Task.FromResult((AuthenticateResult)null));

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

            mockUserService.Verify(x => x.AuthenticateExternalAsync(It.IsAny<ExternalIdentity>()));
        }

        [TestMethod]
        public void LogoutPrompt_WithSignOutMessage_ContainsClientNameInPage()
        {
            var c = TestClients.Get().First();
            var msg = new SignOutMessage
            {
                ClientId = c.ClientId,
                ReturnUrl = "http://foo"
            };
            var id = WriteMessageToCookie(msg);
            var resp = Get(Constants.RoutePaths.Logout + "?id=" + id);
            var model = GetModel<LogoutViewModel>(resp);
            Assert.AreEqual(c.ClientName, model.ClientName);
        }
        
        [TestMethod]
        public void LogoutPrompt_NoSignOutMessage_ContainsNullClientNameInPage()
        {
            var resp = Get(Constants.RoutePaths.Logout);
            var model = GetModel<LogoutViewModel>(resp);
            Assert.IsNull(model.ClientName);
        }

        [TestMethod]
        public void LogoutPrompt_InvalidSignOutMessageId_ContainsNullClientNameInPage()
        {
            var resp = Get(Constants.RoutePaths.Logout + "?id=123");
            var model = GetModel<LogoutViewModel>(resp);
            Assert.IsNull(model.ClientName);
        }

        [TestMethod]
        public void LoggedOut_WithSignOutMessage_ContainsClientNameAndRedirectUrlInPage()
        {
            var c = TestClients.Get().First();
            var msg = new SignOutMessage
            {
                ClientId = c.ClientId,
                ReturnUrl = "http://foo"
            };
            var id = WriteMessageToCookie(msg);
            var resp = client.PostAsync(Url(Constants.RoutePaths.Logout + "?id=" + id), null).Result;
            var model = GetModel<LoggedOutViewModel>(resp);
            Assert.AreEqual(msg.ReturnUrl, model.RedirectUrl);
            Assert.AreEqual(c.ClientName, model.ClientName);
        }

        [TestMethod]
        public void LoggedOut_NoSignOutMessage_ContainsNullForClientNameAndRedirectUrlInPage()
        {
            var resp = client.PostAsync(Url(Constants.RoutePaths.Logout), null).Result;
            var model = GetModel<LoggedOutViewModel>(resp);
            Assert.IsNull(model.RedirectUrl);
            Assert.IsNull(model.ClientName);
        }

        [TestMethod]
        public void LoggedOut_InvalidSignOutMessageId_ContainsNullForClientNameAndRedirectUrlInPage()
        {
            var resp = client.PostAsync(Url(Constants.RoutePaths.Logout + "?id=123"), null).Result;
            var model = GetModel<LoggedOutViewModel>(resp);
            Assert.IsNull(model.RedirectUrl);
            Assert.IsNull(model.ClientName);
        }
    }
}

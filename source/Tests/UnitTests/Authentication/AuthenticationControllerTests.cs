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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Resources;
using Thinktecture.IdentityServer.Core.ViewModels;

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
                if(SignInIdentity.AuthenticationType == Constants.ExternalAuthenticationType)
                {
                    props.Dictionary.Add("katanaAuthenticationType", "Google");
                }
                ctx.Authentication.SignIn(props, SignInIdentity);
                SignInIdentity = null;
            }
        }

        private HttpResponseMessage GetLoginPage(SignInMessage msg = null)
        {
            msg = msg ?? new SignInMessage() { ReturnUrl = Url("authorize") };
            if (msg.ClientId == null) msg.ClientId = TestClients.Get().First().ClientId;

            SignInId = WriteMessageToCookie(msg);

            var resp = Get(Constants.RoutePaths.Login + "?signin=" + SignInId);
            ProcessXsrf(resp);
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
            resp.AssertPage("login");
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
            Assert.AreEqual(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [TestMethod]
        public void GetLogin_PreAuthenticateReturnsNull_ShowsLoginPage()
        {
            var resp = GetLoginPage();
            resp.AssertPage("login");
        }

        [TestMethod]
        public void GetLogin_PreAuthenticateReturnsError_ShowsErrorPage()
        {
            mockUserService
                .Setup(x => x.PreAuthenticateAsync(It.IsAny<IDictionary<string, object>>(), It.IsAny<SignInMessage>()))
                .ReturnsAsync(new AuthenticateResult("SomeError"));

            var resp = GetLoginPage();
            resp.AssertPage("error");
            var model = resp.GetModel<ErrorViewModel>();
            Assert.AreEqual("SomeError", model.ErrorMessage);
        }

        [TestMethod]
        public void GetLogin_PreAuthenticateReturnsFullLogin_IssuesLoginCookie()
        {
            mockUserService
                .Setup(x => x.PreAuthenticateAsync(It.IsAny<IDictionary<string, object>>(), It.IsAny<SignInMessage>()))
                .Returns(Task.FromResult(new AuthenticateResult(IdentityServerPrincipal.Create("sub", "name"))));

            var resp = GetLoginPage();
            resp.AssertCookie(Constants.PrimaryAuthenticationType);
        }

        [TestMethod]
        public void GetLogin_PreAuthenticateReturnsFullLogin_RedirectsToReturnUrl()
        {
            mockUserService
                .Setup(x => x.PreAuthenticateAsync(It.IsAny<IDictionary<string, object>>(), It.IsAny<SignInMessage>()))
                .Returns(Task.FromResult(new AuthenticateResult(IdentityServerPrincipal.Create("sub", "name"))));

            var resp = GetLoginPage();
            Assert.AreEqual(HttpStatusCode.Found, resp.StatusCode);
            Assert.AreEqual(Url("authorize"), resp.Headers.Location.AbsoluteUri);
        }

        [TestMethod]
        public void GetLogin_PreAuthenticateReturnsParialLogin_IssuesPartialLoginCookie()
        {
            mockUserService
                .Setup(x => x.PreAuthenticateAsync(It.IsAny<IDictionary<string, object>>(), It.IsAny<SignInMessage>()))
                .Returns(Task.FromResult(new AuthenticateResult("/foo", IdentityServerPrincipal.Create("tempsub", "tempname"))));

            var resp = GetLoginPage();
            resp.AssertCookie(Constants.PartialSignInAuthenticationType);
        }

        [TestMethod]
        public void GetLogin_PreAuthenticateReturnsParialLogin_IssuesRedirect()
        {
            mockUserService
                .Setup(x => x.PreAuthenticateAsync(It.IsAny<IDictionary<string, object>>(), It.IsAny<SignInMessage>()))
                .Returns(Task.FromResult(new AuthenticateResult("/foo", IdentityServerPrincipal.Create("tempsub", "tempname"))));

            var resp = GetLoginPage();
            Assert.AreEqual(HttpStatusCode.Found, resp.StatusCode);
            Assert.AreEqual(Url("foo"), resp.Headers.Location.AbsoluteUri);
        }

        [TestMethod]
        public void GetLogin_EnableLocalLoginAndOnlyOneProvider_RedirectsToProvider()
        {
            options.AuthenticationOptions.EnableLocalLogin = false;
            var resp = GetLoginPage();
            Assert.AreEqual(HttpStatusCode.Found, resp.StatusCode);
            Assert.IsTrue(resp.Headers.Location.AbsoluteUri.StartsWith(Url(Constants.RoutePaths.LoginExternal) + "?provider=Google"));
        }

        [TestMethod]
        public void GetLogin_EnableLocalLoginMoreThanOneProvider_ShowsLoginPage()
        {
            Action<IAppBuilder, string> config = (app, name)=>{
                base.ConfigureAdditionalIdentityProviders(app, name);
                var google = new Microsoft.Owin.Security.Google.GoogleOAuth2AuthenticationOptions
                {
                    AuthenticationType = "Google2",
                    Caption = "Google2",
                    SignInAsAuthenticationType = Constants.ExternalAuthenticationType,
                    ClientId = "foo",
                    ClientSecret = "bar"
                };
                app.UseGoogleAuthentication(google);
            };
            OverrideIdentityProviderConfiguration = config;
            this.Init();
            
            options.AuthenticationOptions.EnableLocalLogin = false;
            var resp = GetLoginPage();
            resp.AssertPage("login");
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
        public void GetExternalLogin_InvalidProvider_ReturnsUnauthorized()
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
            var resp = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            resp.AssertCookie(Constants.PrimaryAuthenticationType);
        }

        [TestMethod]
        public void PostToLogin_ValidCredentials_RedirectsBackToAuthorization()
        {
            GetLoginPage();
            var resp = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            Assert.AreEqual(HttpStatusCode.Found, resp.StatusCode);
            Assert.AreEqual(resp.Headers.Location, Url("authorize"));
        }

        [TestMethod]
        public void PostToLogin_NoModel_ShowErrorPage()
        {
            GetLoginPage();
            var resp = PostForm(GetLoginUrl(), null);
            resp.AssertPage("login");
            var model = resp.GetModel<LoginViewModel>();
            Assert.AreEqual(model.ErrorMessage, Messages.UsernameRequired);
        }

        [TestMethod]
        public void PostToLogin_InvalidUsername_ShowErrorPage()
        {
            GetLoginPage();
            var resp = PostForm(GetLoginUrl(), new LoginCredentials { Username = "bad", Password = "alice" });
            resp.AssertPage("login");
            var model = resp.GetModel<LoginViewModel>();
            Assert.AreEqual(model.ErrorMessage, Messages.InvalidUsernameOrPassword);
        }

        [TestMethod]
        public void PostToLogin_InvalidPassword_ShowErrorPage()
        {
            GetLoginPage();
            var resp = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "bad" });
            resp.AssertPage("login");
            var model = resp.GetModel<LoginViewModel>();
            Assert.AreEqual(model.ErrorMessage, Messages.InvalidUsernameOrPassword);
        }

        [TestMethod]
        public void PostToLogin_UserServiceReturnsError_ShowErrorPage()
        {
            mockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SignInMessage>()))
                .Returns(Task.FromResult(new AuthenticateResult("bad stuff")));

            GetLoginPage();
            var resp = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            resp.AssertPage("login");
            var model = resp.GetModel<LoginViewModel>();
            Assert.AreEqual(model.ErrorMessage, "bad stuff");
        }

        [TestMethod]
        public void PostToLogin_UserServiceReturnsNull_ShowErrorPage()
        {
            mockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SignInMessage>()))
                .Returns(Task.FromResult((AuthenticateResult)null));

            GetLoginPage();
            var resp = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            resp.AssertPage("login");
            var model = resp.GetModel<LoginViewModel>();
            Assert.AreEqual(model.ErrorMessage, Messages.InvalidUsernameOrPassword);
        }

        [TestMethod]
        public void PostToLogin_UserServiceReturnsParialLogin_IssuesPartialLoginCookie()
        {
            mockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SignInMessage>()))
                .Returns(Task.FromResult(new AuthenticateResult("/foo", IdentityServerPrincipal.Create("tempsub", "tempname"))));

            GetLoginPage();
            var resp = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            resp.AssertCookie(Constants.PartialSignInAuthenticationType);
        }

        [TestMethod]
        public void PostToLogin_UserServiceReturnsParialLogin_IssuesRedirect()
        {
            mockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SignInMessage>()))
                .Returns(Task.FromResult(new AuthenticateResult("/foo", IdentityServerPrincipal.Create("tempsub", "tempname"))));

            GetLoginPage();
            var resp = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            Assert.AreEqual(HttpStatusCode.Found, resp.StatusCode);
            Assert.AreEqual(Url("foo"), resp.Headers.Location.AbsoluteUri);
        }

        [TestMethod]
        public void PostToLogin_CookieOptions_AllowRememberMeIsFalse_IsPersistentIsFalse_DoesNotIssuePersistentCookie()
        {
            this.options.AuthenticationOptions.CookieOptions.AllowRememberMe = false;
            this.options.AuthenticationOptions.CookieOptions.IsPersistent = false;
            GetLoginPage();
            var resp = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
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
            var resp = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
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
            var resp = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
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
            var resp = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice", RememberMe = true });
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
            var resp = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
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
            var resp1 = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
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
            var resp1 = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
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
            var resp1 = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            var resp2 = Get(GetResumeUrlFromPartialSignInCookie(resp1));
            resp2.AssertPage("error");
        }

        [TestMethod]
        public void Logout_ShowsLogoutPromptPage()
        {
            var resp = Get(Constants.RoutePaths.Logout);
            resp.AssertPage("logout");
        }
        
        [TestMethod]
        public void Logout_DisableSignOutPrompt_SkipsLogoutPromptPage()
        {
            this.options.AuthenticationOptions.DisableSignOutPrompt = true;
            var resp = Get(Constants.RoutePaths.Logout);
            resp.AssertPage("loggedOut");
        }

        [TestMethod]
        public void PostToLogout_RemovesCookies()
        {
            GetLoginPage();
            var resp = PostForm(Constants.RoutePaths.Logout, (string)null);
            var cookies = resp.Headers.GetValues("Set-Cookie");
            // 4: primary, partial, external, signin
            Assert.AreEqual(4, cookies.Count());
            // GetCookies will not return values for cookies that are expired/revoked
            Assert.AreEqual(0, resp.GetCookies().Count());
        }
        
        [TestMethod]
        public void PostToLogout_EmitsLogoutUrlsForProtocolIframes()
        {
            GetLoginPage();
            this.options.ProtocolLogoutUrls.Add("/foo/signout");
            var resp = PostForm(Constants.RoutePaths.Logout, (string)null);
            var model = resp.GetModel<LoggedOutViewModel>();
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
            resp3.AssertPage("error");
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
            resp3.AssertPage("login");
            var model = resp3.GetModel<LoginViewModel>();
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
            resp3.AssertPage("login");
            var model = resp3.GetModel<LoginViewModel>();
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
            resp3.AssertPage("login");
            var model = resp3.GetModel<LoginViewModel>();
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
            resp3.AssertPage("login");
            var model = resp3.GetModel<LoginViewModel>();
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
            var model = resp.GetModel<LogoutViewModel>();
            Assert.AreEqual(c.ClientName, model.ClientName);
        }
        
        [TestMethod]
        public void LogoutPrompt_NoSignOutMessage_ContainsNullClientNameInPage()
        {
            var resp = Get(Constants.RoutePaths.Logout);
            var model = resp.GetModel<LogoutViewModel>();
            Assert.IsNull(model.ClientName);
        }

        [TestMethod]
        public void LogoutPrompt_InvalidSignOutMessageId_ContainsNullClientNameInPage()
        {
            var resp = Get(Constants.RoutePaths.Logout + "?id=123");
            var model = resp.GetModel<LogoutViewModel>();
            Assert.IsNull(model.ClientName);
        }

        [TestMethod]
        public void LoggedOut_WithSignOutMessage_ContainsClientNameAndRedirectUrlInPage()
        {
            GetLoginPage();
            var c = TestClients.Get().First();
            var msg = new SignOutMessage
            {
                ClientId = c.ClientId,
                ReturnUrl = "http://foo"
            };
            var id = WriteMessageToCookie(msg);
            var resp = PostForm(Url(Constants.RoutePaths.Logout + "?id=" + id), null);
            var model = resp.GetModel<LoggedOutViewModel>();
            Assert.AreEqual(msg.ReturnUrl, model.RedirectUrl);
            Assert.AreEqual(c.ClientName, model.ClientName);
        }

        [TestMethod]
        public void LoggedOut_NoSignOutMessage_ContainsNullForClientNameAndRedirectUrlInPage()
        {
            GetLoginPage();
            var resp = PostForm(Url(Constants.RoutePaths.Logout), null);
            var model = resp.GetModel<LoggedOutViewModel>();
            Assert.IsNull(model.RedirectUrl);
            Assert.IsNull(model.ClientName);
        }

        [TestMethod]
        public void LoggedOut_InvalidSignOutMessageId_ContainsNullForClientNameAndRedirectUrlInPage()
        {
            GetLoginPage();
            var resp = PostForm(Url(Constants.RoutePaths.Logout + "?id=123"), null);
            var model = resp.GetModel<LoggedOutViewModel>();
            Assert.IsNull(model.RedirectUrl);
            Assert.IsNull(model.ClientName);
        }

        [TestMethod]
        public void Login_ClientFiltersAllowedIdentityProviders_OnlyAllowedIdPsInLoginPage()
        {
            var msg = new SignInMessage() { ReturnUrl = Url("authorize"), ClientId = "no_external_idps" };
            var resp = GetLoginPage(msg);
            var model = resp.GetModel<LoginViewModel>();
            var google = model.ExternalProviders.SingleOrDefault(x => x.Text == "Google");
            Assert.IsNull(google);
        }
        
        [TestMethod]
        public void Login_ClientDoesNotFiltersAllowedIdentityProviders_ExternalIsInLoginPage()
        {
            var msg = new SignInMessage() { ReturnUrl = Url("authorize"), ClientId = "any_external_idps" };
            var resp = GetLoginPage(msg);
            var model = resp.GetModel<LoginViewModel>();
            var google = model.ExternalProviders.SingleOrDefault(x => x.Text == "Google");
            Assert.IsNotNull(google);
        }

        [TestMethod]
        public void Login_InvalidClientId_ShowsErrorPage()
        {
            var msg = new SignInMessage() { ReturnUrl = Url("authorize"), ClientId = "bad_id" };
            var resp = GetLoginPage(msg);
            resp.AssertPage("error");
        }

        [TestMethod]
        public void Login_PostWithJson_ReturnsUnsupportedMediaType()
        {
            GetLoginPage();
            var resp = Post(GetLoginUrl(), (object)null);
            Assert.AreEqual(HttpStatusCode.UnsupportedMediaType, resp.StatusCode);
        }

        [TestMethod]
        public void Login_PostWithoutXsrf_ReturnsError()
        {
            var resp = PostForm(Url(Constants.RoutePaths.Login + "?signin="), (object)null);
            resp.AssertPage("error");
        }

        [TestMethod]
        public void Logout_PostWithoutXsrf_ReturnsError()
        {
            var resp = PostForm(Url(Constants.RoutePaths.Logout), (object)null);
            resp.AssertPage("error");
        }
    }
}

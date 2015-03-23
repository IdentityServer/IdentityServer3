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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Google;
using Moq;
using Owin;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Endpoints;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Resources;
using Thinktecture.IdentityServer.Core.ViewModels;
using Thinktecture.IdentityServer.Tests.Endpoints.Setup;
using Xunit;
using AuthenticateResult = Thinktecture.IdentityServer.Core.Models.AuthenticateResult;

namespace Thinktecture.IdentityServer.Tests.Endpoints
{
    public class AuthenticationControllerTests : IdSvrHostTestBase
    {
        public ClaimsIdentity SignInIdentity { get; set; }
        public string SignInId { get; set; }

        protected override void Postprocess(IOwinContext ctx)
        {
            if (SignInIdentity != null)
            {
                var props = new AuthenticationProperties();
                props.Dictionary.Add(Constants.Authentication.SIGNIN_ID, SignInId);
                if(SignInIdentity.AuthenticationType == Constants.EXTERNAL_AUTHENTICATION_TYPE)
                {
                    props.Dictionary.Add(Constants.Authentication.KATANA_AUTHENTICATION_TYPE, "Google");
                }
                ctx.Authentication.SignIn(props, SignInIdentity);
                SignInIdentity = null;
            }
        }

        private HttpResponseMessage GetLoginPage(SignInMessage msg = null)
        {
            msg = msg ?? new SignInMessage { ReturnUrl = Url("authorize") };
            if (msg.ClientId == null) msg.ClientId = TestClients.Get().First().ClientId;

            SignInId = WriteMessageToCookie(msg);

            var resp = Get(Constants.RoutePaths.LOGIN + "?signin=" + SignInId);
            ProcessXsrf(resp);
            return resp;
        }

        private string GetLoginUrl()
        {
            return Constants.RoutePaths.LOGIN + "?signin=" + SignInId;
        }

        void Login(bool setCookie = true)
        {
            var msg = new SignInMessage { ReturnUrl = Url("authorize") };
            var signInId = WriteMessageToCookie(msg);
            var url = Constants.RoutePaths.LOGIN + "?signin=" + signInId;
            var resp = Get(url);
            ProcessXsrf(resp);

            if (setCookie)
            {
                resp = PostForm(url, new LoginCredentials { Username = "alice", Password = "alice" });
                Client.SetCookies(resp.GetCookies());
            }
        }

        private string GetResumeUrlFromPartialSignInCookie(HttpResponseMessage resp)
        {
            var cookie = resp.GetCookies().Single(x => x.Name == Constants.PARTIAL_SIGN_IN_AUTHENTICATION_TYPE);
            var ticket = TicketFormatter.Unprotect(cookie.Value);
            var urlClaim = ticket.Identity.Claims.Single(x => x.Type == Constants.ClaimTypes.PARTIAL_LOGIN_RETURN_URL);
            return urlClaim.Value;
        }

        [Fact]
        public void GetLogin_WithSignInMessage_ReturnsLoginPage()
        {
            var resp = GetLoginPage();
            resp.AssertPage("login");
        }

        [Fact]
        public void GetLogin_SignInMessageHasIdentityProvider_RedirectsToExternalProviderLogin()
        {
            var msg = new SignInMessage {IdP = "Google"};
            var resp = GetLoginPage(msg);

            resp.StatusCode.Should().Be(HttpStatusCode.Found);
            var expected = new Uri(Url(Constants.RoutePaths.LOGIN_EXTERNAL));
            resp.Headers.Location.AbsolutePath.Should().Be(expected.AbsolutePath);
            resp.Headers.Location.Query.Should().Contain("provider=Google");
        }

        [Fact]
        public void GetLogin_SignInMessageHasLoginHint_UsernameIsPopulatedFromLoginHint()
        {
            Options.AuthenticationOptions.EnableLoginHint = true;

            var msg = new SignInMessage {LoginHint = "test"};

            var resp = GetLoginPage(msg);

            var model = resp.GetModel<LoginViewModel>();
            model.Username.Should().Be("test");
        }

        [Fact]
        public void GetLogin_EnableLoginHintFalse_UsernameIsNotPopulatedFromLoginHint()
        {
            Options.AuthenticationOptions.EnableLoginHint = false;

            var msg = new SignInMessage {LoginHint = "test"};

            var resp = GetLoginPage(msg);

            var model = resp.GetModel<LoginViewModel>();
            model.Username.Should().BeNull();
        }

        [Fact]
        public void PostToLogin_SignInMessageHasLoginHint_UsernameShouldBeUsernamePosted()
        {
            var msg = new SignInMessage {LoginHint = "test"};

            var resp = GetLoginPage(msg);
            var model = resp.GetModel<LoginViewModel>();
            resp = PostForm(model.LoginUrl, new LoginCredentials { Username = "alice", Password = "jdfhjkdf" });
            model = resp.GetModel<LoginViewModel>();
            model.Username.Should().Be("alice");
        }

        [Fact]
        public void GetLogin_NoSignInMessage_ReturnNotFound()
        {
            var resp = Get(Constants.RoutePaths.LOGIN);
            resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public void GetLogin_PreAuthenticateReturnsNull_ShowsLoginPage()
        {
            var resp = GetLoginPage();
            resp.AssertPage("login");
        }

        [Fact]
        public void GetLogin_PreAuthenticateReturnsError_ShowsErrorPage()
        {
            MockUserService
                .Setup(x => x.PreAuthenticateAsync(It.IsAny<SignInMessage>()))
                .ReturnsAsync(new AuthenticateResult("SomeError"));

            var resp = GetLoginPage();
            resp.AssertPage("error");
            var model = resp.GetModel<ErrorViewModel>();
            model.ErrorMessage.Should().Be("SomeError");
        }

        [Fact]
        public void GetLogin_PreAuthenticateReturnsFullLogin_IssuesLoginCookie()
        {
            MockUserService
                .Setup(x => x.PreAuthenticateAsync(It.IsAny<SignInMessage>()))
                .Returns(Task.FromResult(new AuthenticateResult(IdentityServerPrincipal.Create("sub", "name"))));

            var resp = GetLoginPage();
            resp.AssertCookie(Constants.PRIMARY_AUTHENTICATION_TYPE);
        }

        [Fact]
        public void GetLogin_PreAuthenticateReturnsFullLogin_RedirectsToReturnUrl()
        {
            MockUserService
                .Setup(x => x.PreAuthenticateAsync(It.IsAny<SignInMessage>()))
                .Returns(Task.FromResult(new AuthenticateResult(IdentityServerPrincipal.Create("sub", "name"))));

            var resp = GetLoginPage();
            resp.StatusCode.Should().Be(HttpStatusCode.Found);
            resp.Headers.Location.AbsoluteUri.Should().Be(Url("authorize"));
        }

        [Fact]
        public void GetLogin_PreAuthenticateReturnsParialLogin_IssuesPartialLoginCookie()
        {
            MockUserService
                .Setup(x => x.PreAuthenticateAsync(It.IsAny<SignInMessage>()))
                .Returns(Task.FromResult(new AuthenticateResult("/foo", "tempsub", "tempname")));

            var resp = GetLoginPage();
            resp.AssertCookie(Constants.PARTIAL_SIGN_IN_AUTHENTICATION_TYPE);
        }

        [Fact]
        public void GetLogin_PreAuthenticateReturnsParialLogin_IssuesRedirect()
        {
            MockUserService
                .Setup(x => x.PreAuthenticateAsync(It.IsAny<SignInMessage>()))
                .Returns(Task.FromResult(new AuthenticateResult("/foo", "tempsub", "tempname")));

            var resp = GetLoginPage();
            resp.StatusCode.Should().Be(HttpStatusCode.Found);
            resp.Headers.Location.AbsoluteUri.Should().Be(Url("foo"));
        }

        [Fact]
        public void GetLogin_NoLocalLogin_NoExternalProviders_ShowsErrorPage()
        {
            ConfigureIdentityServerOptions = opts =>
            {
                opts.AuthenticationOptions.EnableLocalLogin = false;
                opts.AuthenticationOptions.IdentityProviders = null;
            };
            Init();

            var resp = GetLoginPage();
            resp.AssertPage("error");
            var model = resp.GetModel<ErrorViewModel>();
            model.ErrorMessage.Should().Be(Messages.UnexpectedError);
        }

        [Fact]
        public void GetLogin_PublicHostNameConfigured_PreAuthenticateReturnsParialLogin_IssuesRedirectToCustomHost()
        {
            ConfigureIdentityServerOptions = opts =>
            {
                opts.PublicOrigin = "http://somehost";
            };
            Init();

            MockUserService
                .Setup(x => x.PreAuthenticateAsync(It.IsAny<SignInMessage>()))
                .Returns(Task.FromResult(new AuthenticateResult("/foo", "tempsub", "tempname")));

            var resp = GetLoginPage();
            resp.StatusCode.Should().Be(HttpStatusCode.Found);
            resp.Headers.Location.AbsoluteUri.Should().Be("http://somehost/foo");
        }

        [Fact]
        public void GetLogin_DisableLocalLoginAndOnlyOneProvider_RedirectsToProvider()
        {
            Google2.Caption = null;
            Options.AuthenticationOptions.EnableLocalLogin = false;
            var resp = GetLoginPage();
            resp.StatusCode.Should().Be(HttpStatusCode.Found);
            resp.Headers.Location.AbsoluteUri.StartsWith(Url(Constants.RoutePaths.LOGIN_EXTERNAL) + "?provider=Google").Should().BeTrue();
        }

        [Fact]
        public void GetLogin_DisableLocalLoginMultipleProvidersClientHasSingleHiddenProviderRestriction_RedirectsToProvider()
        {
            Options.AuthenticationOptions.EnableLocalLogin = false;
            Clients.First().IdentityProviderRestrictions = new List<string>
            {
                "HiddenGoogle"
            };
            var resp = GetLoginPage();
            resp.StatusCode.Should().Be(HttpStatusCode.Found);
            resp.Headers.Location.AbsoluteUri.StartsWith(Url(Constants.RoutePaths.LOGIN_EXTERNAL) + "?provider=HiddenGoogle").Should().BeTrue();
        }
        
        [Fact]
        public void GetLogin_DisableLocalLoginMultipleProvidersClientHasSingleVisibleProviderRestriction_RedirectsToProvider()
        {
            Options.AuthenticationOptions.EnableLocalLogin = false;
            Clients.First().IdentityProviderRestrictions = new List<string>
            {
                "Google"
            };
            var resp = GetLoginPage();
            resp.StatusCode.Should().Be(HttpStatusCode.Found);
            resp.Headers.Location.AbsoluteUri.StartsWith(Url(Constants.RoutePaths.LOGIN_EXTERNAL) + "?provider=Google").Should().BeTrue();
        }

        [Fact]
        public void GetLogin_ClientHasDisableLocalLogin_HasSingleProvider_RedirectsToProvider()
        {
            var client = Clients.First();
            client.EnableLocalLogin = false;
            client.IdentityProviderRestrictions = new List<string>
            {
                "Google"
            };
            var resp = GetLoginPage();
            resp.StatusCode.Should().Be(HttpStatusCode.Found);
            resp.Headers.Location.AbsoluteUri.StartsWith(Url(Constants.RoutePaths.LOGIN_EXTERNAL) + "?provider=Google").Should().BeTrue();
        }

        [Fact]
        public void GetLogin_DisableLocalLoginMultipleProvidersClientHasMultipleProviderRestriction_DisplaysLoginPage()
        {
            Options.AuthenticationOptions.EnableLocalLogin = false;
            Clients.First().IdentityProviderRestrictions = new List<string>
            {
                "Google", "Google2"
            };
            var resp = GetLoginPage();
            resp.AssertPage("login");
        }

        [Fact]
        public void GetLogin_EnableLocalLoginMoreThanOneProvider_ShowsLoginPage()
        {
            Action<IAppBuilder, string> config = (app, name)=>{
                ConfigureAdditionalIdentityProviders(app, name);
                var google = new GoogleOAuth2AuthenticationOptions
                {
                    AuthenticationType = "Google2",
                    Caption = "Google2",
                    SignInAsAuthenticationType = Constants.EXTERNAL_AUTHENTICATION_TYPE,
                    ClientId = "foo",
                    ClientSecret = "bar"
                };
                app.UseGoogleAuthentication(google);
            };
            OverrideIdentityProviderConfiguration = config;
            Init();
            
            Options.AuthenticationOptions.EnableLocalLogin = false;
            var resp = GetLoginPage();
            resp.AssertPage("login");
        }
        
        [Fact]
        public void GetLoginExternal_ValidProvider_RedirectsToProvider()
        {
            var msg = new SignInMessage {IdP = "Google"};
            var resp1 = GetLoginPage(msg);

            var resp2 = Client.GetAsync(resp1.Headers.Location.AbsoluteUri).Result;
            resp2.StatusCode.Should().Be(HttpStatusCode.Found);
            resp2.Headers.Location.AbsoluteUri.StartsWith("https://accounts.google.com").Should().BeTrue();
        }

        [Fact]
        public void GetLoginExternal_InvalidProvider_ReturnsError()
        {
            var msg = new SignInMessage {IdP = "Foo"};
            var resp1 = GetLoginPage(msg);

            var resp2 = Client.GetAsync(resp1.Headers.Location.AbsoluteUri).Result;
            resp2.AssertPage("error");
        }

        [Fact]
        public void GetLoginExternal_ClientDoesNotAllowProvider_ShowsErrorPage()
        {
            var clientApp = Clients.First();
            clientApp.IdentityProviderRestrictions = new List<string> { "foo" };
            var msg = new SignInMessage {IdP = "Google", ClientId = clientApp.ClientId};

            var resp1 = GetLoginPage(msg);
            var resp2 = Client.GetAsync(resp1.Headers.Location.AbsoluteUri).Result;
            resp2.AssertPage("error");
        }

        [Fact]
        public void GetLoginExternal_ClientDoesAllowsProvider_RedirectsToProvider()
        {
            var clientApp = Clients.First();
            clientApp.IdentityProviderRestrictions = new List<string> { "Google" };

            var msg = new SignInMessage {IdP = "Google", ClientId = clientApp.ClientId};

            var resp = GetLoginPage(msg);
            resp = Client.GetAsync(resp.Headers.Location.AbsoluteUri).Result;
            resp.StatusCode.Should().Be(HttpStatusCode.Found);
            resp.Headers.Location.AbsoluteUri.StartsWith("https://accounts.google.com").Should().BeTrue();
        }
        
        [Fact]
        public void GetLogin_ClientEnableLocalLoginFalse_NoLoginUrl()
        {
            var clientApp = Clients.First();
            clientApp.EnableLocalLogin = false;

            var resp = GetLoginPage();
            var model = resp.GetModel<LoginViewModel>();
            model.LoginUrl.Should().BeNull();
        }

        [Fact]
        public void PostToLogin_ClientEnableLocalLoginFalse_Fails()
        {
            var url = GetLoginPage().GetModel<LoginViewModel>().LoginUrl;

            var clientApp = Clients.First();
            clientApp.EnableLocalLogin = false;

            var resp = PostForm(url, new LoginCredentials { Username = "alice", Password = "alice" });
            var model = resp.GetModel<ErrorViewModel>();
            model.ErrorMessage.Should().Be(Messages.UnexpectedError);
        }

        [Fact]
        public void PostToLogin_ValidCredentials_IssuesAuthenticationCookie()
        {
            GetLoginPage();
            var resp = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            resp.AssertCookie(Constants.PRIMARY_AUTHENTICATION_TYPE);
        }

        [Fact]
        public void PostToLogin_ValidCredentials_ClearsSignInCookie()
        {
            GetLoginPage();
            var resp = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            var cookies = resp.Headers.GetValues("Set-Cookie");
            var cookie = cookies.SingleOrDefault(x => x.Contains("SignInMessage." + SignInId));
            cookie.Should().NotBeNull();
        }

        [Fact]
        public void PostToLogin_ValidCredentials_RedirectsBackToAuthorization()
        {
            GetLoginPage();
            var resp = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            resp.StatusCode.Should().Be(HttpStatusCode.Found);
            resp.Headers.Location.Should().Be(Url("authorize"));
        }

        [Fact]
        public void PostToLogin_NoModel_ShowErrorPage()
        {
            GetLoginPage();
            var resp = PostForm(GetLoginUrl(), null);
            resp.AssertPage("login");
            var model = resp.GetModel<LoginViewModel>();
            Messages.UsernameRequired.Should().Be(model.ErrorMessage);
        }

        [Fact]
        public void PostToLogin_InvalidUsername_ShowErrorPage()
        {
            GetLoginPage();
            var resp = PostForm(GetLoginUrl(), new LoginCredentials { Username = "bad", Password = "alice" });
            resp.AssertPage("login");
            var model = resp.GetModel<LoginViewModel>();
            Messages.InvalidUsernameOrPassword.Should().Be(model.ErrorMessage);
        }

        [Fact]
        public void PostToLogin_InvalidPassword_ShowErrorPage()
        {
            GetLoginPage();
            var resp = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "bad" });
            resp.AssertPage("login");
            var model = resp.GetModel<LoginViewModel>();
            Messages.InvalidUsernameOrPassword.Should().Be(model.ErrorMessage);
        }

        [Fact]
        public void PostToLogin_UserServiceReturnsError_ShowErrorPage()
        {
            MockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SignInMessage>()))
                .Returns(Task.FromResult(new AuthenticateResult("bad stuff")));

            GetLoginPage();
            var resp = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            resp.AssertPage("login");
            var model = resp.GetModel<LoginViewModel>();
            "bad stuff".Should().Be(model.ErrorMessage);
        }

        [Fact]
        public void PostToLogin_UserServiceReturnsNull_ShowErrorPage()
        {
            MockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SignInMessage>()))
                .Returns(Task.FromResult((AuthenticateResult)null));

            GetLoginPage();
            var resp = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            resp.AssertPage("login");
            var model = resp.GetModel<LoginViewModel>();
            Messages.InvalidUsernameOrPassword.Should().Be(model.ErrorMessage);
        }

        [Fact]
        public void PostToLogin_UserServiceReturnsParialLogin_IssuesPartialLoginCookie()
        {
            MockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SignInMessage>()))
                .Returns(Task.FromResult(new AuthenticateResult("/foo", "tempsub", "tempname")));

            GetLoginPage();
            var resp = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            resp.AssertCookie(Constants.PARTIAL_SIGN_IN_AUTHENTICATION_TYPE);
        }

        [Fact]
        public void PostToLogin_UserServiceReturnsParialLogin_IssuesRedirect()
        {
            MockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SignInMessage>()))
                .Returns(Task.FromResult(new AuthenticateResult("/foo", "tempsub", "tempname")));

            GetLoginPage();
            var resp = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            resp.StatusCode.Should().Be(HttpStatusCode.Found);
            resp.Headers.Location.AbsoluteUri.Should().Be(Url("foo"));
        }

        [Fact]
        public void PostToLogin_CookieOptions_AllowRememberMeIsFalse_IsPersistentIsFalse_DoesNotIssuePersistentCookie()
        {
            Options.AuthenticationOptions.CookieOptions.AllowRememberMe = false;
            Options.AuthenticationOptions.CookieOptions.IsPersistent = false;
            GetLoginPage();
            var resp = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            var cookies = resp.GetRawCookies();
            var cookie = cookies.Single(x => x.StartsWith(Constants.PRIMARY_AUTHENTICATION_TYPE + "="));
            cookie.Contains("expires=").Should().BeFalse();
        }

        [Fact]
        public void PostToLogin_CookieOptions_AllowRememberMeIsFalse_IsPersistentIsTrue_IssuesPersistentCookie()
        {
            Options.AuthenticationOptions.CookieOptions.AllowRememberMe = false;
            Options.AuthenticationOptions.CookieOptions.IsPersistent = true;
            GetLoginPage();
            var resp = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            var cookies = resp.GetRawCookies();
            var cookie = cookies.Single(x => x.StartsWith(Constants.PRIMARY_AUTHENTICATION_TYPE + "="));
            cookie.Contains("expires=").Should().BeTrue();
        }

        [Fact]
        public void PostToLogin_CookieOptions_AllowRememberMeIsTrue_IsPersistentIsTrue_DoNotCheckRememberMe_DoeNotIssuePersistentCookie()
        {
            Options.AuthenticationOptions.CookieOptions.AllowRememberMe = true;
            Options.AuthenticationOptions.CookieOptions.IsPersistent = true;
            GetLoginPage();
            var resp = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            var cookies = resp.GetRawCookies();
            var cookie = cookies.Single(x => x.StartsWith(Constants.PRIMARY_AUTHENTICATION_TYPE + "="));
            cookie.Contains("expires=").Should().BeFalse();
        }
        [Fact]
        public void PostToLogin_CookieOptions_AllowRememberMeIsTrue_IsPersistentIsTrue_CheckRememberMe_IssuesPersistentCookie()
        {
            Options.AuthenticationOptions.CookieOptions.AllowRememberMe = true;
            Options.AuthenticationOptions.CookieOptions.IsPersistent = true;
            GetLoginPage();
            var resp = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice", RememberMe = true });
            var cookies = resp.GetRawCookies();
            var cookie = cookies.Single(x => x.StartsWith(Constants.PRIMARY_AUTHENTICATION_TYPE + "="));
            cookie.Contains("expires=").Should().BeTrue();
        }

        [Fact]
        public void PostToLogin_CookieOptionsIsPersistentIsTrueButResponseIsPartialLogin_DoesNotIssuePersistentCookie()
        {
            MockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SignInMessage>()))
                .Returns(Task.FromResult(new AuthenticateResult("/foo", "tempsub", "tempname")));
            
            Options.AuthenticationOptions.CookieOptions.IsPersistent = true;
            GetLoginPage();
            var resp = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            var cookies = resp.GetRawCookies();
            var cookie = cookies.Single(x => x.StartsWith(Constants.PARTIAL_SIGN_IN_AUTHENTICATION_TYPE + "="));
            cookie.Contains("expires=").Should().BeFalse();
        }

        [Fact]
        public void ResumeLoginFromRedirect_WithPartialCookie_IssuesFullLoginCookie()
        {
            MockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SignInMessage>()))
                .Returns(Task.FromResult(new AuthenticateResult("/foo", "tempsub", "tempname")));

            GetLoginPage();
            var resp1 = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            Client.SetCookies(resp1.GetCookies());
            var resp2 = Get(GetResumeUrlFromPartialSignInCookie(resp1));
            resp2.AssertCookie(Constants.PRIMARY_AUTHENTICATION_TYPE);
        }

        [Fact]
        public void ResumeLoginFromRedirect_WithPartialCookie_IssuesRedirectToAuthorizationPage()
        {
            MockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SignInMessage>()))
                .Returns(Task.FromResult(new AuthenticateResult("/foo", "tempsub", "tempname")));

            GetLoginPage();
            var resp1 = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            Client.SetCookies(resp1.GetCookies());

            var resp2 = Get(GetResumeUrlFromPartialSignInCookie(resp1));
            resp2.StatusCode.Should().Be(HttpStatusCode.Found);
            resp2.Headers.Location.AbsoluteUri.Should().Be(Url("authorize"));
        }

        [Fact]
        public void ResumeLoginFromRedirect_WithoutPartialCookie_ShowsError()
        {
            MockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SignInMessage>()))
                .Returns(Task.FromResult(new AuthenticateResult("/foo", "tempsub", "tempname")));

            GetLoginPage();
            var resp1 = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            var resp2 = Get(GetResumeUrlFromPartialSignInCookie(resp1));
            resp2.AssertPage("error");
        }

        [Fact]
        public void Logout_AnonymousUser_ShowsLoggedOutPage()
        {
            var resp = Get(Constants.RoutePaths.LOGOUT);
            resp.AssertPage("loggedOut");
        }

        [Fact]
        public void Logout_LoggedInUser_ShowsLogoutPromptPage()
        {
            Login();
            
            var resp = Get(Constants.RoutePaths.LOGOUT);
            resp.AssertPage("logout");
        }

        [Fact]
        public void Logout_EnableSignOutPromptSetToFalse_SkipsLogoutPromptPage()
        {
            Login();

            Options.AuthenticationOptions.EnableSignOutPrompt = false;
            var resp = Get(Constants.RoutePaths.LOGOUT);
            resp.AssertPage("loggedOut");
        }
        
        [Fact]
        public void Logout_SignOutMessagePassed_SkipsLogoutPromptPage()
        {
            Login();
            
            var id = WriteMessageToCookie(new SignOutMessage { ClientId = "foo", ReturnUrl = "http://foo" });
            var resp = Get(Constants.RoutePaths.LOGOUT + "?id=" + id);
            resp.AssertPage("loggedOut");
        }

        [Fact]
        public void PostToLogout_AnonymousUser_DoesNotInvokeUserServiceSignOut()
        {
            var resp = PostForm(Constants.RoutePaths.LOGOUT, null);
            MockUserService.Verify(x => x.SignOutAsync(It.IsAny<ClaimsPrincipal>()), Times.Never());
        }
        
        [Fact]
        public void PostToLogout_AuthenticatedUser_InvokesUserServiceSignOut()
        {
            Login();

            var resp = PostForm(Constants.RoutePaths.LOGOUT, null);
            MockUserService.Verify(x => x.SignOutAsync(It.IsAny<ClaimsPrincipal>()));
        }

        [Fact]
        public void PostToLogout_RemovesCookies()
        {
            Login();

            var resp = PostForm(Constants.RoutePaths.LOGOUT, null);
            var cookies = resp.Headers.GetValues("Set-Cookie");
            // cookies: primary, partial, external, session, signout
            cookies.Count().Should().Be(5);
            // GetCookies will not return values for cookies that are expired/revoked
            resp.GetCookies().Count().Should().Be(0);
        }
        
        [Fact]
        public void PostToLogout_EmitsLogoutUrlsForProtocolIframes()
        {
            Login();

            Options.ProtocolLogoutUrls.Add("/foo/signout");
            var resp = PostForm(Constants.RoutePaths.LOGOUT, null);
            var model = resp.GetModel<LoggedOutViewModel>();
            var signOutUrls = model.IFrameUrls.ToArray();
            signOutUrls.Length.Should().Be(2);
            signOutUrls.Should().Contain(Url(Constants.RoutePaths.Oidc.END_SESSION_CALLBACK));
            signOutUrls.Should().Contain(Url("/foo/signout"));
        }

        [Fact]
        public void LoginExternalCallback_WithoutExternalCookie_RendersErrorPage()
        {
            var msg = new SignInMessage();
            msg.IdP = "Google";
            var resp1 = GetLoginPage(msg);
            var resp2 = Client.GetAsync(resp1.Headers.Location.AbsoluteUri).Result;
            var resp3 = Get(Constants.RoutePaths.LOGIN_EXTERNAL_CALLBACK);
            resp3.AssertPage("error");
        }

        [Fact]
        public void LoginExternalCallback_WithNoClaims_RendersLoginPageWithError()
        {
            var msg = new SignInMessage {IdP = "Google"};
            var resp1 = GetLoginPage(msg);

            SignInIdentity = new ClaimsIdentity(Constants.EXTERNAL_AUTHENTICATION_TYPE);
            var resp2 = Client.GetAsync(resp1.Headers.Location.AbsoluteUri).Result;
            Client.SetCookies(resp2.GetCookies());

            var resp3 = Get(Constants.RoutePaths.LOGIN_EXTERNAL_CALLBACK);
            resp3.AssertPage("login");
            var model = resp3.GetModel<LoginViewModel>();
            model.ErrorMessage.Should().Be(Messages.NoMatchingExternalAccount);
        }
        
        [Fact]
        public void LoginExternalCallback_WithoutSubjectOrNameIdClaims_RendersLoginPageWithError()
        {
            var msg = new SignInMessage {IdP = "Google"};
            var resp1 = GetLoginPage(msg);

            SignInIdentity = new ClaimsIdentity(new[]{new Claim("foo", "bar")}, Constants.EXTERNAL_AUTHENTICATION_TYPE);
            var resp2 = Client.GetAsync(resp1.Headers.Location.AbsoluteUri).Result;
            Client.SetCookies(resp2.GetCookies());
            
            var resp3 = Get(Constants.RoutePaths.LOGIN_EXTERNAL_CALLBACK);
            resp3.AssertPage("login");
            var model = resp3.GetModel<LoginViewModel>();
            model.ErrorMessage.Should().Be(Messages.NoMatchingExternalAccount);
        }

        [Fact]
        public void LoginExternalCallback_WithValidSubjectClaim_IssuesAuthenticationCookie()
        {
            var msg = new SignInMessage {IdP = "Google", ReturnUrl = Url("authorize")};
            var resp1 = GetLoginPage(msg);

            var sub = new Claim(Constants.ClaimTypes.SUBJECT, "123", ClaimValueTypes.String, "Google");
            SignInIdentity = new ClaimsIdentity(new[] { sub }, Constants.EXTERNAL_AUTHENTICATION_TYPE);
            var resp2 = Client.GetAsync(resp1.Headers.Location.AbsoluteUri).Result;
            Client.SetCookies(resp2.GetCookies());

            var resp3 = Get(Constants.RoutePaths.LOGIN_EXTERNAL_CALLBACK);
            resp3.AssertCookie(Constants.PRIMARY_AUTHENTICATION_TYPE);
        }

        [Fact]
        public void LoginExternalCallback_WithValidNameIDClaim_IssuesAuthenticationCookie()
        {
            var msg = new SignInMessage {IdP = "Google", ReturnUrl = Url("authorize")};
            var resp1 = GetLoginPage(msg);

            var sub = new Claim(ClaimTypes.NameIdentifier, "123", ClaimValueTypes.String, "Google");
            SignInIdentity = new ClaimsIdentity(new[] { sub }, Constants.EXTERNAL_AUTHENTICATION_TYPE);
            var resp2 = Client.GetAsync(resp1.Headers.Location.AbsoluteUri).Result;
            Client.SetCookies(resp2.GetCookies());

            var resp3 = Get(Constants.RoutePaths.LOGIN_EXTERNAL_CALLBACK);
            resp3.AssertCookie(Constants.PRIMARY_AUTHENTICATION_TYPE);
        }
        
        [Fact]
        public void LoginExternalCallback_WithValidSubjectClaim_RedirectsToAuthorizeEndpoint()
        {
            var msg = new SignInMessage {IdP = "Google", ReturnUrl = Url("authorize")};
            var resp1 = GetLoginPage(msg);

            var sub = new Claim(Constants.ClaimTypes.SUBJECT, "123", ClaimValueTypes.String, "Google");
            SignInIdentity = new ClaimsIdentity(new[] { sub }, Constants.EXTERNAL_AUTHENTICATION_TYPE);
            var resp2 = Client.GetAsync(resp1.Headers.Location.AbsoluteUri).Result;
            Client.SetCookies(resp2.GetCookies());

            var resp3 = Get(Constants.RoutePaths.LOGIN_EXTERNAL_CALLBACK);
            resp3.StatusCode.Should().Be(HttpStatusCode.Found);
            resp3.Headers.Location.AbsoluteUri.Should().Be(Url("authorize"));
        }

        [Fact]
        public void LoginExternalCallback_UserServiceReturnsError_ShowsError()
        {
            MockUserService.Setup(x => x.AuthenticateExternalAsync(It.IsAny<ExternalIdentity>(), It.IsAny<SignInMessage>()))
                .Returns(Task.FromResult(new AuthenticateResult("foo bad")));

            var msg = new SignInMessage {IdP = "Google", ReturnUrl = Url("authorize")};
            var resp1 = GetLoginPage(msg);

            var sub = new Claim(Constants.ClaimTypes.SUBJECT, "123", ClaimValueTypes.String, "Google");
            SignInIdentity = new ClaimsIdentity(new[] { sub }, Constants.EXTERNAL_AUTHENTICATION_TYPE);
            var resp2 = Client.GetAsync(resp1.Headers.Location.AbsoluteUri).Result;
            Client.SetCookies(resp2.GetCookies());

            var resp3 = Get(Constants.RoutePaths.LOGIN_EXTERNAL_CALLBACK);
            resp3.AssertPage("login");
            var model = resp3.GetModel<LoginViewModel>();
            model.ErrorMessage.Should().Be("foo bad");
        }

        [Fact]
        public void LoginExternalCallback_UserServiceReturnsNull_ShowError()
        {
            MockUserService.Setup(x => x.AuthenticateExternalAsync(It.IsAny<ExternalIdentity>(), It.IsAny<SignInMessage>()))
                .Returns(Task.FromResult((AuthenticateResult)null));

            var msg = new SignInMessage {IdP = "Google", ReturnUrl = Url("authorize")};
            var resp1 = GetLoginPage(msg);

            var sub = new Claim(Constants.ClaimTypes.SUBJECT, "123", ClaimValueTypes.String, "Google");
            SignInIdentity = new ClaimsIdentity(new[] { sub }, Constants.EXTERNAL_AUTHENTICATION_TYPE);
            var resp2 = Client.GetAsync(resp1.Headers.Location.AbsoluteUri).Result;
            Client.SetCookies(resp2.GetCookies());

            var resp3 = Get(Constants.RoutePaths.LOGIN_EXTERNAL_CALLBACK);
            resp3.AssertPage("login");
            var model = resp3.GetModel<LoginViewModel>();
            model.ErrorMessage.Should().Be(Messages.NoMatchingExternalAccount);
        }

        [Fact]
        public void LoginExternalCallback_UserIsAnonymous_NoSubjectIsPassedToUserService()
        {
            var msg = new SignInMessage {IdP = "Google", ReturnUrl = Url("authorize")};
            var resp1 = GetLoginPage(msg);

            var sub = new Claim(Constants.ClaimTypes.SUBJECT, "123", ClaimValueTypes.String, "Google");
            SignInIdentity = new ClaimsIdentity(new[] { sub }, Constants.EXTERNAL_AUTHENTICATION_TYPE);
            var resp2 = Client.GetAsync(resp1.Headers.Location.AbsoluteUri).Result;
            Client.SetCookies(resp2.GetCookies());

            Get(Constants.RoutePaths.LOGIN_EXTERNAL_CALLBACK);

            MockUserService.Verify(x => x.AuthenticateExternalAsync(It.IsAny<ExternalIdentity>(), It.IsAny<SignInMessage>()));
        }

        [Fact]
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
            var resp = PostForm(Url(Constants.RoutePaths.LOGOUT + "?id=" + id), null);
            var model = resp.GetModel<LoggedOutViewModel>();
            model.RedirectUrl.Should().Be(msg.ReturnUrl);
            model.ClientName.Should().Be(c.ClientName);
        }

        [Fact]
        public void LoggedOut_NoSignOutMessage_ContainsNullForClientNameAndRedirectUrlInPage()
        {
            GetLoginPage();
            var resp = PostForm(Url(Constants.RoutePaths.LOGOUT), null);
            var model = resp.GetModel<LoggedOutViewModel>();
            model.RedirectUrl.Should().BeNull();
            model.ClientName.Should().BeNull();
        }

        [Fact]
        public void LoggedOut_InvalidSignOutMessageId_ContainsNullForClientNameAndRedirectUrlInPage()
        {
            GetLoginPage();
            var resp = PostForm(Url(Constants.RoutePaths.LOGOUT + "?id=123"), null);
            var model = resp.GetModel<LoggedOutViewModel>();
            model.RedirectUrl.Should().BeNull();
            model.ClientName.Should().BeNull();
        }

        [Fact]
        public void Login_ClientFiltersAllowedIdentityProviders_OnlyAllowedIdPsInLoginPage()
        {
            var msg = new SignInMessage { ReturnUrl = Url("authorize"), ClientId = "no_external_idps" };
            var resp = GetLoginPage(msg);
            var model = resp.GetModel<LoginViewModel>();
            var google = model.ExternalProviders.SingleOrDefault(x => x.Text == "Google");
            google.Should().BeNull();
        }
        
        [Fact]
        public void Login_ClientDoesNotFiltersAllowedIdentityProviders_ExternalIsInLoginPage()
        {
            var msg = new SignInMessage { ReturnUrl = Url("authorize"), ClientId = "any_external_idps" };
            var resp = GetLoginPage(msg);
            var model = resp.GetModel<LoginViewModel>();
            var hasGoogle = model.ExternalProviders.Any(x => x.Text == "Google");
            var hasGoogle2 = model.ExternalProviders.Any(x => x.Text == "Google2");
            hasGoogle.Should().BeTrue();
            hasGoogle2.Should().BeTrue();
        }

        [Fact]
        public void Login_InvalidClientId_ShowsLoginPage()
        {
            var msg = new SignInMessage { ReturnUrl = Url("authorize"), ClientId = "bad_id" };
            var resp = GetLoginPage(msg);
            resp.AssertPage("login");
        }

        [Fact]
        public void Login_PostWithJson_ReturnsUnsupportedMediaType()
        {
            GetLoginPage();
            var resp = Post(GetLoginUrl(), (object)null);
            resp.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
        }

        [Fact]
        public void Login_PostWithoutXsrf_ReturnsError()
        {
            var resp = PostForm(Url(Constants.RoutePaths.LOGIN + "?signin="), null);
            resp.AssertPage("error");
        }

        [Fact]
        public void Logout_PostWithoutXsrf_ReturnsError()
        {
            var resp = PostForm(Url(Constants.RoutePaths.LOGOUT), null);
            resp.AssertPage("error");
        }

        string GetLongString()
        {
            const string value = "x";
            var parts = new string[AuthenticationController.MAX_INPUT_PARAM_LENGTH+1];
            return parts.Aggregate((x, y) => (x??value) + value);
        }

        [Fact]
        public void GetLogin_SignInIdTooLong_ReturnsError()
        {
            var url = GetLoginUrl();
            url += GetLongString();
            var resp = Get(url);
            resp.AssertPage("error");
        }

        [Fact]
        public void GetLogin_SigninMessageThresholdSetToX_GetLoginXTimesOnlyLatestXMessagesAreKept()
        {
            const int signInMessageThreshold = 3;
            Options.AuthenticationOptions.SignInMessageThreshold = signInMessageThreshold;

            for (var i = 0; i < signInMessageThreshold; i++)
            {
                GetLoginPage();
            }

            var theNextRequest = GetLoginPage();
            theNextRequest.RequestMessage.Headers
                .GetValues("Cookie")
                .Count(c => c.StartsWith("SignInMessage."))
                .Should()
                .Be(Options.AuthenticationOptions.SignInMessageThreshold);
        }

        [Fact]
        public void GetLogin_SigninMessageThresholdSetToX_GetLoginMoreThanXTimesOnlyLatestXMessagesAreKept()
        {
            Options.AuthenticationOptions.SignInMessageThreshold = 3;
            var moreThanSignInThreshold = Options.AuthenticationOptions.SignInMessageThreshold + 1;

            for (var i = 0; i < moreThanSignInThreshold; i++)
            {
                GetLoginPage();
            }

            var theNextRequest = GetLoginPage();
            theNextRequest.RequestMessage.Headers
                .GetValues("Cookie")
                .Count(c => c.StartsWith("SignInMessage."))
                .Should()
                .Be(Options.AuthenticationOptions.SignInMessageThreshold);
        }

        [Fact]
        public void GetLogin_SigninMessageThresholdSetToZero_OneSignInMessageKept()
        {
            Options.AuthenticationOptions.SignInMessageThreshold = 0;

            GetLoginPage();

            var theNextRequest = GetLoginPage();
            theNextRequest.RequestMessage.Headers
                .GetValues("Cookie")
                .Count(c => c.StartsWith("SignInMessage."))
                .Should()
                .Be(1);
        }

        [Fact]
        public void GetLogin_SigninMessageThresholdSetToNegative_OneSignInMessageKept()
        {
            Options.AuthenticationOptions.SignInMessageThreshold = -42;

            GetLoginPage();

            var theNextRequest = GetLoginPage();
            theNextRequest.RequestMessage.Headers
                .GetValues("Cookie")
                .Count(c => c.StartsWith("SignInMessage."))
                .Should()
                .Be(1);
        }

        [Fact]
        public void PostLogin_SignInIdTooLong_ReturnsError()
        {
            var resp = GetLoginPage();
            var model = resp.GetModel<LoginViewModel>();
            var url = model.LoginUrl + GetLongString();
            resp = PostForm(url, new LoginCredentials { Username = "alice", Password = "alice" });
            model = resp.GetModel<LoginViewModel>();
            resp.AssertPage("error");
        }
        
        [Fact]
        public void PostLogin_UsernameTooLong_ReturnsLoginPageWithEmptyUidPwd()
        {
            var resp = GetLoginPage();
            var model = resp.GetModel<LoginViewModel>();
            var url = model.LoginUrl;
            resp = PostForm(url, new LoginCredentials { Username = "alice" + GetLongString(), Password = "alice" });
            model = resp.GetModel<LoginViewModel>();
            resp.AssertPage("login");
            model = resp.GetModel<LoginViewModel>();
            model.Username.Should().BeNullOrEmpty();
        }
        
        [Fact]
        public void PostLogin_PasswordTooLong_ReturnsLoginPageWithEmptyUidPwd()
        {
            var resp = GetLoginPage();
            var model = resp.GetModel<LoginViewModel>();
            var url = model.LoginUrl;
            resp = PostForm(url, new LoginCredentials { Username = "alice", Password = "alice" + GetLongString() });
            model = resp.GetModel<LoginViewModel>();
            resp.AssertPage("login");
            model = resp.GetModel<LoginViewModel>();
            model.Username.Should().BeNullOrEmpty();
        }

        [Fact]
        public void GetLoginExternal_IdPTooLong_ReturnsError()
        {
            var msg = new SignInMessage();
            msg.IdP = "Google" + GetLongString();
            var resp1 = GetLoginPage(msg);

            var resp2 = Client.GetAsync(resp1.Headers.Location.AbsoluteUri).Result;
            resp2.AssertPage("error");
        }

        [Fact]
        public void GetLoginExternal_SignInIdTooLong_ReturnsError()
        {
            var url = Url(Constants.RoutePaths.LOGIN_EXTERNAL) + "?signin=" + GetLongString() + "&provider=Google";
            var resp = Client.GetAsync(url).Result;
            resp.AssertPage("error");
        }

        [Fact]
        public void GetLoginExternalCallback_ErrorTooLong_ReturnsError()
        {
            var url = Url(Constants.RoutePaths.LOGIN_EXTERNAL_CALLBACK) + "?error=" + GetLongString();
            var resp = Client.GetAsync(url).Result;
            resp.AssertPage("error");
        }

        [Fact]
        public void ResumeLogin_ResumeIdTooLong_ReturnsError()
        {
            var url = Url(Constants.RoutePaths.RESUME_LOGIN_FROM_REDIRECT) + "?resume=" + GetLongString();
            var resp = Client.GetAsync(url).Result;
            resp.AssertPage("error");
        }

        [Fact]
        public void LogoutPrompt_SignOutIdTooLong_ReturnsError()
        {
            var url = Url(Constants.RoutePaths.LOGOUT) + "?id=" + GetLongString();
            var resp = Client.GetAsync(url).Result;
            resp.AssertPage("error");
        }
        
        [Fact]
        public void LogoutSubmit_SignOutIdTooLong_ReturnsError()
        {
            Login();

            var url = Constants.RoutePaths.LOGOUT + "?id=" + GetLongString();
            var resp = PostForm(url, new { });
            resp.AssertPage("error");
        }
    }
}

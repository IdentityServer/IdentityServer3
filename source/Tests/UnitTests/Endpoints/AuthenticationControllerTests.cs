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
using IdentityServer3.Core;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Resources;
using IdentityServer3.Core.ViewModels;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Google;
using Moq;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using AuthenticateResult = IdentityServer3.Core.Models.AuthenticateResult;

namespace IdentityServer3.Tests.Endpoints
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
                props.Dictionary.Add(Constants.Authentication.SigninId, SignInId);
                if(SignInIdentity.AuthenticationType == Constants.ExternalAuthenticationType)
                {
                    props.Dictionary.Add(Constants.Authentication.KatanaAuthenticationType, "Google");
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

        void Login(bool setCookie = true)
        {
            var msg = new SignInMessage() { ReturnUrl = Url("authorize") };
            var signInId = WriteMessageToCookie(msg);
            var url = Constants.RoutePaths.Login + "?signin=" + signInId;
            var resp = Get(url);
            ProcessXsrf(resp);

            if (setCookie)
            {
                resp = PostForm(url, new LoginCredentials { Username = "alice", Password = "alice" });
                client.SetCookies(resp.GetCookies());
            }
        }

        private string GetResumeUrlFromPartialSignInCookie(HttpResponseMessage resp)
        {
            var cookie = resp.GetCookies().Where(x=>x.Name == Constants.PartialSignInAuthenticationType).Single();
            var ticket = ticketFormatter.Unprotect(cookie.Value);
            var urlClaim = ticket.Identity.Claims.Single(x => x.Type == Constants.ClaimTypes.PartialLoginReturnUrl);
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
            var msg = new SignInMessage();
            msg.IdP = "Google";
            var resp = GetLoginPage(msg);

            resp.StatusCode.Should().Be(HttpStatusCode.Found);
            resp.Headers.Location.AbsoluteUri.StartsWith("https://accounts.google.com").Should().BeTrue();
        }

        [Fact]
        public void GetLogin_SignInMessageHasLoginHint_UsernameIsPopulatedFromLoginHint()
        {
            options.AuthenticationOptions.EnableLoginHint = true;

            var msg = new SignInMessage();
            msg.LoginHint = "test";

            var resp = GetLoginPage(msg);

            var model = resp.GetModel<LoginViewModel>();
            model.Username.Should().Be("test");
        }

        [Fact]
        public void GetLogin_EnableLoginHintFalse_UsernameIsNotPopulatedFromLoginHint()
        {
            options.AuthenticationOptions.EnableLoginHint = false;
            
            var msg = new SignInMessage();
            msg.LoginHint = "test";

            var resp = GetLoginPage(msg);

            var model = resp.GetModel<LoginViewModel>();
            model.Username.Should().BeNull(); ;
        }

        [Fact]
        public void PostToLogin_SignInMessageHasLoginHint_UsernameShouldBeUsernamePosted()
        {
            var msg = new SignInMessage();
            msg.LoginHint = "test";

            var resp = GetLoginPage(msg);
            var model = resp.GetModel<LoginViewModel>();
            resp = PostForm(model.LoginUrl, new LoginCredentials { Username = "alice", Password = "jdfhjkdf" });
            model = resp.GetModel<LoginViewModel>();
            model.Username.Should().Be("alice");
        }

        [Fact]
        public void GetLogin_NoSignInMessage_ReturnError()
        {
            var resp = Get(Constants.RoutePaths.Login);
            resp.AssertPage("error");
        }

        [Fact]
        public void GetLogin_NoSignInMessage_InvalidSignInRedirectUrl_ConfiguredWithRelativePath_RedirectsCorrectly()
        {
            this.options.AuthenticationOptions.InvalidSignInRedirectUrl = "~/fail";
            var resp = Get(Constants.RoutePaths.Login);
            resp.StatusCode.Should().Be(HttpStatusCode.Redirect);
            resp.Headers.Location.AbsoluteUri.Should().Be(Url("/fail"));
        }

        [Fact]
        public void GetLogin_NoSignInMessage_InvalidSignInRedirectUrl_ConfiguredWithAbsolutePath_RedirectsCorrectly()
        {
            this.options.AuthenticationOptions.InvalidSignInRedirectUrl = "/fail";
            var resp = Get(Constants.RoutePaths.Login);
            resp.StatusCode.Should().Be(HttpStatusCode.Redirect);
            resp.Headers.Location.AbsoluteUri.Should().Be(Url("/fail"));
        }

        [Fact]
        public void GetLogin_NoSignInMessage_InvalidSignInRedirectUrl_ConfiguredWithUrl_RedirectsCorrectly()
        {
            this.options.AuthenticationOptions.InvalidSignInRedirectUrl = "http://fail/fail";
            var resp = Get(Constants.RoutePaths.Login);
            resp.StatusCode.Should().Be(HttpStatusCode.Redirect);
            resp.Headers.Location.AbsoluteUri.Should().Be("http://fail/fail");
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
            mockUserService
                .Setup(x => x.PreAuthenticateAsync(It.IsAny<PreAuthenticationContext>()))
                .Callback<PreAuthenticationContext>(ctx=>{
                    ctx.AuthenticateResult = new AuthenticateResult("SomeError");
                })
                .Returns(Task.FromResult(0));

            var resp = GetLoginPage();
            resp.AssertPage("error");
            var model = resp.GetModel<ErrorViewModel>();
            model.ErrorMessage.Should().Be("SomeError");
        }

        [Fact]
        public void GetLogin_PreAuthenticateReturnsFullLogin_IssuesLoginCookie()
        {
            mockUserService
                .Setup(x => x.PreAuthenticateAsync(It.IsAny<PreAuthenticationContext>()))
                .Callback<PreAuthenticationContext>(ctx =>
                {
                    ctx.AuthenticateResult = new AuthenticateResult(IdentityServerPrincipal.Create("sub", "name"));
                })
                .Returns(Task.FromResult(0));

            var resp = GetLoginPage();
            resp.AssertCookie(Constants.PrimaryAuthenticationType);
        }

        [Fact]
        public void GetLogin_PreAuthenticateReturnsFullLogin_RedirectsToReturnUrl()
        {
            mockUserService
                .Setup(x => x.PreAuthenticateAsync(It.IsAny<PreAuthenticationContext>()))
                .Callback<PreAuthenticationContext>(ctx=>{
                    ctx.AuthenticateResult = new AuthenticateResult(IdentityServerPrincipal.Create("sub", "name"));
                })
                .Returns(Task.FromResult(0));

            var resp = GetLoginPage();
            resp.StatusCode.Should().Be(HttpStatusCode.Found);
            resp.Headers.Location.AbsoluteUri.Should().Be(Url("authorize"));
        }

        [Fact]
        public void GetLogin_PreAuthenticateReturnsParialLogin_IssuesPartialLoginCookie()
        {
            mockUserService
                .Setup(x => x.PreAuthenticateAsync(It.IsAny<PreAuthenticationContext>()))
                .Callback<PreAuthenticationContext>(ctx =>
                {
                    ctx.AuthenticateResult = new AuthenticateResult("/foo", "tempsub", "tempname");
                })
                .Returns(Task.FromResult(0));

            var resp = GetLoginPage();
            resp.AssertCookie(Constants.PartialSignInAuthenticationType);
        }

        [Fact]
        public void GetLogin_PreAuthenticateReturnsParialLogin_IssuesRedirect()
        {
            mockUserService
                .Setup(x => x.PreAuthenticateAsync(It.IsAny<PreAuthenticationContext>()))
                .Callback<PreAuthenticationContext>(ctx =>
                {
                    ctx.AuthenticateResult = new AuthenticateResult("/foo", "tempsub", "tempname");
                })
                .Returns(Task.FromResult(0));

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

            mockUserService
                .Setup(x => x.PreAuthenticateAsync(It.IsAny<PreAuthenticationContext>()))
                .Callback<PreAuthenticationContext>(ctx =>
                {
                    ctx.AuthenticateResult = new AuthenticateResult("/foo", "tempsub", "tempname");
                })
                .Returns(Task.FromResult(0));

            var resp = GetLoginPage();
            resp.StatusCode.Should().Be(HttpStatusCode.Found);
            resp.Headers.Location.AbsoluteUri.Should().Be("http://somehost/foo");
        }

        [Fact]
        public void GetLogin_DisableLocalLoginAndOnlyOneProvider_RedirectsToProvider()
        {
            google2.Caption = null;
            options.AuthenticationOptions.EnableLocalLogin = false;
            var resp = GetLoginPage();
            resp.StatusCode.Should().Be(HttpStatusCode.Found);
            resp.Headers.Location.AbsoluteUri.StartsWith(Url(Constants.RoutePaths.LoginExternal) + "?provider=Google").Should().BeTrue();
        }

        [Fact]
        public void GetLogin_DisableLocalLoginMultipleProvidersClientHasSingleHiddenProviderRestriction_RedirectsToProvider()
        {
            options.AuthenticationOptions.EnableLocalLogin = false;
            clients.First().IdentityProviderRestrictions = new List<string>
            {
                "HiddenGoogle"
            };
            var resp = GetLoginPage();
            resp.StatusCode.Should().Be(HttpStatusCode.Found);
            resp.Headers.Location.AbsoluteUri.StartsWith(Url(Constants.RoutePaths.LoginExternal) + "?provider=HiddenGoogle").Should().BeTrue();
        }
        
        [Fact]
        public void GetLogin_DisableLocalLoginMultipleProvidersClientHasSingleVisibleProviderRestriction_RedirectsToProvider()
        {
            options.AuthenticationOptions.EnableLocalLogin = false;
            clients.First().IdentityProviderRestrictions = new List<string>
            {
                "Google"
            };
            var resp = GetLoginPage();
            resp.StatusCode.Should().Be(HttpStatusCode.Found);
            resp.Headers.Location.AbsoluteUri.StartsWith(Url(Constants.RoutePaths.LoginExternal) + "?provider=Google").Should().BeTrue();
        }

        [Fact]
        public void GetLogin_ClientHasDisableLocalLogin_HasSingleProvider_RedirectsToProvider()
        {
            var client = clients.First();
            client.EnableLocalLogin = false;
            client.IdentityProviderRestrictions = new List<string>
            {
                "Google"
            };
            var resp = GetLoginPage();
            resp.StatusCode.Should().Be(HttpStatusCode.Found);
            resp.Headers.Location.AbsoluteUri.StartsWith(Url(Constants.RoutePaths.LoginExternal) + "?provider=Google").Should().BeTrue();
        }

        [Fact]
        public void GetLogin_DisableLocalLoginMultipleProvidersClientHasMultipleProviderRestriction_DisplaysLoginPage()
        {
            options.AuthenticationOptions.EnableLocalLogin = false;
            clients.First().IdentityProviderRestrictions = new List<string>
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
                base.ConfigureAdditionalIdentityProviders(app, name);
                var google = new GoogleOAuth2AuthenticationOptions
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
            Init();
            
            options.AuthenticationOptions.EnableLocalLogin = false;
            var resp = GetLoginPage();
            resp.AssertPage("login");
        }
        
        [Fact]
        public void GetLoginExternal_ValidProvider_RedirectsToProvider()
        {
            var msg = new SignInMessage();
            msg.IdP = "Google";
            var resp = GetLoginPage(msg);
            resp.StatusCode.Should().Be(HttpStatusCode.Found);
            resp.Headers.Location.AbsoluteUri.StartsWith("https://accounts.google.com").Should().BeTrue();
        }

        [Fact]
        public void GetLoginExternal_InvalidProvider_ReturnsError()
        {
            var msg = new SignInMessage();
            msg.IdP = "Foo";
            var resp = GetLoginPage(msg);
            resp.AssertPage("error");
        }

        [Fact]
        public void GetLoginExternal_ClientDoesNotAllowProvider_ShowsErrorPage()
        {
            var clientApp = clients.First();
            clientApp.IdentityProviderRestrictions = new List<string> { "foo" };
            var msg = new SignInMessage();
            msg.IdP = "Google";
            msg.ClientId = clientApp.ClientId;

            var resp = GetLoginPage(msg);
            resp.AssertPage("error");
        }

        [Fact]
        public void GetLoginExternal_ClientDoesAllowsProvider_RedirectsToProvider()
        {
            var clientApp = clients.First();
            clientApp.IdentityProviderRestrictions = new List<string> { "Google" };

            var msg = new SignInMessage();
            msg.IdP = "Google";
            msg.ClientId = clientApp.ClientId;

            var resp = GetLoginPage(msg);
            resp.StatusCode.Should().Be(HttpStatusCode.Found);
            resp.Headers.Location.AbsoluteUri.StartsWith("https://accounts.google.com").Should().BeTrue();
        }
        
        [Fact]
        public void GetLogin_ClientEnableLocalLoginFalse_NoLoginUrl()
        {
            var clientApp = clients.First();
            clientApp.EnableLocalLogin = false;

            var resp = GetLoginPage();
            var model = resp.GetModel<LoginViewModel>();
            model.LoginUrl.Should().BeNull();
        }

        [Fact]
        public void PostToLogin_ClientEnableLocalLoginFalse_Fails()
        {
            var url = GetLoginPage().GetModel<LoginViewModel>().LoginUrl;

            var clientApp = clients.First();
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
            resp.AssertCookie(Constants.PrimaryAuthenticationType);
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
            mockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<LocalAuthenticationContext>()))
                .Callback<LocalAuthenticationContext>(ctx =>
                {
                    ctx.AuthenticateResult = new AuthenticateResult("bad stuff");
                })
                .Returns(Task.FromResult(0));

            GetLoginPage();
            var resp = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            resp.AssertPage("login");
            var model = resp.GetModel<LoginViewModel>();
            "bad stuff".Should().Be(model.ErrorMessage);
        }

        [Fact]
        public void PostToLogin_UserServiceReturnsNull_ShowErrorPage()
        {
            mockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<LocalAuthenticationContext>()))
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
            mockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<LocalAuthenticationContext>()))
                .Callback<LocalAuthenticationContext>(ctx =>
                {
                    ctx.AuthenticateResult = new AuthenticateResult("/foo", "tempsub", "tempname");
                })
                .Returns(Task.FromResult(0));

            GetLoginPage();
            var resp = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            resp.AssertCookie(Constants.PartialSignInAuthenticationType);
        }

        [Fact]
        public void PostToLogin_UserServiceReturnsParialLogin_IssuesRedirect()
        {
            mockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<LocalAuthenticationContext>()))
                .Callback<LocalAuthenticationContext>(ctx =>
                {
                    ctx.AuthenticateResult = new AuthenticateResult("/foo", "tempsub", "tempname");
                })
                .Returns(Task.FromResult(0));

            GetLoginPage();
            var resp = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            resp.StatusCode.Should().Be(HttpStatusCode.Found);
            resp.Headers.Location.AbsoluteUri.Should().Be(Url("foo"));
        }

        [Fact]
        public void PostToLogin_CookieOptions_AllowRememberMeIsFalse_IsPersistentIsFalse_DoesNotIssuePersistentCookie()
        {
            options.AuthenticationOptions.CookieOptions.AllowRememberMe = false;
            options.AuthenticationOptions.CookieOptions.IsPersistent = false;
            GetLoginPage();
            var resp = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            var cookies = resp.GetRawCookies();
            var cookie = cookies.Single(x => x.StartsWith(Constants.PrimaryAuthenticationType + "="));
            cookie.Contains("expires=").Should().BeFalse();
        }

        [Fact]
        public void PostToLogin_CookieOptions_AllowRememberMeIsFalse_IsPersistentIsTrue_IssuesPersistentCookie()
        {
            options.AuthenticationOptions.CookieOptions.AllowRememberMe = false;
            options.AuthenticationOptions.CookieOptions.IsPersistent = true;
            GetLoginPage();
            var resp = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            var cookies = resp.GetRawCookies();
            var cookie = cookies.Single(x => x.StartsWith(Constants.PrimaryAuthenticationType + "="));
            cookie.Contains("expires=").Should().BeTrue();
        }

        [Fact]
        public void PostToLogin_CookieOptions_AllowRememberMeIsTrue_IsPersistentIsTrue_DoNotCheckRememberMe_DoeNotIssuePersistentCookie()
        {
            options.AuthenticationOptions.CookieOptions.AllowRememberMe = true;
            options.AuthenticationOptions.CookieOptions.IsPersistent = true;
            GetLoginPage();
            var resp = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            var cookies = resp.GetRawCookies();
            var cookie = cookies.Single(x => x.StartsWith(Constants.PrimaryAuthenticationType + "="));
            cookie.Contains("expires=").Should().BeFalse();
        }
        [Fact]
        public void PostToLogin_CookieOptions_AllowRememberMeIsTrue_IsPersistentIsTrue_CheckRememberMe_IssuesPersistentCookie()
        {
            options.AuthenticationOptions.CookieOptions.AllowRememberMe = true;
            options.AuthenticationOptions.CookieOptions.IsPersistent = true;
            GetLoginPage();
            var resp = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice", RememberMe = true });
            var cookies = resp.GetRawCookies();
            var cookie = cookies.Single(x => x.StartsWith(Constants.PrimaryAuthenticationType + "="));
            cookie.Contains("expires=").Should().BeTrue();
        }

        [Fact]
        public void PostToLogin_CookieOptionsIsPersistentIsTrueButResponseIsPartialLogin_DoesNotIssuePersistentCookie()
        {
            mockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<LocalAuthenticationContext>()))
                .Callback<LocalAuthenticationContext>(ctx =>
                {
                    ctx.AuthenticateResult = new AuthenticateResult("/foo", "tempsub", "tempname");
                })
                .Returns(Task.FromResult(0));
            
            options.AuthenticationOptions.CookieOptions.IsPersistent = true;
            GetLoginPage();
            var resp = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            var cookies = resp.GetRawCookies();
            var cookie = cookies.Single(x => x.StartsWith(Constants.PartialSignInAuthenticationType + "="));
            cookie.Contains("expires=").Should().BeFalse();
        }

        [Fact]
        public void PostToLogin_PostAuthenticate_IsCalled()
        {
            GetLoginPage();
            var resp = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            mockUserService.Verify(x => x.PostAuthenticateAsync(It.IsAny<PostAuthenticationContext>()));
        }
        
        [Fact]
        public void PostToLogin_PostAuthenticate_is_not_called_for_partial_logins()
        {
            mockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<LocalAuthenticationContext>()))
                .Callback<LocalAuthenticationContext>(ctx =>
                {
                    ctx.AuthenticateResult = new AuthenticateResult("~/partial", "123", "foo", Enumerable.Empty<Claim>());
                }).Returns(Task.FromResult(0));

            GetLoginPage();
            var resp = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            mockUserService.Verify(x => x.PostAuthenticateAsync(It.IsAny<PostAuthenticationContext>()), Times.Never());
        }

        [Fact]
        public void PostToLogin_PostAuthenticate_returns_error_and_error_page_is_rendered_and_user_is_not_logged_in()
        {
            mockUserService.Setup(x => x.PostAuthenticateAsync(It.IsAny<PostAuthenticationContext>()))
                .Callback<PostAuthenticationContext>(ctx =>
                {
                    ctx.AuthenticateResult = new AuthenticateResult("some error");
                }).Returns(Task.FromResult(0));

            GetLoginPage();
            var resp = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            resp.AssertPage("error");

            var cookies = resp.GetRawCookies();
            cookies.Count(x => x.StartsWith(Constants.PrimaryAuthenticationType + "=")).Should().Be(0);
        }

        [Fact]
        public void PostToLogin_PostAuthenticate_returns_partial_login_and_user_is_not_logged_in()
        {
            mockUserService.Setup(x => x.PostAuthenticateAsync(It.IsAny<PostAuthenticationContext>()))
                .Callback<PostAuthenticationContext>(ctx =>
                {
                    ctx.AuthenticateResult = new AuthenticateResult("~/foo", "123", "bob");
                }).Returns(Task.FromResult(0));

            GetLoginPage();
            var resp = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            resp.Headers.Location.AbsoluteUri.Should().Be(Url("foo"));
            var cookies = resp.GetRawCookies();
            cookies.Count(x => x.StartsWith(Constants.PrimaryAuthenticationType + "=")).Should().Be(0);
            cookies.Count(x => x.StartsWith(Constants.PartialSignInAuthenticationType + "=")).Should().Be(1);
        }

        [Fact]
        public void ResumeLoginFromRedirect_WithPartialCookie_IssuesFullLoginCookie()
        {
            mockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<LocalAuthenticationContext>()))
                .Callback<LocalAuthenticationContext>(ctx =>
                {
                    ctx.AuthenticateResult = new AuthenticateResult("/foo", "tempsub", "tempname");
                })
                .Returns(Task.FromResult(0));

            GetLoginPage();
            var resp1 = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            client.SetCookies(resp1.GetCookies());
            var resp2 = Get(GetResumeUrlFromPartialSignInCookie(resp1));
            resp2.AssertCookie(Constants.PrimaryAuthenticationType);
        }

        [Fact]
        public void ResumeLoginFromRedirect_WithPartialCookie_IssuesRedirectToAuthorizationPage()
        {
            mockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<LocalAuthenticationContext>()))
                .Callback<LocalAuthenticationContext>(ctx =>
                {
                    ctx.AuthenticateResult = new AuthenticateResult("/foo", "tempsub", "tempname");
                })
                .Returns(Task.FromResult(0));

            GetLoginPage();
            var resp1 = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            client.SetCookies(resp1.GetCookies());

            var resp2 = Get(GetResumeUrlFromPartialSignInCookie(resp1));
            resp2.StatusCode.Should().Be(HttpStatusCode.Found);
            resp2.Headers.Location.AbsoluteUri.Should().Be(Url("authorize"));
        }

        [Fact]
        public void ResumeLoginFromRedirect_WithoutPartialCookie_ShowsError()
        {
            mockUserService.Setup(x => x.AuthenticateLocalAsync(It.IsAny<LocalAuthenticationContext>()))
                .Callback<LocalAuthenticationContext>(ctx =>
                {
                    ctx.AuthenticateResult = new AuthenticateResult("/foo", "tempsub", "tempname");
                })
                .Returns(Task.FromResult(0));

            GetLoginPage();
            var resp1 = PostForm(GetLoginUrl(), new LoginCredentials { Username = "alice", Password = "alice" });
            var resp2 = Get(GetResumeUrlFromPartialSignInCookie(resp1));
            resp2.AssertPage("error");
        }

        [Fact]
        public void Logout_AnonymousUser_ShowsLoggedOutPage()
        {
            var resp = Get(Constants.RoutePaths.Logout);
            resp.AssertPage("loggedOut");
        }

        [Fact]
        public void Logout_LoggedInUser_ShowsLogoutPromptPage()
        {
            Login();
            
            var resp = Get(Constants.RoutePaths.Logout);
            resp.AssertPage("logout");
        }

        [Fact]
        public void Logout_EnableSignOutPromptSetToFalse_SkipsLogoutPromptPage()
        {
            Login();

            options.AuthenticationOptions.EnableSignOutPrompt = false;
            var resp = Get(Constants.RoutePaths.Logout);
            resp.AssertPage("loggedOut");
        }
        
        [Fact]
        public void Logout_SignOutMessagePassed_SkipsLogoutPromptPage()
        {
            Login();
            
            var id = WriteMessageToCookie(new SignOutMessage { ClientId = "foo", ReturnUrl = "http://foo" });
            var resp = Get(Constants.RoutePaths.Logout + "?id=" + id);
            resp.AssertPage("loggedOut");
        }

        [Fact]
        public void PostToLogout_AnonymousUser_DoesNotInvokeUserServiceSignOut()
        {
            var resp = PostForm(Constants.RoutePaths.Logout, (string)null);
            this.mockUserService.Verify(x => x.SignOutAsync(It.IsAny<SignOutContext>()), Times.Never());
        }
        
        [Fact]
        public void PostToLogout_AuthenticatedUser_InvokesUserServiceSignOut()
        {
            Login();

            var resp = PostForm(Constants.RoutePaths.Logout, (string)null);
            this.mockUserService.Verify(x => x.SignOutAsync(It.IsAny<SignOutContext>()));
        }

        [Fact]
        public void PostToLogout_RemovesCookies()
        {
            Login();

            var resp = PostForm(Constants.RoutePaths.Logout, (string)null);
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

            options.ProtocolLogoutUrls.Add("/foo/signout");
            var resp = PostForm(Constants.RoutePaths.Logout, (string)null);
            var model = resp.GetModel<LoggedOutViewModel>();
            var signOutUrls = model.IFrameUrls.ToArray();
            signOutUrls.Length.Should().Be(2);
            signOutUrls.Should().Contain(Url(Constants.RoutePaths.Oidc.EndSessionCallback));
            signOutUrls.Should().Contain(Url("/foo/signout"));
        }

        [Fact]
        public void LoginExternalCallback_WithoutExternalCookie_RendersErrorPage()
        {
            var msg = new SignInMessage();
            msg.IdP = "Google";
            var resp1 = GetLoginPage(msg);
            var resp2 = client.GetAsync(resp1.Headers.Location.AbsoluteUri).Result;
            var resp3 = Get(Constants.RoutePaths.LoginExternalCallback);
            resp3.AssertPage("error");
        }

        [Fact]
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
            model.ErrorMessage.Should().Be(Messages.NoMatchingExternalAccount);
        }
        
        [Fact]
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
            model.ErrorMessage.Should().Be(Messages.NoMatchingExternalAccount);
        }

        [Fact]
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

        [Fact]
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
        
        [Fact]
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
            resp3.StatusCode.Should().Be(HttpStatusCode.Found);
            resp3.Headers.Location.AbsoluteUri.Should().Be(Url("authorize"));
        }

        [Fact]
        public void LoginExternalCallback_UserServiceReturnsError_ShowsError()
        {
            mockUserService.Setup(x => x.AuthenticateExternalAsync(It.IsAny<ExternalAuthenticationContext>()))
                .Callback<ExternalAuthenticationContext>(ctx =>
                {
                    ctx.AuthenticateResult = new AuthenticateResult("foo bad");
                })
                .Returns(Task.FromResult(0));
            
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
            model.ErrorMessage.Should().Be("foo bad");
        }

        [Fact]
        public void LoginExternalCallback_UserServiceReturnsNull_ShowError()
        {
            mockUserService.Setup(x => x.AuthenticateExternalAsync(It.IsAny<ExternalAuthenticationContext>()))
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
            model.ErrorMessage.Should().Be(Messages.NoMatchingExternalAccount);
        }

        [Fact]
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

            mockUserService.Verify(x => x.AuthenticateExternalAsync(It.IsAny<ExternalAuthenticationContext>()));
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
            var resp = PostForm(Url(Constants.RoutePaths.Logout + "?id=" + id), null);
            var model = resp.GetModel<LoggedOutViewModel>();
            model.RedirectUrl.Should().Be(msg.ReturnUrl);
            model.ClientName.Should().Be(c.ClientName);
        }

        [Fact]
        public void LoggedOut_NoSignOutMessage_ContainsNullForClientNameAndRedirectUrlInPage()
        {
            GetLoginPage();
            var resp = PostForm(Url(Constants.RoutePaths.Logout), null);
            var model = resp.GetModel<LoggedOutViewModel>();
            model.RedirectUrl.Should().BeNull();
            model.ClientName.Should().BeNull();
        }

        [Fact]
        public void LoggedOut_InvalidSignOutMessageId_ContainsNullForClientNameAndRedirectUrlInPage()
        {
            GetLoginPage();
            var resp = PostForm(Url(Constants.RoutePaths.Logout + "?id=123"), null);
            var model = resp.GetModel<LoggedOutViewModel>();
            model.RedirectUrl.Should().BeNull();
            model.ClientName.Should().BeNull();
        }

        [Fact]
        public void Login_ClientFiltersAllowedIdentityProviders_OnlyAllowedIdPsInLoginPage()
        {
            var msg = new SignInMessage() { ReturnUrl = Url("authorize"), ClientId = "no_external_idps" };
            var resp = GetLoginPage(msg);
            var model = resp.GetModel<LoginViewModel>();
            var google = model.ExternalProviders.SingleOrDefault(x => x.Text == "Google");
            google.Should().BeNull();
        }
        
        [Fact]
        public void Login_ClientDoesNotFiltersAllowedIdentityProviders_ExternalIsInLoginPage()
        {
            var msg = new SignInMessage() { ReturnUrl = Url("authorize"), ClientId = "any_external_idps" };
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
            var msg = new SignInMessage() { ReturnUrl = Url("authorize"), ClientId = "bad_id" };
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
            var resp = PostForm(Url(Constants.RoutePaths.Login + "?signin="), (object)null);
            resp.AssertPage("error");
        }

        [Fact]
        public void Logout_PostWithoutXsrf_ReturnsError()
        {
            var resp = PostForm(Url(Constants.RoutePaths.Logout), (object)null);
            resp.AssertPage("error");
        }

        string GetLongString()
        {
            string value = "x";
            var parts = new string[IdentityServer3.Core.Endpoints.AuthenticationController.MaxInputParamLength+1];
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
            options.AuthenticationOptions.SignInMessageThreshold = signInMessageThreshold;

            for (var i = 0; i < signInMessageThreshold; i++)
            {
                GetLoginPage();
            }

            var theNextRequest = GetLoginPage();
            theNextRequest.RequestMessage.Headers
                .GetValues("Cookie")
                .Count(c => c.StartsWith("SignInMessage."))
                .Should()
                .Be(options.AuthenticationOptions.SignInMessageThreshold);
        }

        [Fact]
        public void GetLogin_SigninMessageThresholdSetToX_GetLoginMoreThanXTimesOnlyLatestXMessagesAreKept()
        {
            options.AuthenticationOptions.SignInMessageThreshold = 3;
            var moreThanSignInThreshold = options.AuthenticationOptions.SignInMessageThreshold + 1;

            for (var i = 0; i < moreThanSignInThreshold; i++)
            {
                GetLoginPage();
            }

            var theNextRequest = GetLoginPage();
            theNextRequest.RequestMessage.Headers
                .GetValues("Cookie")
                .Count(c => c.StartsWith("SignInMessage."))
                .Should()
                .Be(options.AuthenticationOptions.SignInMessageThreshold);
        }

        [Fact]
        public void GetLogin_SigninMessageThresholdSetToZero_OneSignInMessageKept()
        {
            options.AuthenticationOptions.SignInMessageThreshold = 0;

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
            options.AuthenticationOptions.SignInMessageThreshold = -42;

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
            var resp = GetLoginPage(msg);
            resp.AssertPage("error");
        }

        [Fact]
        public void GetLoginExternal_SignInIdTooLong_ReturnsError()
        {
            var url = Url(Constants.RoutePaths.LoginExternal) + "?signin=" + GetLongString() + "&provider=Google";
            var resp = client.GetAsync(url).Result;
            resp.AssertPage("error");
        }

        [Fact]
        public void GetLoginExternalCallback_ErrorTooLong_ReturnsError()
        {
            var url = Url(Constants.RoutePaths.LoginExternalCallback) + "?error=" + GetLongString();
            var resp = client.GetAsync(url).Result;
            resp.AssertPage("error");
        }

        [Fact]
        public void ResumeLogin_ResumeIdTooLong_ReturnsError()
        {
            var url = Url(Constants.RoutePaths.ResumeLoginFromRedirect) + "?resume=" + GetLongString();
            var resp = client.GetAsync(url).Result;
            resp.AssertPage("error");
        }

        [Fact]
        public void LogoutPrompt_SignOutIdTooLong_ReturnsError()
        {
            var url = Url(Constants.RoutePaths.Logout) + "?id=" + GetLongString();
            var resp = client.GetAsync(url).Result;
            resp.AssertPage("error");
        }
        
        [Fact]
        public void LogoutSubmit_SignOutIdTooLong_ReturnsError()
        {
            Login();

            var url = Constants.RoutePaths.Logout + "?id=" + GetLongString();
            var resp = PostForm(url, new { });
            resp.AssertPage("error");
        }
    }
}

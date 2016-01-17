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
using IdentityServer3.Core.Services;
using IdentityServer3.Tests.Endpoints;
using System.Net;
using System.Net.Http;
using Xunit;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.ViewModels;

namespace IdentityServer3.Tests.Connect.Endpoints
{
    public class AuthenticationSessionValidatorTests : IdSvrHostTestBase
    {
        public class StubValidator : IAuthenticationSessionValidator
        {
            public bool Response { get; set; }
            public Task<bool> IsAuthenticationSessionValidAsync(ClaimsPrincipal subject)
            {
                return Task.FromResult(Response);
            }
        }

        HttpResponseMessage GetAuthorizePage()
        {
            var resp = Get(Constants.RoutePaths.Oidc.Authorize);
            ProcessXsrf(resp);
            return resp;
        }

        public AuthenticationSessionValidatorTests()
        {
            ConfigureIdentityServerOptions = opts =>
            {
                opts.Factory.AuthenticationSessionValidator = new Registration<IAuthenticationSessionValidator>(_stubValidator);
            };
            base.Init();
        }

        StubValidator _stubValidator = new StubValidator();

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

        [Fact]
        public void GetAuthorize_UserLoggedIn_ValidatorReturnsTrue_ReturnsConsentPage()
        {
            _stubValidator.Response = true;
            Login();

            var resp = Get(Constants.RoutePaths.Oidc.Authorize + "?client_id=implicitclient&redirect_uri=http://localhost:21575/index.html&response_type=id_token&scope=openid&nonce=123");

            resp.AssertPage("consent");
        }

        [Fact]
        public void GetAuthorize_UserLoggedIn_ValidatorReturnsFalse_ReturnsLogin()
        {
            _stubValidator.Response = false;
            Login();

            var resp = Get(Constants.RoutePaths.Oidc.Authorize + "?client_id=implicitclient&redirect_uri=http://localhost:21575/index.html&response_type=id_token&scope=openid&nonce=123");

            resp.StatusCode.Should().Be(HttpStatusCode.Redirect);
            resp.Headers.Location.AbsolutePath.Should().Be("/" + Constants.RoutePaths.Login);
        }
    }
}

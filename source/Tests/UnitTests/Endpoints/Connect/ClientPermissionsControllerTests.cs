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

using System.Linq;
using System.Net;
using FluentAssertions;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Resources;
using Thinktecture.IdentityServer.Core.ViewModels;
using Thinktecture.IdentityServer.Tests.Endpoints.Setup;
using Xunit;

namespace Thinktecture.IdentityServer.Tests.Endpoints.Connect
{
    
    public class ClientPermissionsControllerTests : IdSvrHostTestBase
    {
        readonly string _clientId;

        
        public ClientPermissionsControllerTests()
        {
            _clientId = TestClients.Get().First().ClientId;
        }

        void Login(bool setCookie = true)
        {
            var msg = new SignInMessage() { ReturnUrl = Url("authorize") };
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

        [Fact]
        public void ShowPermissions_RendersPermissionPage()
        {
            Login();
            var resp = Get(Constants.RoutePaths.CLIENT_PERMISSIONS);
            resp.AssertPage("permissions");
        }

        [Fact]
        public void ShowPermissions_EndpointDisabled_ReturnsNotFound()
        {
            Options.Endpoints.EnableClientPermissionsEndpoint = false;
            Login();
            var resp = Get(Constants.RoutePaths.CLIENT_PERMISSIONS);
            resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public void RevokePermission_EndpointDisabled_ReturnsNotFound()
        {
            Options.Endpoints.EnableClientPermissionsEndpoint = false;
            Login();
            var resp = PostForm(Constants.RoutePaths.CLIENT_PERMISSIONS, new { ClientId = _clientId });
            resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public void RevokePermission_JsonMediaType_ReturnsUnsupportedMediaType()
        {
            Login();
            var resp = Post(Constants.RoutePaths.Oidc.CONSENT, new { ClientId = _clientId });
            resp.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
        }

        [Fact]
        public void RevokePermission_NoAntiCsrf_ReturnsErrorPage()
        {
            Login();
            var resp = PostForm(Constants.RoutePaths.Oidc.CONSENT, new { ClientId = _clientId }, includeCsrf: false);
            resp.AssertPage("error");
        }
        
        [Fact]
        public void RevokePermission_NoBody_ShowsError()
        {
            Login();
            var resp = PostForm(Constants.RoutePaths.CLIENT_PERMISSIONS, null);
            var model = resp.GetModel<ClientPermissionsViewModel>();
            model.ErrorMessage.Should().Be(Messages.ClientIdRequired);
        }

        [Fact]
        public void RevokePermission_NoClient_ShowsError()
        {
            Login();
            var resp = PostForm(Constants.RoutePaths.CLIENT_PERMISSIONS, new { ClientId = "" });
            var model = resp.GetModel<ClientPermissionsViewModel>();
            model.ErrorMessage.Should().Be(Messages.ClientIdRequired);
        }

        [Fact]
        public void ShowPermissions_Unauthenticated_ShowsLoginPage()
        {
            Login(false);
            var resp = Get(Constants.RoutePaths.CLIENT_PERMISSIONS);
            resp.StatusCode.Should().Be(HttpStatusCode.Redirect);
            resp.Headers.Location.AbsoluteUri.Should().Contain(Constants.RoutePaths.LOGIN);
        }
        
        [Fact]
        public void RevokePermissions_Unauthenticated_ShowsLoginPage()
        {
            Login(false);
            var resp = PostForm(Constants.RoutePaths.CLIENT_PERMISSIONS, new { ClientId = _clientId });
            resp.StatusCode.Should().Be(HttpStatusCode.Redirect);
            resp.Headers.Location.AbsoluteUri.Should().Contain(Constants.RoutePaths.LOGIN);
        }
    }
}

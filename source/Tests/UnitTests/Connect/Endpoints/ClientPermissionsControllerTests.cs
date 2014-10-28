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
using System.Linq;
using System.Net;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Resources;
using Thinktecture.IdentityServer.Core.ViewModels;

namespace Thinktecture.IdentityServer.Tests.Connect.Endpoints
{
    [TestClass]
    public class ClientPermissionsControllerTests : IdSvrHostTestBase
    {
        string clientId;

        [TestInitialize]
        public new void Init()
        {
            base.Init();
            clientId = TestClients.Get().First().ClientId;
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

        [TestMethod]
        public void ShowPermissions_RendersPermissionPage()
        {
            Login();
            var resp = Get(Constants.RoutePaths.ClientPermissions);
            resp.AssertPage("permissions");
        }

        [TestMethod]
        public void ShowPermissions_EndpointDisabled_ReturnsNotFound()
        {
            base.options.Endpoints.ClientPermissionsEndpoint.IsEnabled = false;
            Login();
            var resp = Get(Constants.RoutePaths.ClientPermissions);
            Assert.AreEqual(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [TestMethod]
        public void RevokePermission_EndpointDisabled_ReturnsNotFound()
        {
            base.options.Endpoints.ClientPermissionsEndpoint.IsEnabled = false;
            Login();
            var resp = PostForm(Constants.RoutePaths.ClientPermissions, new { ClientId = clientId });
            Assert.AreEqual(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [TestMethod]
        public void RevokePermission_JsonMediaType_ReturnsUnsupportedMediaType()
        {
            Login();
            var resp = Post(Constants.RoutePaths.Oidc.Consent, new { ClientId = clientId });
            Assert.AreEqual(HttpStatusCode.UnsupportedMediaType, resp.StatusCode);
        }

        [TestMethod]
        public void RevokePermission_NoAntiCsrf_ReturnsErrorPage()
        {
            Login();
            var resp = PostForm(Constants.RoutePaths.Oidc.Consent, new { ClientId = clientId }, includeCsrf: false);
            resp.AssertPage("error");
        }
        
        [TestMethod]
        public void RevokePermission_NoBody_ShowsError()
        {
            Login();
            var resp = PostForm(Constants.RoutePaths.ClientPermissions, (object)null);
            var model = resp.GetModel<ClientPermissionsViewModel>();
            Assert.AreEqual(Messages.ClientIdRequired, model.ErrorMessage);
        }

        [TestMethod]
        public void RevokePermission_NoClient_ShowsError()
        {
            Login();
            var resp = PostForm(Constants.RoutePaths.ClientPermissions, new { ClientId = "" });
            var model = resp.GetModel<ClientPermissionsViewModel>();
            Assert.AreEqual(Messages.ClientIdRequired, model.ErrorMessage);
        }

        [TestMethod]
        public void ShowPermissions_Unauthenticated_ShowsLoginPage()
        {
            Login(false);
            var resp = Get(Constants.RoutePaths.ClientPermissions);
            Assert.AreEqual(HttpStatusCode.Redirect, resp.StatusCode);
            StringAssert.Contains(resp.Headers.Location.AbsoluteUri, Constants.RoutePaths.Login);
        }
        
        [TestMethod]
        public void RevokePermissions_Unauthenticated_ShowsLoginPage()
        {
            Login(false);
            var resp = PostForm(Constants.RoutePaths.ClientPermissions, new { ClientId = clientId });
            Assert.AreEqual(HttpStatusCode.Redirect, resp.StatusCode);
            StringAssert.Contains(resp.Headers.Location.AbsoluteUri, Constants.RoutePaths.Login);
        }
    }
}

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
using System.Net;
using System.Net.Http;
using Thinktecture.IdentityServer.Core;

namespace Thinktecture.IdentityServer.Tests.Connect.Endpoints
{
    [TestClass]
    public class AuthorizeEndpointControllerTests : IdSvrHostTestBase
    {
        HttpResponseMessage GetAuthorizePage()
        {
            var resp = Get(Constants.RoutePaths.Oidc.Authorize);
            ProcessXsrf(resp);
            return resp;
        }

        [TestMethod]
        public void GetAuthorize_AuthorizeEndpointDisabled_ReturnsNotFound()
        {
            base.options.Endpoints.AuthorizeEndpoint.IsEnabled = false;
            var resp = Get(Constants.RoutePaths.Oidc.Authorize);
            Assert.AreEqual(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [TestMethod]
        public void GetAuthorize_NoQueryStringParams_ReturnsErrorPage()
        {
            var resp = Get(Constants.RoutePaths.Oidc.Authorize);
            resp.AssertPage("error");
        }

        [TestMethod]
        public void PostConsent_JsonMediaType_ReturnsUnsupportedMediaType()
        {
            var resp = Post(Constants.RoutePaths.Oidc.Consent, (object)null);
            Assert.AreEqual(HttpStatusCode.UnsupportedMediaType, resp.StatusCode);
        }

        [TestMethod]
        public void PostConsent_NoAntiCsrf_ReturnsErrorPage()
        {
            var resp = PostForm(Constants.RoutePaths.Oidc.Consent, (object)null);
            resp.AssertPage("error");
        }
        
        [TestMethod]
        public void PostConsent_NoBody_ReturnsErrorPage()
        {
            GetAuthorizePage();
            var resp = PostForm(Constants.RoutePaths.Oidc.Consent, (object)null);
            resp.AssertPage("error");
        }
    }
}

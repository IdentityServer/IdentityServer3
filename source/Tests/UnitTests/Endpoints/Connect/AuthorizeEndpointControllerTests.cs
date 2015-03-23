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

using System.Net;
using FluentAssertions;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Tests.Endpoints.Setup;
using Xunit;

namespace Thinktecture.IdentityServer.Tests.Endpoints.Connect
{
    
    public class AuthorizeEndpointControllerTests : IdSvrHostTestBase
    {
        void GetAuthorizePage()
        {
            var resp = Get(Constants.RoutePaths.Oidc.AUTHORIZE);
            ProcessXsrf(resp);
        }

        [Fact]
        public void GetAuthorize_AuthorizeEndpointDisabled_ReturnsNotFound()
        {
            Options.Endpoints.EnableAuthorizeEndpoint = false;
            var resp = Get(Constants.RoutePaths.Oidc.AUTHORIZE);
            resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public void GetAuthorize_NoQueryStringParams_ReturnsErrorPage()
        {
            var resp = Get(Constants.RoutePaths.Oidc.AUTHORIZE);
            resp.AssertPage("error");
        }

        [Fact]
        public void PostConsent_JsonMediaType_ReturnsUnsupportedMediaType()
        {
            var resp = Post(Constants.RoutePaths.Oidc.CONSENT, (object)null);
            resp.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
        }

        [Fact]
        public void PostConsent_NoAntiCsrf_ReturnsErrorPage()
        {
            var resp = PostForm(Constants.RoutePaths.Oidc.CONSENT, null);
            resp.AssertPage("error");
        }
        
        [Fact]
        public void PostConsent_NoBody_ReturnsErrorPage()
        {
            GetAuthorizePage();
            var resp = PostForm(Constants.RoutePaths.Oidc.CONSENT, null);
            resp.AssertPage("error");
        }
    }
}

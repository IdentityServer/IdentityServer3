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
using IdentityServer3.Tests.Endpoints;
using System.Net;
using System.Net.Http;
using Xunit;

namespace IdentityServer3.Tests.Connect.Endpoints
{
    
    public class AuthorizeEndpointControllerTests : IdSvrHostTestBase
    {
        HttpResponseMessage GetAuthorizePage()
        {
            var resp = Get(Constants.RoutePaths.Oidc.Authorize);
            ProcessXsrf(resp);
            return resp;
        }

        [Fact]
        public void GetAuthorize_AuthorizeEndpointDisabled_ReturnsNotFound()
        {
            base.options.Endpoints.EnableAuthorizeEndpoint = false;
            var resp = Get(Constants.RoutePaths.Oidc.Authorize);
            resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public void GetAuthorize_NoQueryStringParams_ReturnsErrorPage()
        {
            var resp = Get(Constants.RoutePaths.Oidc.Authorize);
            resp.AssertPage("error");
        }

        [Fact]
        public void PostConsent_JsonMediaType_ReturnsUnsupportedMediaType()
        {
            var resp = Post(Constants.RoutePaths.Oidc.Consent, (object)null);
            resp.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
        }

        [Fact]
        public void PostConsent_NoAntiCsrf_ReturnsErrorPage()
        {
            var resp = PostForm(Constants.RoutePaths.Oidc.Consent, (object)null);
            resp.AssertPage("error");
        }
        
        [Fact]
        public void PostConsent_NoBody_ReturnsErrorPage()
        {
            GetAuthorizePage();
            var resp = PostForm(Constants.RoutePaths.Oidc.Consent, (object)null);
            resp.AssertPage("error");
        }
    }
}

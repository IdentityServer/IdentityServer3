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
using IdentityServer3.Core.Configuration.Hosting;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using CorsPolicy = System.Web.Cors.CorsPolicy;

namespace IdentityServer3.Tests.Configuration
{
    internal class TestCorsPolicyProvider : CorsPolicyProvider
    {
        public TestCorsPolicyProvider(IEnumerable<string> paths)
            : base(paths)
        {
        }

        protected override Task<bool> IsOriginAllowed(string origin, IDictionary<string, object> env)
        {
            return Task.FromResult(true);
        }
    }

    public class CorsPolicyProviderTests
    {
        IOwinRequest Request(string origin = null, string path = null)
        {
            var env = new Dictionary<string, object>();
            env.Add("owin.RequestScheme", "https");
            env.Add("owin.RequestPathBase", "");
            env.Add("owin.RequestPath", path);

            var headers = new Dictionary<string, string[]>();
            headers.Add("Host", new string[]{"identityserver.io"});
            env.Add("owin.RequestHeaders", headers);

            var ctx = new OwinContext(env);
            ctx.Request.Path = new PathString(path);
            if (origin != null)
            {
                ctx.Request.Headers.Add("Origin", new string[] { origin });
            }
            return ctx.Request;
        }

        void AssertAllowed(string origin, CorsPolicy cp)
        {
            cp.AllowAnyHeader.Should().BeTrue();
            cp.AllowAnyMethod.Should().BeTrue();
            cp.Origins.Count.Should().Be(1);
            cp.Origins.Should().Contain(origin);
        }

        [Fact]
        public void ctor_NullPaths_Throws()
        {
            Action act = () => new TestCorsPolicyProvider(null);

            act.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("allowedPaths");
        }

        [Fact]
        public void GetCorsPolicyAsync_MatchingPath_AllowsOrigin()
        {
            var origin = "http://foo.com";
            var path = "/bar";

            var subject = new TestCorsPolicyProvider(new string[] { path });

            var cp = subject.GetCorsPolicyAsync(Request(origin, path)).Result;
            AssertAllowed(origin, cp);
        }
        
        [Fact]
        public void GetCorsPolicyAsync_NoOrigin_DoesNotAllowrigin()
        {
            string origin = null;
            var path = "/bar";

            var subject = new TestCorsPolicyProvider(new string[] { path });

            var cp = subject.GetCorsPolicyAsync(Request(origin, path)).Result;
            cp.Should().BeNull();
        }

        [Fact]
        public void GetCorsPolicyAsync_NoMatchingPath_DoesNotAllowOrigin()
        {
            var origin = "http://foo.com";
            var path = "/bar";

            var subject = new TestCorsPolicyProvider(new string[] { path });

            var cp = subject.GetCorsPolicyAsync(Request(origin, "/baz")).Result;
            cp.Should().BeNull();
        }

        [Fact]
        public void GetCorsPolicyAsync_MatchingPaths_AllowsOrigin()
        {
            var origin = "http://foo.com";

            var subject = new TestCorsPolicyProvider(new string[] { "/bar", "/baz", "/quux" });

            var cp = subject.GetCorsPolicyAsync(Request(origin, "/baz")).Result;
            AssertAllowed(origin, cp);
        }

        [Fact]
        public void GetCorsPolicyAsync_NoMatchingPaths_DoesNotAllowOrigin()
        {
            var origin = "http://foo.com";

            var subject = new TestCorsPolicyProvider(new string[] { "/bar", "/baz", "/quux" });

            var cp = subject.GetCorsPolicyAsync(Request(origin, "/bad")).Result;
            cp.Should().BeNull();
        }

        [Fact]
        public void GetCorsPolicyAsync_PathDoesNotStartWithSlash_NormalizesPathCorrectly()
        {
            var origin = "http://foo.com";

            var subject = new TestCorsPolicyProvider(new string[] { "bar" });

            var cp = subject.GetCorsPolicyAsync(Request(origin, "/bar")).Result;
            AssertAllowed(origin, cp);
        }

        [Fact]
        public void GetCorsPolicyAsync_PathEndsWithSlash_NormalizesPathCorrectly()
        {
            var origin = "http://foo.com";

            var subject = new TestCorsPolicyProvider(new string[] { "bar/" });

            var cp = subject.GetCorsPolicyAsync(Request(origin, "/bar")).Result;
            AssertAllowed(origin, cp);
        }
    }
}

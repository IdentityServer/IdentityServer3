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
using System.Threading.Tasks;
using System.Web.Cors;
using FluentAssertions;
using Microsoft.Owin;
using Thinktecture.IdentityServer.Core.Configuration.Hosting;
using Xunit;

namespace Thinktecture.IdentityServer.Tests.Configuration
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
            var env = new Dictionary<string, object> {{"owin.RequestHeaders", new Dictionary<string, string[]>()}};

            var ctx = new OwinContext(env);
            ctx.Request.Path = new PathString(path);
            if (origin != null)
            {
                ctx.Request.Headers.Add("Origin", new[] { origin });
            }
            return ctx.Request;
        }

        static void AssertAllowed(string origin, CorsPolicy cp)
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
            const string origin = "http://foo.com";
            const string path = "/bar";

            var subject = new TestCorsPolicyProvider(new[] { path });

            var cp = subject.GetCorsPolicyAsync(Request(origin, path)).Result;
            AssertAllowed(origin, cp);
        }
        
        [Fact]
        public void GetCorsPolicyAsync_NoOrigin_DoesNotAllowrigin()
        {
            const string path = "/bar";

            var subject = new TestCorsPolicyProvider(new[] { path });

            var cp = subject.GetCorsPolicyAsync(Request(null, path)).Result;
            cp.Should().BeNull();
        }

        [Fact]
        public void GetCorsPolicyAsync_NoMatchingPath_DoesNotAllowOrigin()
        {
            const string origin = "http://foo.com";
            const string path = "/bar";

            var subject = new TestCorsPolicyProvider(new[] { path });

            var cp = subject.GetCorsPolicyAsync(Request(origin, "/baz")).Result;
            cp.Should().BeNull();
        }

        [Fact]
        public void GetCorsPolicyAsync_MatchingPaths_AllowsOrigin()
        {
            const string origin = "http://foo.com";

            var subject = new TestCorsPolicyProvider(new[] { "/bar", "/baz", "/quux" });

            var cp = subject.GetCorsPolicyAsync(Request(origin, "/baz")).Result;
            AssertAllowed(origin, cp);
        }

        [Fact]
        public void GetCorsPolicyAsync_NoMatchingPaths_DoesNotAllowOrigin()
        {
            const string origin = "http://foo.com";

            var subject = new TestCorsPolicyProvider(new[] { "/bar", "/baz", "/quux" });

            var cp = subject.GetCorsPolicyAsync(Request(origin, "/bad")).Result;
            cp.Should().BeNull();
        }

        [Fact]
        public void GetCorsPolicyAsync_PathDoesNotStartWithSlash_NormalizesPathCorrectly()
        {
            const string origin = "http://foo.com";

            var subject = new TestCorsPolicyProvider(new[] { "bar" });

            var cp = subject.GetCorsPolicyAsync(Request(origin, "/bar")).Result;
            AssertAllowed(origin, cp);
        }

        [Fact]
        public void GetCorsPolicyAsync_PathEndsWithSlash_NormalizesPathCorrectly()
        {
            const string origin = "http://foo.com";

            var subject = new TestCorsPolicyProvider(new[] { "bar/" });

            var cp = subject.GetCorsPolicyAsync(Request(origin, "/bar")).Result;
            AssertAllowed(origin, cp);
        }
    }
}

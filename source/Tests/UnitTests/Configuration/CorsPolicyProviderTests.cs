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
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Configuration.Hosting;
using Xunit;
using CorsPolicy = System.Web.Cors.CorsPolicy;

namespace Thinktecture.IdentityServer.Tests.Configuration
{
    public class CorsPolicyProviderTests
    {
        IOwinRequest Request(string origin = null, string path = null)
        {
            var env = new Dictionary<string, object>();
            env.Add("owin.RequestHeaders", new Dictionary<string, string[]>());

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
        public void ctor_NullPolicy_Throws()
        {
            Action act = () => new CorsPolicyProvider(null, new [] { "/" });

            act.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("policy");
        }

        [Fact]
        public void GetCorsPolicyAsync_NoAllowedOriginsNoCallback_DoesNotAllowOrigin()
        {
            var policy = new Core.Configuration.CorsPolicy();
            var subject = new CorsPolicyProvider(policy, new string[] { "/" });
            var cp = subject.GetCorsPolicyAsync(Request("http://foo.com")).Result;
            cp.Should().BeNull();
        }
        
        [Fact]
        public void GetCorsPolicyAsync_PolicyAllowsAll_AllowsRandomOrigin()
        {
            var policy = Core.Configuration.CorsPolicy.AllowAll;
            var subject = new CorsPolicyProvider(policy, new string[] { "/" });

            var rnd = new Random().Next();
            var origin = "http://foo" + rnd + ".com";
            var cp = subject.GetCorsPolicyAsync(Request(origin)).Result;
            AssertAllowed(origin, cp);
        }

        [Fact]
        public void GetCorsPolicyAsync_OriginIsInAllowedOrigins_AllowOrigin()
        {
            var origin = "http://foo.com";
            var policy = new Core.Configuration.CorsPolicy();
            policy.AllowedOrigins.Add(origin);
            var subject = new CorsPolicyProvider(policy, new string[] { "/" });

            var cp = subject.GetCorsPolicyAsync(Request(origin)).Result;
            AssertAllowed(origin, cp);
        }

        [Fact]
        public void GetCorsPolicyAsync_OriginIsInAllowedOriginsButNoOriginRequested_DoesNotAllowOrigin()
        {
            var origin = "http://foo.com";
            var policy = new Core.Configuration.CorsPolicy();
            policy.AllowedOrigins.Add(origin);
            var subject = new CorsPolicyProvider(policy, new string[] { "/" });

            var cp = subject.GetCorsPolicyAsync(Request(null)).Result;
            cp.Should().BeNull();
        }

        [Fact]
        public void GetCorsPolicyAsync_OriginIsNotInAllowedOrigins_DoesNotAllowOrigin()
        {
            var origin = "http://foo.com";
            var policy = new Core.Configuration.CorsPolicy();
            policy.AllowedOrigins.Add(origin);
            var subject = new CorsPolicyProvider(policy, new string[] { "/" });

            var cp = subject.GetCorsPolicyAsync(Request("http://bar.com")).Result;
            cp.Should().BeNull();
        }
        
        [Fact]
        public void GetCorsPolicyAsync_NoOriginRequested_DoesNotAllowOrigin()
        {
            var origin = "http://foo.com";
            var policy = new Core.Configuration.CorsPolicy();
            policy.AllowedOrigins.Add(origin);
            var subject = new CorsPolicyProvider(policy, new string[] { "/" });

            var cp = subject.GetCorsPolicyAsync(Request()).Result;
            cp.Should().BeNull();
        }

        [Fact]
        public void GetCorsPolicyAsync_CallbackAllowOrigin_AllowOrigin()
        {
            var origin = "http://foo.com";
            var policy = new Core.Configuration.CorsPolicy();
            policy.PolicyCallback = o => Task.FromResult(true);
            var subject = new CorsPolicyProvider(policy, new string[] { "/" });

            var cp = subject.GetCorsPolicyAsync(Request(origin)).Result;
            AssertAllowed(origin, cp);
        }

        [Fact]
        public void GetCorsPolicyAsync_CallbackAllowOriginButNoOriginRequested_DoesNotAllowOrigin()
        {
            var policy = new Core.Configuration.CorsPolicy();
            policy.PolicyCallback = o => Task.FromResult(true);
            var subject = new CorsPolicyProvider(policy, new string[] { "/" });

            var cp = subject.GetCorsPolicyAsync(Request()).Result;
            cp.Should().BeNull();
        }

        [Fact]
        public void GetCorsPolicyAsync_CallbackDoesNotAllowOrigin_DoesNotAllowOrigin()
        {
            var origin = "http://foo.com";
            var policy = new Core.Configuration.CorsPolicy();
            policy.PolicyCallback = o => Task.FromResult(false);
            var subject = new CorsPolicyProvider(policy, new string[] { "/" });

            var cp = subject.GetCorsPolicyAsync(Request(origin)).Result;
            cp.Should().BeNull();
        }

        [Fact]
        public void ctor_NullPaths_Throws()
        {
            Action act = () => new CorsPolicyProvider(new Core.Configuration.CorsPolicy(), null);

            act.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("allowedPaths");
        }

        [Fact]
        public void GetCorsPolicyAsync_MatchingPath_AllowsOrigin()
        {
            var origin = "http://foo.com";
            var path = "/bar";
            var policy = Core.Configuration.CorsPolicy.AllowAll;

            var subject = new CorsPolicyProvider(policy, new string[] { path });

            var cp = subject.GetCorsPolicyAsync(Request(origin, path)).Result;
            AssertAllowed(origin, cp);
        }

        [Fact]
        public void GetCorsPolicyAsync_NoMatchingPath_DoesNotAllowOrigin()
        {
            var origin = "http://foo.com";
            var path = "/bar";
            var policy = Core.Configuration.CorsPolicy.AllowAll;

            var subject = new CorsPolicyProvider(policy, new string[] { path });

            var cp = subject.GetCorsPolicyAsync(Request(origin, "/baz")).Result;
            cp.Should().BeNull();
        }

        [Fact]
        public void GetCorsPolicyAsync_MatchingPaths_AllowsOrigin()
        {
            var origin = "http://foo.com";
            var policy = Core.Configuration.CorsPolicy.AllowAll;

            var subject = new CorsPolicyProvider(policy, new string[] { "/bar", "/baz", "/quux" });

            var cp = subject.GetCorsPolicyAsync(Request(origin, "/baz")).Result;
            AssertAllowed(origin, cp);
        }

        [Fact]
        public void GetCorsPolicyAsync_NoMatchingPaths_DoesNotAllowOrigin()
        {
            var origin = "http://foo.com";
            var policy = Core.Configuration.CorsPolicy.AllowAll;

            var subject = new CorsPolicyProvider(policy, new string[] { "/bar", "/baz", "/quux" });

            var cp = subject.GetCorsPolicyAsync(Request(origin, "/bad")).Result;
            cp.Should().BeNull();
        }

        [Fact]
        public void GetCorsPolicyAsync_PathDoesNotStartWithSlash_NormalizesPathCorrectly()
        {
            var origin = "http://foo.com";
            var policy = Core.Configuration.CorsPolicy.AllowAll;

            var subject = new CorsPolicyProvider(policy, new string[] { "bar" });

            var cp = subject.GetCorsPolicyAsync(Request(origin, "/bar")).Result;
            AssertAllowed(origin, cp);
        }

        [Fact]
        public void GetCorsPolicyAsync_PathEndsWithSlash_NormalizesPathCorrectly()
        {
            var origin = "http://foo.com";
            var policy = Core.Configuration.CorsPolicy.AllowAll;

            var subject = new CorsPolicyProvider(policy, new string[] { "bar/" });

            var cp = subject.GetCorsPolicyAsync(Request(origin, "/bar")).Result;
            AssertAllowed(origin, cp);
        }
    }
}

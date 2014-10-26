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
using Microsoft.Owin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Configuration;

namespace Thinktecture.IdentityServer.Tests.Configuration
{
    [TestClass]
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

        void AssertAllowed(string origin, System.Web.Cors.CorsPolicy cp)
        {
            Assert.IsTrue(cp.AllowAnyHeader);
            Assert.IsTrue(cp.AllowAnyMethod);
            Assert.AreEqual(1, cp.Origins.Count);
            CollectionAssert.Contains(cp.Origins.ToArray(), origin);
        }

        [TestMethod]
        public void ctor_NullPolicy_Throws()
        {
            try
            {
                new CorsPolicyProvider(null, new string[] { "/" });
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("policy", ex.ParamName);
            }
        }

        [TestMethod]
        public void GetCorsPolicyAsync_NoAllowedOriginsNoCallback_DoesNotAllowOrigin()
        {
            var policy = new CorsPolicy();
            var subject = new CorsPolicyProvider(policy, new string[] { "/" });
            var cp = subject.GetCorsPolicyAsync(Request("http://foo.com")).Result;
            Assert.IsNull(cp);
        }
        
        [TestMethod]
        public void GetCorsPolicyAsync_PolicyAllowsAll_AllowsRandomOrigin()
        {
            var policy = CorsPolicy.AllowAll;
            var subject = new CorsPolicyProvider(policy, new string[] { "/" });

            var rnd = new Random().Next();
            var origin = "http://foo" + rnd + ".com";
            var cp = subject.GetCorsPolicyAsync(Request(origin)).Result;
            AssertAllowed(origin, cp);
        }

        [TestMethod]
        public void GetCorsPolicyAsync_OriginIsInAllowedOrigins_AllowOrigin()
        {
            var origin = "http://foo.com";
            var policy = new CorsPolicy();
            policy.AllowedOrigins.Add(origin);
            var subject = new CorsPolicyProvider(policy, new string[] { "/" });

            var cp = subject.GetCorsPolicyAsync(Request(origin)).Result;
            AssertAllowed(origin, cp);
        }

        [TestMethod]
        public void GetCorsPolicyAsync_OriginIsInAllowedOriginsButNoOriginRequested_DoesNotAllowOrigin()
        {
            var origin = "http://foo.com";
            var policy = new CorsPolicy();
            policy.AllowedOrigins.Add(origin);
            var subject = new CorsPolicyProvider(policy, new string[] { "/" });

            var cp = subject.GetCorsPolicyAsync(Request(null)).Result;
            Assert.IsNull(cp);
        }

        [TestMethod]
        public void GetCorsPolicyAsync_OriginIsNotInAllowedOrigins_DoesNotAllowOrigin()
        {
            var origin = "http://foo.com";
            var policy = new CorsPolicy();
            policy.AllowedOrigins.Add(origin);
            var subject = new CorsPolicyProvider(policy, new string[] { "/" });

            var cp = subject.GetCorsPolicyAsync(Request("http://bar.com")).Result;
            Assert.IsNull(cp);
        }
        
        [TestMethod]
        public void GetCorsPolicyAsync_NoOriginRequested_DoesNotAllowOrigin()
        {
            var origin = "http://foo.com";
            var policy = new CorsPolicy();
            policy.AllowedOrigins.Add(origin);
            var subject = new CorsPolicyProvider(policy, new string[] { "/" });

            var cp = subject.GetCorsPolicyAsync(Request()).Result;
            Assert.IsNull(cp);
        }

        [TestMethod]
        public void GetCorsPolicyAsync_CallbackAllowOrigin_AllowOrigin()
        {
            var origin = "http://foo.com";
            var policy = new CorsPolicy();
            policy.PolicyCallback = o => Task.FromResult(true);
            var subject = new CorsPolicyProvider(policy, new string[] { "/" });

            var cp = subject.GetCorsPolicyAsync(Request(origin)).Result;
            AssertAllowed(origin, cp);
        }

        [TestMethod]
        public void GetCorsPolicyAsync_CallbackAllowOriginButNoOriginRequested_DoesNotAllowOrigin()
        {
            var policy = new CorsPolicy();
            policy.PolicyCallback = o => Task.FromResult(true);
            var subject = new CorsPolicyProvider(policy, new string[] { "/" });

            var cp = subject.GetCorsPolicyAsync(Request()).Result;
            Assert.IsNull(cp);
        }

        [TestMethod]
        public void GetCorsPolicyAsync_CallbackDoesNotAllowOrigin_DoesNotAllowOrigin()
        {
            var origin = "http://foo.com";
            var policy = new CorsPolicy();
            policy.PolicyCallback = o => Task.FromResult(false);
            var subject = new CorsPolicyProvider(policy, new string[] { "/" });

            var cp = subject.GetCorsPolicyAsync(Request(origin)).Result;
            Assert.IsNull(cp);
        }

        [TestMethod]
        public void ctor_NullPaths_Throws()
        {
            try
            {
                new CorsPolicyProvider(new CorsPolicy(), null);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("allowedPaths", ex.ParamName);
            }
        }

        [TestMethod]
        public void GetCorsPolicyAsync_MatchingPath_AllowsOrigin()
        {
            var origin = "http://foo.com";
            var path = "/bar";
            var policy = CorsPolicy.AllowAll;

            var subject = new CorsPolicyProvider(policy, new string[] { path });

            var cp = subject.GetCorsPolicyAsync(Request(origin, path)).Result;
            AssertAllowed(origin, cp);
        }

        [TestMethod]
        public void GetCorsPolicyAsync_NoMatchingPath_DoesNotAllowOrigin()
        {
            var origin = "http://foo.com";
            var path = "/bar";
            var policy = CorsPolicy.AllowAll;

            var subject = new CorsPolicyProvider(policy, new string[] { path });

            var cp = subject.GetCorsPolicyAsync(Request(origin, "/baz")).Result;
            Assert.IsNull(cp);
        }

        [TestMethod]
        public void GetCorsPolicyAsync_MatchingPaths_AllowsOrigin()
        {
            var origin = "http://foo.com";
            var policy = CorsPolicy.AllowAll;

            var subject = new CorsPolicyProvider(policy, new string[] { "/bar", "/baz", "/quux" });

            var cp = subject.GetCorsPolicyAsync(Request(origin, "/baz")).Result;
            AssertAllowed(origin, cp);
        }

        [TestMethod]
        public void GetCorsPolicyAsync_NoMatchingPaths_DoesNotAllowOrigin()
        {
            var origin = "http://foo.com";
            var policy = CorsPolicy.AllowAll;

            var subject = new CorsPolicyProvider(policy, new string[] { "/bar", "/baz", "/quux" });

            var cp = subject.GetCorsPolicyAsync(Request(origin, "/bad")).Result;
            Assert.IsNull(cp);
        }

        [TestMethod]
        public void GetCorsPolicyAsync_PathDoesNotStartWithSlash_NormalizesPathCorrectly()
        {
            var origin = "http://foo.com";
            var policy = CorsPolicy.AllowAll;

            var subject = new CorsPolicyProvider(policy, new string[] { "bar" });

            var cp = subject.GetCorsPolicyAsync(Request(origin, "/bar")).Result;
            AssertAllowed(origin, cp);
        }

        [TestMethod]
        public void GetCorsPolicyAsync_PathEndsWithSlash_NormalizesPathCorrectly()
        {
            var origin = "http://foo.com";
            var policy = CorsPolicy.AllowAll;

            var subject = new CorsPolicyProvider(policy, new string[] { "bar/" });

            var cp = subject.GetCorsPolicyAsync(Request(origin, "/bar")).Result;
            AssertAllowed(origin, cp);
        }
    }
}

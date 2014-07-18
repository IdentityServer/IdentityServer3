using Microsoft.Owin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Configuration;

namespace Thinktecture.IdentityServer.Tests.Configuration
{
    [TestClass]
    public class CorsPolicyProviderTests
    {
        [TestMethod]
        public void ctor_NullPolicy_Throws()
        {
            try
            {
                new CorsPolicyProvider(null);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("policy", ex.ParamName);
            }
        }

        IOwinRequest Request(string origin = null)
        {
            var env = new Dictionary<string, object>();
            env.Add("owin.RequestHeaders", new Dictionary<string, string[]>());

            var ctx = new OwinContext(env);
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
        public void GetCorsPolicyAsync_NoAllowedOriginsNoCallback_DoesNotAllowOrigin()
        {
            var policy = new CorsPolicy();
            var subject = new CorsPolicyProvider(policy);
            var cp = subject.GetCorsPolicyAsync(Request("http://foo.com")).Result;
            Assert.IsNull(cp);
        }
        
        [TestMethod]
        public void GetCorsPolicyAsync_PolicyAllowsAll_AllowsRandomOrigin()
        {
            var policy = CorsPolicy.AllowAll;
            var subject = new CorsPolicyProvider(policy);

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
            var subject = new CorsPolicyProvider(policy);

            var cp = subject.GetCorsPolicyAsync(Request(origin)).Result;
            AssertAllowed(origin, cp);
        }

        [TestMethod]
        public void GetCorsPolicyAsync_OriginIsInAllowedOriginsButNoOriginRequested_DoesNotAllowOrigin()
        {
            var origin = "http://foo.com";
            var policy = new CorsPolicy();
            policy.AllowedOrigins.Add(origin);
            var subject = new CorsPolicyProvider(policy);

            var cp = subject.GetCorsPolicyAsync(Request(null)).Result;
            Assert.IsNull(cp);
        }

        [TestMethod]
        public void GetCorsPolicyAsync_OriginIsNotInAllowedOrigins_DoesNotAllowOrigin()
        {
            var origin = "http://foo.com";
            var policy = new CorsPolicy();
            policy.AllowedOrigins.Add(origin);
            var subject = new CorsPolicyProvider(policy);

            var cp = subject.GetCorsPolicyAsync(Request("http://bar.com")).Result;
            Assert.IsNull(cp);
        }
        
        [TestMethod]
        public void GetCorsPolicyAsync_NoOriginRequested_DoesNotAllowOrigin()
        {
            var origin = "http://foo.com";
            var policy = new CorsPolicy();
            policy.AllowedOrigins.Add(origin);
            var subject = new CorsPolicyProvider(policy);

            var cp = subject.GetCorsPolicyAsync(Request()).Result;
            Assert.IsNull(cp);
        }

        [TestMethod]
        public void GetCorsPolicyAsync_CallbackAllowOrigin_AllowOrigin()
        {
            var origin = "http://foo.com";
            var policy = new CorsPolicy();
            policy.PolicyCallback = o => Task.FromResult(true);
            var subject = new CorsPolicyProvider(policy);

            var cp = subject.GetCorsPolicyAsync(Request(origin)).Result;
            AssertAllowed(origin, cp);
        }

        [TestMethod]
        public void GetCorsPolicyAsync_CallbackAllowOriginButNoOriginRequested_DoesNotAllowOrigin()
        {
            var policy = new CorsPolicy();
            policy.PolicyCallback = o => Task.FromResult(true);
            var subject = new CorsPolicyProvider(policy);

            var cp = subject.GetCorsPolicyAsync(Request()).Result;
            Assert.IsNull(cp);
        }

        [TestMethod]
        public void GetCorsPolicyAsync_CallbackDoesNotAllowOrigin_DoesNotAllowOrigin()
        {
            var origin = "http://foo.com";
            var policy = new CorsPolicy();
            policy.PolicyCallback = o => Task.FromResult(false);
            var subject = new CorsPolicyProvider(policy);

            var cp = subject.GetCorsPolicyAsync(Request(origin)).Result;
            Assert.IsNull(cp);
        }
    }
}

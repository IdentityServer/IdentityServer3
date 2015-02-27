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
using System;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Services.Default;
using Xunit;

namespace Thinktecture.IdentityServer.Tests.Services.Default
{
    public class DefaultCorsPolicyServiceTests
    {
        DefaultCorsPolicyService subject;

        public DefaultCorsPolicyServiceTests()
        {
            subject = new DefaultCorsPolicyService();
        }

        [Fact]
        public void IsOriginAllowed_OriginIsAllowed_ReturnsTrue()
        {
            subject.AllowedOrigins.Add("http://foo");
            subject.IsOriginAllowedAsync("http://foo").Result.Should().Be(true);
        }

        [Fact]
        public void IsOriginAllowed_OriginIsNotAllowed_ReturnsFalse()
        {
            subject.AllowedOrigins.Add("http://foo");
            subject.IsOriginAllowedAsync("http://bar").Result.Should().Be(false);
        }

        [Fact]
        public void IsOriginAllowed_OriginIsInAllowedList_ReturnsTrue()
        {
            subject.AllowedOrigins.Add("http://foo");
            subject.AllowedOrigins.Add("http://bar");
            subject.AllowedOrigins.Add("http://baz");
            subject.IsOriginAllowedAsync("http://bar").Result.Should().Be(true);
        }

        [Fact]
        public void IsOriginAllowed_OriginIsNotInAllowedList_ReturnsFalse()
        {
            subject.AllowedOrigins.Add("http://foo");
            subject.AllowedOrigins.Add("http://bar");
            subject.AllowedOrigins.Add("http://baz");
            subject.IsOriginAllowedAsync("http://quux").Result.Should().Be(false);
        }

        [Fact]
        public void ctor_CopiesCorsPolicyOrigins()
        {
            var policy = new CorsPolicy();
            policy.AllowedOrigins.Add("http://foo");
            policy.AllowedOrigins.Add("http://bar");
            policy.AllowedOrigins.Add("http://baz");

            Func<string, Task<bool>> func = s => Task.FromResult(true);
            policy.PolicyCallback = func;

            subject = new DefaultCorsPolicyService(policy);
            subject.AllowedOrigins.ShouldAllBeEquivalentTo(new string[] { "http://foo", "http://bar", "http://baz" });
        }

        [Fact]
        public void ctor_UsesCorsPolicyCallback()
        {
            var wasCalled = false;
            var policy = new CorsPolicy();
            Func<string, Task<bool>> func = s => { wasCalled = true; return Task.FromResult(true); };
            policy.PolicyCallback = func;

            subject = new DefaultCorsPolicyService(policy);
            var result = subject.IsOriginAllowedAsync("http://foo").Result;
            result.Should().Be(true);
            wasCalled.Should().Be(true);
        }
        
        [Fact]
        public void IsOriginAllowed_AllowAllTrue_ReturnsTrue()
        {
            subject.AllowAll = true;
            subject.IsOriginAllowedAsync("http://foo").Result.Should().Be(true);
        }
    }
}

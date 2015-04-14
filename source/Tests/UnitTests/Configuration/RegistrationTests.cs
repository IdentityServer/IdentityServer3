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
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services;
using System;
using Xunit;

namespace IdentityServer3.Tests.Configuration
{
    
    public class RegistrationTests
    {
        [Fact]
        public void RegisterSingleton_NullInstance_Throws()
        {
            Action act = () => new Registration<object>((object)null);

            act.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("instance");
        }

        [Fact]
        public void RegisterSingleton_Instance_ReturnsSingleton()
        {
            object theSingleton = new object();
            var reg = new Registration<object>((object)theSingleton);
            var result = reg.Instance;
            result.Should().BeSameAs(theSingleton);
        }

        [Fact]
        public void RegisterFactory_NullFunc_Throws()
        {
            Action act = () => new Registration<object>((Func<IDependencyResolver, object>)null);

            act.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("factory");
        }
        
        [Fact]
        public void RegisterFactory_FactoryInvokesFunc()
        {
            var wasCalled = false;
            Func<IDependencyResolver, object> f = (resolver) => { wasCalled = true; return new object(); };
            var reg = new Registration<object>(f);
            var result = reg.Factory(null);
            wasCalled.Should().BeTrue();
        }

        [Fact]
        public void RegisterType_NullType_Throws()
        {
            Action act = () => new Registration<object>((Type)null);

            act.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("type");
        }

        [Fact]
        public void RegisterType_SetsTypeOnRegistration()
        {
            var result = new Registration<object>(typeof(string));
            result.Type.Should().Be(typeof(string));
        }
    }
}

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

using FluentAssertions;
using System;
using Thinktecture.IdentityServer.Core.Configuration;
using Thinktecture.IdentityServer.Core.Services;
using Xunit;

namespace Thinktecture.IdentityServer.Tests.Configuration
{
    
    public class RegistrationTests
    {
        [Fact]
        public void RegisterSingleton_NullInstance_Throws()
        {
            Action act = () => Registration.RegisterSingleton<object>(null);

            act.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("instance");
        }

        [Fact]
        public void RegisterSingleton_Instance_FactoryReturnsSameInstance()
        {
            object theSingleton = new object();
            var reg = Registration.RegisterSingleton(theSingleton);
            var result = reg.ImplementationFactory(null);
            result.Should().BeSameAs(theSingleton);
        }

        [Fact]
        public void RegisterFactory_NullFunc_Throws()
        {
            Action act = () => Registration.RegisterFactory<object>(null); ;

            act.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("typeFunc");
        }
        
        [Fact]
        public void RegisterFactory_FactoryInvokesFunc()
        {
            var wasCalled = false;
            Func<IDependencyResolver, object> f = (resolver) => { wasCalled = true; return new object(); };
            var reg = Registration.RegisterFactory(f);
            var result = reg.ImplementationFactory(null);
            wasCalled.Should().BeTrue();
        }

        [Fact]
        public void RegisterType_NullType_Throws()
        {
            Action act = () => Registration.RegisterType<object>(null);

            act.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("type");
        }

        [Fact]
        public void RegisterType_SetsTypeOnRegistration()
        {
            var result = Registration.RegisterType<object>(typeof(string));
            result.ImplementationType.Should().Be(typeof(string));
        }
    }
}

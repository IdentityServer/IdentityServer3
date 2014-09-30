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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Thinktecture.IdentityServer.Core.Configuration;

namespace Thinktecture.IdentityServer.Tests.Configuration
{
    [TestClass]
    public class RegistrationTests
    {
        [TestMethod]
        public void RegisterSingleton_NullInstance_Throws()
        {
            try
            {
                Registration.RegisterSingleton<object>(null);
                Assert.Fail();
            }
            catch(ArgumentNullException ex)
            {
                Assert.AreEqual("instance", ex.ParamName);
            }
        }

        [TestMethod]
        public void RegisterSingleton_Instance_FactoryReturnsSameInstance()
        {
            object theSingleton = new object();
            var reg = Registration.RegisterSingleton<object>(theSingleton);
            var result = reg.ImplementationFactory();
            Assert.AreSame(theSingleton, result);
        }

        [TestMethod]
        public void RegisterFactory_NullFunc_Throws()
        {
            try
            {
                Registration.RegisterFactory<object>(null);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("typeFunc", ex.ParamName);
            }
        }
        
        [TestMethod]
        public void RegisterFactory_FactoryInvokesFunc()
        {
            var wasCalled = false;
            Func<object> f = () => { wasCalled = true; return new object(); };
            var reg = Registration.RegisterFactory<object>(f);
            var result = reg.ImplementationFactory();
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        public void RegisterType_NullType_Throws()
        {
            try
            {
                Registration.RegisterType<object>(null);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("type", ex.ParamName);
            }
        }

        [TestMethod]
        public void RegisterType_SetsTypeOnRegistration()
        {
            var result = Registration.RegisterType<object>(typeof(string));
            Assert.AreEqual(typeof(string), result.ImplementationType);
        }
    }
}

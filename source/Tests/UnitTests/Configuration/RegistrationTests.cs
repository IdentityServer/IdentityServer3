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

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
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Thinktecture.IdentityServer.Core.Services.Caching;

namespace Thinktecture.IdentityServer.Tests.Services.Caching
{
    [TestClass]
    public class InMemoryCacheTests
    {
        public class Foo{}

        InMemoryCache<Foo> subject;

        [TestInitialize]
        public void Init()
        {
            subject = new InMemoryCache<Foo>();
        }

        [TestMethod]
        public void TryGet_EmptyCache_ReturnsFalse()
        {
            Foo f;
            var result = subject.TryGet("key", out f);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TryGet_IncorrectKey_ReturnsFalse()
        {
            subject.Set("test", new Foo());

            Foo f;
            var result = subject.TryGet("key", out f);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TryGet_CorrectKey_ReturnsTrue()
        {
            subject.Set("test", new Foo());

            Foo f;
            var result = subject.TryGet("test", out f);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TryGet_CorrectKey_ReturnsCorrectItem()
        {
            var item = new Foo();
            subject.Set("test", item);

            Foo f;
            subject.TryGet("test", out f);
            Assert.AreSame(item, f);
        }

        [TestMethod]
        public void Set_CachesItem()
        {
            var item = new Foo();
            subject.Set("test", item);

            Foo f;
            subject.TryGet("test", out f);
            Assert.AreSame(item, f);
        }
        
        [TestMethod]
        public void Set_Twice_ReplacesItem()
        {
            var item1 = new Foo();
            subject.Set("test", item1);
            var item2 = new Foo();
            subject.Set("test", item2);

            Foo f;
            subject.TryGet("test", out f);
            Assert.AreSame(item2, f);
        }
    }
}

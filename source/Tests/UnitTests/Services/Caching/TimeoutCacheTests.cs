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
using Thinktecture.IdentityServer.Core.Services;
using Moq;

namespace Thinktecture.IdentityServer.Tests.Services.Caching
{
    [TestClass]
    public class TimeoutCacheTests
    {
        public class TimeoutCacheSubject<T> : TimeoutCache<T>
        {
            public TimeoutCacheSubject(TimeSpan duration, ICache<T> inner)
                : base(duration, inner)
            {
            }

            public DateTime Now { get; set; }
            protected override DateTime UtcNow
            {
                get
                {
                    return Now;
                }
            }
        }
        
        public class Foo{}

        TimeoutCache<Foo> subject;
        TimeSpan duration = TimeSpan.FromMinutes(1);
        Mock<ICache<Foo>> mockInner;
        DateTime now = new DateTime(2013, 3, 28, 9, 0, 0);

        public void Init()
        {
            mockInner = new Mock<ICache<Foo>>();
            subject = new TimeoutCacheSubject<Foo>(duration, mockInner.Object)
            {
                Now = now
            };
        }

        [TestMethod]
        public void TestMethod1()
        {
        }
    }
}

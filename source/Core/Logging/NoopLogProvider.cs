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


using System;

namespace IdentityServer3.Core.Logging
{
    /// <summary>
    /// A no-op log provider to disable all logging
    /// </summary>
    public class NoopLogProvider : ILogProvider
    {
        /// <summary>
        ///  Nop logger
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Logger GetLogger(string name)
        {
            return delegate { return false; };
        }

        /// <summary>
        ///  Nop logger
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public IDisposable OpenMappedContext(string key, string value)
        {
            return new NoopClass();
        }

        /// <summary>
        ///  Nop logger
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public IDisposable OpenNestedContext(string message)
        {
            return new NoopClass();
        }

        private class NoopClass : IDisposable
        {
            public void Dispose()
            { }
        }
    }
}
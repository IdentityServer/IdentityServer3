// This software is part of the Autofac IoC container
// Copyright © 2014 Autofac Contributors
// http://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using Microsoft.Owin;
using System;
using System.Security;

namespace Autofac.Integration.Owin
{
    /// <summary>
    /// Extension methods for using Autofac within an OWIN context.
    /// </summary>
    internal static class OwinContextExtensions
    {
        /// <summary>
        /// Gets the current Autofac lifetime scope from the OWIN context.
        /// </summary>
        /// <param name="context">The OWIN context.</param>
        /// <returns>The current lifetime scope.</returns>
        [SecuritySafeCritical]
        public static ILifetimeScope GetAutofacLifetimeScope(this IOwinContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            return context.Get<ILifetimeScope>(Constants.OwinLifetimeScopeKey);
        }
    }
}

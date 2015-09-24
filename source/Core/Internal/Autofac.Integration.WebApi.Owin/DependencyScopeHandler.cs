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

using Autofac.Integration.Owin;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Hosting;

namespace Autofac.Integration.WebApi.Owin
{
    [SecurityCritical]
    class DependencyScopeHandler : DelegatingHandler
    {
        [SecuritySafeCritical]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException("request");

            var owinContext = request.GetOwinContext();
            if (owinContext == null) return base.SendAsync(request, cancellationToken);

            var lifetimeScope = owinContext.GetAutofacLifetimeScope();
            if (lifetimeScope == null) return base.SendAsync(request, cancellationToken);

            var dependencyScope = new AutofacWebApiDependencyScope(lifetimeScope);
            request.Properties[HttpPropertyKeys.DependencyScope] = dependencyScope;

            return base.SendAsync(request, cancellationToken);
        }
    }
}

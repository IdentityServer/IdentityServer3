using Autofac;
using Autofac.Integration.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dependencies;
using System.Web.Http.Hosting;

namespace Thinktecture.IdentityServer.Core
{
    public static class WebApiConfig
    {
        public static HttpConfiguration Configure(IdentityServerCoreOptions options)
        {
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();
            config.SuppressDefaultHostAuthentication();
            config.MessageHandlers.Insert(0, new KatanaDependencyResolver());
            
            return config;
        }
    }

    public class KatanaDependencyResolver : System.Net.Http.DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var owin = request.GetOwinContext();
            var scope = owin.Get<ILifetimeScope>("idsrv:AutofacScope");
            if (scope != null)
            {
                request.Properties[HttpPropertyKeys.DependencyScope] = new AutofacScope(scope);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }

    public class AutofacScope : IDependencyScope
    {
        ILifetimeScope scope;
        public AutofacScope(ILifetimeScope scope)
        {
            this.scope = scope;
        }
        public object GetService(System.Type serviceType)
        {
            return scope.ResolveOptional(serviceType);
        }

        public System.Collections.Generic.IEnumerable<object> GetServices(System.Type serviceType)
        {
            if (!scope.IsRegistered(serviceType))
            {
                return Enumerable.Empty<object>();
            }
            Type type = typeof(IEnumerable<>).MakeGenericType(new Type[] { serviceType });
            return (IEnumerable<object>)scope.Resolve(type);
        }

        public void Dispose()
        {
        }
    }

}

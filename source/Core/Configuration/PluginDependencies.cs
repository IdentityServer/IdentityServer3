using System;
using System.Collections.Generic;
using System.Reflection;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public class PluginDependencies
    {
        public Dictionary<Type, Type> Types { get; set; }
        public Dictionary<Type, Func<object>> Factories { get; set; }
        public List<Assembly> ApiControllerAssemblies { get; set; }

        public PluginDependencies()
        {
            Types = new Dictionary<Type, Type>();
            Factories = new Dictionary<Type, Func<object>>();
            ApiControllerAssemblies = new List<Assembly>();
        }

        public void AddType(Type implementation, Type implementedInterface = null)
        {
            Types.Add(implementation, implementedInterface);
        }

        public void AddTypeFactory(Type type, Func<object> factory)
        {
            Factories.Add(type, factory);
        }

        public void AddApiControllerAssembly(Assembly assembly)
        {
            ApiControllerAssemblies.Add(assembly);
        }
    }
}
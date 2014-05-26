/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Thinktecture.IdentityServer.Core.Configuration
{
    public class PluginConfiguration
    {
        public Dictionary<Type, Type> Types { get; set; }
        public Dictionary<Type, Func<object>> Factories { get; set; }
        public List<Assembly> ApiControllerAssemblies { get; set; }
        public List<string> SignOutCallbackUrls { get; set; }

        public PluginConfiguration()
        {
            Types = new Dictionary<Type, Type>();
            Factories = new Dictionary<Type, Func<object>>();
            ApiControllerAssemblies = new List<Assembly>();
            SignOutCallbackUrls = new List<string>();
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

        public void AddSignOutCallbackUrl(string url)
        {
            SignOutCallbackUrls.Add(url);
        }
    }
}
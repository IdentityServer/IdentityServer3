using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace Thinktecture.IdentityServer.Core.Plumbing
{
    class EmbeddedResourceManager
    {
        static ConcurrentDictionary<string, string> ResourceStrings = new ConcurrentDictionary<string, string>();
        internal static string LoadResourceString(string name)
        {
            string value;
            if (!ResourceStrings.TryGetValue(name, out value))
            {
                var assembly = typeof(EmbeddedResourceManager).Assembly;
                using (var sr = new StreamReader(assembly.GetManifestResourceStream(name)))
                {
                    ResourceStrings[name] = value = sr.ReadToEnd();
                }
            }
            return value;
        }

        internal static string LoadResourceString(string name, IDictionary<string, object> values)
        {
            string value = LoadResourceString(name);
            foreach(var key in values.Keys)
            {
                value = value.Replace("{" + key + "}", values[key].ToString());
            }
            return value;
        }
        
        internal static string LoadResourceString(string name, object values)
        {
            return LoadResourceString(name, Map(values));
        }

        static IDictionary<string, object> Map(object values)
        {
            var dictionary = values as IDictionary<string, object>;
            
            if (dictionary == null) 
            {
                dictionary = new Dictionary<string, object>();
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(values))
                {
                    dictionary.Add(descriptor.Name, descriptor.GetValue(values));
                }
            }

            return dictionary;
        }
    }
}


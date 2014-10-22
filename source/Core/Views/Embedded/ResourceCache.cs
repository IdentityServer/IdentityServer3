using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Views
{
    class ResourceCache
    {
        ConcurrentDictionary<string, string> cache = new ConcurrentDictionary<string, string>();

        public string Read(string name)
        {
            string value;
            if (cache.TryGetValue(name, out value))
            {
                return value;
            }
            return null;
        }

        public void Write(string name, string value)
        {
            cache[name] = value;
        }
    }
}

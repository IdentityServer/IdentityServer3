using System.Collections.Concurrent;
using System.IO;

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
    }
}

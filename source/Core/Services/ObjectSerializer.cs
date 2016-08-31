using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IdentityServer3.Core.Services
{
    internal static class ObjectSerializer
    {
        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };

        private static readonly JsonSerializer serializer = new JsonSerializer
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };

        public static string ToString(object o)
        {
            return JsonConvert.SerializeObject(o, settings);
        }

        public static JObject ToJObject(object o)
        {
            return JObject.FromObject(o, serializer);
        }
    }
}

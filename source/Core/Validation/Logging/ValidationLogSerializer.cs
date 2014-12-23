using Newtonsoft.Json;

namespace Thinktecture.IdentityServer.Core.Validation.Logging
{
    static class ValidationLogSerializer
    {
        static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            Formatting = Formatting.Indented
        };

        public static string Serialize(object logObject)
        {
            return JsonConvert.SerializeObject(logObject, jsonSettings);
        }
    }
}
namespace IdentityServer3.Core.Models
{
    using System;

    using Newtonsoft.Json;

    public class JwtTokenResponse
    {
        [JsonProperty(PropertyName = "id_token")]
        public string IdToken { get; set; }

        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }

        [JsonProperty(PropertyName = "token_type")]
        public string TokenType { get; set; }

        [JsonProperty(PropertyName = "expires_in")]
        public long ExpiresIn { get; set; }

        [JsonProperty(PropertyName = "issued")]
        public DateTime Issued { get; set; }

        [JsonProperty(PropertyName = "expires")]
        public DateTime Expires { get; set; }
    }
}
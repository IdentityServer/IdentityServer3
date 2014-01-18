using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Clients;

namespace Thinktecture.IdentityModel.Oidc
{
    public class OidcClient
    {
        public static Uri GetAuthorizeUrl(Uri authorizeEndpoint, Uri redirectUri, string clientId, string scopes, string state, string responseType = "code")
        {
            var queryString = string.Format("?client_id={0}&scope={1}&redirect_uri={2}&state={3}&response_type={4}",
                clientId,
                scopes,
                redirectUri,
                state,
                responseType);

            return new Uri(authorizeEndpoint.AbsoluteUri + queryString);
        }

        public static OidcAuthorizeResponse HandleAuthorizeResponse(NameValueCollection query)
        {
            var response = new OidcAuthorizeResponse
            {
                Error = query["error"],
                Code = query["code"],
                State = query["state"]
            };

            response.IsError = !string.IsNullOrWhiteSpace(response.Error);
            return response;
        }

        public static OidcTokenResponse CallTokenEndpoint(Uri tokenEndpoint, Uri redirectUri, string code, string clientId, string clientSecret)
        {
            var client = new HttpClient
            {
                BaseAddress = tokenEndpoint
            };

            client.SetBasicAuthentication(clientId, clientSecret);

            var parameter = new Dictionary<string, string>
                {
                    { "grant_type", "authorization_code" },
                    { "code", code },
                    { "redirect_uri", redirectUri.AbsoluteUri }
                };

            var response = client.PostAsync("", new FormUrlEncodedContent(parameter)).Result;
            response.EnsureSuccessStatusCode();

            var json = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            return json.ToObject<OidcTokenResponse>();
        }
        
        public async static Task<OidcTokenResponse> CallTokenEndpointAsync(Uri tokenEndpoint, Uri redirectUri, string code, string clientId, string clientSecret)
        {
            var client = new HttpClient
            {
                BaseAddress = tokenEndpoint
            };

            client.SetBasicAuthentication(clientId, clientSecret);

            var parameter = new Dictionary<string, string>
                {
                    { "grant_type", "authorization_code" },
                    { "code", code },
                    { "redirect_uri", redirectUri.AbsoluteUri }
                };

            var response = await client.PostAsync("", new FormUrlEncodedContent(parameter));
            response.EnsureSuccessStatusCode();

            var json = JObject.Parse(await response.Content.ReadAsStringAsync());
            return json.ToObject<OidcTokenResponse>();
        }

        public static OidcTokenResponse RefreshAccessToken(Uri tokenEndpoint, string clientId, string clientSecret, string refreshToken)
        {
            var client = new OAuth2Client(
                tokenEndpoint,
                clientId,
                clientSecret);

            var response = client.RequestAccessTokenRefreshToken(refreshToken);

            return new OidcTokenResponse
            {
                AccessToken = response.AccessToken,
                ExpiresIn = response.ExpiresIn,
                TokenType = response.TokenType,
                RefreshToken = refreshToken
            };
        }

        public static IEnumerable<Claim> ValidateIdentityToken(string token, string issuer, string audience, X509Certificate2 signingCertificate)
        {
            var idToken = new JwtSecurityToken(token);
            var handler = new JwtSecurityTokenHandler();
            
            var parameters = new TokenValidationParameters
            {
                ValidIssuer = issuer,
                AllowedAudience = audience,
                SigningToken = new X509SecurityToken(signingCertificate)
            };

            return handler.ValidateToken(token, parameters).Claims;
        }

        public async static Task<IEnumerable<Claim>> GetUserInfoClaimsAsync(Uri userInfoEndpoint, string accessToken)
        {
            var client = new HttpClient
            {
                BaseAddress = userInfoEndpoint
            };

            client.SetBearerToken(accessToken);

            var response = await client.GetAsync("");
            response.EnsureSuccessStatusCode();

            var dictionary = await response.Content.ReadAsAsync<Dictionary<string, string>>();

            var claims = new List<Claim>();
            foreach (var pair in dictionary)
            {
                if (pair.Value.Contains(','))
                {
                    foreach (var item in pair.Value.Split(','))
                    {
                        claims.Add(new Claim(pair.Key, item));
                    }
                }
                else
                {
                    claims.Add(new Claim(pair.Key, pair.Value));
                }
            }

            return claims;
        }
        public static IEnumerable<Claim> GetUserInfoClaims(Uri userInfoEndpoint, string accessToken)
        {
            var client = new HttpClient
            {
                BaseAddress = userInfoEndpoint
            };

            client.SetBearerToken(accessToken);

            var response = client.GetAsync("").Result;
            response.EnsureSuccessStatusCode();

            var dictionary = response.Content.ReadAsAsync<Dictionary<string, string>>().Result;

            var claims = new List<Claim>();
            foreach (var pair in dictionary)
            {
                if (pair.Value.Contains(','))
                {
                    foreach (var item in pair.Value.Split(','))
                    {
                        claims.Add(new Claim(pair.Key, item));
                    }
                }
                else
                {
                    claims.Add(new Claim(pair.Key, pair.Value));
                }
            }

            return claims;
        }
    }
}

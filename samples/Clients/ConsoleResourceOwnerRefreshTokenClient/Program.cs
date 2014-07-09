using Newtonsoft.Json.Linq;
using Sample;
using System;
using System.Net.Http;
using System.Text;
using Thinktecture.IdentityModel.Client;
using Thinktecture.IdentityModel.Extensions;

namespace ConsoleResourceOwnerClient
{
    class Program
    {
        static OAuth2Client _oauth2;

        static void Main(string[] args)
        {
            _oauth2 = new OAuth2Client(
                new Uri(Constants.TokenEndpoint),
                "roclient",
                "secret");

            var response = RequestToken();
            ShowResponse(response);

            Console.ReadLine();

            while (true)
            {
                response = RefreshToken(response.RefreshToken);
                ShowResponse(response);

                Console.ReadLine();
                CallService(response.AccessToken);
            }
        }

        static TokenResponse RequestToken()
        {
            return _oauth2.RequestResourceOwnerPasswordAsync
                ("bob", "bob", "read write offline_access").Result;
        }

        private static TokenResponse RefreshToken(string refreshToken)
        {
            return _oauth2.RequestRefreshTokenAsync(refreshToken).Result;
        }

        static void CallService(string token)
        {
            var baseAddress = Constants.AspNetWebApiSampleApi;

            var client = new HttpClient
            {
                BaseAddress = new Uri(baseAddress)
            };

            client.SetBearerToken(token);
            var response = client.GetStringAsync("identity").Result;

            "\n\nService claims:".ConsoleGreen();
            Console.WriteLine(JArray.Parse(response));
        }

        private static void ShowResponse(TokenResponse response)
        {
            if (!response.IsError)
            {
                "Token response:".ConsoleGreen();
                Console.WriteLine(response.Json);

                if (response.AccessToken.Contains("."))
                {
                    "\nAccess Token (decoded):".ConsoleGreen();

                    var parts = response.AccessToken.Split('.');
                    var header = parts[0];
                    var claims = parts[1];

                    Console.WriteLine(JObject.Parse(Encoding.UTF8.GetString(Base64Url.Decode(header))));
                    Console.WriteLine(JObject.Parse(Encoding.UTF8.GetString(Base64Url.Decode(claims))));
                }
            }
            else
            {
                if (response.IsHttpError)
                {
                    "HTTP error: ".ConsoleGreen();
                    Console.WriteLine(response.HttpErrorStatusCode);
                    "HTTP error reason: ".ConsoleGreen();
                    Console.WriteLine(response.HttpErrorReason);
                }
                else
                {
                    "Protocol error response:".ConsoleGreen();
                    Console.WriteLine(response.Json);
                }
            }
        }
    }
}
using Newtonsoft.Json.Linq;
using Sample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Client;
using Thinktecture.IdentityModel.Extensions;

namespace ConsoleClientCredentialsClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var response = RequestToken();
            ShowResponse(response);
        }

        static TokenResponse RequestToken()
        {
            var client = new OAuth2Client(
                new Uri(Constants.TokenEndpoint),
                "client",
                "secret");

            return client.RequestClientCredentialsAsync("read").Result;
        }

        private static void ShowResponse(TokenResponse response)
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
    }
}

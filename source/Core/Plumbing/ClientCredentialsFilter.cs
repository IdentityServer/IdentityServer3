//using System;
//using System.Collections.Specialized;
//using System.IO;
//using System.Net.Http;
//using System.Net.Http.Formatting;
//using System.Security.Claims;
//using System.Security.Principal;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Web.Http.Filters;
//using System.Web.Http.Results;

//namespace Thinktecture.IdentityServer.Core
//{
//    public class ClientCredentialsFilter : Attribute, IAuthenticationFilter
//    {
//        public bool AllowMultiple { get { return false; } }

//        public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
//        {
//            var credentials = ParseBasicAuthenticationScheme(context.Request);

//            if (credentials.IsMalformed)
//            {
//                context.ErrorResult = new BadRequestResult(context.Request);
//                return;
//            }

//            if (credentials.IsPresent)
//            {
//                context.Principal = CreatePrincipal(credentials);
//                return;
//            }

//            await context.Request.Content.LoadIntoBufferAsync();
//            credentials = ParsePostBody(await context.Request.Content.ReadAsFormDataAsync());

//            if (credentials.IsPresent)
//            {
//                context.Principal = CreatePrincipal(credentials);
//            }
//        }

//        private IPrincipal CreatePrincipal(ClientCredentials credentials)
//        {
//            var identity = new ClaimsIdentity("Basic");

//            identity.AddClaim(new Claim("id", credentials.ClientId));
//            identity.AddClaim(new Claim("secret", credentials.Secret));

//            return new ClaimsPrincipal(identity);
//        }


//        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
//        {
//            return Task.FromResult<object>(null);
//        }

//        private ClientCredentials ParseBasicAuthenticationScheme(HttpRequestMessage request)
//        {
//            if (request.Headers.Authorization == null ||
//                !request.Headers.Authorization.Scheme.Equals("Basic", StringComparison.OrdinalIgnoreCase))
//            {
//                return new ClientCredentials
//                {
//                    IsPresent = false,
//                    IsMalformed = false
//                };
//            }

//            string pair;
//            try
//            {
//                pair = Encoding.UTF8.GetString(
//                    Convert.FromBase64String(request.Headers.Authorization.Parameter));
//            }
//            catch (FormatException)
//            {
//                return new ClientCredentials
//                {
//                    IsPresent = false,
//                    IsMalformed = true
//                };
//            }
//            catch (ArgumentException)
//            {
//                return new ClientCredentials
//                {
//                    IsPresent = false,
//                    IsMalformed = true
//                };
//            }

//            var ix = pair.IndexOf(':');
//            if (ix == -1)
//            {
//                return new ClientCredentials
//                {
//                    IsPresent = false,
//                    IsMalformed = false
//                };
//            }

//            return new ClientCredentials
//            {
//                ClientId = pair.Substring(0, ix),
//                Secret = pair.Substring(ix + 1),

//                IsPresent = true,
//                IsMalformed = false
//            };
//        }

//        private ClientCredentials ParsePostBody(NameValueCollection body)
//        {
//            var id = body.Get("client_id");
//            var secret = body.Get("client_secret");

//            if (id.IsPresent() && secret.IsPresent())
//            {
//                return new ClientCredentials
//                {
//                    ClientId = id,
//                    Secret = secret,

//                    IsMalformed = false,
//                    IsPresent = true
//                };
//            }

//            return new ClientCredentials
//            {
//                IsPresent = false
//            };
//        }
//    }

    

//}


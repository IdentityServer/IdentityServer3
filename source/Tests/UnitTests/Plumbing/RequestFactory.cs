using System.Collections.Specialized;
using Thinktecture.IdentityServer.Core;

namespace UnitTests
{
    static class RequestFactory
    {
        public static NameValueCollection GetMinimalAuthorizeRequest()
        {
            var parameters = new NameValueCollection();

            parameters.Add(Constants.AuthorizeRequest.ClientId, "client");
            parameters.Add(Constants.AuthorizeRequest.Scope, "openid");
            parameters.Add(Constants.AuthorizeRequest.RedirectUri, "https://server/callback");
            parameters.Add(Constants.AuthorizeRequest.ResponseType, "code");

            return parameters;
        }
    }
}

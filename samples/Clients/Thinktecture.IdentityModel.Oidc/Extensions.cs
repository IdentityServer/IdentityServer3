using System;
using System.Web;

namespace Thinktecture.IdentityModel.Oidc
{
    public static class Extensions
    {
        public static string GetApplicationUrl(this HttpRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");

            var baseUrl =
                request.Url.Scheme +
                "://" +
                request.Url.Host + (request.Url.Port == 80 ? "" : ":" + request.Url.Port) +
                request.ApplicationPath;
            if (!baseUrl.EndsWith("/")) baseUrl += "/";

            return baseUrl;
        }
    }
}

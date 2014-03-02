using System;
using System.Net.Http;

namespace Thinktecture.IdentityServer.Core.Extensions
{
    public static class HttpRequestMessageExtensions
    {
        public static string GetBaseUrl(this HttpRequestMessage request, string host = null)
        {
            if (host.IsMissing())
            {
                host = "https://" + request.Headers.Host;
            }

            var baseUrl = new Uri(new Uri(host), request.GetRequestContext().VirtualPathRoot).AbsoluteUri;
            if (!baseUrl.EndsWith("/")) baseUrl += "/";

            return baseUrl;
        }
    }
}

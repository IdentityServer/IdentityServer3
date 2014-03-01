using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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

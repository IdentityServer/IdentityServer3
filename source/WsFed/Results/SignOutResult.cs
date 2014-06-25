/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Logging;

namespace Thinktecture.IdentityServer.WsFederation.Results
{
    public class SignOutResult : IHttpActionResult
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly IEnumerable<string> _urls;

        public SignOutResult(IEnumerable<string> urls)
        {
            _urls = urls ?? Enumerable.Empty<string>();
        }

        public Task<HttpResponseMessage> ExecuteAsync(System.Threading.CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        HttpResponseMessage Execute()
        {
            const string format = "<iframe style=\"visibility: hidden; width: 1px; height: 1px\" src=\"{0}?wa=wsignoutcleanup1.0\"></iframe>";
            var sb = new StringBuilder(128);

            foreach (var url in _urls)
            {
                sb.AppendFormat(format, url);
            }

            var content = new StringContent(sb.ToString(), Encoding.UTF8, "text/html");

            Logger.Debug("Returning WS-Federation signout response");
            return new HttpResponseMessage { Content = content };
        }
    }
}
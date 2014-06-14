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

namespace Thinktecture.IdentityServer.WsFed.Results
{
    public class SignOutResult : IHttpActionResult
    {
        private readonly IEnumerable<string> _urls;
        private readonly ILog _logger;

        public SignOutResult(IEnumerable<string> urls)
        {
            _logger = LogProvider.GetCurrentClassLogger();
            _urls = urls;

            if (_urls == null)
            {
                _urls = Enumerable.Empty<string>();
            }
        }

        public Task<HttpResponseMessage> ExecuteAsync(System.Threading.CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        HttpResponseMessage Execute()
        {
            var format = "<iframe style=\"visibility: hidden; width: 1px; height: 1px\" src=\"{0}?wa=wsignoutcleanup1.0\"></iframe>";
            var sb = new StringBuilder(128);

            foreach (var url in _urls)
            {
                sb.AppendFormat(format, url);
            }

            var content = new StringContent(sb.ToString(), Encoding.UTF8, "text/html");

            _logger.Debug("Returning WS-Federation signout response");
            return new HttpResponseMessage { Content = content };
        }
    }
}
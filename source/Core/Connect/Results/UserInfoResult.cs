/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Logging;

namespace Thinktecture.IdentityServer.Core.Connect.Results
{
    public class UserInfoResult : IHttpActionResult
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly Dictionary<string, object> _claims;
        
        public UserInfoResult(Dictionary<string, object> claims)
        {
            _claims = claims;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        private HttpResponseMessage Execute()
        {
            var content = new ObjectContent<Dictionary<string, object>>(_claims, new JsonMediaTypeFormatter());
            var message = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = content
            };

            Logger.Info("Returning userinfo response.");
            return message;
        }
    }
}

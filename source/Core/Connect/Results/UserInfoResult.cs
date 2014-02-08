using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Thinktecture.IdentityServer.Core.Connect.Results
{
    public class UserInfoResult : IHttpActionResult
    {
        private Dictionary<string, object> _claims;
        
        public UserInfoResult(Dictionary<string, object> claims)
        {
            _claims = claims;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<HttpResponseMessage>(Execute());
        }

        private HttpResponseMessage Execute()
        {
            var content = new ObjectContent<Dictionary<string, object>>(_claims, new JsonMediaTypeFormatter());
            var message = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = content
            };

            //_logger.InformationFormat("Post back form: {0}", form);
            return message;
        }
    }
}

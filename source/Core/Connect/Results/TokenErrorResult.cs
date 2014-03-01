using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Thinktecture.IdentityServer.Core.Connect.Results
{
    public class TokenErrorResult : IHttpActionResult
    {
        public string Error { get; set; }

        public TokenErrorResult(string error)
        {
            Error = error;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<HttpResponseMessage>(Execute());
        }

        private HttpResponseMessage Execute()
        {
            var dto = new ErrorDto
            {
                error = Error 
            };

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new ObjectContent<ErrorDto>(dto, new JsonMediaTypeFormatter())
            };

            return response;
        }

        internal class ErrorDto
        {
            public string error { get; set; }
        }    
    }
}

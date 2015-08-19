using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace IdentityServer3.Core.Results
{
    internal abstract class AsyncResponseActionResult : IHttpActionResult
    {
        readonly Func<Task<HttpResponseMessage>> responseFunc;

        public AsyncResponseActionResult(Func<Task<HttpResponseMessage>> responseFunc)
        {
            this.responseFunc = responseFunc;
        }

        public async Task<HttpResponseMessage> GetResponseMessage()
        {
            return await responseFunc();
        }

        public async Task<HttpResponseMessage> ExecuteAsync(System.Threading.CancellationToken cancellationToken)
        {
            return await GetResponseMessage();
        }
    }
}

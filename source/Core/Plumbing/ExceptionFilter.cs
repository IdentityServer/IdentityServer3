using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Plumbing
{
    class ExceptionFilter : IExceptionFilter
    {
        private readonly ILogger _logger;

        public ExceptionFilter(ILogger logger)
        {
            _logger = logger;
        }

        public Task ExecuteExceptionFilterAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            _logger.ErrorFormat("Unhandled exception: {0}", actionExecutedContext.Exception);

            return Task.FromResult<object>(null);
        }

        public bool AllowMultiple
        {
            get { return true; }
        }
    }
}
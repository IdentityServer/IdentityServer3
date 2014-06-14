/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;
using Thinktecture.IdentityServer.Core.Logging;

namespace Thinktecture.IdentityServer.Core.Plumbing
{
    class IdentityServerExceptionLogger : IExceptionLogger
    {
        public Task LogAsync(ExceptionLoggerContext context, CancellationToken cancellationToken)
        {
            var logger = LogProvider.GetCurrentClassLogger();
            logger.ErrorFormat("Unhandled exception: {0}", context.Exception);

            return Task.FromResult<object>(null);
        }
    }
}
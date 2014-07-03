/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;
using Thinktecture.IdentityServer.Core.Logging;

namespace Thinktecture.IdentityServer.Core.Hosting
{
    public class LogProviderExceptionLogger : IExceptionLogger
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        public Task LogAsync(ExceptionLoggerContext context, CancellationToken cancellationToken)
        {
            Logger.ErrorException("Unhandled exception", context.Exception);

            return Task.FromResult<object>(null);
        }
    }
}
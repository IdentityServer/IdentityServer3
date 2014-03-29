/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Diagnostics;

namespace Thinktecture.IdentityServer.Core.Services
{
    public static class ILoggerExtensions
    {
        [DebuggerStepThrough]
        public static void VerboseFormat(this ILogger logger, string message, params object[] values)
        {
            logger.Verbose(string.Format(message, values));
        }

        [DebuggerStepThrough]
        public static void InformationFormat(this ILogger logger, string message, params object[] values)
        {
            logger.Information(string.Format(message, values));
        }

        [DebuggerStepThrough]
        public static void WarningFormat(this ILogger logger, string message, params object[] values)
        {
            logger.Warning(string.Format(message, values));
        }

        [DebuggerStepThrough]
        public static void ErrorFormat(this ILogger logger, string message, params object[] values)
        {
            logger.Error(string.Format(message, values));
        }
    }
}
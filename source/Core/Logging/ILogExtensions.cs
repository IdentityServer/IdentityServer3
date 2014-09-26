/*
 * Copyright 2014 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Thinktecture.IdentityServer.Core.Logging
{
    using System;
    using System.Diagnostics;
    using System.Globalization;

    public static class ILogExtensions
    {
        [DebuggerStepThrough]
        public static void Debug(this ILog logger, Func<string> messageFunc)
        {
            GuardAgainstNullLogger(logger);
            logger.Log(LogLevel.Debug, messageFunc);
        }

        [DebuggerStepThrough]
        public static void Debug(this ILog logger, string message)
        {
            GuardAgainstNullLogger(logger);
            logger.Log(LogLevel.Debug, () => message);
        }

        [DebuggerStepThrough]
        public static void DebugFormat(this ILog logger, string message, params object[] args)
        {
            GuardAgainstNullLogger(logger);
            logger.Log(LogLevel.Debug, () => string.Format(CultureInfo.InvariantCulture, message, args));
        }

        [DebuggerStepThrough]
        public static void Error(this ILog logger, string message)
        {
            GuardAgainstNullLogger(logger);
            logger.Log(LogLevel.Error, () => message);
        }

        [DebuggerStepThrough]
        public static void ErrorFormat(this ILog logger, string message, params object[] args)
        {
            GuardAgainstNullLogger(logger);
            logger.Log(LogLevel.Error, () => string.Format(CultureInfo.InvariantCulture, message, args));
        }

        [DebuggerStepThrough]
        public static void ErrorException(this ILog logger, string message, Exception exception)
        {
            GuardAgainstNullLogger(logger);
            logger.Log(LogLevel.Error, () => message, exception);
        }

        [DebuggerStepThrough]
        public static void Info(this ILog logger, Func<string> messageFunc)
        {
            GuardAgainstNullLogger(logger);
            logger.Log(LogLevel.Info, messageFunc);
        }

        [DebuggerStepThrough]
        public static void Info(this ILog logger, string message)
        {
            GuardAgainstNullLogger(logger);
            logger.Log(LogLevel.Info, () => message);
        }

        [DebuggerStepThrough]
        public static void InfoFormat(this ILog logger, string message, params object[] args)
        {
            GuardAgainstNullLogger(logger);
            logger.Log(LogLevel.Info, () => string.Format(CultureInfo.InvariantCulture, message, args));
        }

        [DebuggerStepThrough]
        public static void Warn(this ILog logger, Func<string> messageFunc)
        {
            GuardAgainstNullLogger(logger);
            logger.Log(LogLevel.Warn, messageFunc);
        }

        [DebuggerStepThrough]
        public static void Warn(this ILog logger, string message)
        {
            GuardAgainstNullLogger(logger);
            logger.Log(LogLevel.Warn, () => message);
        }

        [DebuggerStepThrough]
        public static void WarnFormat(this ILog logger, string message, params object[] args)
        {
            GuardAgainstNullLogger(logger);
            logger.Log(LogLevel.Warn, () => string.Format(CultureInfo.InvariantCulture, message, args));
        }

        [DebuggerStepThrough]
        public static void WarnException(this ILog logger, string message, Exception ex)
        {
            GuardAgainstNullLogger(logger);
            logger.Log(LogLevel.Warn, () => string.Format(CultureInfo.InvariantCulture, message), ex);
        }

        [DebuggerStepThrough]
        private static void GuardAgainstNullLogger(ILog logger)
        {
            if(logger == null)
            {
                throw new ArgumentException("logger is null", "logger");
            }
        }
    }
}
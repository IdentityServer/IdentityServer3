/*
 * Copyright 2014, 2015 Dominick Baier, Brock Allen
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

using System;
using System.Diagnostics;
using Thinktecture.IdentityServer.Core.Extensions;

namespace Thinktecture.IdentityServer.Core.Logging
{
    /// <summary>
    /// Implementation of <see cref="ILogProvider"/> that uses the <see cref="DiagnosticsTraceLogger"/>.
    /// </summary>
    public class DiagnosticsTraceLogProvider : ILogProvider
    {
        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public ILog GetLogger(string name)
        {
            return new DiagnosticsTraceLogger(name);
        }
    }

    /// <summary>
    /// Implementation of <see cref="ILog"/> that uses <see cref="System.Diagnostics.Trace"/>.
    /// </summary>
    public class DiagnosticsTraceLogger : ILog
    {
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticsTraceLogger"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public DiagnosticsTraceLogger(string name)
        {
            _name = string.Format("[{0}]", name);
        }

        /// <summary>
        /// Logs the specified log level.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="messageFunc">The message function.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception = null)
        {
            if (messageFunc != null)
            {
                if (exception == null)
                {
                    var message = string.Format("{0}: {1} -- {2}", _name, DateTimeOffsetHelper.UtcNow, messageFunc());
                    TraceMsg(logLevel, message);
                }
                else
                {
                    var message = string.Format("{0}: {1} -- {2}\n{3}", _name, DateTimeOffsetHelper.UtcNow, messageFunc(), exception);
                    TraceMsg(logLevel, message);
                }
            }

            return true;
        }

        /// <summary>
        /// Log a message and exception at the specified log level.
        /// </summary>
        /// <typeparam name="TException">The type of the exception.</typeparam>
        /// <param name="logLevel">The log level.</param>
        /// <param name="messageFunc">The message function.</param>
        /// <param name="exception">The exception.</param>
        /// <remarks>
        /// Note to implementors: the message func should not be called if the loglevel is not enabled
        /// so as not to incur perfomance penalties.
        /// </remarks>
        public void Log<TException>(LogLevel logLevel, Func<string> messageFunc, TException exception) where TException : Exception
        {
            if (messageFunc != null && exception != null)
            {
                var message = string.Format("{0}: {1} -- {2}\n{3}", _name, DateTimeOffsetHelper.UtcNow.ToString(), messageFunc(), exception);
                TraceMsg(logLevel, message);
            }
        }

        private static void TraceMsg(LogLevel logLevel, string message)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                    Trace.WriteLine(message, logLevel.ToString());
                    break;
                case LogLevel.Info:
                    Trace.TraceInformation(message);
                    break;
                case LogLevel.Warn:
                    Trace.TraceWarning(message);
                    break;
                case LogLevel.Error:
                    Trace.TraceError(message);
                    break;
                case LogLevel.Fatal:
                    Trace.TraceError(string.Format("FATAL : {0}", message));
                    break;
            }
        }
    }
}
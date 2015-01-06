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

namespace Thinktecture.IdentityServer.Core.Logging
{
    /// <summary>
    /// Implementation of <see cref="ILogProvider"/> that uses <see cref="TraceSourceLogger"/>.
    /// </summary>
    public class TraceSourceLogProvider : ILogProvider
    {
        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public ILog GetLogger(string name)
        {
            return new TraceSourceLogger(name);
        }
    }

    /// <summary>
    /// Implementation of <see cref="ILog"/> that uses <see cref="System.Diagnostics.TraceSource"/>.
    /// </summary>
    public class TraceSourceLogger : ILog
    {
        private readonly string _name;
        private static readonly TraceSource _source;

        static TraceSourceLogger()
        {
            _source = new TraceSource("Thinktecture.IdentityServer");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceSourceLogger"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public TraceSourceLogger(string name)
        {
            _name = name;
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
                var eventType = GetEventType(logLevel);
                EnsureCorrelationId();

                if (exception == null)
                {
                    _source.TraceEvent(eventType, 0, string.Format("{0}: {1}", _name, messageFunc()));
                }
                else
                {
                    _source.TraceEvent(eventType, 0, string.Format("{0}: {1}\n{2}", _name, messageFunc(), exception));
                }
            }

            return true;
        }

        private TraceEventType GetEventType(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Debug:
                    return TraceEventType.Verbose;
                case LogLevel.Error:
                    return TraceEventType.Error;
                case LogLevel.Fatal:
                    return TraceEventType.Critical;
                case LogLevel.Info:
                    return TraceEventType.Information;
                case LogLevel.Trace:
                    return TraceEventType.Verbose;
                case LogLevel.Warn:
                    return TraceEventType.Warning;
            }

            return TraceEventType.Verbose;
        }

        private void EnsureCorrelationId()
        {
            if (Trace.CorrelationManager.ActivityId == Guid.Empty)
            {
                Trace.CorrelationManager.ActivityId = Guid.NewGuid();
            }
        }
    }
}
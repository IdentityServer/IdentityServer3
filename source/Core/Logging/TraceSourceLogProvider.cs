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

using System;
using System.Diagnostics;

namespace Thinktecture.IdentityServer.Core.Logging
{
    /// <summary>
    /// Implementation of <see cref="ILogProvider"/> that uses <see cref="TraceSourceLogger"/>.
    /// </summary>
    public class TraceSourceLogProvider : ILogProvider
    {
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
        /// Log a message the specified log level.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="messageFunc">The message function.</param>
        /// <returns></returns>
        /// <remarks>
        /// Note to implementors: the message func should not be called if the loglevel is not enabled
        /// so as not to incur perfomance penalties.
        /// </remarks>
        public bool Log(LogLevel logLevel, Func<string> messageFunc)
        {
            if (messageFunc != null)
            {
                var eventType = GetEventType(logLevel);
                EnsureCorrelationId();

                _source.TraceEvent(eventType, 0, string.Format("{0}: {1}", _name, messageFunc()));
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
                var eventType = GetEventType(logLevel);
                EnsureCorrelationId();

                _source.TraceEvent(eventType, 0, string.Format("{0}: {1}\n{2}", _name, messageFunc(), exception.ToString()));
            }
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
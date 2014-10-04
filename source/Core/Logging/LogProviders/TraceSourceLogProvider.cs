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
    public class TraceSourceLogProvider : ILogProvider
    {
        public ILog GetLogger(string name)
        {
            return new TraceSourceLogger(name);
        }
    }

    public class TraceSourceLogger : ILog
    {
        private readonly string _name;
        private static readonly TraceSource _source;

        static TraceSourceLogger()
        {
            _source = new TraceSource("Thinktecture.IdentityServer");
        }

        public TraceSourceLogger(string name)
        {
            _name = name;
        }

        public void Log(LogLevel logLevel, Func<string> messageFunc)
        {
            var eventType = GetEventType(logLevel);
            EnsureCorrelationId();

            _source.TraceEvent(eventType, 0, string.Format("{0}: {1}", _name, messageFunc()));
        }

        public void Log<TException>(LogLevel logLevel, Func<string> messageFunc, TException exception) where TException : Exception
        {
            var eventType = GetEventType(logLevel);
            EnsureCorrelationId();

            _source.TraceEvent(eventType, 0, string.Format("{0}: {1}\n{2}", _name, messageFunc(), exception.ToString()));
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

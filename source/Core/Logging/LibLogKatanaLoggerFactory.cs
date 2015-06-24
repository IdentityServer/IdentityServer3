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

using Microsoft.Owin.Logging;
using System;
using System.Diagnostics;

namespace IdentityServer3.Core.Logging
{
    internal class LibLogKatanaLoggerFactory : ILoggerFactory
    {
        public ILogger Create(string name)
        {
            return new LibLogLogger(LogProvider.GetLogger(name));
        }

        private class LibLogLogger : ILogger
        {
            private readonly ILog _logger;

            public LibLogLogger(ILog logger)
            {
                _logger = logger;
            }

            public bool WriteCore(
                TraceEventType eventType,
                int eventId,
                object state,
                Exception exception,
                Func<object, Exception, string> formatter)
            {
                return state == null
                    ? _logger.Log(Map(eventType), null) // Equivalent to IsLogLevelXEnabled 
                    //TODO What to do with eventId?
                    : _logger.Log(Map(eventType), () => formatter(state, exception), exception);
            }

            private LogLevel Map(TraceEventType eventType)
            {
                switch (eventType)
                {
                    case TraceEventType.Critical:
                        return LogLevel.Fatal;
                    case TraceEventType.Error:
                        return LogLevel.Error;
                    case TraceEventType.Warning:
                        return LogLevel.Warn;
                    case TraceEventType.Information:
                        return LogLevel.Info;
                    case TraceEventType.Verbose:
                        return LogLevel.Trace;
                    case TraceEventType.Start:
                        return LogLevel.Info;
                    case TraceEventType.Stop:
                        return LogLevel.Info;
                    case TraceEventType.Suspend:
                        return LogLevel.Info;
                    case TraceEventType.Resume:
                        return LogLevel.Info;
                    case TraceEventType.Transfer:
                        return LogLevel.Info;
                    default:
                        return LogLevel.Info;
                }
            }
        }
    }
}

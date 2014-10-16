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
    public class DiagnosticsTraceLogProvider : ILogProvider
    {
        public ILog GetLogger(string name)
        {
            return new DiagnosticsTraceLogger(name);
        }
    }

    public class DiagnosticsTraceLogger : ILog
    {
        private readonly string _name = string.Empty;

        public DiagnosticsTraceLogger(string name)
        {
            _name = string.Format("[{0}]", name);
        }

        public void Log(LogLevel logLevel, Func<string> messageFunc)
        {
            var message = string.Format("{0}: {1} -- {2}", _name, DateTime.UtcNow.ToString(), messageFunc());
            TraceMsg(logLevel, message);
        }

        public void Log<TException>(LogLevel logLevel, Func<string> messageFunc, TException exception) where TException : Exception
        {
            var message = string.Format("{0}: {1} -- {2}\n{3}", _name, DateTime.UtcNow.ToString(), messageFunc(), exception.ToString());
            TraceMsg(logLevel, message);
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
                default:
                    break;
            }
        }
    }
}
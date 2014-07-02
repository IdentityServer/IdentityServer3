/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
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
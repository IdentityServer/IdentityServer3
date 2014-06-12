using System;
using System.Diagnostics;

namespace Thinktecture.IdentityServer.Core.Logging
{
    public class DiagnosticsTraceLogProvider : ILogProvider
    {
        public ILog GetLogger(string name)
        {
            return new DiagnosticsTraceLogger();
        }
    }

    public class DiagnosticsTraceLogger : ILog
    {
        public void Log(LogLevel logLevel, Func<string> messageFunc)
        {
            Trace.WriteLine(messageFunc(), logLevel.ToString());
        }

        public void Log<TException>(LogLevel logLevel, Func<string> messageFunc, TException exception) where TException : Exception
        {
            Trace.WriteLine(messageFunc() + ": " + exception.ToString(), logLevel.ToString());
        }
    }
}
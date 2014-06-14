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
        string _name;

        public DiagnosticsTraceLogger()
        {
            _name = "";
        }

        public DiagnosticsTraceLogger(string name)
        {
            _name = string.Format("[{0}]: ", name);
        }

        public void Log(LogLevel logLevel, Func<string> messageFunc)
        {
            Trace.WriteLine(_name + messageFunc(), logLevel.ToString());
        }

        public void Log<TException>(LogLevel logLevel, Func<string> messageFunc, TException exception) where TException : Exception
        {
            Trace.WriteLine(_name + messageFunc() + ": " + exception.ToString(), logLevel.ToString());
        }
    }
}
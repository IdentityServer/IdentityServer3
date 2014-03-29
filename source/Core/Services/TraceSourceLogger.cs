/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Diagnostics;

namespace Thinktecture.IdentityServer.Core.Services
{
    public class TraceSourceLogger : ILogger
    {
        TraceSource ts = new TraceSource("Thinktecture.IdentityServer");

        [DebuggerStepThrough]
        public void Verbose(string message)
        {
            ts.TraceEvent(TraceEventType.Verbose, 0, message);
        }

        [DebuggerStepThrough]
        public void Information(string message)
        {
            ts.TraceEvent(TraceEventType.Information, 0, message);
        }

        [DebuggerStepThrough]
        public void Warning(string message)
        {
            ts.TraceEvent(TraceEventType.Warning, 0, message);
        }

        [DebuggerStepThrough]
        public void Error(string message)
        {
            ts.TraceEvent(TraceEventType.Error, 0, message);
        }

        [DebuggerStepThrough]
        public void Start(string action)
        {
            ts.TraceEvent(TraceEventType.Start, 0, action);
        }
    }
}

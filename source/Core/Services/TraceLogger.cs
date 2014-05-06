/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Services
{
    public class TraceLogger : ILogger
    {
        [DebuggerStepThrough]
        public void Verbose(string message)
        {
            Trace.TraceInformation(message);
        }

        [DebuggerStepThrough]
        public void Information(string message)
        {
            Trace.TraceInformation(message);
        }

        [DebuggerStepThrough]
        public void Warning(string message)
        {
            Trace.TraceWarning(message);
        }

        [DebuggerStepThrough]
        public void Error(string message)
        {
            Trace.TraceError(message);
        }

        [DebuggerStepThrough]
        public void Start(string action)
        {
            Trace.TraceInformation("Start: " + action);
        }
    }
}
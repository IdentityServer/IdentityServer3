/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System.Diagnostics;

namespace Thinktecture.IdentityServer.Core.Services
{
    public class DebugLogger : ILogger
    {
        [DebuggerStepThrough]
        public void Verbose(string message)
        {
            System.Diagnostics.Trace.WriteLine(message);
        }

        [DebuggerStepThrough]
        public void Information(string message)
        {
            System.Diagnostics.Trace.WriteLine(message);
        }

        [DebuggerStepThrough]
        public void Warning(string message)
        {
            System.Diagnostics.Trace.WriteLine(message);
        }

        [DebuggerStepThrough]
        public void Error(string message)
        {
            System.Diagnostics.Trace.WriteLine(message);
        }

        [DebuggerStepThrough]
        public void Start(string action)
        {
            System.Diagnostics.Trace.WriteLine(action);
        }
    }
}

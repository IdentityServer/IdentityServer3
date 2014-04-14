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
        public void Verbose(string message)
        {
            Trace.TraceInformation(message);
        }

        public void Information(string message)
        {
            Trace.TraceInformation(message);
        }

        public void Warning(string message)
        {
            Trace.TraceWarning(message);
        }

        public void Error(string message)
        {
            Trace.TraceError(message);
        }

        public void Start(string action)
        {
            Trace.TraceInformation("Start: " + action);
        }
    }
}
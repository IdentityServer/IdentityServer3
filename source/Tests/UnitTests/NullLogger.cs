using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Services;

namespace UnitTests
{
    class NullLogger : ILogger
    {
        [DebuggerStepThrough]
        public void Verbose(string message)
        {
            
        }

        [DebuggerStepThrough]
        public void Information(string message)
        {
            
        }

        [DebuggerStepThrough]
        public void Warning(string message)
        {
            
        }

        [DebuggerStepThrough]
        public void Error(string message)
        {
            
        }

        [DebuggerStepThrough]
        public void Start(string action)
        {
            
        }
    }
}
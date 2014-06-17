/*
 * Copyright (c) Damian Hickey.  All rights reserved.
 * see license
 */
namespace Thinktecture.IdentityServer.Core.Logging
{
	using System;
	using System.Diagnostics;

	public static class LogProvider
	{
		private static ILogProvider currentLogProvider;

        public static ILog GetCurrentClassLogger()
        {
            var stackFrame = new StackFrame(1, false);
            var method = stackFrame.GetMethod();
            if (method == null || method.DeclaringType == null)
            {
                return GetLogger("unknown");
            }

            return GetLogger(method.DeclaringType);
        }

		public static ILog GetLogger(Type type)
		{
			return GetLogger(type.FullName);
		}

		public static ILog GetLogger(string name)
		{
			ILogProvider temp = currentLogProvider ?? ResolveLogProvider();
			return temp == null ? new NoOpLogger() : (ILog)new LoggerExecutionWrapper(temp.GetLogger(name));
		}

		public static void SetCurrentLogProvider(ILogProvider logProvider)
		{
			currentLogProvider = logProvider;
		}

		private static ILogProvider ResolveLogProvider()
		{
			if (NLogLogProvider.IsLoggerAvailable())
			{
				return new NLogLogProvider();
			}
			if (Log4NetLogProvider.IsLoggerAvailable())
			{
				return new Log4NetLogProvider();
			}
			return null;
		}

		public class NoOpLogger : ILog
		{
			public void Log(LogLevel logLevel, Func<string> messageFunc)
			{}

			public void Log<TException>(LogLevel logLevel, Func<string> messageFunc, TException exception)
				where TException : Exception
			{}
		}
	}
}
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
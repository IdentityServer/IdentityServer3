namespace Thinktecture.IdentityServer.Core.Logging
{
	using System;

	public interface ILog
	{
		void Log(LogLevel logLevel, Func<string> messageFunc);

		void Log<TException>(LogLevel logLevel, Func<string> messageFunc, TException exception) where TException : Exception;
	}
}
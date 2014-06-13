namespace Thinktecture.IdentityServer.Core.Logging
{
	public interface ILogProvider
	{
		ILog GetLogger(string name);
	}
}
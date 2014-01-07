namespace Thinktecture.IdentityServer.Core.Repositories
{
    public interface ISettingsRepository
    {
        void Set(string name, string value);
        string Get(string name);
    }
}

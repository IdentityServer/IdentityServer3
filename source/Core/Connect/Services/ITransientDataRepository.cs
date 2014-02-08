
namespace Thinktecture.IdentityServer.Core.Connect.Services
{
    public interface ITransientDataRepository<T>
    {
        void Store(string key, T value);
        T Get(string key);
        void Remove(string key);
    }

   
}

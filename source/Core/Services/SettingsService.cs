using Thinktecture.IdentityServer.Core.Repositories;

namespace Thinktecture.IdentityServer.Core.Services
{
    public class SettingsService : ISettingsService
    {
        ISettingsRepository _repository;

        private string _prefix = "global";

        public string Prefix
        {
            get { return _prefix; }
            set { _prefix = value; }
        }

        public SettingsService(ISettingsRepository repository)
        {
            _repository = repository;
        }

        public string Get(string name)
        {
            return _repository.Get(_prefix + ":" + name);
        }


        public void Set(string name, string value)
        {
            throw new System.NotImplementedException();
        }
    }
}
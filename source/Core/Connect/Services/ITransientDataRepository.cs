using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.IdentityServer.Core.Connect.Services
{
    public interface ITransientDataRepository<T>
    {
        void Store(string key, T value);
        T Get(string key);
        void Remove(string key);
    }

   
}

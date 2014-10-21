using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Services.Default
{
    public class TokenMetadataConsentStoreAdapter : IConsentStore
    {
        Func<string, Task<IEnumerable<ITokenMetadata>>> get;
        Func<string, string, Task> delete;

        public TokenMetadataConsentStoreAdapter(
            Func<string, Task<IEnumerable<ITokenMetadata>>> get, 
            Func<string, string, Task> delete)
        {
            if (get == null) throw new ArgumentNullException("get");
            if (delete == null) throw new ArgumentNullException("delete");

            this.get = get;
            this.delete = delete;
        }

        public async Task<IEnumerable<Models.Consent>> LoadAllAsync(string subject)
        {
            var tokens = await get(subject);
            
            var query =
                from token in tokens
                select new Consent
                {
                    ClientId = token.ClientId,
                    Subject = token.SubjectId,
                    Scopes = token.Scopes
                };

            return query.ToArray();
        }

        public Task<Models.Consent> LoadAsync(string subject, string client)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Models.Consent consent)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAsync(string subject, string client)
        {
            await delete(subject, client);
        }
    }
}

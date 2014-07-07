using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Connect.Models;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Connect.Services
{
    public class DefaultRefreshTokenService : IRefreshTokenService
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly IRefreshTokenStore _store;
        
        public DefaultRefreshTokenService(IRefreshTokenStore store)
        {
            _store = store;
        }

        public async Task<string> CreateRefreshTokenAsync(Token accessToken, Client client)
        {
            Logger.Debug("Creating refresh token");

            var refreshToken = new RefreshToken
            {
                Handle = Guid.NewGuid().ToString("N"),
                ClientId = client.ClientId,
                CreationTime = DateTime.UtcNow,
                LifeTime = client.RefreshTokenLifetime,
                AccessToken = accessToken
            };

            await _store.StoreAsync(refreshToken.Handle, refreshToken);
            return refreshToken.Handle;
        }

        public async Task<string> UpdateRefreshTokenAsync(RefreshToken refreshToken, Client client)
        {
            bool needsUpdate = false;
            string oldHandle = refreshToken.Handle;

            if (client.RefreshTokenUsage == TokenUsage.OneTimeOnly)
            {
                // generate new handle
                refreshToken.Handle = Guid.NewGuid().ToString("N");
                needsUpdate = true;
            }

            if (client.RefreshTokenExpiration == TokenExpiration.Sliding)
            {
                refreshToken.LifeTime = refreshToken.CreationTime.GetLifetimeInSeconds() + client.RefreshTokenLifetime;
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                // delete old one
                await _store.RemoveAsync(oldHandle);

                // create new one
                await _store.StoreAsync(refreshToken.Handle, refreshToken);

                return refreshToken.Handle;
            }

            return oldHandle;
        }
    }
}
using System;
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

            int lifetime;
            if (client.RefreshTokenExpiration == TokenExpiration.Absolute)
            {
                lifetime = client.AbsoluteRefreshTokenLifetime;
            }
            else
            {
                lifetime = client.SlidingRefreshTokenLifetime;
            }

            var refreshToken = new RefreshToken
            {
                Handle = Guid.NewGuid().ToString("N"),
                ClientId = client.ClientId,
                CreationTime = DateTime.UtcNow,
                LifeTime = lifetime,
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
                // todo: make sure we don't exceed absolute exp
                // cap it at absolute exp

                var currentLifetime = refreshToken.CreationTime.GetLifetimeInSeconds();
                var newLifetime = currentLifetime + client.SlidingRefreshTokenLifetime;
    
                if (newLifetime > client.AbsoluteRefreshTokenLifetime)
                {
                    newLifetime = client.AbsoluteRefreshTokenLifetime;
                }

                refreshToken.LifeTime = newLifetime;
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
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

using System;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Services.Default
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
                Logger.Debug("Setting an absolute lifetime: " + client.AbsoluteRefreshTokenLifetime);
                lifetime = client.AbsoluteRefreshTokenLifetime;
            }
            else
            {
                Logger.Debug("Setting a sliding lifetime: " + client.SlidingRefreshTokenLifetime);
                lifetime = client.SlidingRefreshTokenLifetime;
            }

            var handle = Guid.NewGuid().ToString("N");
            var refreshToken = new RefreshToken
            {
                ClientId = client.ClientId,
                CreationTime = DateTime.UtcNow,
                LifeTime = lifetime,
                AccessToken = accessToken
            };

            await _store.StoreAsync(handle, refreshToken);
            return handle;
        }

        public async Task<string> UpdateRefreshTokenAsync(string handle, RefreshToken refreshToken, Client client)
        {
            Logger.Debug("Updating refresh token");

            bool needsUpdate = false;

            if (client.RefreshTokenUsage == TokenUsage.OneTimeOnly)
            {
                Logger.Debug("Token usage is one-time only. Generating new handle");

                // generate new handle
                needsUpdate = true;
            }

            if (client.RefreshTokenExpiration == TokenExpiration.Sliding)
            {
                Logger.Debug("Refresh token expiration is sliding - extending lifetime");

                // make sure we don't exceed absolute exp
                // cap it at absolute exp
                var currentLifetime = refreshToken.CreationTime.GetLifetimeInSeconds();
                Logger.Debug("Current lifetime: " + currentLifetime.ToString());

                var newLifetime = currentLifetime + client.SlidingRefreshTokenLifetime;
                Logger.Debug("New lifetime: " + newLifetime.ToString());

                if (newLifetime > client.AbsoluteRefreshTokenLifetime)
                {
                    newLifetime = client.AbsoluteRefreshTokenLifetime;
                    Logger.Debug("New lifetime exceeds absolute lifetime, capping it to " + newLifetime.ToString());
                }

                refreshToken.LifeTime = newLifetime;
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                // delete old one
                await _store.RemoveAsync(handle);

                // create new one
                string newHandle = Guid.NewGuid().ToString("N");
                await _store.StoreAsync(newHandle, refreshToken);

                Logger.Debug("Updated refresh token in store");
                return newHandle;
            }

            Logger.Debug("No updates to refresh token done");
            return handle;
        }
    }
}
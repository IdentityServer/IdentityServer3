﻿/*
 * Copyright 2014, 2015 Dominick Baier, Brock Allen
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

using System.Threading.Tasks;
using Thinktecture.IdentityModel;
using Thinktecture.IdentityServer.Core.Extensions;
using Thinktecture.IdentityServer.Core.Logging;
using Thinktecture.IdentityServer.Core.Models;

namespace Thinktecture.IdentityServer.Core.Services.Default
{
    /// <summary>
    /// Default refresh token service
    /// </summary>
    public class DefaultRefreshTokenService : IRefreshTokenService
    {
        /// <summary>
        /// The logger
        /// </summary>
        protected readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        /// <summary>
        /// The refresh token store
        /// </summary>
        protected readonly IRefreshTokenStore _store;

        /// <summary>
        /// The _events
        /// </summary>
        protected readonly IEventService _events;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRefreshTokenService" /> class.
        /// </summary>
        /// <param name="store">The refresh token store.</param>
        /// <param name="events">The events.</param>
        public DefaultRefreshTokenService(IRefreshTokenStore store, IEventService events)
        {
            _store = store;
            _events = events;
        }

        /// <summary>
        /// Creates the refresh token.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="client">The client.</param>
        /// <returns>
        /// The refresh token handle
        /// </returns>
        public virtual async Task<string> CreateRefreshTokenAsync(Token accessToken, Client client)
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

            var handle = CryptoRandom.CreateUniqueId();
            var refreshToken = new RefreshToken
            {
                CreationTime = DateTimeOffsetHelper.UtcNow,
                LifeTime = lifetime,
                AccessToken = accessToken
            };

            await _store.StoreAsync(handle, refreshToken);

            RaiseRefreshTokenIssuedEvent(handle, refreshToken);
            return handle;
        }

        /// <summary>
        /// Updates the refresh token.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="client">The client.</param>
        /// <returns>
        /// The refresh token handle
        /// </returns>
        public virtual async Task<string> UpdateRefreshTokenAsync(string handle, RefreshToken refreshToken, Client client)
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
                string newHandle = CryptoRandom.CreateUniqueId();
                await _store.StoreAsync(newHandle, refreshToken);

                RaiseRefreshTokenRefreshedEvent(handle, newHandle, refreshToken);
                Logger.Debug("Updated refresh token in store");

                return newHandle;
            }

            RaiseRefreshTokenRefreshedEvent(handle, handle, refreshToken);
            Logger.Debug("No updates to refresh token done");

            return handle;
        }

        /// <summary>
        /// Raises the refresh token issued event.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="token">The token.</param>
        protected void RaiseRefreshTokenIssuedEvent(string handle, RefreshToken token)
        {
            _events.RaiseRefreshTokenIssuedEvent(handle, token);
        }

        /// <summary>
        /// Raises the refresh token refreshed event.
        /// </summary>
        /// <param name="oldHandle">The old handle.</param>
        /// <param name="newHandle">The new handle.</param>
        /// <param name="token">The token.</param>
        protected void RaiseRefreshTokenRefreshedEvent(string oldHandle, string newHandle, RefreshToken token)
        {
            _events.RaiseSuccessfulRefreshTokenRefreshEvent(oldHandle, newHandle, token);
        }
    }
}
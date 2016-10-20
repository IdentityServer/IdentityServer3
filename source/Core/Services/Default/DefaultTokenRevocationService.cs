using System.Threading.Tasks;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Logging;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Validation;

namespace IdentityServer3.Core.Services.Default
{
    /// <summary>
    /// Default token revocation service
    /// </summary>
    public class DefaultTokenRevocationService : ITokenRevocationService
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly ITokenHandleStore _tokenHandles;
        private readonly IRefreshTokenStore _refreshTokens;
        private readonly IEventService _events;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTokenRevocationService" /> class.
        /// </summary>
        /// <param name="tokenHandles">The token handle store.</param>
        /// <param name="refreshTokens">The refresh token store.</param>
        /// <param name="events">The event service.</param>
        public DefaultTokenRevocationService(ITokenHandleStore tokenHandles, IRefreshTokenStore refreshTokens, IEventService events)
        {
            _tokenHandles = tokenHandles;
            _refreshTokens = refreshTokens;
            _events = events;
        }

        /// <summary>
        /// Revokes token.
        /// </summary>
        /// <param name="requestResult">The token revocation requestResult</param>
        /// <returns></returns>
        public async Task<OperationResult> RevokeAsync(TokenRevocationRequestValidationResult requestResult)
        {
            // revoke tokens
            switch (requestResult.TokenTypeHint)
            {
                case Constants.TokenTypeHints.AccessToken:
                    return await RevokeAccessTokenAsync(requestResult.Token, requestResult.Client);
                case Constants.TokenTypeHints.RefreshToken:
                    return await RevokeRefreshTokenAsync(requestResult.Token, requestResult.Client);
            }
            
            var revokeAccessToken = await RevokeAccessTokenAsync(requestResult.Token, requestResult.Client);
            if (revokeAccessToken.IsDone)
            {
                return revokeAccessToken;
            }
            return await RevokeRefreshTokenAsync(requestResult.Token, requestResult.Client);
        }
        
        private async Task<OperationResult> RevokeAccessTokenAsync(string handle, Client client)
        {
            var token = await _tokenHandles.GetAsync(handle);

            if (token == null)
            {
                return new OperationResult(false);
            }

            // revoke access token only if it belongs to client doing the request
            if (token.ClientId != client.ClientId)
            {
                var message = string.Format("Client {0} tried to revoke an access token belonging to a different client: {1}", client.ClientId, token.ClientId);
                Logger.Warn(message);
                return new OperationResult(message);
            }

            await _tokenHandles.RemoveAsync(handle);
            await _events.RaiseTokenRevokedEventAsync(token.SubjectId, handle, Constants.TokenTypeHints.AccessToken);
            return new OperationResult(true);
        }

        private async Task<OperationResult> RevokeRefreshTokenAsync(string handle, Client client)
        {
            var token = await _refreshTokens.GetAsync(handle);
            
            if (token == null)
            {
                return new OperationResult(false);
            }

            // revoke refresh token only if it belongs to client doing the request
            if (token.ClientId != client.ClientId)
            {
                var message = string.Format("Client {0} tried to revoke an access token belonging to a different client: {1}", client.ClientId, token.ClientId);
                Logger.Warn(message);
                return new OperationResult(message);
            }

            await _refreshTokens.RevokeAsync(token.SubjectId, token.ClientId);
            await _tokenHandles.RevokeAsync(token.SubjectId, token.ClientId);
            await _events.RaiseTokenRevokedEventAsync(token.SubjectId, handle, Constants.TokenTypeHints.RefreshToken);
            return new OperationResult(true);
        }
    }
}
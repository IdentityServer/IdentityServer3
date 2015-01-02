### General

- Refresh tokens are supported for the following flows: authorization code, hybrid and resource owner password credential flow.
- The clients needs to be allowed to request the _offline_access_ scope to get a refresh token.

### Settings on the Client class
- `RefreshTokenUsage` 
    - ReUse: the refresh token handle will stay the same when refreshing tokens
    - OneTime: the refresh token handle will be updated when refreshing tokens
- `RefreshTokenExpiration`
    - Absolute: the refresh token will expire on a fixed point in time (specified by the AbsoluteRefreshTokenLifetime)
    - Sliding: when refreshing the token, the lifetime of the refresh token will be renewed (by the amount specified in SlidingRefreshTokenLifetime). The lifetime will not exceed the absolute lifetime.
- `AbsoluteRefreshTokenLifetime`
    - Maximum lifetime of a refresh token in seconds. Defaults to 2592000 seconds / 30 days
- `SlidingRefreshTokenLifetime`
    - Sliding lifetime of a refresh token in seconds. Defaults to 1296000 seconds / 15 days

### Usage

- Request the offline_access scope (via code or resource owner flow)
- Refresh the token by using a refresh_token grant


### Samples

- The Clients [sample](https://github.com/thinktecture/Thinktecture.IdentityServer.v3.Samples/tree/master/source/Clients) has both [resource owner](https://github.com/thinktecture/Thinktecture.IdentityServer.v3.Samples/tree/master/source/Clients/ConsoleResourceOwnerRefreshTokenClient) and [code flow](https://github.com/thinktecture/Thinktecture.IdentityServer.v3.Samples/tree/master/source/Clients/MvcCodeFlowClientManual) clients.
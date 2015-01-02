IdentityServer v3 contains many features for implementing OpenID Connect and OAuth2. Many of these features have been designed so they can replaced. This would be useful for the scenarios where the default logic doesn’t match the hosting application’s requirements, or simply the application wishes to provide an entirely different implementation. And in fact, there are some extensibility point within IdentityServer v3 that are required to be provided by the hosting application (such as the storage for configuration data or the identity management implementation for validating users’ credentials).

The `Thinktecture.IdentityServer.Core.Configuration.IdentityServerServiceFactory` holds all these building blocks and must be supplied at startup time using the `IdentityServerOptions` class (see [here](https://github.com/thinktecture/Thinktecture.IdentityServer.v3/wiki/Configuration-Options) for more information on configuration options).

The extensibility points fall into three categories.

#### Mandatory

* `UserService`
    * Implements user authentication against the local user store as well as association of external users. There are two standard implementations for [MembershipReboot](https://github.com/thinktecture/Thinktecture.IdentityServer.v3.MembershipReboot) and [ASP.NET Identity](https://github.com/thinktecture/Thinktecture.IdentityServer.v3.AspNetIdentity)
* `ScopeStore`
    * Implements retrieval of scopes configuration data
* `ClientStore`
    * Implements retrieval of client configuration data

The `InMemoryFactory` allows setting up a service factory by providing in-memory stores for users, clients and scopes (see [here](https://github.com/thinktecture/Thinktecture.IdentityServer.v3/blob/master/source%2FCore%2FConfiguration%2FInMemoryFactory.cs)).

#### Mandatory for production scenarios (but with default in-memory implementation)

* `AuthorizationCodeStore`
    * Implements storage and retrieval of authorization codes ([interface](https://github.com/thinktecture/Thinktecture.IdentityServer.v3/blob/master/source%2FCore%2FServices%2FITransientDataRepository.cs))
* `TokenHandleStore` 
    * Implements storage and retrieval of handles for reference tokens ([interface](https://github.com/thinktecture/Thinktecture.IdentityServer.v3/blob/master/source%2FCore%2FServices%2FITransientDataRepository.cs))
* `RefreshTokenStore` 
    * Implements storage and retrieval of refresh tokens ([interface](https://github.com/thinktecture/Thinktecture.IdentityServer.v3/blob/master/source%2FCore%2FServices%2FITransientDataRepository.cs))
* `ConsentStore` 
    * Implements storage and retrieval of consent decisions ([interface](https://github.com/thinktecture/Thinktecture.IdentityServer.v3/blob/master/source/Core/Services/IConsentStore.cs))
* `ViewService`
    * Implements retrieval of UI assets. Defaults to using the embedded assets. ([interface](https://github.com/thinktecture/Thinktecture.IdentityServer.v3/blob/master/source%2FCore%2FServices%2FIViewService.cs))

#### Optional

* `TokenService`
    * Implements creation of identity and access tokens ([interface](https://github.com/thinktecture/Thinktecture.IdentityServer.v3/blob/master/source%2FCore%2FServices%2FITokenService.cs))
* `ClaimsProvider`
    * Implements retrieval of claims for identity and access tokens ([interface](https://github.com/thinktecture/Thinktecture.IdentityServer.v3/blob/master/source%2FCore%2FServices%2FIClaimsProvider.cs))
* `TokenSigningService`
    * Implements creation and signing of security tokens ([interface](https://github.com/thinktecture/Thinktecture.IdentityServer.v3/blob/master/source%2FCore%2FServices%2FITokenSigningService.cs))
* `CustomGrantValidator`
    * Implements validation of custom grant types ([interface](https://github.com/thinktecture/Thinktecture.IdentityServer.v3/blob/master/source%2FCore%2FServices%2FICustomGrantValidator.cs))
* `CustomRequestValidator`
    * Implements custom additional validation of authorize and token requests ([interface](https://github.com/thinktecture/Thinktecture.IdentityServer.v3/blob/master/source%2FCore%2FServices%2FICustomRequestValidator.cs))
* `RefreshTokenService`
    * Implements creation and updates of refresh tokens ([interface](https://github.com/thinktecture/Thinktecture.IdentityServer.v3/blob/master/source%2FCore%2FServices%2FIRefreshTokenService.cs))
* `ExternalClaimsFilter`
    * Implements filtering and transformation of claims for external identity providers ([interface](https://github.com/thinktecture/Thinktecture.IdentityServer.v3/blob/master/source%2FCore%2FServices%2FIExternalClaimsFilter.cs))
* `CustomTokenValidator`
    * Implements custom additional validation of tokens for the token validation endpoints ([interface](https://github.com/thinktecture/Thinktecture.IdentityServer.v3/blob/master/source%2FCore%2FServices%2FICustomTokenValidator.cs))
* `ConsentService` 
    * Implements logic of consent decisions ([interface](https://github.com/thinktecture/Thinktecture.IdentityServer.v3/blob/master/source/Core/Services/IConsentService.cs))
* `ClientPermissionsService`
    * Implements retrieval and revocation of consents, reference and refresh tokens ([interface](https://github.com/thinktecture/Thinktecture.IdentityServer.v3/blob/master/source%2FCore%2FServices%2FIClientPermissionsService.cs))

See [here](https://github.com/thinktecture/Thinktecture.IdentityServer.v3/wiki/Registering-Custom-Services) for more information on registering your custom service and store implementations.
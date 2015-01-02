**The sample for this topic can be found [here](https://github.com/thinktecture/Thinktecture.IdentityServer.v3.Samples/tree/master/source/CustomUserService)**

IdentityServer v3 defines the `IUserService` interface to abstract the underlying identity management system being used for users. It provides semantics for users to authenticate with local accounts as well as external accounts. It also provides identity and claims to IdentityServer needed for tokens and the user info endpoint. Additionally, the user service can control the workflow a user will experience at login time. 

## Authentication

There are two APIs on the `IUserService` that model authentication -- one for local authentication and one for authentication with external identity providers.

* `Task<AuthenticateResult> AuthenticateLocalAsync(string username, string password, SignInMessage message)`
 * Invoked when the user provides a `username` and `password` to authenticate with a local account.
* `Task<AuthenticateResult> AuthenticateExternalAsync(ExternalIdentity externalUser)`
 * Invoked when the user has authenticated with an external provider. The user's identity from the external provider is passed via the `externalUser` parameter which contains the provider identifier and the provider's identifier for the user.

The returned `AuthenticateResult` indicates one of many possible outcomes. The constructor for the object returned from the authentication APIs indicate which of these outcomes is chosen. The outcomes are:

### Full login

To fully log the user the authentication API must produce a `subject` and a `name` that represent the user. The `subject` is the user service's unique identifier for the user and the `name` is a display name for the 
user that will be displayed in the user interface.

For external authentication, the `provider` must also be indicated which will be included as the `idp` claim in the identity token and the user info endpoint.

This full login is performed by using the Katana cookie authentication middleware with an `AuthenticationType` indicated by the constant `Constants.PrimaryAuthenticationType`.

### Partial login (with redirect)

In addition to a full login, the authentication APIs can perform a "partial login". A partial login indicates that the user has proven their identity, has a local account, but is not yet allowed to continue. They must first perform some other action or provide some other data before being allowed to login fully. This is useful to customize the user's workflow before allowing them to fully login. This could be useful to force a user to fill in a registration page, change a password, or accept a EULA before letting them continue.

The partial login will issue a `subject` and `name` for the user (which should be the same as the full login from above) as well as a `redirectPath`. This partial login is performed by using the Katana cookie authentication middleware with an `AuthenticationType` indicated by the constant `Constants.PartialSignInAuthenticationType`.

The `redirectPath` represents a custom web page provided by the hosting application that the user will be redirected to. On that web page the user's `subject` and `name` claims can be used to identify the user. In order to obtain these claims, the page must use the Katana authentication middleware to authenticate against the `Constants.PartialSignInAuthenticationType` authentication type.

Once the user has completed their work on the custom web page, they can be redirected back to IdentityServer to continue with the full login process. The URL to redirect the user back to provided as a claim in the `Constants.PartialSignInAuthenticationType` authentication type and is identified by the claim type `Constants.ClaimTypes.PartialLoginReturnUrl`.

### External login (with redirect)

It's possible that the user has performed an external authentication, but there is no local account for the user. A custom user service can choose to redirect the user without a local account. This is performed by creating a `AuthenticateResult` with a redirect and the `ExternalIdentity` passed to the `AuthenticateExternalAsync` API. This performs a partial login (as above via the `PartialSignInAuthenticationType`) but there is no subject claim in the issued cookie. Instead there is a claim of type `external_provider_user_id` (or via `Constants.ClaimTypes.ExternalProviderUserId`) whose `Issuer` is the external provider identifier and whose value is the external provider's identifier for the user. These values can then be used to create a local account and associate the external account.

Once the user has completed their registration on the custom web page, they can be redirected back to IdentityServer via the same `PartialLoginReturnUrl` as described above.

### Login error

Finally, the authentication APIs can provide an error that will be displayed on the login view.

## Profile

Once the user has been authenticated, IdentityServer uses the other two APIs on the `IUserService` interface to obtain claims and profile information about the user. These APIs are:

* `Task<IEnumerable<Claim>> GetProfileDataAsync(ClaimsPrincipal subject, IEnumerable<string> requestedClaimTypes = null)`
 * Returns the claims for a `subject`. The additional `requestedClaimTypes` parameter indicates which claims are requested and thus should act as a filter for the claims returned.
* `Task<bool> IsActiveAsync(ClaimsPrincipal subject)`
 * Indicates if the `subject` is still active and is allowed to obtain tokens.

---
layout: docs-default
---

**The `Thinktecture.IdentityServer.Core.Models.Scope` class models an OpenID Connect or OAuth2 scope.**

* `Name`
    * Unique name of the scope
* `DisplayName`
    * Display name for consent screen
* `Description`
    * Description
* `Required`
    * Specifies whether the user can de-select the scope on the consent screen
* `Emphasize`
    * Specifies whether the scope is emphasized on the consent screen
* `Type`
    * Either `Identity` (OpenID Connect related) or `Resource` (OAuth2 resources).
* `Claims`
    * Specifies a static list of claims that should get emitted into the corresponding identity and access token.
* `IncludeAllClaimsForUser`
    * If enabled, all claims for the user will be included in the token
* `ClaimsRule`
    * Rule for determining which claims should be included in the token (this is implementation specific)
* `ShowInDiscoveryDocument`
    * Specifies whether this scope is shown in the discovery document (defaults to true)

**Scope can also specify claims that go into the corresponding token - the `ScopeClaim` class has the following properties:**

* `Name`
    * Name of the claim
* `Description`
    * Description of the claim
* `AlwaysIncludeInIdToken`
    * Specifies whether this claim should always be present in the identity token (even if an access token has been requested as well). Applies to identity scopes only.

**Example of a `role` identity scope:**

```csharp
var roleScope = new Scope
{
    Name = "roles",
    DisplayName = "Roles",
    Description = "Your organizational roles",
    Type = ScopeType.Identity,

    Claims = new[]
    {
        new ScopeClaim(Constants.ClaimTypes.Role, alwaysInclude: true)
    }
};
```

The 'AlwaysIncludeInIdentityToken' property specifies that a certain should always be part of the identity token, even when an access token for the userinfo endpoint is requested.

**Example of a scope for the `IdentityManager` API:**

```csharp
var idMgrScope = new Scope
{
    Name = "idmgr",
    DisplayName = "Thinktecture IdentityManager",
    Type = ScopeType.Resource,
    Emphasize = true,
                        
    Claims = new[]
    {
        new ScopeClaim(Constants.ClaimTypes.Name),
        new ScopeClaim(Constants.ClaimTypes.Role)
    }
};
```

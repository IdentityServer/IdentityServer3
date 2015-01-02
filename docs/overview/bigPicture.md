---
layout: docs-default
---

IdentityServer's security model is based on two fundamental primitives: clients and scopes.

#### Client
A client is a piece of software that requests access or identity tokens at IdentityServer. Clients can come in different flavors: native desktop or mobile, browser-based or server based. Depending on the client type, OpenID Connect and OAuth2 describe patterns (also called flows) how to request tokens. Check the specs for more information about flows.  
By default a client can request any scope that is defined in IdentityServer - but you can restrict per client which scopes can be requested.

#### Scope
Scopes shape the outgoing identity and access tokens and come in two flavors: `identity` and `resource` scopes.

A resource scope is an identifier for a resource (also often called Web API). You could e.g. create a scope called "calendar" for you calendar API - or "calendar.readonly" if you want to partition you calendar API into sub "areas" - in that case read-only access. 

A client can then request a token e.g. for the "calendar" scope - and if allowed, this scope will be included as a claim in the access token. The calendar API (or resource) can then make sure the scope is present while validating the access token.

In addition you can associate user claims with a resource scope, to add information about the user into the access token.

Depending on the flow and configuration, the requested scopes will be shown to the user before the token is issued. This gives the user a chance to allow or deny access to the service. This is called consent.

Identity scopes define which user claims will be disclosed to the client (as opposed to the resource). These claims are either directly part of the identity token, or available at the userinfo endpoint.
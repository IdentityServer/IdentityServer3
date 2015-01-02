The `IdentityServerOptions` [class](https://github.com/thinktecture/Thinktecture.IdentityServer.v3/blob/master/source%2FCore%2FConfiguration%2FIdentityServerOptions.cs) is the top level container for all configuration settings of IdentityServer.

* `IssuerUri` (required)
    * Unique name of this server instance, e.g. https://myissuer.com
* `SiteName`
    * Display name of the site used in standard views.
* `SigningCertificate`
    * X.509 certificate (and corresponding private key) for signing security tokens.
* `SecondarySigningCertificate`
    * A secondary certificate that will appear in the discovery document. Can be used to prepare clients for certificate rollover.
* `RequireSsl`
    * Indicates if SSL is required for IdentityServer. Defaults to `true`.
* `PublicHostName`
    * By default, IdentityServer uses the host name from the HTTP request when creating links. This might not be accurate in reverse proxy or load-balancing situations. You can override the host name used for link generation using this property.
* `Endpoints`
    * Allows enabling or disabling specific endpoints (by default all endpoints are enabled).
* `Factory`
    * Sets the [IdentityServerFactory](https://github.com/thinktecture/Thinktecture.IdentityServer.v3/wiki/Service-Factory)
* `DataProtector`
    * Sets the [data protector](https://github.com/thinktecture/Thinktecture.IdentityServer.v3/wiki/Data-Protection).
* `AuthenticationOptions`
    * Configures [AuthenticationOptions](https://github.com/thinktecture/Thinktecture.IdentityServer.v3/wiki/Authentication-Options)
* `PluginConfiguration`
    * Allows adding [protocol plugins](https://github.com/thinktecture/Thinktecture.IdentityServer.v3/wiki/Protocol-and-Endpoint-Plugins).
* `CorsPolicy`
    * Configures [CORS](https://github.com/thinktecture/Thinktecture.IdentityServer.v3/wiki/CORS)
* `CspOptions`
    * Configures [CSP](https://github.com/thinktecture/Thinktecture.IdentityServer.v3/wiki/CSP)
* `ProtocolLogoutUrls`
    * Configures callback URLs that should be called during logouts (mainly useful for [protocol plugins](https://github.com/thinktecture/Thinktecture.IdentityServer.v3/wiki/Protocol-and-Endpoint-Plugins))
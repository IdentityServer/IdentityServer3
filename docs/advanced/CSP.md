IdentityServer incorporates the use of [Content Security Policy]((https://developer.mozilla.org/en-US/docs/Web/Security/CSP)) (CSP) for all HTML pages displayed.

### CspOptions

IdentityServer v3 allows the hosting application to configure a `CspOptions` on the `IdentityServerOptions` to control the CSP behavior. Below are the settings that are configurable:

* `Enabled` : indicates if CSP is enabled or disabled. Defaults to `true`.
* `ReportEndpoint` : indicates if the CSP report endpoint is enabled. Defaults to `Disabled`.
* `ScriptSrc` : allows for additional `script-src` values to be added to the default policy.
* `StyleSrc` : allows for additional `style-src` values to be added to the default policy.


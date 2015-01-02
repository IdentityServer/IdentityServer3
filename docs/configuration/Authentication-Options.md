**The sample for this topic can be found [here](https://github.com/thinktecture/Thinktecture.IdentityServer.v3.Samples/tree/master/source/CustomUserService)**

The `AuthenticationOptions` is a property on the `IdentityServerOptions` to customize the login view.

### EnableLocalLogin

`EnableLocalLogin` indicates if IdentityServer will allow users to authenticate with a local account. Disabling this setting will not display the username/password form on the login page. This also will disable the resource owner password flow.

### CookieOptions

IdentityServer v3 issues cookies to track the logged in user. By default this cookie expires after 10 hours, is not persistent, and does not have a sliding expiration. These settings can be changed via the `CookieOptions` property on the `AuthenticationOptions`. The lifetime of the cookie is controller by the `ExpireTimeSpan` property. Also, the `IsPersistent` property controls issuing a persistent cookie. 

Additionally, the `CookieOptions` has a `AllowRememberMe` property to indicate if IdentityServer will prompt the user to "Remember their login", which has the effect of issuing a persistent authentication cookie. The persistent cookie duration is controlled by the `RememberMeDuration` value. If this setting is in use then the user's decision (either yes or no) will override the `IsPersistent` setting. In other words, if both `IsPersistent` and `AllowRememberMe` is enabled and the user decides to not remember their login, then no persistent cookie will be issued.

Finally, the `CookieOptions` has a `Prefix` property that can be used to prefix the name of the cookies issued. This can be used if the hosting application has any name collisions with the cookies IdentityServer issues and any other cookies that might be used in the application.

### Login Links

`AuthenticationOptions` contains a collection of `LoginPageLink` objects. These `LoginPageLink`s allow the login view to provide the user custom links to other web pages that they might need to visit before they can login (such as a registration page, or a password reset page). 

The custom web page represented by the `LoginPageLink` would be provided by the hosting application. Once it has performed its task then it can resume the login workflow by redirecting the user back to the login view.

When a user follows one of the `LoginPageLink`s, a `signin` query string parameter is passed to the page. This parameter should be echoed back as a `signin` query string parameter to the login page when the user wishes to resume their login. The login view is located at the path "~/login" relative to IdentityServer's application base. 

### DisableSignOutPrompt
When a client initiates a sign-out, by default IdentityServer will ask the user for confirmation. This is a mitigation technique against "logout spam". This confirmation can be turned off.

### IdentityProviders
Allows configuring additional identity providers - see [here](https://github.com/thinktecture/Thinktecture.IdentityServer.v3/wiki/Identity-Providers).
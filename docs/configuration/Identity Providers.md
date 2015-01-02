IdentityServer supports authentication using external identity providers. The external authentication mechanism must be encapsulated in a Katana authentication middleware.

Katana itself ships with middleware for Google, Facebook, Twitter, Microsoft Accounts, WS-Federation and OpenID Connect - but there are also community developed middlewares.

To configure the middleware for the external providers, add a method to your project that accepts an `IAppBuilder` and a `string` as parameters.
```csharp
public static void ConfigureIdentityProviders(IAppBuilder app, string signInAsType)
{
    var google = new GoogleOAuth2AuthenticationOptions
    {
        AuthenticationType = "Google",
        Caption = "Google",
        SignInAsAuthenticationType = signInAsType,
        ClientId = "...",
        ClientSecret = "..."
    };
    app.UseGoogleAuthentication(google);

    var fb = new FacebookAuthenticationOptions
    {
        AuthenticationType = "Facebook",
        Caption = "Facebook",
        SignInAsAuthenticationType = signInAsType,
        AppId = "...",
        AppSecret = "..."
    };
    app.UseFacebookAuthentication(fb);

    var twitter = new TwitterAuthenticationOptions
    {
        AuthenticationType = "Twitter",
        Caption = "Facebook",
        SignInAsAuthenticationType = signInAsType,
        ConsumerKey = "...",
        ConsumerSecret = "..."
    };
    app.UseTwitterAuthentication(twitter);
}
```
**Remarks** 
* `AuthenticationType` must be a unique value to identify the external identity provider. This value will also be used for the `idp` claim in the resulting tokens. Furthermore the same value can be used to pre-select identity providers during authorization/authentication requests using the `login_hint` parameters. This value is also used to restrict the allowed identity providers on the `Client` configuration.
* `Caption` specifies the label of the button on the login page for the identity provider. If `Caption` is an empty string, the identity provider will not be shown on the login page. But can still be used via the login hint.
* `SignInAsAuthenticationType` must be set to the value we pass in via the `signInAsType` parameter

Assign the configuration method to the `IdentityProviders` property on the `AuthenticationOptions`:

```csharp
var idsrvOptions = new IdentityServerOptions
{
    IssuerUri = "https://idsrv3.com",
    SiteName = "Thinktecture IdentityServer v3",
    Factory = factory,
    SigningCertificate = Cert.Load(),

    AuthenticationOptions = new AuthenticationOptions 
    {
        IdentityProviders = ConfigureIdentityProviders
    }
};

app.UseIdentityServer(idsrvOptions);
```

### Adding WS-Federation Identity Providers
WS-Federation based identity providers can be added in the exact same way as shown above.

For backwards compatibility reasons, the WS-Federation middleware listens to all incoming requests and inspects them for incoming token posts. This is not an issue if you only have one WS-Federation middleware configured, but if you have more than one, you need to set an explicit and unique `CallbackPath` property that matches the reply URL configuration on the IdP.

```csharp
var adfs = new WsFederationAuthenticationOptions
{
    AuthenticationType = "adfs",
    Caption = "ADFS",
    SignInAsAuthenticationType = signInAsType,

    MetadataAddress = "https://adfs.leastprivilege.vm/federationmetadata/2007-06/federationmetadata.xml",
    Wtrealm = "urn:idsrv3"
};
app.UseWsFederationAuthentication(adfs);
```
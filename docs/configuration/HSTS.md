[HTTP Strict Transport Security](http://www.html5rocks.com/en/tutorials/security/transport-layer-security/) (or HSTS) is an important aspect in web security. IdentityServer v3 provides a configuration option to include the HSTS headers in all of its HTTP responses. To enable, use the `UseHsts` extension method on the `IAppBuilder` in your OWIN configuration:

```
public void Configuration(IAppBuilder app)
{
    app.UseHsts();

    // ...
}
```

If you wish to set the expiration (`max-age`), then `UseHsts` has overloads that accept an `int` for the number of days, or a `TimeSpan` for a custom duration.

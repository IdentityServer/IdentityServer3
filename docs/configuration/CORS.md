Many endpoints in IdentityServer will be accessed via Ajax calls from JavaScript. Given that IdentityServer will most likely be hosted on a different origin than these clients, this implies that [Cross-Origin Resource Sharing](http://www.html5rocks.com/en/tutorials/cors/) (CORS) will be an issue. 

### CorsPolicy

IdentityServer v3 allows the hosting application to configure a `CorsPolicy` on the `IdentityServerOptions` to control which origins are allowed. 

#### AllowedOrigins

The `CorsPolicy` has two ways to indicate which origins are allowed. The first is the `AllowedOrigins` collection of host names. This is useful if at application start time the list of origins is known (either hard coded or perhaps loaded from a database).

```
var idsvrOptions = new IdentityServerOptions();
idsrvOptions.CorsPolicy.AllowedOrigins.Add("http://myclient.com");
idsrvOptions.CorsPolicy.AllowedOrigins.Add("http://myotherclient.org
```

#### PolicyCallback

The second way the `CorsPolicy` allows the hosting application to indicate which origins are allowed is the `PolicyCallback` delegate which is a `Func<string, Task<bool>>`. This delegate is invoked at runtime when a CORS request is being made to IdentityServer and passed the origin being requested. A return `bool` value of `true` indicates that the origin is allowed. This is useful if the list of allowed origins changes frequently or is sufficiently large such that a database lookup is preferred.

```
var idsvrOptions = new IdentityServerOptions();
idsrvOptions.CorsPolicy.PolicyCallback = async origin =>
{
    return await SomeDatabaseCallToCheckIfOriginIsAllowed(origin);
};
```

#### CorsOptions.AllowAll

For convenience there is a static property `CorsOptions.AllowAll` that will allow all origins. This is useful for debugging or development.

```
var idsvrOptions = new IdentityServerOptions {
    CorsPolicy = CorsPolicy.AllowAll
};
```
**The sample for this topic can be found [here](https://github.com/thinktecture/Thinktecture.IdentityServer.v3.Samples/tree/master/source/DependencyInjection)**

IdentityServer v3 has extensibility points for various services. The default implementations of these services are designed to be decoupled from other moving parts of IdentityServer and as such we use dependency injection to get everything wired up. 

### Injecting IdentityServer services

The default services provided by IdentityServer can be replaced by the hosting application if desired. Custom implementations can also use dependency injection and have IdentityServer types or even custom types injected. This is only supported with custom services and stores that are registered with `Registration.RegisterType<T>`.

For custom services to accept types defined by IdentityServer, simply indicate those dependencies as constructor arguments. When IdentityServer instantiates your registered type the constructor arguments will be resolved automatically. For example:

```
public class MyCustomTokenSigningService: ITokenSigningService
{
    Thinktecture.IdentityServer.Core.Configuration.IdentityServerOptions options;

    public MyCustomTokenSigningService(
        Thinktecture.IdentityServer.Core.Configuration.IdentityServerOptions options)
    {
        this.options = options;
    }

    public System.Threading.Tasks.Task<string> SignTokenAsync(Thinktecture.IdentityServer.Core.Connect.Models.Token token)
    {
        // ...
    }
}
```

That was registered as such:

```
var factory = new IdentityServerServiceFactory();
factory.TokenSigningService = Registration.RegisterType<ITokenSigningService>(typeof(MyCustomTokenSigningService));
```
### Injecting custom services

Your custom services might also have dependencies on your own types. Those can also be injected as long as they have been configured with IdentityServer’s dependency injection system. This is done via the `IdentityServerServiceFactory`’s `Register<TService>(Type implementationType)` method. For example, if you have a custom logger that is needed in your service:

```
public interface ICustomLogger
{
    void Log(string message);
}

public class DebugLogger : ICustomLogger
{
    public void Log(string message)
    {
        Debug.WriteLine(message);
    }
}

public class MyCustomTokenSigningService: ITokenSigningService
{
    Thinktecture.IdentityServer.Core.Configuration.IdentityServerOptions options;
    ICustomLogger logger;

    public MyCustomTokenSigningService(
        Thinktecture.IdentityServer.Core.Configuration.IdentityServerOptions options,
        ICustomLogger logger)
    {
        this.options = options;
        this.logger = logger;
    }

    public System.Threading.Tasks.Task<string> SignTokenAsync(Thinktecture.IdentityServer.Core.Connect.Models.Token token)
    {
        // ...
    }
}
```

Then it would be registered as such:

```
var factory = new IdentityServerServiceFactory();
factory.TokenSigningService = Registration.RegisterType<ITokenSigningService>(typeof(MyCustomTokenSigningService));
factory.Register(Registration.RegisterType<ICustomLogger>(typeof(MyCustomDebugLogger)));
```

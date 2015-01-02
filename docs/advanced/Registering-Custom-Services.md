IdentityServer v3 provides many extensibility points for storage of data, validation logic and general functionality that are needed to support IdentityServer's operation as a token service. These various extensibility points are collectively referred to as "services". 

To see the full list of the services that are replaceable consult the `IdentityServerServiceFactory` class (https://github.com/thinktecture/Thinktecture.IdentityServer.v3/blob/master/source/Core/Configuration/IdentityServerServiceFactory.cs). The various properties allow for custom implementations to be registered.

### Required services

Some services are required to be provided by the hosting application. These requested services are:
* UserService
* ScopeStore
* ClientStore

The `UserService` encapsulates the identity management piece, which involves validating credentials, associating an account from an external identity provider to a local account and providing claims that are issued in tokens. The `ScopeStore` acts as a means to access the configuration database that stores the OAuth2 and OpenID Connect scopes used by IdentityServer. The `ClientStore` provides access to load client information from a configuration database.

### Registering custom services

To register your own implementation for any of these services you must obtain an instance of a `Registration` object and assign it to the appropriate property on the `IdentityServerServiceFactory`.

A `Registration` represents a way for IdentityServer to obtain an instance of your service. Depending upon the design of your service you might want to have a new instance on every request, use a singleton, or you might require special instantiation logic each time an instance is needed. To accommodate these different possibilities, the `Registration` class provides three different APIs to initialize your service:

* Registration.RegisterType&lt;TService&gt;(Type implementationType)
* Registration.RegisterSingleton&lt;TService&gt;(TService instance)
* Registration.RegisterFactory&lt;TService&gt;(Func&lt;TService&gt; factoryFunc)

`RegisterType` will instantiate a new instance of the implementation type on every request. `RegisterSingleton` will simply re-use the instance provided. `RegisterFactory` allows you to provide a delegate to a function that produces the service to be used. Here’s an example of registering a custom user service:

```
var factory = new IdentityServerServiceFactory();
factory.UserService = Registration.RegisterType<IUserService>(typeof(MyCustomUserService));
```

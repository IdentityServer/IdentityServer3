# Thinktecture IdentityServer v3 #

**Current status: Beta 3**

[![Gitter](https://badges.gitter.im/Join Chat.svg)](https://gitter.im/thinktecture/Thinktecture.IdentityServer.v3?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

## Overview ##

IdentityServer is a framework and a hostable component that allows implementing single sign-on and access control for modern web applications and APIs using protocols like OpenID Connect and OAuth2. It supports a wide range of clients like mobile, web, SPAs and desktop applications and is extensible to allow integration in new and existing architectures.

[OpenID Connect specification](http://openid.net/specs/openid-connect-core-1_0.html) / [OAuth2 specification](http://tools.ietf.org/html/rfc6749 "OAuth2 specification")

Also check out my [Introduction to OpenID Connect, OAuth2 and IdentityServer](https://vimeo.com/97344501) talk from NDC Oslo.

## Getting started ##
We currently don't provide a setup tool or a UI, but IdentityServer is remarkably easy to setup. Start with downloading/cloning the repo. Open the solution in Visual Studio and start it. Use the various clients in the [samples](https://github.com/thinktecture/Thinktecture.IdentityServer.v3.Samples) repository to exercise the various flows. Also check the wiki for more information.

IdSrv3 is designed as an OWIN/Katana component. The following configuration (in the host project) gives you a minimal implementation with in-memory repositories and user authentication.

```csharp
public void Configuration(IAppBuilder appBuilder)
{
    var factory = InMemoryFactory.Create(
        users:   Users.Get(), 
        clients: Clients.Get(), 
        scopes:  Scopes.Get());

    var options = new IdentityServerOptions
    {
        IssuerUri = "https://idsrv3.com",
        SiteName = "Thinktecture IdentityServer v3"

        SigningCertificate = Certificate.Get(),
        Factory = factory,
    };

    appBuilder.UseIdentityServer(options);
}
```

You can find a test signing certificate and setup instructions in the [certificates](https://github.com/thinktecture/Thinktecture.IdentityServer.v3.Samples/tree/master/source/Certificates) folder in the samples repository.

The host [samples](https://github.com/thinktecture/Thinktecture.IdentityServer.v3.Samples/) shows other configuration options, including
* support for MembershipReboot and ASP.NET Identity based user stores
* support for additional Katana authentication middleware (e.g. Google, Twitter, Facebook etc)
* support for EntityFramework based persistence of configuration
* support for WS-Federation

## Related repositories ##
* [Samples](https://github.com/thinktecture/Thinktecture.IdentityServer.v3.Samples)
* [MembershipReboot support](https://github.com/thinktecture/Thinktecture.IdentityServer.v3.MembershipReboot)
* [ASP.Net Identity support](https://github.com/thinktecture/Thinktecture.IdentityServer.v3.AspNetIdentity)
* [EntityFramework support](https://github.com/thinktecture/Thinktecture.IdentityServer.v3.EntityFramework)
* [WS-Federation plugin](https://github.com/thinktecture/Thinktecture.IdentityServer.v3.WsFederation)


## Credits ##
IdentityServer is built using the following great open source projects:

- [ASP.NET Web API](https://aspnetwebstack.codeplex.com/)
- [Autofac](http://autofac.org/)
- [Json.Net](http://james.newtonking.com/json)
- [LibLog](https://github.com/damianh/LibLog)
- [Katana](https://katanaproject.codeplex.com/)

thanks to all [contributors](https://github.com/thinktecture/Thinktecture.IdentityServer.v3/graphs/contributors)!

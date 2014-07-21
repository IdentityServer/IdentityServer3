# Thinktecture IdentityServer v3 #

**current status: preview 1**

## Overview ##

IdentityServer v3 is a .NET-based open source implementation of an OpenID Connect provider and OAuth2 authorization server.

[OpenID Connect specification](http://openid.net/specs/openid-connect-core-1_0.html) / [OAuth2 specification](http://tools.ietf.org/html/rfc6749 "OAuth2 specification")

Also check out my [Introduction to OpenID Connect, OAuth2 and IdentityServer](https://vimeo.com/97344501) talk from NDC Oslo.

## Getting started ##
We currently don't provide a setup tool or a UI. This release is meant to test drive the authorization/token engine. But it is remarkably easy to setup. Start with downloading/cloning the repo. Open the solution in Visual Studio and start it. Use the various clients in the samples repository to exercise the various flows.

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
        SiteName = "Thinktecture IdentityServer v3 - preview 1 (SelfHost)"

        SigningCertificate = Certificate.Get(),
        Factory = factory,
    };

    appBuilder.UseIdentityServer(options);
}
```

You can find the *CN=idsrv3test* certificate and setup instructions in the [certificates](https://github.com/thinktecture/Thinktecture.IdentityServer.v3.Samples/tree/master/source/Certificates) folder in the repository.

The host [samples](https://github.com/thinktecture/Thinktecture.IdentityServer.v3.Samples/) shows other configuration options
* support for MembershipReboot and ASP.NET Identity based user stores
* support for additional Katana authentication middleware (e.g. Google, Twitter, Facebook etc)
* support for WS-Federation

IdentityServer is built using the following great open source projects:

- ASP.NET Web API
- Autofac
- Json.Net
- Thinktecture IdentityModel
- ILMerge
- DH.Logging
- Katana

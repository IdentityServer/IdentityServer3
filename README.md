# Thinktecture IdentityServer v3 #

**current status: preview 1**

## Overview ##

IdSrv3 is a .NET-based open source implementation of an OpenID Connect provider and OAuth2 authorization server (check the [wiki](https://github.com/thinktecture/Thinktecture.IdentityServer.v3/wiki) for more info).

[OpenID Connect specification](http://openid.net/specs/openid-connect-core-1_0.html)

[OAuth2 specification](http://tools.ietf.org/html/rfc6749 "OAuth2 specification")


**Endpoints:**

* **/connect/authorize** - OpenID Connect/OAuth2 code and implicit client support
* **/connect/token** - OpenID Connect/OAuth2 code, password, client credentials and assertion grant support
* **/connect/userinfo** - OpenID Connect userinfo endpoint
* **/connect/logout** - client initiated logout (aka end session endpoint)
* **/.well-known/openid-configuration** - OpenID Connect discovery endpoint

## Getting started ##
We currently don't provide a setup tool or a UI. This release is meant to test drive the authorization/token engine. But it is remarkably easy to setup. Start with downloading/cloning the repo. Open the solution in Visual Studio and start it. Use the various clients in the samples folder to exercise the various flows.

IdSrv3 is designed as an OWIN/Katana component. The following configuration (in the host project) gives you a minimal implementation with in-memory repositories and user authentication (username must always equal password).

```csharp
app.Map("/core", coreApp =>
    {
        var factory = Factory.Create(
            issuerUri: "https://idsrv3.com",
            siteName:  "Thinktecture IdentityServer v3 - preview 1");
                    
        var opts = new IdentityServerOptions
        {
            Factory = factory,
            PublicHostName = "http://localhost:3333"
        };

        coreApp.UseIdentityServer(opts);
    });
```

You can find the *CN=idsrv3test* certificate and setup instructions in the [certificates](https://github.com/thinktecture/Thinktecture.IdentityServer.v3/tree/master/samples/Certificates) folder in the repository.

The host project shows other configuration options
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

# IdentityServer3 #

Dev build: [![Build status](https://ci.appveyor.com/api/projects/status/rtaj3nb7c60xg7cb/branch/dev?svg=true)](https://ci.appveyor.com/project/leastprivilege/thinktecture/branch/dev)
[![Gitter](https://badges.gitter.im/Join Chat.svg)](https://gitter.im/IdentityServer/IdentityServer3?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

![openid_certified](https://cloud.githubusercontent.com/assets/1454075/7611268/4d19de32-f97b-11e4-895b-31b2455a7ca6.png)

[Certified](http://openid.net/certification/) OpenID Connect implementation.

## Overview ##

IdentityServer is a .NET/Katana-based framework and hostable component that allows implementing single sign-on and access control for modern web applications and APIs using protocols like OpenID Connect and OAuth2. It supports a wide range of clients like mobile, web, SPAs and desktop applications and is extensible to allow integration in new and existing architectures.

Watch this for the big picture: [Introduction to OpenID Connect, OAuth2 and IdentityServer](https://vimeo.com/113604459).

Go to the documentation [site](https://identityserver.github.io/Documentation/).

[OpenID Connect specification](http://openid.net/specs/openid-connect-core-1_0.html) / [OAuth2 specification](http://tools.ietf.org/html/rfc6749 "OAuth2 specification")

## Getting started ##
IdentityServer is designed as an OWIN/Katana component. By referencing the library or nuget you get a `UseIdentityServer` extension method for `IAppBuilder` that allows setting up IdentityServer in your OWIN host:  

```csharp
public void Configuration(IAppBuilder app)
{
    var options = new IdentityServerOptions
    {
        SigningCertificate = Certificate.Get(),
        Factory = Factory.Create()
    };

    app.UseIdentityServer(options);
}
```

*Note:* If you're hosting in IIS, make sure you [enable RAMMFAR in your web.config file](http://identityserver.github.io/Documentation/docs/configuration/overview.html).

For more information, e.g.

* support for MembershipReboot and ASP.NET Identity based user stores
* support for additional Katana authentication middleware (e.g. Google, Twitter, Facebook etc)
* support for EntityFramework based persistence of configuration
* support for WS-Federation
* extensibility

check out the [documentation](https://identityserver.github.io/Documentation/) and the [samples](https://github.com/identityserver/IdentityServer3.Samples).

## Related repositories ##
* [Access Token Validation](https://github.com/identityserver/IdentityServer3.AccessTokenValidation)
* [EntityFramework support](https://github.com/identityserver/IdentityServer3.EntityFramework)
* [MembershipReboot support](https://github.com/identityserver/IdentityServer3.MembershipReboot)
* [ASP.Net Identity support](https://github.com/identityserver/IdentityServer3.AspNetIdentity)
* [WS-Federation plugin](https://github.com/identityserver/IdentityServer3.WsFederation)
* [Samples](https://github.com/IdentityServer/IdentityServer3.Samples)

## Credits ##
IdentityServer is built using the following great open source projects:

- [ASP.NET Web API](https://aspnetwebstack.codeplex.com/)
- [Autofac](http://autofac.org/)
- [Json.Net](http://james.newtonking.com/json)
- [LibLog](https://github.com/damianh/LibLog)
- [Katana](https://katanaproject.codeplex.com/)
- [Web Protection Library](https://wpl.codeplex.com/)
- [XUnit](https://github.com/xunit/xunit)
- [License Header Manager](https://visualstudiogallery.msdn.microsoft.com/5647a099-77c9-4a49-91c3-94001828e99e)

..and is supported by the following open source friendly companies:

- [JetBrains](http://www.jetbrains.com)
- [Gitter](http://gitter.im)
- [Huboard](http://huboard.com)
- [AppVeyor](http://appveyor.com)
- [MyGet](http://myget.org)

...and last but not least thanks to all [contributors](https://github.com/IdentityServer/IdentityServer3/graphs/contributors)!

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

IdSrv3 is designed as an OWIN/Katana component. The following configuration gives you a minimal implementation with in-memory repositories and user authentication (username must always equal password).

	public void Configuration(IAppBuilder app)
	{
	    app.Map("/core", coreApp =>
	        {
	            var factory = TestOptionsFactory.Create(
	                issuerUri:         "https://idsrv3.com",
	                siteName:          "Thinktecture IdentityServer v3 - preview 1",
	                publicHostAddress: "http://localhost:3333");
	                    
	            var options = new IdentityServerCoreOptions
	            {
	                Factory = factory,
	            };
	
	            coreApp.UseIdentityServerCore(options);
	        });
	}

You can find the *CN=idsrv3test* certificate and setup instructions in the [certificates](https://github.com/thinktecture/Thinktecture.IdentityServer.v3/tree/master/samples/Certificates) folder in the repository.

If you want to plugin a real user storage system, we provide out of the box support for MembershipReboot and ASP.NET identity - simply uncomment the UserService assignment.

	public void Configuration(IAppBuilder app)
    {
        app.Map("/core", coreApp =>
            {
                var factory = TestOptionsFactory.Create(
                    issuerUri: "https://idsrv3.com",
                    siteName: "Thinktecture IdentityServer v3 - preview 1",
                    publicHostAddress: "http://localhost:3333");

                //factory.UserService = Thinktecture.IdentityServer.MembershipReboot.UserServiceFactory.Factory;
                //factory.UserService = Thinktecture.IdentityServer.AspNetIdentity.UserServiceFactory.Factory;

                var options = new IdentityServerCoreOptions
                {
                    Factory = factory,
                };

                coreApp.UseIdentityServerCore(options);
            });
    }

To support social logins, you can simply add existing OWIN/Katana middleware to the IdSrv3 configuration:

	public void Configuration(IAppBuilder app)
    {
        app.Map("/core", coreApp =>
            {
                var factory = TestOptionsFactory.Create(
                    issuerUri: "https://idsrv3.com",
                    siteName: "Thinktecture IdentityServer v3 - preview 1",
                    certificateName: "CN=idsrv3test",
                    publicHostAddress: "http://localhost:3333");

                //factory.UserService = Thinktecture.IdentityServer.MembershipReboot.UserServiceFactory.Factory;
                //factory.UserService = Thinktecture.IdentityServer.AspNetIdentity.UserServiceFactory.Factory;

                var options = new IdentityServerCoreOptions
                {
                    Factory = factory,
                    SocialIdentityProviderConfiguration = ConfigureSocialIdentityProviders
                };

                coreApp.UseIdentityServerCore(options);
            });
    }

    public static void ConfigureSocialIdentityProviders(IAppBuilder app, string signInAsType)
    {
        var google = new GoogleAuthenticationOptions
        {
            AuthenticationType = "Google",
            SignInAsAuthenticationType = signInAsType
        };
        app.UseGoogleAuthentication(google);

        var fb = new FacebookAuthenticationOptions
        {
            AuthenticationType = "Facebook",
            SignInAsAuthenticationType = signInAsType,
            AppId = "67...8",
            AppSecret = "9....3"
        };
        app.UseFacebookAuthentication(fb);
    }
    

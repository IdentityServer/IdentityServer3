---
layout: docs-default
---

This tutorial walks you through the necessary steps to get a minimal IdentityServer up and running. For simplicity we will host IdentityServer and the client in the same web application - this is probably not a very realistic scenario, but let's you get started without making it too complicated.

The full source code can be found [here](https://github.com/thinktecture/Thinktecture.IdentityServer.v3.Samples/tree/master/source/MVC%20Authentication).

# Part 1 - MVC Authentication & Authorization
In the first part we will create a simple MVC application and add authentication via IdentityServer to it. Then we will have a closer look at claims, claims transformation and authorization

## Create the web application
In Visual Studio 2013, create a standard MVC application and set authentication to "No authentication".

![create mvc app](https://cloud.githubusercontent.com/assets/1454075/4604880/16fa22f0-51bd-11e4-96aa-e82206d21e26.png)

You can switch the project now to SSL using the properties window:

![set ssl](https://cloud.githubusercontent.com/assets/1454075/4604894/9f18b656-51bd-11e4-935c-e3ecdb3d1905.png)

**Important** Don't forget to update the start URL in your project properties.

## Adding IdentityServer
IdentityServer is based on OWIN/Katana and distributed as a Nuget package. To add it to the newly created web host, install the following two packages:

````
install-package Microsoft.Owin.Host.Systemweb
install-package Thinktecture.IdentityServer.v3 -pre
````

## Configuring IdentityServer - Clients
IdentityServer needs some information about the clients it is going to support, this can be simply supplied using a `Client` object:

```csharp
public static class Clients
{
    public static IEnumerable<Client> Get()
    {
        return new[]
        {
            new Client 
            {
                Enabled = true,
                ClientName = "MVC Client",
                ClientId = "mvc",
                Flow = Flows.Implicit,

                RedirectUris = new List<string>
                {
                    "https://localhost:44319/"
                }
            }
        };
    }
}
```

## Configuring IdentityServer - Users
Next we gonna add some users to IdentityServer - again this can be accomplished by providing a simple C# class. You can retrieve user information from any data store and we provide out of the box support for ASP.NET Identity and MembershipReboot.

```csharp
public static class Users
{
    public static List<InMemoryUser> Get()
    {
        return new List<InMemoryUser>
        {
            new InMemoryUser
            {
                Username = "bob",
                Password = "secret",
                Subject = "1",

                Claims = new[]
                {
                    new Claim(Constants.ClaimTypes.GivenName, "Bob"),
                    new Claim(Constants.ClaimTypes.FamilyName, "Smith")
                }
            }
        };
    }
}
```

## Adding Startup
IdentityServer gets wired up in the startup class. Here we provide information about the clients, users, scopes, the signing certificate and some other configuration options. In production you should load the signing certificate from the Windows certificate store or some other secured source, here we simply added it to the project as a file. For info on how to load the certificate from Azure Websites see http://azure.microsoft.com/blog/2014/10/27/using-certificates-in-azure-websites-applications/.

```csharp
public class Startup
{
    public void Configuration(IAppBuilder app)
    {
        app.Map("/identity", idsrvApp =>
            {
                idsrvApp.UseIdentityServer(new IdentityServerOptions
                {
                    SiteName = "Embedded IdentityServer",
                    IssuerUri = "https://idsrv3/embedded",
                    SigningCertificate = LoadCertificate(),

                    Factory = InMemoryFactory.Create(
                        users  : Users.Get(),
                        clients: Clients.Get(),
                        scopes : StandardScopes.All)
                });
            });
    }

    X509Certificate2 LoadCertificate()
    {
        return new X509Certificate2(
            string.Format(@"{0}\bin\identityServer\idsrv3test.pfx", AppDomain.CurrentDomain.BaseDirectory), "idsrv3test");
    }
}
```

At this point you have a fully functional IdentityServer and you can browse to the discovery endpoint to inspect the configuration:


![disco](https://cloud.githubusercontent.com/assets/1454075/4604932/43a3d7da-51c0-11e4-8a88-b74db2b7e771.png)

## RAMMFAR
One last thing, please don't forget to add RAMMFAR to your web.config, otherwise some of our embedded assets will not be loaded correctly by IIS:

```xml
<system.webServer>
  <modules runAllManagedModulesForAllRequests="true" />
</system.webServer>
```

## Adding and configuring the OpenID Connect authentication middleware
To add OIDC authentication to the MVC application, we need to add two packages:

````
install-package Microsoft.Owin.Security.Cookies
install-package Microsoft.Owin.Security.OpenIdConnect
````

The cookie middleware gets configured with its default values:

```csharp
app.UseCookieAuthentication(new CookieAuthenticationOptions
    {
        AuthenticationType = "Cookies"
    });
```
The OpenID Connect middleware points to our embedded version of IdentityServer and uses the previously configured client configuration:

```csharp
app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
    {
        Authority = "https://localhost:44319/identity",
        ClientId = "mvc",
        RedirectUri = "https://localhost:44319/",
        ResponseType = "id_token",

        SignInAsAuthenticationType = "Cookies"
    });
```

## Adding a protected resource and showing claims
To initiate the authentication with IdentityServer you need to create a protected resource, e.g. by adding a global authorization filter. For our sample we will simply protect the `About` action on the `Home` controller. In addition we will hand over the claims to the view so we can see which claims got emitted by IdentityServer:

````csharp
[Authorize]
public ActionResult About()
{
    return View((User as ClaimsPrincipal).Claims);
}
````

The corresponding view looks like this:

````html
@model IEnumerable<System.Security.Claims.Claim>

<dl>
    @foreach (var claim in Model)
    {
        <dt>@claim.Type</dt>
        <dd>@claim.Value</dd>
    }
</dl>
````

## Authentication and claims
Clicking on the about link will now trigger the authentication. IdentityServer will show the login screen and send a token back to the main application. The OpenID Connect middleware validates the token, extracts the claims and passes them on to the cookie middleware, which will in turn set the authentication cookie. The user is now signed in.


![login](https://cloud.githubusercontent.com/assets/1454075/4604964/7f5d6eb0-51c2-11e4-8016-feac5ed2f67a.png)

![claims](https://cloud.githubusercontent.com/assets/1454075/4604966/91126e8a-51c2-11e4-8fd9-d9c7af1d096b.png)

## Adding role claims and scope
In the next step we want to add some role claims to our user which we will use later on for authorization.

For now we got away with the OIDC standard scopes - let's define a roles scope that includes the role claim and add that to the standard scopes:

```csharp
public static class Scopes
{
    public static IEnumerable<Scope> Get()
    {
        var scopes = new List<Scope>
        {
            new Scope
            {
                Enabled = true,
                Name = "roles",
                Type = ScopeType.Identity,
                Claims = new List<ScopeClaim>
                {
                    new ScopeClaim("role")
                }
            }
        };

        scopes.AddRange(StandardScopes.All);

        return scopes;
    }
}
```

Also change the factory in `Startup` to use the new Scopes:

```csharp
Factory = InMemoryFactory.Create(
    users: Users.Get(),
    clients: Clients.Get(),
    scopes: Scopes.Get())
```

Next we add a couple of role claims to Bob:

```csharp
public static class Users
{
    public static IEnumerable<InMemoryUser> Get()
    {
        return new[]
        {
            new InMemoryUser
            {
                Username = "bob",
                Password = "secret",
                Subject = "1",

                Claims = new[]
                {
                    new Claim(Constants.ClaimTypes.GivenName, "Bob"),
                    new Claim(Constants.ClaimTypes.FamilyName, "Smith"),
                    new Claim(Constants.ClaimTypes.Role, "Geek"),
                    new Claim(Constants.ClaimTypes.Role, "Foo")
                }
            }
        };
    }
}
```

## Changing the middleware configuration to ask for roles
By default the OIDC middleware asks for a two scopes `openid` and `profile` - this is why IdentityServer includes the subject and name claims. Now we add a request to the `roles` scope:

```csharp
app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
    {
        Authority = "https://localhost:44319/identity",
                    
        ClientId = "mvc",
        Scope = "openid profile roles",
        RedirectUri = "https://localhost:44319/",
        ResponseType = "id_token",

        SignInAsAuthenticationType = "Cookies"
    });
```

After successful authentication, you should now see the role claims in the user's claims collection:

![role claims](https://cloud.githubusercontent.com/assets/1454075/4605904/0397adc2-5203-11e4-9e20-32b1b53c7570.png)

## Claims transformation
When you inspect the claims on the about page, you will notice two things: some claims have odd long type names and there are more claims than you probably need in your application.

The long claim names come from Microsoft's JWT handler trying to map some claim types to .NET's `ClaimTypes` class types. You can turn off this behavior with the following line of code (in `startup`)

```csharp
JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();
```

The claims will now look like this:

![shorter claims](https://cloud.githubusercontent.com/assets/1454075/4606129/2fb799fa-5210-11e4-8b30-22a47e9cbeb1.png)

This is an improvement, but there are still some low level protocol claims that are certainly not need by typical business logic. The process of turning raw incoming claims into application specific claims is called claims transformation. During this process you take the incoming claims, decide which claims you want to keep and maybe need to contact additional data stores to retrieve more claims that are required by the application.

The OIDC middleware has a notification that you can use to do claims transformation - the resulting claims will be stored in the cookie:

```csharp
app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
    {
        Authority = "https://localhost:44319/identity",
                    
        ClientId = "mvc",
        Scope = "openid profile roles",
        RedirectUri = "https://localhost:44319/",
        ResponseType = "id_token",

        SignInAsAuthenticationType = "Cookies",
        UseTokenLifetime = false,

        Notifications = new OpenIdConnectAuthenticationNotifications
        {
            SecurityTokenValidated = async n =>
                {
                    var id = n.AuthenticationTicket.Identity;

                    // we want to keep first name, last name, subject and roles
                    var givenName = id.FindFirst(Constants.ClaimTypes.GivenName);
                    var familyName = id.FindFirst(Constants.ClaimTypes.FamilyName);
                    var sub = id.FindFirst(Constants.ClaimTypes.Subject);
                    var roles = id.FindAll(Constants.ClaimTypes.Role);

                    // create new identity and set name and role claim type
                    var nid = new ClaimsIdentity(
                        id.AuthenticationType,
                        Constants.ClaimTypes.GivenName,
                        Constants.ClaimTypes.Role);

                    nid.AddClaim(givenName);
                    nid.AddClaim(familyName);
                    nid.AddClaim(sub);
                    nid.AddClaims(roles);

                    // add some other app specific claim
                    nid.AddClaim(new Claim("app_specific", "some data"));                   

                    n.AuthenticationTicket = new AuthenticationTicket(
                        nid,
                        n.AuthenticationTicket.Properties);
                }
        }
    });
```

After adding the above code, our claim set now looks like this:
![transformed claims](https://cloud.githubusercontent.com/assets/1454075/4606148/00033a74-5211-11e4-8cec-ed456a271d6e.png)

## Authorization
Now that we have authentication and some claims, we can start adding simple authorization rules.

MVC has a built-in attribute called `[Authorize]` to require authenticated users, you could also use this attribute to annotate role membership requirements.
We don't recommend this approach because this typically leads to code that mixes concerns like business/controller logic and authorization policy.
We rather recommend separating the authorization logic from the controller which leads to cleaner code and better testability (read more [here](http://leastprivilege.com/2014/06/24/resourceaction-based-authorization-for-owin-and-mvc-and-web-api/)).

To add the new authorization infrastructure and the new attribute, we add a Nuget package:
```
install-package Thinktecture.IdentityModel.Owin.ResourceAuthorization.Mvc
```

Next we annotate the `Contact` action on the `Home` controller with an attribute that expresses that executing that action is going to `Read` the `ContactDetails` resource:

```csharp
[ResourceAuthorize("Read", "ContactDetails")]
public ActionResult Contact()
{
    ViewBag.Message = "Your contact page.";

    return View();
}
```

Note that the attribute is **not** expressing who is allowed to read the contacts - we separate that logic into an authorization manager that knows about actions, resources and who is allowed to do which operation in your application:

```csharp
public class AuthorizationManager : ResourceAuthorizationManager
{
    public override Task<bool> CheckAccessAsync(ResourceAuthorizationContext context)
    {
        switch (context.Resource.First().Value)
        {
            case "ContactDetails":
                return AuthorizeContactDetails(context);
            default:
                return Nok();
        }
    }

    private Task<bool> AuthorizeContactDetails(ResourceAuthorizationContext context)
    {
        switch (context.Action.First().Value)
        {
            case "Read":
                return Eval(context.Principal.HasClaim("role", "Geek"));
            case "Write":
                return Eval(context.Principal.HasClaim("role", "Operator"));
            default:
                return Nok();
        }
    }
}
```

And finally we wire up the authorization manager into the OWIN pipeline in `Startup`:

```csharp
app.UseResourceAuthorization(new AuthorizationManager());
```

Run the sample and step through the code to familiarize yourself with the flow.

## More authorization and dealing with access denied scenarios
Let's do a bit more authorization by adding a new action method to the `Home` controller:

```csharp
[ResourceAuthorize("Write", "ContactDetails")]
public ActionResult UpdateContact()
{
    ViewBag.Message = "Upate your contact details!";

    return View();
}
```

When you try to invoke that action by navigating to the `/home/updatecontact` URL you will see a `forbidden` error page. 

![iis forbidden](https://cloud.githubusercontent.com/assets/1454075/4611482/35584714-52bb-11e4-97f1-12be348905d7.png)

In fact you will see different response based on the fact if the user is already authenticated or not. If not MVC will redirect to the login page, if authenticated, you will see the forbidden response. This is by design (read more [here](http://leastprivilege.com/2014/10/02/401-vs-403/)).

You can handle the forbidden condition by checking for `403` status codes - we provide such a filter out of the box:

```csharp
[ResourceAuthorize("Write", "ContactDetails")]
[HandleForbidden]
public ActionResult UpdateContact()
{
    ViewBag.Message = "Upate your contact details!";

    return View();
}
```

The `HandleForbidden` filter (which can be also global of course) will redirect to a specified view whenever a 403 got emitted - by default we look for a view called `Forbidden`.

![forbidden](https://cloud.githubusercontent.com/assets/1454075/4611314/0fcaa7aa-52b9-11e4-8a1a-c158a3d89697.png)

You can also use the authorization manager imperatively, which gives you even more options:

```csharp
[HandleForbidden]
public ActionResult UpdateContact()
{
    if (!HttpContext.CheckAccess("Write", "ContactDetails", "some more data"))
    {
        // either 401 or 403 based on authentication state
        return this.AccessDenied();
    }

    ViewBag.Message = "Upate your contact details!";
    return View();
}
```

## Adding Logout
Adding logout is easy, simply add a new action that calls the `Signout` method in the Katana authentication manager:

```csharp
public ActionResult Logout()
{
    Request.GetOwinContext().Authentication.SignOut();
    return Redirect("/");
}
```

This will initiate a roundtrip to the so called _endsession_ endpoint on IdentityServer. This endpoint will clear the authentication cookie and terminate your session:

![simple logout](https://cloud.githubusercontent.com/assets/1454075/4641154/71d7e23a-5432-11e4-9fb7-94dc8d53e5d0.png)

Typically the most secure thing to do now would be to simply close the browser window to get rid of all session data. Some applications though would like to give the user a chance to return as an anonymous user.

This is possible, but requires some steps - first you need to register a valid URL to return to after the logout procedure is complete. This is done in the client definition for the MVC application (note the new `PostLogoutRedirectUris` setting:

```csharp
new Client 
{
    Enabled = true,
    ClientName = "MVC Client",
    ClientId = "mvc",
    Flow = Flows.Hybrid,

    RedirectUris = new List<Uri>
    {
        new Uri("https://localhost:44319/")
    },
    PostLogoutRedirectUris = new List<Uri>
    {
        new Uri("https://localhost:44319/")
    }
}

```

Next the client has to prove its identity to the logout endpoint to make sure we redirect to the right URL (and not some spammer/phishing page). This is done by sending the initial identity token back that the client received during the authentication process. So far we have discarded this token, now it's the time to change the claims transformation logic to preserve it.

This is accomplished by adding this line of code to our `SecurityTokenValidated` notification:

```csharp
// keep the id_token for logout
nid.AddClaim(new Claim("id_token", n.ProtocolMessage.IdToken));
```

And as a last step, we have to attach the id_token when the user logs out and we make the roundtrip to IdentityServer. This is also done using a notification on the OIDC middleware:

```csharp
RedirectToIdentityProvider = async n =>
    {
        if (n.ProtocolMessage.RequestType == OpenIdConnectRequestType.LogoutRequest)
        {
            var idTokenHint = n.OwinContext.Authentication.User.FindFirst("id_token").Value;
            n.ProtocolMessage.IdTokenHint = idTokenHint;
        }
    }
```

With these changes, IdentityServer will give the user a link back to the calling application:

![logout with redirect](https://cloud.githubusercontent.com/assets/1454075/4641278/b7db4130-5434-11e4-94c3-6f3ff2d5d0ce.png)

## Adding Google Authentication
Next we want to enable external authentication. This is done by adding additional Katana authentication middleware to IdentityServer - for our example we will use Google.

### Registering IdentityServer with Google
First of all we need to register IdentityServer at Google's developer console. This consists of a few steps.

First navigate to:

https://console.developers.google.com

**Create a new Project**

![googlecreateproject](https://cloud.githubusercontent.com/assets/1454075/4843029/d6a3eb68-602c-11e4-83a1-edfceea419e5.png)

**Enable the Google+ API**

![googleapis](https://cloud.githubusercontent.com/assets/1454075/4843041/ebb8ed46-602c-11e4-8932-73b48d6a83fc.png)

**Configure the consent screen with email address and product name**

![googleconfigureconsent](https://cloud.githubusercontent.com/assets/1454075/4843066/2214d8fa-602d-11e4-8686-f6d6ba6ab6e8.png)

**Create a client application**

![googlecreateclient](https://cloud.githubusercontent.com/assets/1454075/4843077/44071554-602d-11e4-9214-191168ba425a.png)

After you create the client application, the developer console will show you a client id and a client secret. We will need these two values later when we configure the Google middleware.

### Adding the Google authentication middleware

Add the middleware by installing the following package:

`install-package Microsoft.Owin.Security.Google`

### Configure the middleware

Add the following method to your `Startup`:

```csharp
private void ConfigureIdentityProviders(IAppBuilder app, string signInAsType)
{
    app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions
        {
            AuthenticationType = "Google",
            Caption = "Sign-in with Google",
            SignInAsAuthenticationType = signInAsType,

            ClientId = "...",
            ClientSecret = "..."
        });
}

```

Next we point our IdentityServer options class to this method:


```csharp
idsrvApp.UseIdentityServer(new IdentityServerOptions
{
    SiteName = "Embedded IdentityServer",
    IssuerUri = "https://idsrv3/embedded",
    SigningCertificate = LoadCertificate(),

    Factory = InMemoryFactory.Create(
        users: Users.Get(),
        clients: Clients.Get(),
        scopes: Scopes.Get()),

    AuthenticationOptions = new Thinktecture.IdentityServer.Core.Configuration.AuthenticationOptions
    {
        IdentityProviders = ConfigureIdentityProviders
    }
});
```

That's it! The next time the user logs in - there will be a "Sign-in with Google" button on the login page:

![googlesignin](https://cloud.githubusercontent.com/assets/1454075/4843133/46d07194-602e-11e4-9530-c84b6544bfcb.png)

Notice that the `role` claim is missing when signing-in with Google. That makes sense since Google does not have the concept of roles. Be prepared that not all identity providers will offer the same claim types.


# Part 2 - Adding and calling a Web API
In this part we'll be adding a Web API to the solution. The API will be secured by IdentityServer. Next our MVC application will call the API using both the trust subsystem and identity delegation approach.

## Adding the Web API Project
The easiest way to create a clean API project is by adding an empty web project.

![add empty api](https://cloud.githubusercontent.com/assets/1454075/4625264/e745ebda-5373-11e4-827a-122ad39b7ef0.png)

Next we'll add Web API and Katana hosting using Nuget:

```
install-package Microsoft.Owin.Host.SystemWeb
install-package Microsoft.Aspnet.WebApi.Owin
```

## Adding a Test Controller
The following controller will return all claims back to the caller - this will allow us to inspect the token that will get send to the API.

```csharp
[Route("identity")]
[Authorize]
public class IdentityController : ApiController
{
    public IHttpActionResult Get()
    {
        var user = User as ClaimsPrincipal;
        var claims = from c in user.Claims
                        select new
                        {
                            type = c.Type,
                            value = c.Value
                        };

        return Json(claims);
    }
}
```

## Wiring up Web API and Security in Startup
As always with Katana-based hosting, all configuration takes place in `Startup`:

```csharp
public class Startup
{
    public void Configuration(IAppBuilder app)
    {
        // web api configuration
        var config = new HttpConfiguration();
        config.MapHttpAttributeRoutes();

        app.UseWebApi(config);
    }
}
```

In addition we want to secure our API using IdentityServer - two things are needed for that:

* accept only tokens issued by IdentityServer
* accept only tokens that are issued for our API - for that we'll give the API a name of *sampleApi* (also called `scope`)

To accomplish that, we add a Nuget packages:

```
install-package Thinktecture.IdentityServer.v3.AccessTokenValidation -pre
```

..and use them in `Startup`:


```csharp
public class Startup
{
    public void Configuration(IAppBuilder app)
    {
        app.UseIdentityServerBearerTokenAuthentication(new IdentityServerBearerTokenAuthenticationOptions
        {
            Authority = "https://localhost:44319/identity",
            RequiredScopes = new[] { "sampleApi" }
        });
        
        // web api configuration
        var config = new HttpConfiguration();
        config.MapHttpAttributeRoutes();

        app.UseWebApi(config);
    }
}
```

**Note**: IdentityServer issues standard JSON Web Tokens (JWT), and you could use the plain Katana JWT middleware to validate them. Our middleware is just a convenience since it can auto-configure itself using the IdentityServer discovery document (metadata).

## Registering the API in IdentityServer

Next we need to register the API - this is done by extending the scopes. This time we add a so called resource scope:

```csharp
public static class Scopes
{
    public static IEnumerable<Scope> Get()
    {
        var scopes = new List<Scope>
        {
            new Scope
            {
                Enabled = true,
                Name = "roles",
                Type = ScopeType.Identity,
                Claims = new List<ScopeClaim>
                {
                    new ScopeClaim("role")
                }
            },
            new Scope
            {
                Enabled = true,
                Name = "sampleApi",
                Description = "Access to a sample API",
                Type = ScopeType.Resource
            }
        };

        scopes.AddRange(StandardScopes.All);

        return scopes;
    }
}
```

## Registering a Web API Client
Next we gonna call the API. You can do that either as using client credentials (think service account) or by delegating the users identity. We will start with the client credentials.

First we need to register a new client for the MVC app. For security reasons, IdentityServer only allows one flow per client, and since our existing MVC client already uses hybrid flow, we need to create a new client for the service to service communication.

```csharp
public static class Clients
{
    public static IEnumerable<Client> Get()
    {
        return new[]
        {
            new Client 
            {
                Enabled = true,
                ClientName = "MVC Client",
                ClientId = "mvc",
                Flow = Flows.Hybrid,

                RedirectUris = new List<Uri>
                {
                    new Uri("https://localhost:44319/")
                }
            },
            new Client
            {
                Enabled = true,
                ClientName = "MVC Client (service communication)",
                ClientId = "mvc_service",
                ClientSecret = "secret",
                Flow = Flows.ClientCredentials
            }
        };
    }
}
```

## Calling the API
Calling the API consists of two parts:

* Requesting a token for the API from IdentityServer using the client credentials
* Calling the API using the access token

To make the interaction with the OAuth2 token endpoint easier, we add a client package via Nuget:

```
install-package Thinktecture.IdentityModel.Client
```

The following code snippet will request the token for *sampleApi* using the client credentials:

```csharp
private async Task<TokenResponse> GetTokenAsync()
{
    var client = new OAuth2Client(
        new Uri("https://localhost:44319/identity/connect/token"),
        "mvc_service",
        "secret");

    return await client.RequestClientCredentialsAsync("sampleApi");
}
```

Whereas the following snippet calls our identity endpoint using the requested access token:

```csharp
private async Task<string> CallApi(string token)
{
    var client = new HttpClient();
    client.SetBearerToken(token);

    var json = await client.GetStringAsync("https://localhost:44321/identity");
    return JArray.Parse(json).ToString();
}
```

Bringing that all together, a newly added controller calls the service and displays the resulting claims on a view:

```csharp
public class CallApiController : Controller
{
    // GET: CallApi/ClientCredentials
    public async Task<ActionResult> ClientCredentials()
    {
        var response = await GetTokenAsync();
        var result = await CallApi(response.AccessToken);

        ViewBag.Json = result;
        return View("ShowApiResult");
    }

    // helpers omitted
}
```

The result will look like this:

![callapiclientcreds](https://cloud.githubusercontent.com/assets/1454075/4625853/2ba6d94a-537b-11e4-9ad0-3be144a913f0.png)

In other words - the API knows about the caller:

* the issuer name, audience and expiration (used by the token validation middleware)
* for which scope the token was issued (used by the scope validation middleware)
* the client id

All claims in the token will be turned into a ClaimsPrincipal and are available via the *.User* property on the controller.

## Calling the API on behalf of the User
Next we want to call the API using the user's identity. This is accomplished by adding the `sampleApi` scope to the list of scopes in the OpenID Connect middleware configuration. We also need to indicate that we want to request an access token by changing the response type:


```csharp
Scope = "openid profile roles sampleApi",
ResponseType = "id_token token"
```

As soon as a response type of `token` is requested, IdentityServer stops including the claims in the identity token. This is for optimization purposes, since you now have an access token that allows retrieving the claims from the userinfo endpoint and while keeping the identity token small.

Accessing the userinfo endpoint is not hard - the `UserInfoClient` class can make this even simpler. In addition we also now store the access token in the cookie, so we can use it whenever we want to access the API on behalf of the user:


```csharp
SecurityTokenValidated = async n =>
    {
        var nid = new ClaimsIdentity(
            n.AuthenticationTicket.Identity.AuthenticationType,
            Constants.ClaimTypes.GivenName,
            Constants.ClaimTypes.Role);

        // get userinfo data
        var userInfoClient = new UserInfoClient(
            new Uri(n.Options.Authority + "/connect/userinfo"),
            n.ProtocolMessage.AccessToken);

        var userInfo = await userInfoClient.GetAsync();
        userInfo.Claims.ToList().ForEach(ui => nid.AddClaim(new Claim(ui.Item1, ui.Item2)));

        // keep the id_token for logout
        nid.AddClaim(new Claim("id_token", n.ProtocolMessage.IdToken));

        // add access token for sample API
        nid.AddClaim(new Claim("access_token", n.ProtocolMessage.AccessToken));

        // keep track of access token expiration
        nid.AddClaim(new Claim("expires_at", DateTimeOffset.Now.AddSeconds(int.Parse(n.ProtocolMessage.ExpiresIn)).ToString()));

        // add some other app specific claim
        nid.AddClaim(new Claim("app_specific", "some data"));

        n.AuthenticationTicket = new AuthenticationTicket(
            nid,
            n.AuthenticationTicket.Properties);
    }
```

Another option would be to reconfigure the scopes in IdentityServer and set the `AlwaysIncludeInIdToken` flag on the scope claims to force inclusion of the claims in the identity token - I'll leave that as an exercise.

**Calling the API**

Since the access token is now stored in the cookie, we can simply retrieve it from the claims principal and use it to call the service:

```csharp
// GET: CallApi/ClientCredentials
public async Task<ActionResult> UserCredentials()
{
    var user = User as ClaimsPrincipal;
    var token = user.FindFirst("access_token").Value;
    var result = await CallApi(token);

    ViewBag.Json = result;
    return View("ShowApiResult");
}
```

On the result page, you can now see that the `sub` claim is included, which means that the API is now working on behalf of a user:

![userdelegation](https://cloud.githubusercontent.com/assets/1454075/5453086/246392fc-8523-11e4-9a3f-8100af390d53.png)

If you now add a scope claim for `role` to the `sampleApi` scope - the roles of the user will be included in the access token as well:

```csharp
new Scope
{
    Enabled = true,
    Name = "sampleApi",
    Description = "Access to a sample API",
    Type = ScopeType.Resource,

    Claims = new List<ScopeClaim>
    {
        new ScopeClaim("role")
    }
}
```

![delegationroles](https://cloud.githubusercontent.com/assets/1454075/5453110/9ca835ec-8523-11e4-874f-771b648b7016.png)
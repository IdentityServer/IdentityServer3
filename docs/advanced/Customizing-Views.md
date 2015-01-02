### Views

IdentityServer v3 displays various “views” to the user. IdentityServer requires views for login, logout prompt, logged out, consent, and errors. These views are simply web pages displayed in the browser. To obtain the markup for these views, IdentityServer defines the `IViewService` interface. The view service is one of the optional extensibility points in IdentityServer.

#### Embedded View Service

**The sample for this sub-topic can be found [here](https://github.com/thinktecture/Thinktecture.IdentityServer.v3.Samples/tree/master/source/EmbeddedAssetsViewService)**

The default implementation of the view service used by IdentityServer is the `EmbeddedAssetsViewService`. The various assets (HTML, JavaScript, CSS, and fonts) that comprise the views are served up from embedded resources within the IdentityServer.Core.dll assembly.

The `EmbeddedAssetsViewService` allows for some minor customization. The hosting application can provide a list of either CSS and/or JavaScript files to include in the default web pages. This allows for branding without the need to completely replace the assets themselves. 

The `EmbeddedAssetsViewServiceConfiguration` class is used to indicate these CSS and/or JavaScript files via the `Stylesheets` and `Scripts` collections:

```
var embeddedViewServiceConfig = new EmbeddedAssetsViewServiceConfiguration();
embeddedViewServiceConfig.Stylesheets.Add("~/Content/Site.css");
```
The paths passed to `Add` can either be relative to IdentityServer’s base path by prefixing the path with a “~” (such as “~/path/file.css”), or the path can be host-relative by prefixing the path with a “/”(such as “/path/file.css”). Currently, absolute URLs are not supported given IdentityServer's use of CSP.

To then register the ` EmbeddedAssetsViewServiceConfiguration` the dependency injection system can be used to register the instance configured by the hosting application:
```
var embeddedViewServiceConfig = new EmbeddedAssetsViewServiceConfiguration();
embeddedViewServiceConfig.Stylesheets.Add("~/Content/Site.css");

var factory = new IdentityServerServiceFactory();
factory.Register(Registration.RegisterSingleton(embeddedViewServiceConfig));
```

#### Custom View Service

**The sample for this sub-topic can be found [here](https://github.com/thinktecture/Thinktecture.IdentityServer.v3.Samples/tree/master/source/CustomViewService)**

If the hosting application requires complete control over the views (HTML, CSS, JavaScript, etc.) then it can implement the `IViewService` to control all of the markup rendered for the views.  The custom view service would then be registered with the `ViewService` property of the `IdentityServerServiceFactory`.

The methods of the `IViewService` interface each are expected to produce a `Stream` that contains the UTF8 encoded markup to be displayed for the various views (login, consent, etc.). These methods all accept as the first parameter the OWIN environment dictionary (`IDictionary<string, object>`). The second parameter to each of the methods is a model that provides contextual information that will most likely be needed to be presented to the user (for example the client and scopes on the consent view, or the error message on the error view, or the URL to submit credentials to login).

Most views will need to make requests back to various endpoints within IdentityServer. These GET and POST requests are required to contain the same inputs that the default views send to the server.

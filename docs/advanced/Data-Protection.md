IdentityServer needs to protect (sign and encrypt) various data, e.g:

* client secrets
* cookies

Katana provides data protection mechanisms out of the box: ASP.NET machine key based for IIS hosting, DPAPI based for self hosting. If you don't explicitly specify your own data protector on the configuration options, IdentityServer will fall back to the Katana implementations.

However, you can implement your own `IDataProtector` ([interface](https://github.com/thinktecture/Thinktecture.IdentityServer.v3/blob/master/source%2FCore%2FConfiguration%2FIDataProtector.cs)), if you want to take over data protection.
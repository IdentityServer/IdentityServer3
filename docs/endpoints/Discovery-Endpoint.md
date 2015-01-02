The discovery endpoint can be used to retrieve metadata about IdentityServer - it returns information like the issuer name, key material, supported scopes etc.

see [spec](http://openid.net/specs/openid-connect-discovery-1_0.html)

### Example

```
GET /.well-known/openid-configuration
```

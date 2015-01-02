The identity token validation endpoint can be used to validate identity tokens. This is useful for clients that don't have access to the appropriate JWT or crypto libraries (e.g. JavaScript).

### Example

```
GET /connect/identitytokenvalidation?token=<token>&client_id=<expected_client_id>
```

A successful response will return a status code of 200 and the associated claims for the token. An unsuccessful response will return a 400 with an error message.

```
{
  "nonce": "nonce",
  "iat": "1413203421",
  "sub": "88421113",
  "amr": "password",
  "auth_time": "1413203419",
  "idp": "idsrv",
  "iss": "https://idsrv3.com",
  "aud": "implicitclient",
  "exp": "1413203781",
  "nbf": "1413203421"
}
```
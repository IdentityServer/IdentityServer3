---
layout: default
---
The access token validation endpoint can be used to validate reference tokens. It can be also used to validate self-contained JWTs if the consumer does not have support for appropriate JWT or cryptographic libraries.

### Example

```
GET /connect/accesstokenvalidation?token=<token>
```

A successful response will return a status code of 200 and the associated claims for the token. An unsuccessful response will return a 400 with an error message.

It is also possible to pass a scope that is expected to be inside the token:
```
GET /connect/accesstokenvalidation?token=<token>&expectedScope=calendar
```
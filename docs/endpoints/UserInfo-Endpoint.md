The UserInfo endpoint can be used to retrieve identity information about a subject. It requires a valid access token with at least the 'openid' scope.

see [spec](http://openid.net/specs/openid-connect-core-1_0.html#UserInfo)

### Example

```
GET /connect/userinfo
Authorization: Bearer <access_token>
```

```
HTTP/1.1 200 OK
Content-Type: application/json

{
   "sub": "248289761001",
   "name": "Bob Smith",
   "given_name": "Bob",
   "family_name": "Smith",
   "role": [
       "user",
       "admin"
   ]
}
```
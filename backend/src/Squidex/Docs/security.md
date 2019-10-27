Squidex uses oauth2 client authentication. Read more about it at: https://oauth.net/2/ and https://tools.ietf.org/html/rfc6750.

To retrieve an access token, the client id must make a request to the token url. For example:

    $ curl
        -X POST '<TOKEN_URL>' 
        -H 'Content-Type: application/x-www-form-urlencoded' 
        -d 'grant_type=client_credentials&
            client_id=[APP_NAME]:[CLIENT_ID]&
            client_secret=[CLIENT_SECRET]&
			scope=squidex-api'

`[APP_NAME]` is the name of your app. You have to create a client to generate an access token.

You must send this token in the `Authorization` header when making requests to the API:

     Authorization: Bearer <token>
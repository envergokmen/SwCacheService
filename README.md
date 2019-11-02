# SwCacheService
C# OutProc HttpCache Service

SwCacheService is simple & fast outproc HTTP cache solution which developed in C#.
It's running as http server in windows services. You can set and get cache values with keys.

It is fast beacuse not depends on any framework (without .net framework).
It's support http GET and POST so you can easily use from anywhere, no connection needed.

## Post Request Detail to set cache -> http://localhost:8080/SetCache

```json
{
	"expiresAt":"2020-10-10",
	"key":"YourKeyHere",
	"value":"Your Value Here"
}
```


## GET Request Detail to set cache -> http://localhost:8080/GetCache

```json
{
	"key":"YourKeyHere"
}
```

## Flush all items from the cache Request Detail to set cache -> http://localhost:8080/Flush
## GET Request Detail to set cache -> http://localhost:8080/GetKeys
## Get All Request Detail to set cache -> http://localhost:8080/GetAll


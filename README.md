# SwCacheService

SwCacheService is simple & fast outofproc HTTP cache solution which is developed with C#.
It's running as http server in windows services. You can set and get cache values with keys.

It is fast. It does not depend on any framework, there is no mvc or webapi solution (except for .net framework).
It's support http GET and POST so you can easily use from anywhere, no connection needed. It works on plain http listener.
It usually has reponse times 1-5 ms which is not possible with .net's mvc or webapi projects.

I developed this project because  it is hard to find reliable cache solutions for windows. Redis and Memcached like projects usually come from linux distributions. 
They are little bit complex and sometimes may cause issues. This project may help people who needs basic out of proc cache solution with basic setup.

It is native .net solution fully compatible with windows, can be run as windows service.


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


## find the port configuration under app.config

```xml
  <appSettings>
      <add key="port" value="8080" />
  </appSettings>
```

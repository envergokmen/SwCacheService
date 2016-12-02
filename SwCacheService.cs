using Newtonsoft.Json;
using SwCache.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Caching;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SwCache
{
    public partial class SwCacheService : ServiceBase
    {
        EventLog eventLog1;

        public SwCacheService(string[] args)
        {
            string eventSourceName = "SwCacheSource";
            string logName = "SwCacheNewLog";

            if (args.Count() > 0) { eventSourceName = args[0]; }
            if (args.Count() > 1) { logName = args[1]; }

            eventLog1 = new System.Diagnostics.EventLog();


            if (!EventLog.SourceExists(eventSourceName))
            {
                EventLog.CreateEventSource(eventSourceName, logName);
            }

            eventLog1.Source = eventSourceName;
            eventLog1.Log = logName;


            InitializeComponent();
            
            Initialize(8080);

        }

        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry("Oley Sw Cachce servisi başladı http server");
        }

        protected override void OnStop()
        {

            eventLog1.WriteEntry("Aaah Sw Cachce servisi durdu http server");
        }
         
        private Thread _serverThread;
        private HttpListener _listener;
        private int _port;

        public int Port
        {
            get { return _port; }
            private set { }
        }

        /// <summary>
        /// Construct server with given port.
        /// </summary>
        /// <param name="path">Directory path to serve.</param>
        /// <param name="port">Port of the server.</param>
        public SwCacheService(int port)
        {
            this.Initialize(port);
        }

        /// <summary>
        /// Construct server with suitable port.
        /// </summary>
        /// <param name="path">Directory path to serve.</param>
        public SwCacheService()
        {
            //get an empty port
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            this.Initialize(port);
        }

        /// <summary>
        /// Stop server and dispose all functions.
        /// </summary>
        public void Stop()
        {
            _serverThread.Abort();
            _listener.Stop();
        }

        private void Listen()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://*:" + _port.ToString() + "/");
            _listener.Start();
            while (true)
            {
                try
                {
                    HttpListenerContext context = _listener.GetContext();
                    Process(context);
                }
                catch (Exception ex)
                {

                }
            }
        }


        /// <summary>
        /// Generates stream from string
        /// </summary>
        /// <param name="s">string for the stream</param>
        /// <returns>Stream</returns>
        private Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// Read and convert to string POST request body
        /// </summary>
        /// <param name="context">Current HttpListener</param>
        /// <returns>RequestBody as string</returns>
        private string GetBodyRequestBodyAsString(HttpListenerContext context)
        {
            string requestBody = "";

            if (context.Request.HasEntityBody)
            {
                using (System.IO.Stream body = context.Request.InputStream) // here we have data
                {
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(body, System.Text.Encoding.UTF8))
                    {
                        requestBody = reader.ReadToEnd();

                    }
                }
            }

            return requestBody;
        }

        /// <summary>
        /// Parse Request Params and Convert To CacheViewModel Object
        /// </summary>
        /// <param name="requestString">request post body</param>
        /// <returns>CacheRequestViewModel</returns>
        private CacheRequestViewModel GetAsCacheRequest(string requestString)
        {

            CacheRequestViewModel cacheRequest = null;
            var paramsAndValues = requestString.Split('&');

            if (paramsAndValues != null && paramsAndValues.Length >= 1)
            {
                cacheRequest = new CacheRequestViewModel();

                foreach (var curParamAndVal in paramsAndValues)
                {
                    var keyAndVal = curParamAndVal.Split('=');

                    if (keyAndVal != null && keyAndVal.Length >= 2)
                    {
                        string key = keyAndVal[0];
                        string value = keyAndVal[1];
                        switch (key)
                        {
                            case "CacheKey": cacheRequest.CacheKey = value; break;
                            case "CacheValue": cacheRequest.CacheValue = value; break;
                            case "CacheEndDate": cacheRequest.CacheEndDate = Convert.ToDateTime(value); break;
                        }

                    }
                }
            }


            return cacheRequest;

        }

        /// <summary>
        /// Writes text to http result with headers
        /// </summary>
        /// <param name="responseBody">which text will be write</param>
        /// <param name="context">Current HttpListener Context</param>
        private void WriteStringToHttpResult(string responseBody, HttpListenerContext context, HttpStatusCode statusCode= HttpStatusCode.OK)
        {
            try
            {
                string path = context.Request.Url.AbsolutePath;
                Stream input = GenerateStreamFromString(responseBody);

                context.Response.ContentEncoding = System.Text.Encoding.UTF8;
                context.Response.ContentType = "text/html"; // _mimeTypeMappings.TryGetValue(".html", out mime) ? mime : "application/octet-stream";
                context.Response.ContentLength64 = input.Length;
                context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                context.Response.AddHeader("Last-Modified", System.IO.File.GetLastWriteTime(path).ToString("r"));


                byte[] buffer = new byte[1024 * 16];
                int nbytes;
                while ((nbytes = input.Read(buffer, 0, buffer.Length)) > 0)
                    context.Response.OutputStream.Write(buffer, 0, nbytes);
                input.Close();

                context.Response.StatusCode = (int)statusCode;
                context.Response.OutputStream.Flush();


            }
            catch (Exception ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

        }

        /// <summary>
        /// Removes all cache keys
        /// </summary>
        /// <param name="context">Current HttpListener Context</param>
        private void RemoveAllCache(HttpListenerContext context)
        {
            
            ObjectCache cache = MemoryCache.Default; 

            foreach (var item in MemoryCache.Default)
            { 
                cache.Remove(item.Key);
            }
 
            WriteStringToHttpResult("OK", context);

        }

        /// <summary>
        /// Removes only matched cache items which starts with key CacheKey
        /// </summary>
        /// <param name="context">Current HttpListener Context</param>
        private void RemoveCacheStartsWith(HttpListenerContext context)
        {
            string path = context.Request.Url.AbsolutePath;

           
            if (context.Request.HasEntityBody)
            {
                string requestBody = GetBodyRequestBodyAsString(context);
                CacheRequestViewModel cacheForRemove = GetAsCacheRequest(requestBody);
                 
                List<string> cacheKeys = new List<string>();
                 
                if (cacheForRemove != null && !String.IsNullOrWhiteSpace(cacheForRemove.CacheKey))
                {
                    ObjectCache cache = MemoryCache.Default;

                    foreach (var item in MemoryCache.Default)
                    {
                        if (item.Key.StartsWith(cacheForRemove.CacheKey))
                        {
                            cache.Remove(item.Key); 
                        } 
                    } 

                }
            }

            WriteStringToHttpResult("OK", context);

        }

        /// <summary>
        /// Remove only item which key's equal to CacheKey
        /// </summary> 
        /// <param name="context">Current HttpListener Context</param>
        private void RemoveCache(HttpListenerContext context)
        {
            string path = context.Request.Url.AbsolutePath;

            if (context.Request.HasEntityBody)
            {
                string requestBody = GetBodyRequestBodyAsString(context);
                CacheRequestViewModel cacheForRemove = GetAsCacheRequest(requestBody);

                if (cacheForRemove != null && !String.IsNullOrWhiteSpace(cacheForRemove.CacheKey))
                {

                    ObjectCache cache = MemoryCache.Default;
                    cache.Remove(cacheForRemove.CacheKey); 

                }
            }

            WriteStringToHttpResult("OK", context);
        }

        /// <summary>
        /// Set Requestbody to to cache (Request body contains CacheKey, CacheEndDate, CacheValue items)
        /// </summary> 
        /// <param name="context">Current HttpListener Context</param>
        private void SetCache(HttpListenerContext context)
        {
            string path = context.Request.Url.AbsolutePath;

            if (context.Request.HasEntityBody)
            {
                try
                {


                    string requestBody = GetBodyRequestBodyAsString(context);
                    CacheRequestViewModel cacheForTheSet = GetAsCacheRequest(requestBody);

                    if (cacheForTheSet != null && !String.IsNullOrWhiteSpace(cacheForTheSet.CacheKey) && !String.IsNullOrWhiteSpace(cacheForTheSet.CacheValue))
                    {

                        ObjectCache cache = MemoryCache.Default;
                        CacheItemPolicy policy = new CacheItemPolicy();

                        cache.Remove(cacheForTheSet.CacheKey);
                         
                        policy.AbsoluteExpiration = cacheForTheSet.CacheEndDate;
                        cache.Add(cacheForTheSet.CacheKey, cacheForTheSet.CacheValue, policy);
                         
                        WriteStringToHttpResult("OK", context);


                    }
                }
                catch (Exception)
                {
                    WriteStringToHttpResult("FAIL", context, HttpStatusCode.InternalServerError);

                    //throw;
                }
            }
        }

        /// <summary>
        /// Get Cached Item with CacheKey
        /// </summary> 
        /// <param name="context">Current HttpListener Context</param>
        private void GetCache(HttpListenerContext context)
        {
            string requestBody = GetBodyRequestBodyAsString(context);
            CacheRequestViewModel cacheRequest = GetAsCacheRequest(requestBody);

            if (cacheRequest != null && !String.IsNullOrWhiteSpace(cacheRequest.CacheKey))
            {
                ObjectCache cache = MemoryCache.Default;
                var cachedContent = cache.Get(cacheRequest.CacheKey);
              
                if (cachedContent != null)
                { 
                    WriteStringToHttpResult(cachedContent.ToString(), context);
                }
            }
        }

        /// <summary>
        /// Process Http Request
        /// </summary>
        /// <param name="context"></param>
        private void Process(HttpListenerContext context)
        {
            string path = context.Request.Url.AbsolutePath;

            switch (path)
            {
                case "/GetCache": GetCache(context); break;
                case "/SetCache": SetCache(context); break;
                case "/RemoveCache": RemoveCache(context); break;
                case "/RemoveAllCache": RemoveAllCache(context); break;
                case "/RemoveCacheStartsWith": RemoveCacheStartsWith(context); break;
                case "/Manage": Manage(context); break;
                case "/GetKeys": GetKeys(context); break;
                case "/Flush": RemoveAllCache(context); break; 
            }
             
            context.Response.OutputStream.Close();

        }

        private void GetKeys(HttpListenerContext context)
        { 
            string requestBody = GetBodyRequestBodyAsString(context);
           
                ObjectCache cache = MemoryCache.Default;

                List<string> keys = new List<string>();
                foreach (var item in MemoryCache.Default)
                {
                    keys.Add(item.Key);
                }
 
                WriteStringToHttpResult(JsonConvert.SerializeObject(keys), context);
            
            context.Response.OutputStream.Close();
        }


        private void Manage(HttpListenerContext context)
        {
           
            string appLocation = System.Reflection.Assembly.GetEntryAssembly().Location;
            var directoryPath = Path.GetDirectoryName(appLocation); 
            string filename = Path.Combine(directoryPath, "Manage","index.html");

            if (File.Exists(filename))
            {
                try
                {
                    Stream input = new FileStream(filename, FileMode.Open);

                    //Adding permanent http response headers 
                    context.Response.ContentType = "text/html"; 
                    context.Response.ContentLength64 = input.Length;
                    //context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                    //context.Response.AddHeader("Last-Modified", System.IO.File.GetLastWriteTime(filename).ToString("r"));

                    byte[] buffer = new byte[1024 * 16];
                    int nbytes;
                    while ((nbytes = input.Read(buffer, 0, buffer.Length)) > 0)
                        context.Response.OutputStream.Write(buffer, 0, nbytes);
                    input.Close();

                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    context.Response.OutputStream.Flush();
                }
                catch (Exception ex)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }

            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }

            context.Response.OutputStream.Close();
        }


        private void Initialize(int port)
        {
            this._port = port;
            _serverThread = new Thread(this.Listen);
            _serverThread.Start();
        }

    }
}

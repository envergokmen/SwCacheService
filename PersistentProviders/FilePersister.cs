using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SwCache.ViewModels;

namespace SwCache.PersistentProviders
{
    public class FilePersister : IPersistentProvider
    {
        private static object lockObj = new object();
        public bool PersistentMode { get; set; } = false;
        private string CacheFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CachedFiles");
       
        public FilePersister()
        {
            if (!Directory.Exists(this.CacheFolder)) { Directory.CreateDirectory(this.CacheFolder); }
        }

        public void AddToPersistentCache(CacheRequestViewModel item)
        {
            if (!PersistentMode) return;

            try
            {

                lock (lockObj)
                {
                    File.WriteAllText(Path.Combine(this.CacheFolder, item.key + ".txt"), JsonConvert.SerializeObject(item), Encoding.UTF8);
                }

            }
            finally
            {


            }
        }

        public void DeleteCacheBulk(string startsWith = null)
        {
            if (!PersistentMode) return;

            try
            {
                var files = new DirectoryInfo(this.CacheFolder).GetFiles();
                if (startsWith != null) files = files.Where(c => c.Name.StartsWith(startsWith)).ToArray();

                foreach (var fileItem in files)
                {
                    var cachedFile = fileItem.FullName;

                    if (File.Exists(cachedFile))
                    {
                        lock (lockObj)
                        {
                            File.Delete(cachedFile);
                        }

                    }
                }
            }
            finally
            {

            }
        }

        public Dictionary<string, CacheRequestViewModel> GetAllCachedItems()
        {

            Dictionary<string, CacheRequestViewModel> cachedItems = new Dictionary<string, CacheRequestViewModel>();

            try
            {
                var files = new DirectoryInfo(this.CacheFolder).GetFiles();

                foreach (var fileItem in files)
                {
                    var cachedFile = fileItem.FullName;

                    if (File.Exists(cachedFile))
                    {
                        var cachedFileItem = JsonConvert.DeserializeObject<CacheRequestViewModel>(File.ReadAllText(cachedFile, Encoding.UTF8));

                        if (cachedFileItem != null)
                        {
                            if (cachedFileItem.expiresAt < DateTime.Now)
                            {
                                lock (this)
                                {
                                    File.Delete(cachedFile);
                                }
                            }
                            else
                            {
                                cachedItems.Add(cachedFileItem.key, cachedFileItem);
                               // AddToMemoryCache(cachedFileItem);
                            }
                        }
                    }

                }
            }
            finally
            {

            }

            return cachedItems;
        }

        public void DeleteCache(string key)
        {
            if (!PersistentMode) return;

            try
            {
                var cachedFile = Path.Combine(this.CacheFolder, key + ".txt");
                if (File.Exists(cachedFile))
                {
                    lock (lockObj)
                    {
                        File.Delete(cachedFile);
                    }
                }

            }
            finally
            {

            }
        }

        public CacheRequestViewModel TryToGetFromPersistent(string key)
        {
            CacheRequestViewModel cachedFileItem = null;

            try
            {
                if (PersistentMode)
                {
                    var cachedFile = Path.Combine(this.CacheFolder, key + ".txt");
                    if (File.Exists(cachedFile))
                    {
                        cachedFileItem = JsonConvert.DeserializeObject<CacheRequestViewModel>(File.ReadAllText(cachedFile, Encoding.UTF8));

                        if (cachedFileItem != null)
                        {
                            if (cachedFileItem.expiresAt < DateTime.Now)
                            {
                                lock (lockObj)
                                {
                                    File.Delete(cachedFile);
                                }
                            }
                            else
                            {
                                //AddToMemoryCache(cachedFileItem);

                            }
                        }
                    }

                }
            }
            finally
            {

            }

            return cachedFileItem;
        }

       
    }
}

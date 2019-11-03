using SwCache.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwCache.PersistentProviders
{
    public interface IPersistentProvider
    {
        bool PersistentMode { get; set; }
        void AddToPersistentCache(CacheRequestViewModel item);
        CacheRequestViewModel TryToGetFromPersistent(string key);
        Dictionary<string, CacheRequestViewModel> GetAllCachedItems();
        void DeleteFileCacheBulk(string startsWith = null);
        void RemoveFileCache(string key);

    }
}

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
        void AddToPersistentCache(CacheRequestViewModel item);
        CacheRequestViewModel TryToGetFromPersistent(string key);
        Dictionary<string, CacheRequestViewModel> GetAllCachedItems();
        void DeleteCacheBulk(string startsWith = null);
        void DeleteCache(string key);

    }
}

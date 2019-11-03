using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SwCache.ViewModels;

namespace SwCache.PersistentProviders
{
    public class NoPersistentProvider : IPersistentProvider
    {
        public bool PersistentMode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void AddToPersistentCache(CacheRequestViewModel item)
        {
            return;
        }

        public void DeleteCache(string key)
        {
            return;
        }

        public void DeleteCacheBulk(string startsWith = null)
        {
            return;
        }

        public Dictionary<string, CacheRequestViewModel> GetAllCachedItems()
        {
            return new Dictionary<string, CacheRequestViewModel>();
        }

        public CacheRequestViewModel TryToGetFromPersistent(string key)
        {
            return null;
        }
    }
}

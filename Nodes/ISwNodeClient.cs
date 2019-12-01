using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwCache.Nodes
{
    public interface ISwNodeClient
    {
        string Id { get; }
        T Get<T>(string key) where T : class;
        void Set<T>(string key, T data);
        void Set<T>(string key, T data, DateTime expireDate);
        void Set<T>(string key, T data, DateTime expireDate, string[] fileDependencies = null);
        void Remove(string key);
        void ClearAllCache();
        List<string> GetAllKeys();
        void RemoveKeyStartsWith(string pattern);
    }
}

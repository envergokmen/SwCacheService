using SwCache.ViewModels;

namespace SwCache.Nodes
{
    public interface INodeSyncronizer
    {
        void AddToNodes(CacheRequestViewModel cacheForTheSet, string fromNode);
        void DeleteFromAllNodes(CacheRequestViewModel cacheSource, string fromNode);
        void DeleteFromNodesStartsWith(CacheRequestViewModel cacheToRemove, string fromNode);
        void DeleteFromNodes(CacheRequestViewModel cacheToRemove, string fromNode);
    }
}
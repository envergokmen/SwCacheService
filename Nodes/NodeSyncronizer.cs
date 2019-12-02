using SwCache.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwCache.Nodes
{
    public class NodeSyncronizer : INodeSyncronizer
    {
        string currentNodeId = ConfigurationManager.AppSettings["id"];
        List<ISwNodeClient> nodes = new NodeClientFactory().Nodes.Where(c => c.Id != ConfigurationManager.AppSettings["id"]).ToList();

        public void DeleteFromNodes(CacheRequestViewModel cacheToRemove, string fromNode)
        {
            if (fromNode == null)//check if it sub request for infinite loop
            {
                foreach (var node in this.nodes)
                {
                    node.Remove(cacheToRemove.key, currentNodeId);

                }
            }

        }

        public void AddToNodes(CacheRequestViewModel cacheForTheSet, string fromNode)
        {
            if (fromNode == null)//check if it sub request for infinite loop
            {
                foreach (var node in this.nodes.Where(c => c.Id != this.currentNodeId))
                {
                    if (cacheForTheSet.expiresAt.HasValue)
                    {
                        node.Set<string>(cacheForTheSet.key, cacheForTheSet.value, cacheForTheSet.expiresAt.Value, fromNode: currentNodeId);
                    }
                    else
                    {
                        node.Set<string>(cacheForTheSet.key, cacheForTheSet.value, fromNode: currentNodeId);
                    }
                }
            }
        }

        public void DeleteFromNodesStartsWith(CacheRequestViewModel cacheToRemove, string fromNode)
        {
            if (fromNode == null)
            {
                foreach (var node in this.nodes)
                {
                    node.RemoveKeyStartsWith(cacheToRemove.key, fromNode);
                }
            }

        }



        public void DeleteFromAllNodes(CacheRequestViewModel cacheSource, string fromNode)
        {
            if (fromNode == null)
            {
                foreach (var node in this.nodes)
                {
                    node.ClearAllCache(currentNodeId);
                }
            }
        }



    }
}

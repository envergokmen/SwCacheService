using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwCache.Nodes
{
   
    public class NodeClientFactory
    {
        public List<ISwNodeClient> Nodes
        {
            get
            {
                string persistentType = ConfigurationManager.AppSettings["persisterType"];

                List<ISwNodeClient> nodes = new List<ISwNodeClient>();

                //var Path = ConfigurationManager.AppSettings["node1path"];
                //var Port = ConfigurationManager.AppSettings["node1port"];

                string[] nodeSettings = ConfigurationManager.AppSettings.AllKeys
                             .Where(key => key.StartsWith("node"))
                             .Select(key => key)
                             .ToArray();

                foreach (var item in nodeSettings.Where(c=>c.EndsWith("path")))
                {
                    var Path = ConfigurationManager.AppSettings[item];
                    var Port = ConfigurationManager.AppSettings[item.Replace("path","port")];

                    var server = new SwCacheServer(Path, Port);
                    var cacheClient = new SwNodeClient(server);

                    nodes.Add(cacheClient);
                }

                return nodes;

            }
        }
    }
}

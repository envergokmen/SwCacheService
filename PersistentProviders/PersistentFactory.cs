using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwCache.PersistentProviders
{
    public class PersistentFactory
    {
        public IPersistentProvider Persister
        {
            get
            {
                string persistentType = ConfigurationManager.AppSettings["persister"];

                IPersistentProvider persister;

                switch (persistentType)
                {

                    default: persister = new FilePersister(); break;
                }

                return persister;
            }
        }
    }
}

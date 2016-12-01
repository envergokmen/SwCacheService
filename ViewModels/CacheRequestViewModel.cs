using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwCache.ViewModels
{
    
    public class CacheRequestViewModel
    {
        public string CacheKey { get; set; }
        public string CacheValue { get; set; }
        public DateTime CacheEndDate { get; set; }

    }
}

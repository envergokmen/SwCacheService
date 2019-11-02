using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwCache.ViewModels
{
    
    public class CacheRequestViewModel
    {
        public string key { get; set; }
        public string value { get; set; }
        public DateTime? expiresAt { get; set; }

    }
}

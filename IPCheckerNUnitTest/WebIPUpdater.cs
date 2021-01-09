using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IP_Checker
{
    public interface WebIPUpdater
    {
        public Action<string> UpdateIPAction { get; set; }
        public Action<HashSet<string>> UpdateWebsitesAction { get; set; }
    }
}

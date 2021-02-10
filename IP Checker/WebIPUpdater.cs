using System;
using System.Collections.Generic;

namespace IP_Checker
{
    public interface WebIPUpdater
    {
        public Action<string> UpdateIPAction { get; set; }
        public Action<HashSet<string>> UpdateWebsitesAction { get; set; }
    }
}

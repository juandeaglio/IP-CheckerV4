using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IP_Checker
{
    public class WebsiteHashSet : HashSet<string>
    {
        public WebsiteHashSet() : base()
        {
        }
        public new void Add(string websiteFieldText)
        {
            if (!base.Contains(websiteFieldText))
            { 
                base.Add(websiteFieldText);
                IPMonitor.UpdateWebsitesAction(this);
            }
        }
        public new void Remove(string websiteFieldText)
        {
            if (base.Contains(websiteFieldText))
            {
                base.Remove(websiteFieldText);
                IPMonitor.UpdateWebsitesAction(this);
            }
        }
    }
}

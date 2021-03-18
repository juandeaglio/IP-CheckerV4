using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IP_Checker
{
    public class WebsiteTester
    {
        private HashSet<string> websiteSet;
        string FastestWebsite;
        public WebsiteTester(HashSet<string> set)
        {
            SetWebsiteSet(set);
        }
        public void SetWebsiteSet(HashSet<string> set)
        {
            websiteSet = set;
        }
        public HashSet<string> GetWebsiteSet()
        {
            return websiteSet;
        }
        public string TestForFastestWebsite()
        {
            string fastest = null;
            return fastest;
        }
        public virtual void Add(string name)
        {
            HashSetWebsiteHelper.AddIfValid(name, ref websiteSet, new TimedWebClient());
        }
        public virtual void Remove(string name)
        {
            HashSetWebsiteHelper.Remove(name, ref websiteSet);
        }
    }
}

using System.Collections.Generic;
using System.Net;

namespace IP_Checker
{
    public class WebsiteHashSet : HashSet<string>
    {
        public WebsiteHashSet() : base()
        {
        }
        public new void Add(string websiteFieldText)
        {
            try
            {
                using (var client = new TimedWebClient())
                    client.OpenRead(websiteFieldText);
                if (!base.Contains(websiteFieldText))
                {
                    lock (this)
                    {
                        base.Add(websiteFieldText);
                    }
                    IPMonitor.UpdateWebsitesAction(this);
                }
            }
            catch (WebException ex)
            {
                //TODO: Logging incorrect website added or unreachable website. GUI feedback if incorrect.
            }

        }
        public new void Remove(string websiteFieldText)
        {
            if (base.Contains(websiteFieldText))
            {
                lock (this)
                {
                    base.Remove(websiteFieldText);
                }
                IPMonitor.UpdateWebsitesAction(this);
            }
        }
    }
}

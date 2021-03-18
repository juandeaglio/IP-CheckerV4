using System.Collections.Generic;
using System.Net;

namespace IP_Checker
{
    public static class HashSetWebsiteHelper
    {
        public static void Add(string websiteFieldText, ref HashSet<string> websiteSet)
        {
            try
            {
                using (var client = new TimedWebClient())
                    client.OpenRead(websiteFieldText);
                if (!websiteSet.Contains(websiteFieldText))
                {
                    lock (websiteSet)
                    {
                        websiteSet.Add(websiteFieldText);
                    }
                }
            }
            catch (WebException ex)
            {
                //TODO: Logging incorrect website added or unreachable website. GUI feedback if incorrect.
            }

        }
        public static void Remove(string websiteFieldText, ref HashSet<string> websiteSet)
        {
            if (websiteSet.Contains(websiteFieldText))
            {
                lock (websiteSet)
                {
                    websiteSet.Remove(websiteFieldText);
                }
            }
        }
    }
}

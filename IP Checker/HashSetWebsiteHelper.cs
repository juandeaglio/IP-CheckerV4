using System.Collections.Generic;
using System.Net;

namespace IP_Checker
{
    public static class HashSetWebsiteHelper
    {
        public static void AddIfValid(string websiteFieldText, ref HashSet<string> websiteSet, WebClient client)
        {
            if (!websiteSet.Contains(websiteFieldText) && IsWebsiteActive(websiteFieldText, client))
            {
                lock (websiteSet)
                {
                    websiteSet.Add(websiteFieldText);
                }
            }
        }

        private static bool IsWebsiteActive(string websiteFieldText, WebClient client)
        {
            try
            {
                client.OpenRead(websiteFieldText);
                return true;
            }
            catch (WebException ex)
            {
                return false;
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IP_Checker
{
    public class WebsiteTester
    {
        private HashSet<string> websiteSet;
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
        public static string TestForFastestWebsite(HashSet<string> websiteSet, ref bool error, CancellationTokenSource cancelToken)
        {


            string fastest = null;
            void WaitForWebsiteToChange(ref string fastest)
            {
                while (fastest == null)
                { }
            }
            bool result = false;
            new Thread(() =>
            {
                ParallelOptions parOpts = new ParallelOptions();
                parOpts.CancellationToken = cancelToken.Token;
                parOpts.MaxDegreeOfParallelism = websiteSet.Count < Environment.ProcessorCount ? websiteSet.Count : Environment.ProcessorCount;
                result = false;
                bool written = false;
                try
                {
                    bool temp = false;
                    Parallel.ForEach(websiteSet, parOpts, (currentWebsite) =>
                    {
                        TryWebsite(currentWebsite, ref temp);
                        if (!written)
                        {
                            fastest = currentWebsite;
                            written = true;
                        }
                    });
                    result = result || temp;
                }
                catch (OperationCanceledException ex)
                {
                    //TODO: Log
                }
                catch (WebException ex)
                {
                    result = true;
                }

            }).Start();
            WaitForWebsiteToChange(ref fastest);
            cancelToken.Cancel();
            error = result;
            return fastest;
        }
        public static bool TryWebsite(string website, ref bool error)
        {
            try
            {
                using (var client = new TimedWebClient())
                using (client.OpenRead(website))
                error = false;
                return true;
            }
            catch (WebException ex)
            {
                //TODO: Logging if a particular link timedOut (needs replacement or checking) GUI feedback if this occurs.
            }
            catch (OperationCanceledException ex)
            {
                //TODO: Logging which one is slower. GUI Feedback of stats on website speeds.
            }
            return false;
        }
        public virtual void Add(string name)
        {
            HashSetWebsiteHelper.AddIfValid(name, ref websiteSet, new TimedWebClient());
            bool error = false;
        }
        public virtual void Remove(string name)
        {
            HashSetWebsiteHelper.Remove(name, ref websiteSet);
        }
    }
}

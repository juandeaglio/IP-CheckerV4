using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace IP_Checker
{
    public static class IPMonitor
    {
        public static string Title { get; set; } = "IP Checka";
        private static HashSet<string> websites;
        public static string currentIP;
        public static string currentIPField;
        private static string websiteStr;
        //public static bool stop = false;
        private static string CurrentWebsite { get; set; } = "";
        private static CancellationTokenSource cancelToken = new CancellationTokenSource();
        private static string testWebsite = "";
        static IPMonitor()
        {
            SetWebsites(new WebsiteHashSet());
        }
        public static HashSet<string> GetWebsites()
        {
            return websites;
        }
        public static void SetWebsites(HashSet<string> newWebsites)
        {
            websites = newWebsites;
        }
        public static void Run()
        {
            Task.Factory.StartNew(CheckIPAndUpdate);
            new Thread(() =>
            {
                while(currentIP == null|| currentIP.Equals(""))
                { }
                SynchronousSocketListener.StartListening(currentIP);
            }).Start();
        }
        private static void UpdateIPField(string currentStatus)
        {
            if(!currentStatus.Equals(""))
                UpdateIPFieldAction?.Invoke(currentStatus);
            else
                UpdateIPFieldAction?.Invoke("No internet or 0 websites listed.");
        }
        public static bool TryFetchIP(string website)
        {
            if (!website.Equals(""))
            {
                HttpDownload(website);
                currentIPField = currentIP + " using " + website;
                UpdateIPField(currentIPField);
                return true;
            }
            else
            {
                currentIP = "";
                UpdateIPField(currentIP);
                return false;
            }

        }

        private static void HttpDownload(string website)
        {
            if (website.Length > 0)
            {
                TimedWebClient wc = new TimedWebClient();
                wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(FormatHTMLToIPString);
                wc.DownloadStringAsync(new Uri(website));
            }
        }

        public static void FormatHTMLToIPString(object sender, DownloadStringCompletedEventArgs e)
        {
            string regexPattern = @"\d*\.\d*\.\d*\.\d*";
            Regex rgx = new Regex(regexPattern);
            currentIP = rgx.Match(e.Result).Value;
        }

        public static void CheckIPAndUpdate()
        {
            while (true)
            {
                if (IsConnectionActive())
                {
                    TryFetchIP(CurrentWebsite);
                    Thread.Sleep(100);
                }
                else
                {
                    currentIP = "";
                    UpdateIPField(currentIP);
                }
            }
        }
        static bool TryWebsite(string website, ref bool error)
        {
            try
            {
                using (var client = new TimedWebClient())
                using (client.OpenRead(website))
                websiteStr = website;
                error = false;
                return true;
            }
            catch (WebException ex)
            {
                //TODO: Logging if a particular link timedOut (needs replacement or checking) GUI feedback if this occurs.
                testWebsite = website;
            }
            catch (OperationCanceledException ex)
            {
                //TODO: Logging which one is slower. GUI Feedback of stats on website speeds.
            }
            return false;
        }
        public static bool IsConnectionActive()
        {
            websiteStr = null;
            bool error = true;
            lock (websites)
            {
                if (websites.Count > 1)
                {
                    if (CurrentWebsite != null)
                    {
                        if (CurrentWebsite.Equals(""))
                        {
                            new Thread(() => ParallelTryWebsite(ref error)).Start();
                            WaitForWebsiteToChange();
                            cancelToken.Cancel();
                            cancelToken = new CancellationTokenSource();
                        }
                        else
                            TryWebsite(CurrentWebsite, ref error);
                    }
                    
                }
                else if(websites.Count == 1)
                    TryWebsite(websites.First(), ref error);
            }
            CurrentWebsite = websiteStr;
            return !error;
        }

        private static void WaitForWebsiteToChange()
        {
            while (websiteStr == null)
            { }
        }

        private static void ParallelTryWebsite(ref bool error)
        {
            ParallelOptions parOpts = new ParallelOptions();
            parOpts.CancellationToken = cancelToken.Token;
            parOpts.MaxDegreeOfParallelism = websites.Count < Environment.ProcessorCount ? websites.Count : Environment.ProcessorCount;
            error = false;
            bool written = false;
            try
            {
                bool temp = false;
                Parallel.ForEach(websites, parOpts, (currentWebsite) =>
                {
                    TryWebsite(currentWebsite, ref temp);
                    if(!written)
                    {
                        websiteStr = currentWebsite;
                        written = true;
                    }
                });
                error = error || temp;
            }
            catch (OperationCanceledException ex)
            {
                //TODO: Log
            }
            catch (WebException ex)
            {
                websiteStr = testWebsite;
                error = true;
            }

        }

        //Delegates are simple and defined by MainWindow.
        public static Action<string> UpdateIPFieldAction;
        public static Action<HashSet<string>> UpdateWebsitesAction;
    }
}

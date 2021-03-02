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
        public static WebsiteHashSet websites;
        public static string currentIP;
        public static string currentIPField;
        private static string websiteStr;
        //public static bool stop = false;
        private static string CurrentWebsite { get; set; } = "";
        private static CancellationTokenSource cancelToken = new CancellationTokenSource();
        private static string testWebsite = "";
        //TODO: Future plans of timer
        //private static TimerCallback timerCB;
        static IPMonitor()
        {
            websites = new WebsiteHashSet();
        }

        public static void Run()
        {
            Task.Factory.StartNew(CheckIP);
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
            Thread.Sleep(1000);
        }
        public static bool TryFetchIP()
        {
            if (IsConnectionActive())
            {
                TimedWebClient wc = new TimedWebClient();
                wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(wc_DownloadStringCompleted);
                wc.DownloadStringAsync(new Uri(CurrentWebsite));
                void wc_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
                {
                    string regexPattern = @"\d*\.\d*\.\d*\.\d*";
                    Regex rgx = new Regex(regexPattern);
                    currentIP = rgx.Match(e.Result).Value;
                    currentIPField = currentIP + " using " + CurrentWebsite;
                    UpdateIPField(currentIPField);
                }
                return true;
            }
            //TODO: Check for internet outage on delegate counter. Shuts down after a certain limit. Logs shutdown.
            else
            {
                currentIP = "";
                UpdateIPField(currentIP);
                return false;
            }
        }
        public static void CheckIP()
        {
            while (true)
            {
                if (websites.Count > 0)
                {
                    TryFetchIP();
                }
                else
                {
                    currentIP = "";
                    UpdateIPField(currentIP);
                }
            }
        }
        static bool TryWebsite(string website, ParallelOptions parOpts)
        {
            try
            {
                using (var client = new TimedWebClient())
                using (client.OpenRead(website))
                parOpts?.CancellationToken.ThrowIfCancellationRequested();
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
            //TODO: Refactor code so that websites with actual IP returns are prioritized. 
            websiteStr = null;
            bool error = false;
            Thread.Sleep(2000);
            lock (websites)
            {
                if (websites.Count > 1)
                {
                    new Thread(() => ParallelTryWebsite(ref error)).Start();
                    while (websiteStr == null)
                    { }
                    cancelToken.Cancel();
                    cancelToken = new CancellationTokenSource();
                }
                else
                {
                    try
                    {
                        if (websites.Count() == 0)
                            return false;
                        if (TryWebsite(websites.First(), null))
                            websiteStr = websites.First();
                    }
                    catch (WebException ex)
                    {
                        websiteStr = testWebsite;
                        error = true;
                    }
                }
            }
            while(websiteStr == null || websiteStr.Equals(""))
            {
            }

            CurrentWebsite = websiteStr;
            Title = CurrentWebsite + "is now being used...";

            return !error;
        }

        private static void ParallelTryWebsite(ref bool error)
        {
            ParallelOptions parOpts = new ParallelOptions();
            parOpts.CancellationToken = cancelToken.Token;
            parOpts.MaxDegreeOfParallelism = websites.Count < Environment.ProcessorCount ? websites.Count : Environment.ProcessorCount;
            string temp = null;
            bool written = false;
            try
            {
                Parallel.ForEach(websites, (currentWebsite) =>
                {
                    if(TryWebsite(currentWebsite, parOpts))
                    {
                        if (!written)
                        {
                            websiteStr = currentWebsite;
                        }
                    }
                }
                );
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
        public static void AddWebsite(string websiteFieldText)
        {
            try
            {
                using (var client = new TimedWebClient())
                    client.OpenRead(websiteFieldText);
                websites.Add(websiteFieldText);
            }
            catch (WebException ex)
            {
                //TODO: Logging incorrect website added or unreachable website. GUI feedback if incorrect.
            }
        }
        public static void RemoveWebsite(string websiteFieldText)
        {
            websites.Remove(websiteFieldText);
        }
    }
}

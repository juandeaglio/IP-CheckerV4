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
        public static bool TryFetchIP()
        {
            if (websites.Count() != 0)
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
            else
            {
                currentIP = "";
                UpdateIPField(currentIP);
                return false;
            }

        }
        public static void CheckIPAndUpdate()
        {
            while (true)
            {
                if (IsConnectionActive())
                {

                    TryFetchIP();
                    Thread.Sleep(100);
                }
                else
                {
                    currentIP = "";
                    UpdateIPField(currentIP);
                }
            }
        }
        static bool TryWebsite(string website)
        {
            try
            {
                using (var client = new TimedWebClient())
                using (client.OpenRead(website))
                websiteStr = website;
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
            bool error = false;
            lock (websites)
            {
                if (websites.Count > 1)
                {
                    new Thread(() => ParallelTryWebsite(ref error)).Start();
                    WaitForWebsiteToChange();
                    cancelToken.Cancel();
                    cancelToken = new CancellationTokenSource();
                }
                else
                    TryWebsite(websites.First());
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
            bool written = false;
            try
            {
                Parallel.ForEach(websites, (currentWebsite) =>
                {
                    if(TryWebsite(currentWebsite))
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

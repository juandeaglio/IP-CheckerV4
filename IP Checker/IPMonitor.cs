using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace IP_Checker
{
    public static class IPMonitor
    {
        public static string Title { get; set; } = "IP Checka";
        public static WebsiteHashSet websites;
        public static string currentIP;
        public static string currentIPField;
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
        static string TryWebsite(string website, ParallelOptions parOpts)
        {
            try
            {
                using (var client = new TimedWebClient())
                using (client.OpenRead(website)) ;
                parOpts?.CancellationToken.ThrowIfCancellationRequested();
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
            return website;
        }
        public static bool IsConnectionActive()
        {
            //TODO: Refactor code so that websites with actual IP returns are prioritized. 
            string websiteStr = null;
            bool error = false;

            lock (websites)
            {
                if (websites.Count > 1)
                {
                    ParallelTryWebsite(ref websiteStr, ref error);
                }
                else
                {
                    string temp;
                    try
                    {
                        if (websites.Count() == 0)
                            return false;
                        temp = TryWebsite(websites.First(), null);
                        websiteStr = temp;
                    }
                    catch (WebException ex)
                    {
                        websiteStr = testWebsite;
                        error = true;
                    }
                }
            }
            cancelToken.Cancel();
            CurrentWebsite = websiteStr;
            Title = CurrentWebsite + "is now being used...";
            cancelToken = new CancellationTokenSource();
            return !error;
        }

        private static void ParallelTryWebsite(ref string websiteStr, ref bool error)
        {
            string temp;
            ParallelOptions parOpts = new ParallelOptions();
            parOpts.CancellationToken = cancelToken.Token;
            parOpts.MaxDegreeOfParallelism = websites.Count < Environment.ProcessorCount ? websites.Count : Environment.ProcessorCount;
            try
            {
                Parallel.ForEach(websites, parOpts, website =>
                {
                    temp = TryWebsite(website, parOpts);
                    website = temp;
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
            //Display add success notification.
        }
        public static void RemoveWebsite(string websiteFieldText)
        {
            websites.Remove(websiteFieldText);
        }
    }
}

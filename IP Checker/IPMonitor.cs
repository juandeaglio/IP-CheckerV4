using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

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
        private static bool done = false;
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
        public static void CheckIP()
        {
            while (true)
            {
                static void UpdateIP(string currentStatus)
                {
                    UpdateIPAction(currentStatus);
                    Thread.Sleep(1000);
                }
                if (websites.Count > 0)
                {
                    if (IsConnectionActive())
                    {
                        TimedWebClient wc = new TimedWebClient();
                        wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(wc_DownloadStringCompleted);
                        wc.DownloadStringAsync(new Uri(CurrentWebsite));
                        //Download IP, sort out useless info with regex.
                        void wc_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
                        {
                            try
                            {
                                //When string is downloaded use a regex pattern to get IP: (digits [dot] digits [dot] digits [dot] digits)
                                string regexPattern = @"\d*\.\d*\.\d*\.\d*";
                                Regex rgx = new Regex(regexPattern);
                                //if(!stop)
                                currentIP = rgx.Match(e.Result).Value;
                                //else
                                //    currentIP = "126.44.36.226";
                                currentIPField = currentIP + " using " + CurrentWebsite;
                                UpdateIP(currentIPField);
                            }
                            catch
                            {
                                //TODO: Some sort of logging of exception for a download failed.
                            }
                        }
                    }
                    //TODO: Check for internet outage on delegate counter. Shuts down after a certain limit. Logs shutdown.
                    else
                    {
                        currentIP = "";
                        UpdateIP("No Internet: " + CurrentWebsite);
                    }
                }
                else
                {
                    currentIP = "";
                    UpdateIP("No websites, unable to determine.");
                }
            }
        }
        public static bool IsConnectionActive()
        {
            //TODO: Refactor code so that websites with actual IP returns are prioritized. 
            //Doesn't necessarily first returned but creates a priority queue in which IPs are ordered by order of website reached.
            static string TryWebsite(string website, ParallelOptions parOpts)
            {
                try
                {
                    using (var client = new TimedWebClient())
                    using (client.OpenRead(website));
                    //parOpts?.CancellationToken.ThrowIfCancellationRequested();
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
            string websiteStr = "";
            bool timedOut = false;
            bool error = false;
            done = false;
            //cancelToken = new CancellationTokenSource();
            ParallelOptions parOpts = new ParallelOptions();
            parOpts.CancellationToken = cancelToken.Token;
            lock (websites)
            {
                string temp;
                if (websites.Count == 0)
                    return false;
                else if (websites.Count > 1)
                {
                    parOpts.MaxDegreeOfParallelism = websites.Count < Environment.ProcessorCount ? websites.Count : Environment.ProcessorCount;
                    //TODO: async triple IP check.
                    /*try
                    {
                        Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                while (true)
                                {
                                    if (!websiteStr.Equals("") && done)
                                        //cancelToken.Cancel();
                                    //parOpts?.CancellationToken.ThrowIfCancellationRequested();
                                }
                            }
                            catch (OperationCanceledException ex)
                            {
                                
                            }
                        });
                    }
                    catch (OperationCanceledException ex)
                    {
                        //cancelToken = new CancellationTokenSource();
                        //throw;
                    }*/
                    try
                    {
                        Parallel.ForEach(websites, parOpts, website =>
                        {
                            temp = TryWebsite(website, parOpts);
                            lock(websiteStr)
                            {
                                if(!done)
                                    websiteStr = temp;
                                done = true;
                            }
                        }
                        );
                    }
                    catch (WebException ex)
                    {
                        websiteStr = testWebsite;
                        error = true;
                    }
                }
                else
                {
                    try
                    {
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
            //If a timeout or error occurs, we will return false(IP not updated), otherwise true.
            cancelToken = new CancellationTokenSource();
            return !error;
        }

        //Delegates are simple and defined by MainWindow.
        public static Action<string> UpdateIPAction;
        public static Action<WebsiteHashSet> UpdateWebsitesAction;
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

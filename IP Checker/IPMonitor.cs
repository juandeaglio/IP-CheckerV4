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
        public static HashSet<string> websites;
        public static string currentIP;
        private static string CurrentWebsite { get; set; } = "";
        private static CancellationTokenSource cancelToken = new CancellationTokenSource();
        //TODO: Future plans of timer
        //private static TimerCallback timerCB;
        static IPMonitor()
        {
            websites = new HashSet<string>();
        }

        public static void Run()
        {
            Task.Factory.StartNew(CheckIP);
            while (true)
            {
                Thread.Sleep(50);
            }
        }
        public static void CheckIP()
        {
            while (true)
            {
                static void UpdateIP(string currentStatus)
                {
                    currentIP = currentStatus;
                    UpdateIPAction(currentIP);
                    Thread.Sleep(50);
                }
                if (websites.Count > 0)
                {
                    if (IsConnectionActive())
                    {
                        MyWebClient wc = new MyWebClient();
                        wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(wc_DownloadStringCompleted);
                        //TODO: Multiple IP checks.
                        wc.DownloadStringAsync(new Uri(CurrentWebsite));
                        //Download IP, sort out useless info with regex.
                        void wc_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
                        {
                            try
                            {
                                string regexPattern = @"\d*\.\d*\.\d*\.\d*";
                                Regex rgx = new Regex(regexPattern);
                                currentIP = rgx.Match(e.Result).Value + " using " + CurrentWebsite;
                                UpdateIP(currentIP);
                            }
                            catch
                            {
                                //TODO: Some sort of logging of exception
                            }
                        }
                    }
                    //TODO: Check for internet outage on delegate counter. Shuts down after a certain limit. Logs shutdown.
                    else
                        UpdateIP("No Internet: " + CurrentWebsite);
                }
                else
                {
                    UpdateIP("No websites, unable to determine.");
                }
            }
        }

        public static bool IsConnectionActive()
        {
            bool connected = false;
            connected = ReadFromWebsites();
            return connected;
        }
        private static void ResetTitle(object state)
        {

            try
            {
                Title = state.ToString();
                Thread.Sleep(2000);
                Title = "IP Checka";
                //Send notification to Linux/Win
            }
            catch (NullReferenceException ex)
            {
                //Loggers
                Title = "Null";
            }
        }
        public static bool ReadFromWebsites()
        {
            //CurrentWebsite = "";
            string websiteStr = "";
            bool timedOut = false;
            bool error = false;
            ParallelOptions parOpts = new ParallelOptions();
            parOpts.CancellationToken = cancelToken.Token;
            lock (websites)
            {
                if (websites.Count == 0)
                    return false;
                parOpts.MaxDegreeOfParallelism = websites.Count < System.Environment.ProcessorCount ? websites.Count : System.Environment.ProcessorCount;

                //TODO: async triple IP check.

                Parallel.ForEach(websites, parOpts, website =>
                {
                    try
                    {
                        using (var client = new MyWebClient())
                        using (client.OpenRead(website))
                            parOpts.CancellationToken.ThrowIfCancellationRequested();
                        websiteStr = website;

                    }
                    catch (OperationCanceledException ex)
                    {
                    //TODO: Logging which one is slower. GUI Feedback of stats on website speeds.
                    }
                    catch (WebException ex)
                    {
                    //TODO: Logging if a particular link timedOut (needs replacement or checking) GUI feedback if this occurs.
                        timedOut = true;
                        websiteStr = website + " has timed out.";
                    }
                    finally
                    {
                    //May remove this clause
                    }
                }
                );

                if (!websiteStr.Equals(""))
                    timedOut = false;

                //timerCB = new TimerCallback(ResetTitle);
                //var _ = new Timer(timerCB, null, 2000, 2000);
                cancelToken.Cancel();
                cancelToken = new CancellationTokenSource();
                CurrentWebsite = websiteStr;
                Title = CurrentWebsite + "is now being used...";
                //If a timeout or error occurs, we will return false(IP not updated), otherwise true.
                return !(timedOut || error);
            }
        }
        //Delegates are simple and defined by MainWindow.
        public static Action<string> UpdateIPAction;
        public static Action<HashSet<string>> UpdateWebsitesAction;
        public static void AddWebsite(string websiteFieldText)
        {

            if (!websites.Contains(websiteFieldText))
            {
                try
                {
                    using (var client = new MyWebClient())
                        client.OpenRead(websiteFieldText);
                    lock (websites)
                    {
                        websites.Add(websiteFieldText);
                    }
                    UpdateWebsitesAction(websites);
                }
                catch (WebException ex)
                {
                    //TODO: Logging incorrect website added or unreachable website. GUI feedback if incorrect.
                }         
            }
            //Display add success notification.
        }
        public static void RemoveWebsite(string websiteFieldText)
        {
            if (websites.Contains(websiteFieldText))
            {
                // Your code...
                lock (websites)
                {
                    websites.Remove(websiteFieldText);
                }
            }
            UpdateWebsitesAction(websites);
        }
    }
}

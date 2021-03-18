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
    public class IPMonitor
    {
        public string Title { get; set; } = "IP Checka";
        private HashSet<string> websiteSet;
        public string currentIP;
        public string currentIPField;
        private string websiteStr;
        //public static bool stop = false;
        private string CurrentWebsite { get; set; } = "";
        private CancellationTokenSource cancelToken = new CancellationTokenSource();
        private string testWebsite = "";
        public IPMonitor()
        {
            SetWebsites(new HashSet<string>());
            CurrentWebsite = "";
        }
        public IPMonitor(HashSet<string> websites)
        {
            SetWebsites(websites);
            CurrentWebsite = "";
        }
        public HashSet<string> GetWebsites()
        {
            return websiteSet;
        }
        public void SetWebsites(HashSet<string> newWebsites)
        {
            websiteSet = newWebsites;
        }
        public void Run()
        {
            Task.Factory.StartNew(CheckIPAndUpdate);
            new Thread(() =>
            {
                while(currentIP == null|| currentIP.Equals(""))
                { }
                SynchronousSocketListener.StartListening(currentIP);
            }).Start();
        }
        private void UpdateIPField(string currentStatus)
        {
            if(!currentStatus.Equals(""))
                UpdateIPFieldAction?.Invoke(currentStatus);
            else
                UpdateIPFieldAction?.Invoke("No internet or 0 websites listed.");
        }
        public bool TryFetchIP(string website)
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

        private void HttpDownload(string website)
        {
            if (website.Length > 0)
            {
                TimedWebClient wc = new TimedWebClient();
                wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(FormatHTMLToIPString);
                wc.DownloadStringAsync(new Uri(website));
            }
        }

        public void FormatHTMLToIPString(object sender, DownloadStringCompletedEventArgs e)
        {
            string regexPattern = @"\d*\.\d*\.\d*\.\d*";
            Regex rgx = new Regex(regexPattern);
            currentIP = rgx.Match(e.Result).Value;
        }

        public void CheckIPAndUpdate()
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
        bool TryWebsite(string website, ref bool error)
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
        public bool IsConnectionActive()
        {
            websiteStr = null;
            bool error = true;
            lock (websiteSet)
            {
                if (websiteSet.Count > 1)
                {
                    if (CurrentWebsite == null || CurrentWebsite.Equals(""))
                    {
                        new Thread(() => ParallelTryWebsite(ref error)).Start();
                        WaitForWebsiteToChange();
                        cancelToken.Cancel();
                        cancelToken = new CancellationTokenSource();
                    }
                    else
                        TryWebsite(CurrentWebsite, ref error);
                }
                else if(websiteSet.Count == 1)
                    TryWebsite(websiteSet.First(), ref error);
            }
            CurrentWebsite = websiteStr;
            return !error;
        }

        private void WaitForWebsiteToChange()
        {
            while (websiteStr == null)
            { }
        }

        private void ParallelTryWebsite(ref bool error)
        {
            ParallelOptions parOpts = new ParallelOptions();
            parOpts.CancellationToken = cancelToken.Token;
            parOpts.MaxDegreeOfParallelism = websiteSet.Count < Environment.ProcessorCount ? websiteSet.Count : Environment.ProcessorCount;
            error = false;
            bool written = false;
            try
            {
                bool temp = false;
                Parallel.ForEach(websiteSet, parOpts, (currentWebsite) =>
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
        public void AddWebsite(string name)
        {
            HashSetWebsiteHelper.Add(name, ref websiteSet);
            UpdateWebsitesAction(websiteSet);
        }
        public void RemoveWebsite(string name)
        {
            HashSetWebsiteHelper.Remove(name, ref websiteSet);
            UpdateWebsitesAction(websiteSet);
        }
        //Delegates are simple and defined by MainWindow.
        public Action<string> UpdateIPFieldAction;
        public Action<HashSet<string>> UpdateWebsitesAction;
    }
}

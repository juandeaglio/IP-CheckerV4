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

        public string FastestWebsite { get; private set; }

        //public static bool stop = false;
        private string CurrentWebsite { get; set; } = "";
        private CancellationTokenSource cancelToken = new CancellationTokenSource();
        private WebsiteTester websiteTester;
        public IPMonitor(WebsiteTester webTester)
        {
            websiteTester = webTester;
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

        public bool IsConnectionActive()
        {
            websiteStr = null;
            bool error = true;
            lock (websiteSet)
            {
                if (websiteSet.Count > 1)
                {
                    if (FastestWebsite == null || FastestWebsite.Equals(""))
                    {
                        FastestWebsite = WebsiteTester.TestForFastestWebsite(websiteTester.GetWebsiteSet(), ref error, cancelToken);
                        CurrentWebsite = FastestWebsite;
                        cancelToken = new CancellationTokenSource();
                    }
                    else if (!websiteSet.Contains(CurrentWebsite) || !WebsiteTester.TryWebsite(CurrentWebsite, ref error))
                    {   
                        FastestWebsite = null;
                        CurrentWebsite = "";
                        return false;
                    }
                }
                else if (websiteSet.Count == 1 && WebsiteTester.TryWebsite(websiteSet.First(), ref error))
                {
                    CurrentWebsite = websiteSet.First();
                    FastestWebsite = CurrentWebsite;
                }
            }

            return !error;
        }


        public void AddWebsite(string name)
        {
            websiteTester.Add(name);
            websiteSet = websiteTester.GetWebsiteSet();
            UpdateWebsitesAction(websiteSet);
        }
        public void RemoveWebsite(string name)
        {
            websiteTester.Remove(name);
            websiteSet = websiteTester.GetWebsiteSet();
            UpdateWebsitesAction(websiteSet);
        }
        //Delegates are simple and defined by MainWindow.
        public Action<string> UpdateIPFieldAction;
        public Action<HashSet<string>> UpdateWebsitesAction;
    }
}

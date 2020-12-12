using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using Gtk;
using Gdk;
namespace IP_Checker
{
    class Program
    {
        static void Main(string[] args)
        {
            //IPMonitor.UpdateIPAction = CallWhenIPChanges;
            //IPMonitor.UpdateWebsitesAction = CallWhenWebsitesChanged;
            Console.WriteLine("Hello World!");
            GtkWindow gtkWindow = new GtkWindow();
            //Adds an initial test website.  
            gtkWindow.CreateWindow();
        }
    }
     public class GtkWindow
    {
        public bool running = false;
        public bool running2 = false;
        public Gtk.Window myWin;
        WindowComponentManager manager;
        public string websiteFieldText;
        private string currentIP;
        private const string WEBSITE1 = "http://icanhazip.com";
        private const string WEBSITE2 = "http://checkip.dyndns.org/";
        private const string WEBSITE3 = "http://ifconfig.me/ip";
        private string websitesStr;
        public void CreateWindow()
        {
            //Quick delegate assignment for website/IP changes.
            //IPMonitor.UpdateWebsitesAction = CallWhenWebsitesChanged;
            Application.Init();

            myWin = new Gtk.Window("IP Checka");
            manager = new WindowComponentManager(myWin);
            InitializeTextFields();
            //Adds an initial test website.
            //Task.Factory.StartNew(() => Application.Run());
            Task.Factory.StartNew(() => IPMonitor.Run());
            Task.Factory.StartNew(() => IPMonitor.AddWebsite(WEBSITE1));
            Task.Factory.StartNew(() => IPMonitor.AddWebsite(WEBSITE2));
            ListenForShutdown();
            Task.Factory.StartNew(() => IPMonitor.AddWebsite(WEBSITE3));
            Application.Run();
        }
        public void InitializeTextFields()
        {
            //Quick delegate assignment for website/IP changes.
            IPMonitor.UpdateIPAction = CallWhenIPChanges;
            IPMonitor.UpdateWebsitesAction = CallWhenWebsitesChanged;
            VPN_Stability_Monitor.UpdateStabilityAction = CallWhenStabilityChanges;
        }
        public void ListenForShutdown()
        {
            /*
            Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        if (VPN_Stability_Monitor.shutdown)
                        {
                            Application curApp = Application.Current;
                            //curApp.Dispatcher.Invoke(curApp.Shutdown);
                            Thread.Sleep(200);
                            Process.Start("shutdown", "/s /t 0");
                        }
                    }
                }
            );
            */
        }

        public void CallWhenWebsitesChanged(WebsiteHashSet websites)
        {
            //Updates our UI with current websites we are considering to be used.
            websitesStr = "";
            foreach(string website in websites)
            {
                websitesStr += website + "\n";
            }
            manager.ChangeBuffer(manager.WebsiteField, websitesStr);
        }
        public void CallWhenIPChanges(string ip)
        {
            //Updates our UI with current IP + website being used.
            currentIP = ip;
            //CallWhenDisplayIPClicked();
            manager.ChangeBuffer(manager.CurrentIPField, currentIP);
        }
        public void CallWhenStabilityChanges(string stability)
        {
            manager.ChangeBuffer(manager.VPNStabilityField,stability);
        }
        public void CallWhenDisplayIPClicked()
        {
             var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = "-c notify-send " + currentIP,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            proc.WaitForExit();
        }
        public void DisplayTextRequest(string text)
        {
            //Display text as requested by IPMonitor.
        }
        public void ShowConnectionStats()
        {
            //TODO: displays speeds of all websites
        }
        public void DisplayWarningWhenTimedOut()
        {
            //TODO: displays a warning for timed out errors for active websites.
        }
        public void DisplayWarningWhenInvalidWebsite()
        {
            //TODO: displays warning when the website added does not resolve or is unreachable.
        }
    }
}

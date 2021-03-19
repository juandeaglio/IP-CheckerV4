using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace IP_Checker
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string WEBSITE1 = "http://icanhazip.com";
        public const string WEBSITE2 = "http://checkip.dyndns.org/";
        private const string WEBSITE3 = "http://ifconfig.me/ip";
        private IPMonitor ipMonitor;
        private VPN_Stability_Monitor vpnMonitor;

        public MainWindow()
        {
            ipMonitor = new IPMonitor(new WebsiteTester(new HashSet<string>()));
            vpnMonitor = new VPN_Stability_Monitor(ipMonitor);
            InitializeComponent();
            InitializeTextFields();
            Task.Factory.StartNew(() => ipMonitor.Run());
            Task.Factory.StartNew(() => vpnMonitor.Run());

            //Shutdown app when called for (can be replaced with system shutdown)
            Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        if (vpnMonitor.shutdown)
                        {
                            Application curApp = Application.Current;
                            //curApp.Dispatcher.Invoke(curApp.Shutdown);
                            Thread.Sleep(200);
                            Process.Start("shutdown", "/s /t 0");
                        }
                    }
                }
            );
            this.Dispatcher.Invoke(() => ipMonitor.AddWebsite(WEBSITE1));
            this.Dispatcher.Invoke(() => ipMonitor.AddWebsite(WEBSITE2));
            this.Dispatcher.Invoke(() => ipMonitor.AddWebsite(WEBSITE3));
        }
        public void InitializeTextFields()
        {
            //Quick delegate assignment for website/IP changes.
            ipMonitor.UpdateIPFieldAction = CallWhenIPChanges;
            ipMonitor.UpdateWebsitesAction = CallWhenWebsitesChanged;
            vpnMonitor.UpdateStabilityAction = CallWhenStabilityChanges;
        }
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (!Website_Field.Text.Equals(""))
            {
                this.Dispatcher.Invoke(() => ipMonitor.AddWebsite(Website_Field.Text));
            }
        }
        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (!Website_Field.Text.Equals(""))
            {
                this.Dispatcher.Invoke(() => ipMonitor.RemoveWebsite(Website_Field.Text));
            }
        }
        public void CallWhenWebsitesChanged(HashSet<string> websites)
        {
            //Updates our UI with current websites we are considering to be used.
            string websitesStr;
            websitesStr = "";
            foreach (string website in websites)
            {
                websitesStr += website + "\n";
            }
            CurrentWebsites_Field.Text = websitesStr;
        }
        public void CallWhenIPChanges(string ip)
        {
            //Updates our UI with current IP + website being used.
            this.Dispatcher.Invoke(() => IP_Value.Text = ip);
        }
        public void CallWhenStabilityChanges(string stability)
        {
            this.Dispatcher.Invoke(() => VPNStability_Field.Text = stability);
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

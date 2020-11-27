using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        public MainWindow()
        {
            InitializeComponent();
            InitializeTextFields();
            //TODO: Implement IP/VPN logic.
            Task.Factory.StartNew(() => IPMonitor.Run());
            IPMonitor.AddWebsite(WEBSITE1);
            Task.Factory.StartNew(() => VPN_Stability_Monitor.Run());
            
            //Shutdown app when called for (can be replaced with system shutdown)
            Task.Factory.StartNew(() =>
                {
                    while (true)
                        if (VPN_Stability_Monitor.shutdown)
                        {
                            Application curApp = Application.Current;
                            curApp.Dispatcher.Invoke(curApp.Shutdown);
                        }
                }
            );

        }
        public void InitializeTextFields()
        {
            //Quick delegate assignment for website/IP changes.
            IPMonitor.UpdateIPAction = CallWhenIPChanges;
            IPMonitor.UpdateWebsitesAction = CallWhenWebsitesChanged;
            VPN_Stability_Monitor.UpdateStabilityAction = CallWhenStabilityChanges;
        }
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (!Website_Field.Text.Equals(""))
            {
                IPMonitor.AddWebsite(Website_Field.Text);
            }
        }
        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (!Website_Field.Text.Equals(""))
            {
                IPMonitor.RemoveWebsite(Website_Field.Text);
            }
        }
        public void CallWhenWebsitesChanged(HashSet<string> websites)
        {
            //Updates our UI with current websites we are considering to be used.
            string websitesStr;
            websitesStr = "";
            foreach(string website in websites)
            {
                websitesStr += website + "\n";
            }
            CurrentWebsites_Field.Text = websitesStr;
        }
        public void CallWhenIPChanges(string ip)
        {
            //Updates our UI with current IP + website being used.
            this.Dispatcher.Invoke(() => IP_Value.Text = ip );
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

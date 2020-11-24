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
        public string websiteFieldText;
        private string currentIP;
        private const string WEBSITE1 = "http://icanhazip.com";
        private const string WEBSITE2 = "http://checkip.dyndns.org/";
        private const string WEBSITE3 = "http://ifconfig.me/ip";
        private string websitesStr;

        public MainWindow()
        {
            //Quick delegate assignment for website/IP changes.
            IPMonitor.UpdateIPAction = CallWhenIPChanges;
            IPMonitor.UpdateWebsitesAction = CallWhenWebsitesChanged;
            InitializeComponent();
            //Adds an initial test website.
            Task.Factory.StartNew(() => IPMonitor.Run());
            IPMonitor.AddWebsite("http://icanhazip.com");
        }
        private void CheckIP_Click(object sender, RoutedEventArgs e)
        {

            currentIP = IPMonitor.currentIP;
            //IP_Value.Text = currentIP;
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
            currentIP = ip;
            this.Dispatcher.Invoke(() => { IP_Value.Text = currentIP; });
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

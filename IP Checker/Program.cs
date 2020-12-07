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
        Label ipLabel;
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

            VBox box1 = new VBox(false, 10);
            myWin.Add(box1);
            box1.Show();

            VBox box2 = new VBox(false, 10);
            box2.BorderWidth = 10;
            box1.PackStart(box2, true, true, 0);
            myWin.Add(box2);
            box2.Show();

            Button button1 = new Button("Add Website");
            box2.PackStart(button1, true, true, 0);
            button1.Show();

            Button button2 = new Button("Remove Website");
            box2.PackStart(button2, true, true, 0);
            button2.Show();

            GUICreateCurrentIPField(box1);
            GUICreateWebsitesField(box1);

            myWin.ShowAll();

            IPMonitor.UpdateIPAction = CallWhenIPChanges;
            IPMonitor.UpdateWebsitesAction = CallWhenWebsitesChanged;
            //Adds an initial test website.
            //Task.Factory.StartNew(() => Application.Run());
            Task.Factory.StartNew(() => IPMonitor.Run());
            Task.Factory.StartNew(() => IPMonitor.AddWebsite("http://icanhazip.com"));
            Application.Run();
        }
        private void GUICreateCurrentIPField(Box parentBox)
        {          
            HSeparator separator1 = new HSeparator();
            parentBox.PackStart(separator1, false, true, 0);
            separator1.Show();
            VBox box = CreateVBox(parentBox);
            CreateBoxLabel(box, "Current IP:");
            
            manager.CurrentIPField = new TextBuffer(new TextTagTable());
            
            CreateTextView(box, manager.CurrentIPField);
        }

        private void GUICreateWebsitesField(Box parentBox)
        {
            VBox box = CreateVBox(parentBox);
            VSeparator separator2 = new VSeparator();
            box.PackStart(separator2, false, true, 0);
            separator2.Show();
            CreateBoxLabel(box, "Websites List:");

            manager.WebsiteField = new TextBuffer(new TextTagTable());
            CreateTextView(box, manager.WebsiteField);
        }
        private Label CreateBoxLabel(Box parentBox, string labelText)
        {
            Label label = new Label(labelText);
            parentBox.PackStart(label, false, true, 0);
            return label;
        }
        private TextView CreateTextView(Box parentBox, TextBuffer buffer)
        {
            TextView textView = new TextView(buffer);
            parentBox.PackStart(textView, true, true, 0);
            textView.Show();
            return textView;
        }
        private VBox CreateVBox(Box parentBox)
        {
            VBox box = new VBox(false, 10);
            box.BorderWidth = 10;
            parentBox.PackStart(box, false, true, 0);
            box.Show();
            return box;
        }
        private HBox CreateHBox(Box parentBox)
        {
            HBox box = new HBox(false, 10);
            box.BorderWidth = 10;
            parentBox.PackStart(box, false, true, 0);
            box.Show();
            return box;
        }
        private void Add_Click(object sender, EventArgs e)
        {
            //if (!Website_Field.Text.Equals(""))
            {
            //    IPMonitor.AddWebsite(Website_Field.Text);
            }
        }
        private void Remove_Click(object sender, EventArgs e)
        {
            //if (!Website_Field.Text.Equals(""))
            {
            //    IPMonitor.RemoveWebsite(Website_Field.Text);
            }
        }
        public void CallWhenWebsitesChanged(HashSet<string> websites)
        {
            //Updates our UI with current websites we are considering to be used.
            websitesStr = "";
            if(!running2)
            {
                running2 = true;
                Thread.Sleep(3000);
                foreach(string website in websites)
                {
                    websitesStr += website + "\n";
                }
                manager.WebsiteField.Text = websitesStr;
                running2 = false;
            }
        }
        public void CallWhenIPChanges(string ip)
        {
            //Updates our UI with current IP + website being used.
            currentIP = ip;
            if(!running)
            {
                running = true;
                Thread.Sleep(3000);
                //CallWhenDisplayIPClicked();
                manager.CurrentIPField.Text = currentIP;
                running = false;
            }
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

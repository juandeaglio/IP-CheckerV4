using Gtk;
using Gdk;
using System;
namespace IP_Checker
{
    //Makes new components for our GTK gui, saves a lot of space of declaration and changing fields so that they are not unreadable and cumbersome to code.
    public class WindowComponentManager
    {
        public Gtk.Window currentWindow;
        private Gtk.TextBuffer newWebsiteField;
        private Gtk.TextBuffer websiteField;
        private Gtk.TextBuffer currentIPField;
        private Gtk.TextBuffer vpnStabilityField;
        public Gtk.TextBuffer NewWebsiteField
        {
            get => newWebsiteField;
        }
        public Gtk.TextBuffer WebsiteField
        {
            get => websiteField;
        }
        public Gtk.TextBuffer CurrentIPField
        {
            get => currentIPField;
        }
        public Gtk.TextBuffer VPNStabilityField
        {
            get => vpnStabilityField;
        }
        public WindowComponentManager(Gtk.Window gtkWin)
        {
            currentWindow = new Gtk.Window("IP Checker Window");
            
            VBox box1 = CreateVBox(null);
            currentWindow.Add(box1);
            VBox box2 = CreateVBox(box1);
            currentWindow.Add(box2);
            Button addButton = CreateButton(box2, "Add Website");
            Button removeButton = CreateButton(box2, "Remove Website");
            addButton.Clicked += Add_Click;
            removeButton.Clicked  += Remove_Click;

            newWebsiteField = new TextBuffer(new TextTagTable());
            CreateTextView(box2, newWebsiteField, true);

            GUICreateCurrentIPField(box1);
            GUICreateWebsitesField(box1);

            currentWindow.ShowAll();
        }
        private void GUICreateCurrentIPField(Box parentBox)
        {          
            HSeparator separator1 = new HSeparator();
            parentBox.PackStart(separator1, false, true, 0);
            separator1.Show();
            VBox box = CreateVBox(parentBox);
            CreateBoxLabel(box, "Current IP:");
            
            currentIPField = new TextBuffer(new TextTagTable());
            
            CreateTextView(box, currentIPField, false);
        }

        private void GUICreateWebsitesField(Box parentBox)
        {
            VBox box = CreateVBox(parentBox);
            VSeparator separator2 = new VSeparator();
            box.PackStart(separator2, false, true, 0);
            separator2.Show();
            CreateBoxLabel(box, "Websites List:");

            websiteField = new TextBuffer(new TextTagTable());
            CreateTextView(box, websiteField, false);
        }
        private Label CreateBoxLabel(Box parentBox, string labelText)
        {
            Label label = new Label(labelText);
            parentBox.PackStart(label, false, true, 0);
            return label;
        }
        private TextView CreateTextView(Box parentBox, TextBuffer buffer, bool editable)
        {
            TextView textView;
            if(!editable)
                textView = new UneditableTextView(buffer);
            else
                textView = new TextView(buffer);
            parentBox.PackStart(textView, true, true, 0);
            textView.Show();
            return textView;
        }
        private Button CreateButton(Box parentBox, string buttonLabel)
        {
            Button button = new Button(buttonLabel);
            parentBox.PackStart(button, false, true, 0);
            return button;
        }
        private void Add_Click(object sender, EventArgs e)
        {
            if (!NewWebsiteField.Text.Equals(""))
                IPMonitor.AddWebsite(NewWebsiteField.Text);
        }
        private void Remove_Click(object sender, EventArgs e)
        {
            if (!NewWebsiteField.Text.Equals(""))
                IPMonitor.RemoveWebsite(NewWebsiteField.Text);
        }
        private VBox CreateVBox(Box parentBox)
        {
            VBox box = new VBox(false, 10);
            box.BorderWidth = 10;
            if(parentBox != null)
                parentBox.PackStart(box, false, true, 0);
            box.Show();
            return box;
        }
        private HBox CreateHBox(Box parentBox)
        {
            HBox box = new HBox(false, 10);
            box.BorderWidth = 10;
            if(parentBox != null)
                parentBox.PackStart(box, false, true, 0);
            box.Show();
            return box;
        }
        public void ChangeBuffer(TextBuffer buffer, string toStr)
        {
            Gdk.Threads.AddIdle(1, () => 
            {
                buffer.Text = toStr;
                return false;
            });
        }
    }
}
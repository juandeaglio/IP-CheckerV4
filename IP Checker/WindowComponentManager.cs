using Gtk;
using Gdk;
namespace IP_Checker
{
    //Makes new components for our GTK gui, saves a lot of space of declaration and changing fields so that they are not unreadable and cumbersome to code.
    public class WindowComponentManager
    {
        public Gtk.Window currentWindow;
        public Gtk.TextBuffer WebsiteField{get;set;}
        public Gtk.TextBuffer CurrentIPField{get;set;}
        
        public WindowComponentManager(Gtk.Window gtkWin)
        {
            currentWindow = new Gtk.Window("IP Checker Window");
            
            VBox box1 = new VBox(false, 10);
            currentWindow.Add(box1);
            box1.Show();

            VBox box2 = new VBox(false, 10);
            box2.BorderWidth = 10;
            box1.PackStart(box2, true, true, 0);
            currentWindow.Add(box2);
            box2.Show();

            Button button1 = new Button("Add Website");
            box2.PackStart(button1, true, true, 0);
            button1.Show();

            Button button2 = new Button("Remove Website");
            box2.PackStart(button2, true, true, 0);
            button2.Show();

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
            
            CurrentIPField = new TextBuffer(new TextTagTable());
            
            CreateTextView(box, CurrentIPField);
        }

        private void GUICreateWebsitesField(Box parentBox)
        {
            VBox box = CreateVBox(parentBox);
            VSeparator separator2 = new VSeparator();
            box.PackStart(separator2, false, true, 0);
            separator2.Show();
            CreateBoxLabel(box, "Websites List:");

            WebsiteField = new TextBuffer(new TextTagTable());
            CreateTextView(box, WebsiteField);
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
    }
}
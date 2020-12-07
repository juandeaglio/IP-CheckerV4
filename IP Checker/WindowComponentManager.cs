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
        }
    }
}
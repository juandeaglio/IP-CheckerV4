using Gtk;
namespace IP_Checker
{
    public class UneditableTextView : TextView
    {
        public UneditableTextView(TextBuffer buffer) : base(buffer)
        {
            base.Editable = false;
        }
    }
}
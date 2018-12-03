using System.Windows; 
using System.Windows.Documents;

namespace FluidNGPermissionGranter
{
    public partial class LogWindow : Window
    {
        public static string RichText = ">Log";

        public LogWindow()
        {
            InitializeComponent();
            Console.AppendText(RichText);
        }

        // Removing icon on the top of window 
        private void Window_SourceInitialized(object sender, System.EventArgs e)
        {
            IconHelper.RemoveIcon(this);
        }

        /*
        private void TextBox_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return && !(input.Text == "")) // check on Enter keyboard button and is Input field not null 
            {
                console.AppendText("\rInput: " + input.Text + "" + MainWindow.SendToADB(input.Text));
                input.Clear();
            }
        }
        */

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            RichText = new TextRange(Console.Document.ContentStart, Console.Document.ContentEnd).Text;
        }
    }
}
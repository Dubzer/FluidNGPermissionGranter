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
using System.Windows.Shapes;
using SharpAdbClient;

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
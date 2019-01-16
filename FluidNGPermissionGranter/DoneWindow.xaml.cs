using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace FluidNGPermissionGranter
{

    public partial class DoneWindow : Window
    {
        public DoneWindow()
        {
            InitializeComponent();
        }
        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            IconHelper.RemoveIcon(this);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainWindow.StopApplication();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.StopApplication();
        }
    }
}

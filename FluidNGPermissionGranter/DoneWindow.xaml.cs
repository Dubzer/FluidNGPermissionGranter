using System;
using System.Windows;

namespace FluidNGPermissionGranter
{

    public partial class DoneWindow
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

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
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FluidNGPermissionGranter
{
    public partial class ADBGuide
    {
        public ADBGuide()
        {
            InitializeComponent();
        }

        private void PixelButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://tgraph.io/Enable-developer-options-and-debugging-11-24-2");
        }

        private void SamsungButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.syncios.com/android/how-to-debug-samsung-galaxy-s9.html");
        }

        private void HuaweiButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.syncios.com/android/how-to-debug-huawei-honor-9.html");
        }

        private void XiaomiButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.syncios.com/android/how-to-debug-huawei-honor-9.html");
        }

        private void OtherButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://tgraph.io/Enable-developer-options-and-debugging-11-24"); 
        }
    }
}

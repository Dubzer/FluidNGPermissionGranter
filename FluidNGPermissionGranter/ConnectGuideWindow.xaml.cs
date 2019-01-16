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

namespace FluidNGPermissionGranter
{
    /// <summary>
    /// Логика взаимодействия для ConnectGuide.xaml
    /// </summary>
    public partial class ConnectGuide : Window
    {
        public ConnectGuide()
        {
            InitializeComponent();
        }
        private void Window_SourceInitialized(object sender, System.EventArgs e)
        {
            IconHelper.RemoveIcon(this);
        }
    }
}

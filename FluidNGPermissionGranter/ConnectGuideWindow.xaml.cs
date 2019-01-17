using System.Windows;

namespace FluidNGPermissionGranter
{
    /// <summary>
    /// Логика взаимодействия для ConnectGuide.xaml
    /// </summary>
    public partial class ConnectGuide
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

using System;

namespace FluidNGPermissionGranter
{
    public partial class AuthorizeWindow
    {
        public AuthorizeWindow()
        {
            InitializeComponent();
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            IconHelper.RemoveIcon(this);
        }
    }
}

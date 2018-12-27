using System;

namespace FluidNGPermissionGranter
{
    public partial class AllowADBWindow
    {
        public AllowADBWindow()
        {
            InitializeComponent();
        }

        private void Window_SourceInitialized(object sender, System.EventArgs e)
        {
            IconHelper.RemoveIcon(this);
        }

        internal void SetPropertyThreadSafe(Func<object> p, object status)
        {
            throw new NotImplementedException();
        }
    }
}

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
    }
}

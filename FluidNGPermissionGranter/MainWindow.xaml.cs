using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows;
using SharpAdbClient;

namespace FluidNGPermissionGranter
{
    public partial class MainWindow : Window
    {
        private AdbServer server = new AdbServer();
        private string adbPath;
        private LogWindow logWindow;
        private ADBGuide adbGuideWindow;

        public MainWindow()
        {
            InitializeComponent();
            adbPath = (System.IO.Directory.GetCurrentDirectory() + @"\adb\adb.exe"); // Getting path of ADB tools 
        }

        // Closing adb server after closing program
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (Process proc in Process.GetProcessesByName("adb"))
            {
                proc.Kill();
            }
        }
        #region Button actions 

        private void GuideButton_Click(object sender, RoutedEventArgs e)
        {
            if (adbGuideWindow == null)
            {
                adbGuideWindow = new ADBGuide();
                adbGuideWindow.Show();
            }
            else
            {
                //  Bad code that I have to change later
                adbGuideWindow.Close();
                adbGuideWindow = new ADBGuide();
                adbGuideWindow.Show();
                adbGuideWindow.Focus();
            }
        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            StartADB();
            FindDevice();
        }

        private void GrantButton_Click(object sender, RoutedEventArgs e)
        {
            SendToADB("pm grant com.fb.fluid android.permission.WRITE_SECURE_SETTINGS");
        }
        private void LogButton_Click(object sender, RoutedEventArgs e)
        {
            if (logWindow == null)
            {
                logWindow = new LogWindow();
                logWindow.Show();
            }
            else
            {
                //  Bad code that I have to change later
                logWindow.Close();
                logWindow = new LogWindow();
                logWindow.Show();
                logWindow.Focus();
            }
        }
        #endregion
        private void StartADB()
        {
            // Trying to start ADB server and find device 
            try
            {
                var result = server.StartServer(adbPath, restartServerIfNewer: false);
                LogWindow.richText += "\nADB server started successful: " + result;
            }
            catch
            {
                MessageBox.Show("Can't start ADB server. Please check Log and ask developer for it");
                LogWindow.richText += "Can't start ADB server";
            }
        }

        //  Trying to find device and show message about it 
        private void FindDevice()
        {
            try
            {
                var devices = AdbClient.Instance.GetDevices();
                foreach (var device in devices)
                {
                    MessageBox.Show("The device has been found. Your device is: " + device.Name, "Message");
                    grantButton.IsEnabled = true;
                }
            }
            catch
            {
                MessageBox.Show("Something go wrong. Please check your connection with device.", "Message");
            }
        }

        private void StartDeviceMonitor()
        {
            var monitor = new DeviceMonitor(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)));
            monitor.DeviceConnected += OnDeviceConnected;
            monitor.Start();
        }

        private void OnDeviceConnected(object sender, DeviceDataEventArgs e)
        {
            MessageBox.Show("The device has been found. Your device is: " + e.Device.Name);
        }

        public static string SendToADB(string command)
        {
            var device = AdbClient.Instance.GetDevices().First();
            var receiver = new ConsoleOutputReceiver();
            AdbClient.Instance.ExecuteRemoteCommand(command, device, receiver);
            MessageBox.Show("Done! Restart application to see the changes", "Congratulations");
            return receiver.ToString();
        }
    }
}
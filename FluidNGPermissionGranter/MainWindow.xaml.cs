using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using SharpAdbClient;

namespace FluidNGPermissionGranter
{
    public partial class MainWindow
    {
        private readonly AdbServer server = new AdbServer();
        private readonly string adbPath;
        private ADBGuide adbGuideWindow;
        private AllowADBWindow allowWindow;
        private DeviceMonitor monitor;

        public MainWindow()
        {
            InitializeComponent();
            adbPath = (System.IO.Directory.GetCurrentDirectory() + @"\adb\adb.exe"); // Getting path of ADB tools 
        }

        // Closing adb server after closing program
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                foreach (var proc in System.Diagnostics.Process.GetProcessesByName("adb"))
                {
                    proc.Kill();
                }
            }
            catch
            {
                Application.Current.Shutdown();
            }
            Application.Current.Shutdown();
        }

        // Removing icon on the top of window 
        protected override void OnSourceInitialized(EventArgs e)
        {
            IconHelper.RemoveIcon(this);
        }

        #region Button actions 

        private void GuideButton_Click(object sender, RoutedEventArgs e)
        {
            adbGuideWindow = new ADBGuide { ShowInTaskbar = false, Owner = Application.Current.MainWindow };
            adbGuideWindow.ShowDialog();
        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            StartAdb(); //  Start ADB server
            System.Collections.Generic.List<DeviceData> devices = AdbClient.Instance.GetDevices();  //  Getting list of connected devices
            //  If device connected - just find it
            if (devices != null && devices.Count != 0)
            {
                StartDeviceMonitor();
            }
            //  If device is not connected - then show help window 
            else
            {
                allowWindow = new AllowADBWindow { Title = "Help", Owner = Application.Current.MainWindow };
                allowWindow.Show();
                StartDeviceMonitor();
            }
        }

        private void GrantButton_Click(object sender, RoutedEventArgs e)
        {
            string output = SendToAdb("pm grant com.fb.fluid android.permission.WRITE_SECURE_SETTINGS");    //  Trying to grant permission 
            //  If output is *nothing*, then it means that there is no any erroФrs, and we can show Successful window
            if (output == "")    
            {
                MessageBox.Show("Done! Now you can hide navigation bar", "Successful");
                //  Restarting Fluid Navigation Gestures app on device
                SendToAdb("am force-stop com.fb.fluid");
                SendToAdb("am start -n com.fb.fluid/com.fb.fluid.ActivityMain");
            }
            else
            {
                MessageBox.Show("Can't grant permission. Please check your connection and try again. \nError: " + output);
            }
        }
        #endregion
        private void StartAdb()
        {
            // Trying to start ADB server and find device 
            try
            {
                server.StartServer(adbPath, false);
            }
            catch
            {
                MessageBox.Show("Can't start ADB server. Please check Log and ask developer for it");
                //LogWindow.RichText += "Can't start ADB server";
            }
        }

        private static string SendToAdb(string command)
        {
            try
            {
                var device = AdbClient.Instance.GetDevices().First();
                var receiver = new ConsoleOutputReceiver();
                AdbClient.Instance.ExecuteRemoteCommand(command, device, receiver);
                return receiver.ToString();
            }
            catch { return "error"; } 
        }

        private void StartDeviceMonitor()
        {
            if(monitor == null)
            {
                monitor = new DeviceMonitor(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)));
                monitor.DeviceConnected += this.OnDeviceConnected;
                monitor.Start();
            }
        }

        private void OnDeviceConnected(object sender, DeviceDataEventArgs e)
        {
            MessageBox.Show("The device has been found." );
        }

        private void OnAllowWindowLoaded()
        {
            allowWindow.Close();
            allowWindow.Loaded += null;
        }
    }
}   
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
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
        private event Action CloseHelpWindow = delegate { };

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
            //  If device is not connected => Show help window 
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
                MessageBox.Show("Done! Now you can hide navigation bar", "Success");

                //  Restarting Fluid Navigation Gestures app on device
                SendToAdb("am force-stop com.fb.fluid");
                SendToAdb("am start -n com.fb.fluid/com.fb.fluid.ActivityMain");
            }
            else
            {
                MessageBox.Show(output);
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
            catch { return "Can`t send command by ADB. Please check your connection and try again. Also try to redo second step."; } 
        }

        //  This thing searching for device every time after started 
        private void StartDeviceMonitor()
        {
            //  If device monitor is not started =>
            if (monitor == null)
            {
                //  Starting device monitor here 
           
                monitor = new DeviceMonitor(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)));
                monitor.DeviceConnected += this.OnDeviceConnected;

                monitor.Start();
            }
        }
        
        //  What happens after device connected
        private void OnDeviceConnected(object sender, DeviceDataEventArgs e)
        {
            //  If allowWindow exists => Close it when device connected 
            if (allowWindow != null)
            {
                //  Shitty code (as usual), but it's worse than before, so I have to change it (and never change)
                allowWindow.Dispatcher.BeginInvoke(new ThreadStart(() => allowWindow.Close()));
            }
            MessageBox.Show("The device has been found");
            Dispatcher.BeginInvoke(new ThreadStart(() => GrantButton.IsEnabled = true));
        }
    }
}
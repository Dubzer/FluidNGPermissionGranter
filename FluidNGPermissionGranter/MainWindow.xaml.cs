using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using SharpAdbClient;

namespace FluidNGPermissionGranter
{
    public partial class MainWindow
    {
        private readonly AdbServer server = new AdbServer();
        private readonly string adbPath;
        private ADBGuide adbGuideWindow;
        private AllowADBWindow allowADBWindow;
        private ConnectGuide connectWindow;
        private DeviceMonitor monitor;
        private DoneWindow doneWindow;

        public MainWindow()
        {
            InitializeComponent();
            adbPath = (System.IO.Directory.GetCurrentDirectory() + @"\adb\adb.exe"); // Getting path of ADB tools \
            
        }

        // Closing adb server after closing program
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopApplication();
        }

        public static void StopApplication()
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
                Environment.Exit(0);
            }
            Environment.Exit(0);
        }

        // Removing icon on the top of window 
        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            IconHelper.RemoveIcon(this);
        }

        #region Button actions 

        private void GuideButton_Click(object sender, RoutedEventArgs e)
        {
            adbGuideWindow = new ADBGuide { ShowInTaskbar = false, Owner = Application.Current.MainWindow };
            adbGuideWindow.ShowDialog();
        }

        private void GrantButton_Click(object sender, RoutedEventArgs e)
        {
            StartAdb(); //  Start ADB server
            try
            {
                System.Collections.Generic.List<DeviceData> devices = AdbClient.Instance.GetDevices();  //  Getting list of connected devices
                //  If device connected - just find it
                if (devices != null && devices.Count != 0)
                {
                    StartDeviceMonitor();
                }
                //  If device is not connected => Show help window 
                else
                {
                    connectWindow = new ConnectGuide{ Title = "", Owner = Application.Current.MainWindow };
                    StartDeviceMonitor();
                    connectWindow.ShowDialog();
                }
            }
            catch (Exception exception)
            {
                MessageBoxResult result = MessageBox.Show("Some error detected. Please, send screenshot with this text to developer. Do you want to send now?: \n \n" + exception,"Error", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    Process.Start("https://dubzer.github.io");
                }
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
            catch(Exception exception)
            {
                MessageBoxResult result = MessageBox.Show("Can`t send command by ADB. Please check your connection and try again. Also, try to redo the second step. Moreover, you can ask for it to the developer: \n \n" + exception, "Error", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    Process.Start("https://dubzer.github.io");
                }
                return "error";
            }
        }

        private void GrantPermission()
        {
            string output = SendToAdb("pm grant com.fb.fluid android.permission.WRITE_SECURE_SETTINGS");    //  Trying to grant permission 
            //  If output is *nothing*, then it means that there is no any errors, and we can show Successful window
            if (output == "")
            {
                doneWindow = new DoneWindow() { Title = "", Owner = Application.Current.MainWindow };
                doneWindow.ShowDialog();
                //  Restarting Fluid Navigation Gestures app on device
                SendToAdb("am force-stop com.fb.fluid");
                SendToAdb("am start -n com.fb.fluid/com.fb.fluid.ActivityMain");
            }
            else
            {
                MessageBox.Show("Unexpected output: \n \n + output");
            }
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
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                if (connectWindow != null)
                {
                    connectWindow.Close();
                    connectWindow = null;
                }

                switch (AdbClient.Instance.GetDevices().First().State)
                {
                    case DeviceState.Online:
                        try
                        {
                            GrantPermission();
                        }
                        catch (Exception exception)
                        {
                            MessageBox.Show(exception.ToString());
                            throw;
                        }
                        break;
                    case DeviceState.Unauthorized:
                        //Showing help window to authorize adb server 
                        allowADBWindow = new AllowADBWindow();
                        allowADBWindow.Show();
                        while (allowADBWindow.IsLoaded == false) { }    //  Waiting for loading window 
                        while (AdbClient.Instance.GetDevices().First().State != DeviceState.Online) { } //  Waiting to get Online phone 
                        allowADBWindow.Close();
                        GrantButton.IsEnabled = true;
                        try
                        {
                            GrantPermission();
                        }
                        catch (Exception exception)
                        {
                            MessageBox.Show(exception.ToString());
                            throw;
                        }
                        break;
                }
            }));
        }

    }
}

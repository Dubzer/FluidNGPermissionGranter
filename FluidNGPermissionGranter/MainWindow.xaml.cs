using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using SharpAdbClient;

namespace FluidNGPermissionGranter
{
    public partial class MainWindow
    {
        #region ADB stuff

        private readonly AdbServer server = new AdbServer();
        private readonly string adbPath;
        private DeviceMonitor monitor;  //  Device monitor. Its searching for device after creating 

        #endregion

        #region Windows

        private ADBGuide adbGuideWindow;    //  There you can find guide about activating ADB on phone
        private ConnectGuide connectWindow;     // That's a connect message 
        private AuthorizeWindow authorizeWindow;      // That's an authorize adb server message
        private DoneWindow doneWindow;      //  This window appears when everything is done

        #endregion

        private readonly Action phoneAuthorized;
        private static DeviceData device;
        private Thread authorizationCheckThread;

        public MainWindow() 
        {
            InitializeComponent();
            adbPath = (System.IO.Directory.GetCurrentDirectory() + @"\adb\adb.exe"); // Getting path of ADB tools 
            phoneAuthorized += AfterAuthorization;  // Setiing method for action 
        }

        // Removing icon on the top of the window 
        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            IconHelper.RemoveIcon(this);
        }

        #region Button methods 

        private void GuideButton_Click(object sender, RoutedEventArgs e)
        {
            OpenGuideWindow();
        }

        private void GrantButton_Click(object sender, RoutedEventArgs e)
        {
            StartAdb();
            try
            {
                var devices = AdbClient.Instance.GetDevices();
                if (devices != null && devices.Count != 0)
                { 
                    StartDeviceMonitor(monitor);
                }
                else
                {
                    OpenConnectWindow();
                    StartDeviceMonitor(monitor);
                }
            }
            catch (Exception exception)
            {
                MessageBoxResult result = MessageBox.Show("Some error detected. Please, send screenshot with this text to developer. Do you want to send now?: \n \n" + exception,"Error", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes) { Process.Start("https://dubzer.github.io"); }
            }
        }

        #endregion

        #region Windows methods

        private void OpenGuideWindow()
        {
            adbGuideWindow = new ADBGuide { ShowInTaskbar = false, Owner = Application.Current.MainWindow };
            adbGuideWindow.ShowDialog();
        }

        private void OpenConnectWindow()
        {
            connectWindow = new ConnectGuide { Title = "", Owner = Application.Current.MainWindow };
            connectWindow.ShowDialog();
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
                MessageBox.Show(File.Exists(adbPath)
                    ? "Can't start ADB server. Please, check Log and ask developer for it"
                    : "Can't find ADB server EXE. Please, redownload app. ");
            }
        }

        //  This thing searching for device every time after started 
        private void StartDeviceMonitor(DeviceMonitor monitor)
        {
            //  If device monitor is not started =>
            if (monitor == null)
            {
                //  Starting device monitor here 

                monitor = new DeviceMonitor(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)));
                monitor.DeviceConnected += OnDeviceConnected;

                monitor.Start();
            }
            else
            {
                monitor = null;
                monitor = new DeviceMonitor(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)));
                monitor.DeviceConnected += OnDeviceConnected;

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
                        device = AdbClient.Instance.GetDevices().First();
                        Debug.Print(device.GetType().ToString());


                        //Showing help window to authorize PC
                        authorizeWindow = new AuthorizeWindow();
                        
                        authorizeWindow.Show();
                        AdbClient.Instance.GetDevices();

                        authorizeWindow.ContentRendered += AuthorizeWindowContentRendered;
                        break;
                    default:
                        OpenConnectWindow();
                        break;
                }
            }));
        }

        private void AuthorizeWindowContentRendered(object sender, EventArgs e)
        {
            authorizationCheckThread = new Thread(StartCheckForAuthorization);
            authorizationCheckThread.Start();
        }

        private void StartCheckForAuthorization()
        {
            //  Waiting to get Online phone
            while (AdbClient.Instance.GetDevices().Any() &&
                   AdbClient.Instance.GetDevices().First().State == DeviceState.Unauthorized)
            {
                Thread.Sleep(300);
            }

            phoneAuthorized();
        }

        private void AfterAuthorization()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => { authorizeWindow.Close(); }));

            string grantOutput = GrantPermission();

            if (grantOutput == "success")
            {
                //  Showing "done" window 
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    doneWindow = new DoneWindow { Title = "", Owner = Application.Current.MainWindow };
                    doneWindow.ShowDialog();

                }));
            }
            else
            {
                MessageBoxResult result = MessageBox.Show("Can`t send command by ADB. Please check your connection and try again. Also, try to redo the second step. Moreover, you can ask for help to the developer: \n \n" + grantOutput, "Error", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    Process.Start("https://dubzer.github.io");
                }
            }

            authorizeWindow.ContentRendered -= AuthorizeWindowContentRendered;
        }

        private static string SendToAdb(string command)
        {
            //  Trying to send command to device via ADB
            try
            {
                device = AdbClient.Instance.GetDevices().First();
                var receiver = new ConsoleOutputReceiver();
                AdbClient.Instance.ExecuteRemoteCommand(command, device, receiver);
                return receiver.ToString();
            }
            // if can't => show error message and give a link to contact developer
            catch (Exception exception)
            {
                return exception.ToString();
            }
        }

        private string GrantPermission()
        {
            string output = SendToAdb("pm grant com.fb.fluid android.permission.WRITE_SECURE_SETTINGS");    //  Trying to grant permission 
            //  If output is *nothing*, then it means that there is no any errors, and we can show Successful window
            if (output == "")
            {
                //  Restarting Fluid Navigation Gestures app on device
                SendToAdb("am force-stop com.fb.fluid");
                SendToAdb("am start -n com.fb.fluid/com.fb.fluid.ActivityMain");
                return "success";
            }

            return output;
        }

        // Stopping ADB server after closing program
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            authorizationCheckThread.Abort();
            #region closing all windows

            if (adbGuideWindow != null)
            {
                adbGuideWindow.Close();
                adbGuideWindow = null;
            }
            else if (connectWindow != null)
            {
                connectWindow.Close();
                connectWindow = null;
            }
            else if (authorizeWindow != null)
            {
                authorizeWindow.Close();
                authorizeWindow = null;
            }
            else if (doneWindow != null)
            {
                doneWindow.Close();
                doneWindow = null;
            }

            #endregion

            StopApplication();
        }

        public static void StopApplication()
        {
            //  Trying to kill ADB Server process 
            try
            {
                foreach (var proc in Process.GetProcessesByName("adb"))
                {
                    proc.Kill();
                }
            }
            //  If can't => just closing app
            catch
            {
                Environment.Exit(0);
            }
            Environment.Exit(0);
        }
    }
}
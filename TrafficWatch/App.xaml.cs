using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using TrafficWatch.Properties;
using TrafficWatch.Services.Detail;

namespace TrafficWatch
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Mutex _mMutex;
        private readonly PortProcessMap portProcessMap = PortProcessMap.GetInstance();
        public bool screenLengthMaxOne = false;
        //public System.Windows.Forms.MenuItem menuExit, menuEdgeHide, menuShowTrayIcon, menuStartOnBoot, menuTransparency, menuAutoUpdate, menuCheckUpdate, menuAbout, History;
        private CaptureManager captureManager;
        private readonly UDMap udMap = new UDMap();
        int index = 0;
        public static History _History = new History();
        private readonly System.Timers.Timer timer = new System.Timers.Timer
        {
            Interval = 1000,
            AutoReset = true
        }; private View.WelcomeWindow welcomeWindow;
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //MessageBox.Show("App");
            //SystemThemeColors= App.Current.Resources.MergedDictionaries[0];
            var assembly = Assembly.GetExecutingAssembly();
            var mutexName = string.Format(CultureInfo.InvariantCulture, "Local\\{{{0}}}{{{1}}}", assembly.GetType().GUID, assembly.GetName().Name);

            _mMutex = new Mutex(true, mutexName, out bool mutexCreated);
            if (!mutexCreated)
            {
                _mMutex = null;
                Current.Shutdown();
                return;
            }
            if(TrafficWatch.Properties.Settings.Default.Startup)
                Program.SetStartup();
            Init();
            _Server = new Server();
            _Server.Start();
        }
        Server _Server;
        #region Show Trafick
        void Init()
        {
            // There is no instance until now.
            captureManager = new CaptureManager(udMap);
            welcomeWindow = new View.WelcomeWindow();
            welcomeWindow.Show();
            Thread t = new Thread(new ThreadStart(() =>
            {
                    //如果用户按的足够快，先按了exit，那么会先执行Exit，后执行captureManager.InitAndStart() !!! This is a bug, but it will not trigger unless user is really really fast !!!.
                    if (!captureManager.InitAndStart())
                {
                    Dispatcher.InvokeAsync(new Action(() =>
                    {
                        MessageBox.Show("WinPcap is one dependency of NetSpeedMonitor.\nYou can visit https://www.winpcap.org/ to install this software.\nAnd make sure WinPcap is properly installed on the local machine. \n\n[NetSpeedMonitor]");
                        Process.Start("https://www.winpcap.org/");
                        Shutdown();
                    }));
                }
                else
                {
                    Dispatcher.InvokeAsync(new Action(() =>
                    {
                        InitViewAndNeedClosedResourcees();
                        welcomeWindow.ReduceAndClose(new Point(mainWindow.Left + mainWindow.Width / 2, mainWindow.Top + mainWindow.Height / 2));
                    }));
                }
            }));
            t.Start();


        }
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            UDStatistic statistics = udMap.NextStatistic(10, portProcessMap);
            Dispatcher.Invoke(new Action(() =>
            {
                mainWindow.NewData(statistics);
                
                _History.Add(statistics, DateTime.Now);
                if (index == 300)//5 minet
                {
                    index = -1;
                    _History.Save();
                }
                index++;
            }));
        }
        public static bool IsWindows8orhigher()
        {
            Version win8version = new Version(6, 2, 9200, 0);

            if (Environment.OSVersion.Platform == PlatformID.Win32NT &&
                Environment.OSVersion.Version >= win8version)
            {
                // its win8 or higher.
                return true;
            }
            else
                return false;
        }
        /// <summary>
        /// Release the Mutex. It is not necessary to invoke this function when this program is closing.
        /// It is used when restart. Thus the new process can own the Mutex before the old process release the Mutex.
        /// </summary>
        public void FreeMutex()
        {
            if (_mMutex != null)
            {
                try
                {
                    _mMutex.Dispose();
                }
                catch (Exception)
                {

                }
            }
        }
        public void NeedPortProcessMap(object sender, bool need)
        {
            if (need)
            {
                portProcessMap.RegisterCustomer(sender);
            }
            else
            {
                portProcessMap.UnRegisterCustomer(sender);
            }
        }
        public void TryToExit()
        {
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged;
            SystemParameters.StaticPropertyChanged -= SystemParameters_StaticPropertyChanged;
            PopWindow.Me.RegisterAppBar(false);
            //timer.Enabled = false;
            PopWindow.Me.Disposenotification();
            FreeMutex();
            Shutdown();
            Thread t = new Thread(captureManager.Stop);
            t.Start();
        }
        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            CheckScreenCount();
        }
        private void CheckScreenCount()
        {
            if (System.Windows.Forms.Screen.AllScreens.Length != 1)
            {
                if (!screenLengthMaxOne)
                {
                    screenLengthMaxOne = true;
                    Control.TrayIcon.WindowMenuEdgeHideEnabled = false;
                    PopWindow.Me.TryToEdgeShow();
                }
            }
            else
            {
                if (screenLengthMaxOne)
                {
                    screenLengthMaxOne = false;
                    Control.TrayIcon.WindowMenuEdgeHideEnabled = true;
                    PopWindow.Me.TryToEdgeHide();
                }
            }
        }
        PopWindow mainWindow;
        private void SystemParameters_StaticPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "WorkArea")
            {
                Tool.MoveWindowBackToWorkArea(PopWindow.Me, PopWindow.Me.windowPadding);
            }
        }
        private void InitViewAndNeedClosedResourcees()
        {
            mainWindow = new PopWindow();
            if (Settings.Default.MainWindowLeft > -200000 && Settings.Default.MainWindowTop > -200000)
            {
                mainWindow.Left = Settings.Default.MainWindowLeft;
                mainWindow.Top = Settings.Default.MainWindowTop;
                Dispatcher.InvokeAsync(new Action(() =>
                {
                    Tool.MoveWindowBackToWorkArea(mainWindow, mainWindow.windowPadding);
                    mainWindow.isEdgeHide = true;
                    mainWindow.TryToEdgeShow();
                    mainWindow.TryToEdgeHide();
                }));
            }
            else
            {
                Dispatcher.InvokeAsync(new Action(() =>
                {
                    mainWindow.isEdgeHide = true;
                    mainWindow.TryToEdgeShow();
                    mainWindow.TryToEdgeHide();
                    mainWindow.UpdateWindowPosition();
                }));
            }
            mainWindow.Show();
            CheckScreenCount();
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
            SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;
            timer.Elapsed += Timer_Elapsed;
            timer.Enabled = true;
        }
#endregion
    }
}

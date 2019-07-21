using EarTrumpet;
using EarTrumpet.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using TrafficWatch.Control;
using TrafficWatch.Extensions;
using TrafficWatch.Properties;
using TrafficWatch.Services.Detail;
using TrafficWatch.View;

namespace TrafficWatch
{
    /// <summary>
    /// Interaction logic for PopWindow.xaml
    /// </summary>
    public partial class PopWindow : Window, ICanMoveDetailWindowToRightPlace
    {

        #region Network
        readonly List<NetworkInterface> goodAdapters = new List<NetworkInterface>();
        /// <summary>
        /// Initialize all network interfaces on this computer
        /// </summary>
        private void InitializeNetworkInterface()
        {
            var nicArr = NetworkInterface.GetAllNetworkInterfaces();

            goodAdapters.Clear();
            foreach (NetworkInterface nicnac in nicArr)
            {
                if (nicnac.SupportsMulticast && nicnac.GetIPv4Statistics().UnicastPacketsReceived >= 1 && nicnac.OperationalStatus.ToString() == "Up")
                {
                    goodAdapters.Add(nicnac);
                    //cmbInterface.Items.Add(nicnac.Name);
                }

            }
            if (goodAdapters.Count != cmbInterface.Items.Count && goodAdapters.Count != 0)
            {
                cmbInterface.Items.Clear();
                foreach (NetworkInterface gadpt in goodAdapters)
                {
                    cmbInterface.Items.Add(gadpt.Name);
                }
                cmbInterface.SelectedIndex = 0;
            }
            if (goodAdapters.Count == 0) cmbInterface.Items.Clear();
        }

        
        

        int refresh = 11;

        #endregion
        private readonly Control.TrayIcon _trayIcon;
        public static PopWindow Me;
        public PopWindow()
        {
            InitializeComponent();
            Me = this;
            _trayIcon = Control.TrayIcon.GetInstance();
            _trayIcon.Invoked += TrayIcon_Invoked;


            //G1.Background = null;



            // Move keyboard focus to the first element. Disabled this since it is ugly but not sure invisible
            // visuals are preferrable.
            // Activated += (s,e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.First));

            SourceInitialized += (s, e) => UpdateTheme();
            InitializeTimer();
            R2.Height = new GridLength(1, GridUnitType.Auto);
            R1.Height = new GridLength(0);
            R0.Height = new GridLength(0);
            Arrow1.Fill = this.Foreground;
            Arrow2.Fill = this.Foreground;

            var t = Program.ReadSetting("TopWindows", "0");
            if (t == "1") this.Topmost = true; else this.Topmost = false;
            var c = Program.ReadSetting("Chart", "0");
            if (c == "1") Chart(true); else Chart(false);
            var m = Program.ReadSetting("Max", "1");
            if (m == "1") Max(true); else Max(false);
            var h = Program.ReadSetting("Hiden", "0");
            if (h == "1") CreateAndHideWindow(); else CreateAndShowWindow();
            double temp = MaxDownload;
            _=double.TryParse(Program.ReadSetting("MaxDownload", "0"), out temp);
            MaxDownload = temp;
            SetMaxDownload(MaxDownload);
            //
            detailWindow = new DetailWindow(this);
            detailWindow.IsVisibleChanged += DetailWindow_IsVisibleChanged;
            var menu = new System.Windows.Controls.ContextMenu
            {
                ItemsSource = _trayIcon.BuildContextMenu(false).Items
            };
            //this.ContextMenu = menu;
            
        }
        internal void TopWindows()
        {
            if (!this.Topmost)
            { Program.WriteSetting("TopWindows", "1"); this.Topmost = true; }
            else { Program.WriteSetting("TopWindows", "0"); this.Topmost = false; }
        }
        #region Detail + Edge
        readonly System.Windows.Threading.DispatcherTimer TimerHiddenEdge = new System.Windows.Threading.DispatcherTimer();
        
        
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            IntPtr handle = (new WindowInteropHelper(this)).Handle;
            HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WindowProc));

            taskBarCreatedMsg = WinAPIWrapper.RegisterWindowMessage("TaskbarCreated");
            uCallBackMsg = WinAPIWrapper.RegisterWindowMessage("APPBARMSG_CSDN_HELPER_USTC.Software.hanyizhao.NetSpeedMonitor");
            RegisterAppBar(true);
        }
        /// <summary>
        /// Initialize the Timer
        /// </summary>
        private void InitializeTimer()
        {
            TimerHiddenEdge.Interval = new TimeSpan(0, 0, 1);
            TimerHiddenEdge.Tick += new EventHandler(Timer_Tick);
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            //Console.WriteLine("DispatcherTimer_Tick: " + DateTime.Now);
            if (CheckHasFullScreenApp(out _))
            {
                //TimerHiddenEdge.IsEnabled = false;
                HideAllView(true);
            }
            //else
            //{
            //    if (TimerHiddenEdge.Interval < maxSpan)
            //    {
            //        TimerHiddenEdge.Interval += spaceTimeSpan;
            //    }
            //}
        }
        private void DetailWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (detailWindow.Visibility == Visibility.Hidden)
            {
                TryToEdgeHide();
            }
        }
        public void NewData(UDStatistic statistics)
        {
            //Detaile windows
            if (detailWindow?.Visibility == Visibility.Visible)
            {
                detailWindow.NewData(statistics.items, statistics.timeSpan);
            }
            //MonitorProcess
            View.Monitor.MonitorProcess.Me?.NewData(statistics.items, statistics.timeSpan);
            if (!isEdgeHide && refresh > 10)
            {
                InitializeNetworkInterface();
                refresh = -1;
            }
            refresh++;
            //update Ui
            UpdateUi(statistics, statistics.timeSpan);
            State = statistics;
            //set max download
            if (statistics.download > MaxDownload)
            {
                SetMaxDownload(statistics.download);
                TrayIcon.ShowNotification("Traffic Watch", $"Max Download : {Tool.GetNetSpeedString(statistics.download, statistics.timeSpan) }");
            }
            //Mid = (MaxDownload / 8) * limited;
            if (statistics.download >= Mid)
            {
                ListMaxDownload.Add(new ItemMaxDownload(statistics.download,DateTime.Now));
                ShowToster( $"Download : {Tool.GetNetSpeedString(statistics.download, statistics.timeSpan) }");
                //index += 1;
            }
            //else
            //{

            //}
            
        }
        public UDStatistic State;
        /// <summary>
        /// Update GUI components for the network interfaces
        /// </summary>
        private void UpdateUi(UDStatistic item, double timeSpan)
        {
            //MessageBox.Show(cmbInterface.Items.Count.ToString());
            if (cmbInterface.Items.Count >= 1)
            {
                // Grab NetworkInterface object that describes the current interface
                NetworkInterface nic = goodAdapters[cmbInterface.SelectedIndex];
                // Grab the stats for that interface
                //IPv4InterfaceStatistics interfaceStats = nic.GetIPv4Statistics();
                // Update the labels
                lblInterfaceType.Content = nic.NetworkInterfaceType.ToString();


                //All Trafic
                lblBytesSent.Content = Tool.ToString(App._History.Data.Total.Upload);

                lblBytesReceived.Content = Tool.ToString(App._History.Data.Total.Download);

                //Now Trafic
                lblUpload.Content = Tool.GetNetSpeedString(item.upload, timeSpan);
                lblDownload.Content = Tool.GetNetSpeedString(item.download, timeSpan);
                //update try Balloon
                _trayIcon.BalloonTipText("▲ " + lblUpload.Content + "▼ " + lblDownload.Content);

                
                if (!isEdgeHide)
                {//Byte to kB/s
                    Chart1.Downloaded(item.download / 1024);
                    Chart1.Uploaded(item.upload / 1024);
                }

                UnicastIPAddressInformationCollection ipInfo = nic.GetIPProperties().UnicastAddresses;

                foreach (UnicastIPAddressInformation Info in ipInfo)
                {
                    if (Info.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        labelIPAddress.Content = Info.Address.ToString();
                        //uniCastIPInfo = item;
                        break;
                    }
                }
            }
            else
            {
                lblInterfaceType.Content = "-";
                lblDownload.Content = 0;
                lblUpload.Content = 0;
                lblBytesReceived.Content = 0;
                lblBytesSent.Content = 0;
                labelIPAddress.Content = "0.0.0.0";
            }
        }
        //int index = 0;
        /// <summary>
        /// set MaxDownload and mathd Mid
        /// </summary>
        /// <param name="SpeedDownload">Download</param>
        private void SetMaxDownload(double SpeedDownload)
        {
            if (MaxDownload != SpeedDownload) Program.WriteSetting("MaxDownload", SpeedDownload.ToString());
            MaxDownload = SpeedDownload;
            Mid = (MaxDownload / 8) * limited;
            
        }

        private readonly int limited = 1;
        void ShowToster(string Message)
        {
            if (toster == null)
            {
                toster = new Toster(Message);
                toster.Closed += Toster_Closed;
                toster.Show();
            }
            else
                toster.Text.Text = Message;
        }

        private void Toster_Closed(object sender, EventArgs e)
        {
            toster.Closed -= Toster_Closed;
            toster = null;
        }

        Toster toster;
        double Mid = 0;
        readonly List<ItemMaxDownload> ListMaxDownload = new List<ItemMaxDownload>();

        private double maxDownload = 0;
        public void ResetMaxSpeed()
        {
            SetMaxDownload(0);
        }
        public void ResetHistory()
        {
            SetMaxDownload(0);
            App._History.Data.Total = new DataHistory();
            App._History.Data.Date = DateTime.Now;
            App._History.Data.ListHistory.Clear();
            App._History.Save();
        }
        private int uCallBackMsg, taskBarCreatedMsg=0;
        private TimeSpan minSpan = new TimeSpan(2500000);// 0.2s
        //private TimeSpan maxSpan = new TimeSpan(0, 0, 7);
        //private TimeSpan spaceTimeSpan = new TimeSpan(7500000);
        private readonly DetailWindow detailWindow;

        public bool isEdgeHide = false;
        private readonly double edgeHideSpace = 4;

        private double oldLeft, oldTop;
        private DateTime leftPressTime = DateTime.Now;

        public readonly Thickness windowPadding = new Thickness(-3, 0, -3, -3);
        private void HideAllView(bool hide)
        {
            if (hide)
            {
                Hide();
                detailWindow?.OthersWantHide(true);
            }
            else
            {
                Show();
            }
        }


        public void TryToEdgeShow()
        {
            if (isEdgeHide)
            {
                if (Top + windowPadding.Top < 0)
                {
                    Top = windowPadding.Top;
                }
                if (Top + Height + windowPadding.Bottom > SystemParameters.PrimaryScreenHeight)
                {
                    Top = SystemParameters.PrimaryScreenHeight - Height - windowPadding.Bottom;
                }
                if (Left + windowPadding.Left < 0)
                {
                    Left = windowPadding.Left;
                }
                if (Left + Width + windowPadding.Right > SystemParameters.PrimaryScreenWidth)
                {
                    Left = SystemParameters.PrimaryScreenWidth - Width - windowPadding.Right;
                }
                isEdgeHide = false;
                SaveLeftAndTopToSettings();
            }

        }

        public void TryToEdgeHide()
        {
            if (Application.Current is App app)
            {
                if (!app.screenLengthMaxOne)
                {
                    if (Settings.Default.edgeHide)
                    {
                        if (!isEdgeHide)
                        {
                            if (!detailWindow.IsVisible)
                            {
                                if (Top - windowPadding.Top <= 2)
                                {
                                    Top = -windowPadding.Bottom - Height + edgeHideSpace;
                                    isEdgeHide = true;
                                }
                                else if (SystemParameters.PrimaryScreenHeight - (Top + Height + windowPadding.Bottom) <= 2)
                                {
                                    Top = SystemParameters.PrimaryScreenHeight + windowPadding.Top - edgeHideSpace;
                                    isEdgeHide = true;
                                }
                                else if (Left - windowPadding.Left <= 2)
                                {
                                    Left = -windowPadding.Right - Width + edgeHideSpace;
                                    isEdgeHide = true;
                                }
                                else if (SystemParameters.PrimaryScreenWidth - (Left + Width + windowPadding.Right) <= 2)
                                {
                                    Left = SystemParameters.PrimaryScreenWidth + windowPadding.Left - edgeHideSpace;
                                    isEdgeHide = true;
                                }
                                SaveLeftAndTopToSettings();
                            }
                        }
                    }

                }
            }
        }
        public void TryToSetEdgeHide(bool edgeHide)
        {
            TimerHiddenEdge.IsEnabled = true;
            Settings.Default.edgeHide = edgeHide;
            Settings.Default.Save();
            if (edgeHide)
            {
                TryToEdgeHide();
            }
            else
            {
                TryToEdgeShow();
            }
        }
        private void SaveLeftAndTopToSettings()
        {
            Settings.Default.MainWindowLeft = Left;
            Settings.Default.MainWindowTop = Top;
            Settings.Default.Save();
        }

        private void VolumeWindow_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            //var cm = _trayIcon.BuildContextMenu();
            //cm.Placement = PlacementMode.Mouse;
            //cm.IsOpen = true;
        }

        private void G1_MouseEnter(object sender, MouseEventArgs e)
        {
            //detailWindow.OthersWantShow(false);
            TryToEdgeShow();
            UpdateTheme();
        }


        private void G1_MouseLeave(object sender, MouseEventArgs e)
        {
            detailWindow.OthersWantHide(false);
            TryToEdgeHide();
            UpdateTheme();
        }

        private void G1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //detailWindow.OthersWantHide(true);
            leftPressTime = DateTime.Now;
            oldLeft = Left;
            oldTop = Top;
            try { DragMove(); } catch { }
            
            SaveLeftAndTopToSettings();
        }

        public void RegisterAppBar(bool register)
        {
            Services.Detail.APPBARDATA abd = new Services.Detail.APPBARDATA();
            abd.cbSize = Marshal.SizeOf(abd);
            abd.hWnd = new WindowInteropHelper(this).Handle;

            if (register)
            {
                //register
                abd.uCallbackMessage = uCallBackMsg;
                _ = WinAPIWrapper.SHAppBarMessage((int)Services.Detail.ABMsg.ABM_NEW, ref abd);
                // Check whether there is a full screen app now.
                HideAllView(CheckHasFullScreenApp(out _));
            }
            else
            {
                WinAPIWrapper.SHAppBarMessage((int)Services.Detail.ABMsg.ABM_REMOVE, ref abd);
            }
        }
        /// <summary>
        /// Call this method in main thread.
        /// </summary>
        /// <param name="causeNotFillScreen">The foreground window is not a full screen App because it doesn't fill the screen.
        /// Otherwise, because it is Window Explorer or...</param>
        /// <returns></returns>
        private bool CheckHasFullScreenApp(out bool causeNotFillScreen)
        {
            causeNotFillScreen = false;
            bool result;
            IntPtr foreWindow = WinAPIWrapper.GetForegroundWindow();
            WinAPIWrapper.GetWindowThreadProcessId(foreWindow, out uint processid);
            String foreGroundWindowName = "";
            try
            {
                foreGroundWindowName = Process.GetProcessById((int)processid).ProcessName;
                //Console.WriteLine("foreGroundWindowName:" + foreGroundWindowName);
            }
            catch (Exception)
            {
            }
            if (foreGroundWindowName != "explorer" && !foreWindow.Equals(new WindowInteropHelper(this).Handle))//"SearchUI"
            {
                IntPtr deskWindow = WinAPIWrapper.GetDesktopWindow();
                if (!foreWindow.Equals(deskWindow) && !foreWindow.Equals(WinAPIWrapper.GetShellWindow()))
                {
                    WinAPIWrapper.GetWindowRect(foreWindow, out Services.Detail.RECT foreWindowRECT);
                    WinAPIWrapper.GetWindowRect(deskWindow, out Services.Detail.RECT deskWindowRECT);
                    //Console.WriteLine("foreWindow RECT:" + foreWindowRECT);
                    //Console.WriteLine("deskWindow RECT:" + deskWindowRECT);
                    // Check whether foreground Window fills main screen.
                    result = foreWindowRECT.left <= deskWindowRECT.left
                        && foreWindowRECT.top <= deskWindowRECT.top
                        && foreWindowRECT.right >= deskWindowRECT.right
                        && foreWindowRECT.bottom >= deskWindowRECT.bottom;
                    causeNotFillScreen = true;
                }
                else
                {
                    // Foreground Window is DeskWindow or ShellWindow.
                    result = false;
                }
            }
            else
            {
                // Foreground window is Windows Explorer or MainWindow itself.
                result = false;
            }
            if (result)
            {

            }
            return result;
        }
        private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == taskBarCreatedMsg)
            {
                Console.WriteLine("Receive Message: TaskbarCreated");
                TimerHiddenEdge.IsEnabled = false;
                RegisterAppBar(false);
                RegisterAppBar(true);
            }
            if (msg == uCallBackMsg)
            {
                if (wParam.ToInt32() == (int)ABNotify.ABN_FULLSCREENAPP)
                {
                    bool hasFull = false;
                    if (lParam.ToInt32() == 1)
                    {
                        hasFull = CheckHasFullScreenApp(out bool notSure);
                        if (!hasFull && notSure)
                        {
                            // Now taskbar tells us where is a full screen app. But it doesn't not fill screen now. Maybe it will fill screen later.
                            if (!TimerHiddenEdge.IsEnabled)
                            {
                                TimerHiddenEdge.Interval = minSpan;
                                TimerHiddenEdge.IsEnabled = true;
                            }
                        }
                        else
                        {
                            TimerHiddenEdge.IsEnabled = false;
                        }
                    }
                    else
                    {
                        TimerHiddenEdge.IsEnabled = false;
                    }
                    HideAllView(hasFull);
                }
            }
            return IntPtr.Zero;
        }

        public readonly Thickness windowMargin = new Thickness(-3, 3, -3, 0);

        public double MaxDownload { get => maxDownload; set => maxDownload = value; }

        void ICanMoveDetailWindowToRightPlace.MoveDetailWindowToRightPlace(DetailWindow dw)
        {
            Thickness pa = windowMargin;
            Rect mainRect = new Rect(Left - pa.Left, Top - pa.Top,
                Width + pa.Left + pa.Right, Height + pa.Top + pa.Bottom);
            Rect workArea = SystemParameters.WorkArea;
            if (workArea.Bottom - mainRect.Bottom >= dw.Height)//bellow
            {
                dw.Top = mainRect.Bottom;
                if (mainRect.Left + dw.Width <= workArea.Right)
                {
                    dw.Left = mainRect.Left;
                }
                else
                {
                    dw.Left = mainRect.Right - dw.Width;
                }

            }
            else if (mainRect.Top - workArea.Top >= dw.Height)//top
            {
                dw.Top = mainRect.Top - dw.Height;
                if (mainRect.Left + dw.Width <= workArea.Right)
                {
                    dw.Left = mainRect.Left;
                }
                else
                {
                    dw.Left = mainRect.Right - dw.Width;
                }
            }
            else//left or right
            {
                if (mainRect.Right + dw.Width <= workArea.Right)//right
                {
                    dw.Left = mainRect.Right;
                }
                else
                {
                    dw.Left = mainRect.Left - dw.Width;//left
                }
                if (mainRect.Top + dw.Height <= workArea.Bottom)
                {
                    dw.Top = mainRect.Top;
                }
                else
                {
                    dw.Top = workArea.Bottom - dw.Height;
                }
            }
        }
        #endregion
        public void ShowChart()
        {
            //M2.Visibility = Visibility.Visible;
            if (M3.Visibility == Visibility.Hidden)
            {
                R0.Height = new GridLength(1, GridUnitType.Auto);
                M3.Visibility = Visibility.Visible;
                LayoutRoot.Height += 100;
                Program.WriteSetting("Chart", "1");
            }
            else
            {
                R0.Height = new GridLength(0);
                M3.Visibility = Visibility.Hidden;
                LayoutRoot.Height -= 100;
                Program.WriteSetting("Chart", "0");
            }
            //UpdateWindowPosition();
        }
        public void Chart(bool Show)
        {
            if (Show)
            {
                R0.Height = new GridLength(1, GridUnitType.Auto);
                M3.Visibility = Visibility.Visible;
                LayoutRoot.Height += 100;
            }
            else
            {
                R0.Height = new GridLength(0);
                M3.Visibility = Visibility.Hidden;
                LayoutRoot.Height -= 100;
            }
            //UpdateWindowPosition();
        }
        private void Window_Deactivated(object sender, EventArgs e)
        {
            //this.Hide();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Hide();
                Program.WriteSetting("Hiden", "1");
            }
        }
        private void CreateAndHideWindow()
        {
            // Ensure the Win32 and WPF windows are created to fix first show issues with DPI Scaling
            Opacity = 0;
            Show();
            Hide();
            Opacity = 1;
        }
        private void CreateAndShowWindow()
        {
            // Ensure the Win32 and WPF windows are created to fix first show issues with DPI Scaling
            Opacity = 0;
            Show();
            Opacity = 1;
            UpdateTheme();
            //UpdateWindowPosition();
        }
        void TrayIcon_Invoked()
        {
            if (this.Visibility == Visibility.Visible)
            {
                this.Hide(); Program.WriteSetting("Hiden", "1");
            }
            else
            {
                UpdateTheme();
                //UpdateWindowPosition();
                this.Show(); Program.WriteSetting("Hiden", "0");
            }
        }
        #region theme
        private void UpdateTheme()
        {
            // Call UpdateTheme before UpdateWindowPosition in case sizes change with the theme.
            if (App.IsWindows8orhigher())
            {
                ThemeService.UpdateThemeResources(Resources);
                if (ThemeService.IsWindowTransparencyEnabled)
                {
                    BlurWindowExtensions.EnableBlur(this);
                }
                else
                {
                    BlurWindowExtensions.DisableBlur(this);
                }
            }
            else
            {
                BlurWindowExtensions.SetGlass(this);
                SolidColorBrush Color =(SolidColorBrush) FindResource("WindowForeground");
                Color.Color=Colors.White; GBlur.Visibility = Visibility.Visible;
            }
                
        }
        private void UpdateWindowSize()
        {
            LayoutRoot.UpdateLayout();
            LayoutRoot.Measure(new Size(double.PositiveInfinity, MaxHeight));
            if(LayoutRoot.DesiredSize.Height> Height)
            {
                Top = Top - LayoutRoot.DesiredSize.Height + Height;
            }
            else
            {
                Top = Top +  Height- LayoutRoot.DesiredSize.Height;
            }
            Height = LayoutRoot.DesiredSize.Height;

        }
        public void UpdateWindowPosition()
        {
            LayoutRoot.UpdateLayout();
            LayoutRoot.Measure(new Size(double.PositiveInfinity, MaxHeight));
            Height = LayoutRoot.DesiredSize.Height;
            var taskbarState = TaskbarService.GetWinTaskbarState();
            switch (taskbarState.TaskbarPosition)
            {
                case TaskbarPosition.Left:
                    Left = (taskbarState.TaskbarSize.right / this.DpiWidthFactor());
                    Top = (taskbarState.TaskbarSize.bottom / this.DpiHeightFactor()) - Height;
                    break;
                case TaskbarPosition.Right:
                    Left = (taskbarState.TaskbarSize.left / this.DpiWidthFactor()) - Width;
                    Top = (taskbarState.TaskbarSize.bottom / this.DpiHeightFactor()) - Height;
                    break;
                case TaskbarPosition.Top:
                    Left = (taskbarState.TaskbarSize.right / this.DpiWidthFactor()) - Width;
                    Top = (taskbarState.TaskbarSize.bottom / this.DpiHeightFactor());
                    break;
                case TaskbarPosition.Bottom:
                    Left = (taskbarState.TaskbarSize.right / this.DpiWidthFactor()) - Width;
                    Top = (taskbarState.TaskbarSize.top / this.DpiHeightFactor()) - Height;
                    break;
            }
        }
        #endregion

        private void Exclod_Click(object sender, RoutedEventArgs e)
        {
            int Chart;
            if (M3.Visibility == Visibility.Visible)
            {
                Chart = 100;
            }
            else { Chart = 0; }
            if (M1.Visibility==Visibility.Visible)
            {
                M1.Visibility = Visibility.Hidden;
                M2.Visibility = Visibility.Visible;
                LayoutRoot.Height = 32+ Chart;
                R2.Height = new GridLength(0);
                R1.Height = new GridLength(1, GridUnitType.Auto);
                //UpdateWindowPosition();
                
                Program.WriteSetting("Max", "1");
            }
            else
            {
                M2.Visibility = Visibility.Hidden;
                M1.Visibility = Visibility.Visible;
                LayoutRoot.Height = 198 + Chart;//224
                R2.Height = new GridLength(1, GridUnitType.Auto);
                R1.Height = new GridLength(0);
                //UpdateWindowPosition();
                Program.WriteSetting("Max", "0");
            }
            UpdateWindowSize();
        }

        

        private void Max(bool Steit)
        {
            int Chart = Program.ReadSetting("Chart", "0")=="1" ? 100 : 0;//224
            if (Steit)
            {
                M1.Visibility = Visibility.Hidden;
                M2.Visibility = Visibility.Visible;
                LayoutRoot.Height = 32 + Chart;
                R2.Height = new GridLength(0);
                R1.Height = new GridLength(1, GridUnitType.Auto);
                //UpdateWindowPosition();
            }
            else
            {
                M2.Visibility = Visibility.Hidden;
                M1.Visibility = Visibility.Visible;
                LayoutRoot.Height = 198 + Chart;//224
                R2.Height = new GridLength(1, GridUnitType.Auto);
                R1.Height = new GridLength(0);
                //UpdateWindowPosition();
            }
            UpdateWindowSize();
        }
        #region ButtenView
        private void ButtenView_MouseLeave(object sender, MouseEventArgs e)
        {
            See.Fill = this.Foreground;
        }

        private void ButtenView_MouseEnter(object sender, MouseEventArgs e)
        {
            See.Fill = this.Background;
        }

        private void VolumeWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (oldLeft == Left && oldTop == Top)
                {
                    if (DateTime.Now.Subtract(leftPressTime).TotalMilliseconds < 500)
                    {
                        TryToEdgeShow();
                        detailWindow.OthersWantShow(true);
                    }
                }
                else
                {
                    detailWindow.OthersWantShow(false);
                    Tool.MoveWindowBackToWorkArea(this, windowPadding);
                }
            }
        }

        private void ButtenView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            new WindowSetting().ShowDialog();
        }
        #endregion
        public void Disposenotification()
        {
            _trayIcon.Cloce();
        }
    }
}

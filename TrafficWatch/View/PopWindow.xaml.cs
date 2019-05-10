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
        List<NetworkInterface> goodAdapters = new List<NetworkInterface>();
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
        System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();

        internal void TopWindows()
        {
            if (!this.Topmost)
            { Program.WriteSetting("TopWindows", "1"); this.Topmost = true; }
            else { Program.WriteSetting("TopWindows", "0"); this.Topmost = false; }
        }

        /// <summary>
        /// Initialize the Timer
        /// </summary>
        private void InitializeTimer()
        {
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Start();

        }
        double Send = 0;
        double Received = 0;
        /// <summary>
        /// Update GUI components for the network interfaces
        /// </summary>
        private void UpdateNetworkInterface()
        {
            //MessageBox.Show(cmbInterface.Items.Count.ToString());
            if (cmbInterface.Items.Count >= 1)
            {
                if(!Dis)
                    Dis = true;
                // Grab NetworkInterface object that describes the current interface
                NetworkInterface nic = goodAdapters[cmbInterface.SelectedIndex];

                // Grab the stats for that interface
                IPv4InterfaceStatistics interfaceStats = nic.GetIPv4Statistics();


                Double bytesSentSpeed = (Double)(interfaceStats.BytesSent - Send) / 1024;
                Double bytesReceivedSpeed = (Double)(interfaceStats.BytesReceived - Received) / 1024;

                // Update the labels
                //lblSpeed.Text = nic.Speed.ToString();
                lblInterfaceType.Content = nic.NetworkInterfaceType.ToString();
                //lblSpeed.Text = (nic.Speed).ToString("N0");
                Send = interfaceStats.BytesSent;
                Received = interfaceStats.BytesReceived;
                lblBytesSent.Content = Conv(Send).ToString();

                lblBytesReceived.Content = Conv(Received) ;


                if (bytesSentSpeed < 1024)
                    lblUpload.Content =  bytesSentSpeed.ToString("f2")+ " KB/s" ;
                else
                    lblUpload.Content = (bytesSentSpeed / 1024).ToString("f2") + " MB/s";
                if (bytesReceivedSpeed < 1024)
                    lblDownload.Content =  bytesReceivedSpeed.ToString("f2") + " KB/s";
                else
                    lblDownload.Content = (bytesReceivedSpeed / 1024).ToString("f2") + " MB/s";

                _trayIcon.BalloonTipText("▲ " + lblUpload.Content + "▼ " + lblDownload.Content);
                MiniLabelD.Content =  lblDownload.Content;
                MiniLabelU.Content =  lblUpload.Content;
                //Chart1.Downloaded = bytesReceivedSpeed;
                //Chart1.Uploaded = bytesSentSpeed;
                UnicastIPAddressInformationCollection ipInfo = nic.GetIPProperties().UnicastAddresses;

                foreach (UnicastIPAddressInformation item in ipInfo)
                {
                    if (item.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        labelIPAddress.Content = item.Address.ToString();
                        //uniCastIPInfo = item;
                        break;
                    }
                }
            }
            else
            {
                if(Dis)
                {
                    Dis = false;
                    lblInterfaceType.Content = "-";
                    lblDownload.Content = 0;
                    lblUpload.Content = 0;
                    MiniLabelD.Content = 0;
                    MiniLabelU.Content = 0;
                    lblBytesReceived.Content = 0;
                    lblBytesSent.Content = 0;
                    labelIPAddress.Content = "0.0.0.0";
                }
                
            }
        }
        bool Dis = true;
        static readonly string[] SizeSuffixes =
                   { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        static string SizeSuffix(Int64 value, int decimalPlaces = 1)
        {
            if (value < 0) { return "-" + SizeSuffix(-value); }
            if (value == 0) { return "0.0 bytes"; }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
        }
        static string SizeSuffix(Double value, int decimalPlaces = 1)
        {
            if (value < 0) { return "-" + SizeSuffix(-value); }
            if (value == 0) { return "0.0 bytes"; }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
        }
        private string Conv(double n)
        {

            return SizeSuffix(n).ToString();
        }
        int refresh = 11;
        void Timer_Tick(object sender, EventArgs e)
        {
            if (!isEdgeHide && refresh>10)
            {
                InitializeNetworkInterface();
                refresh = -1;
            }
            refresh++;
            UpdateNetworkInterface();

        }
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
            //
            detailWindow = new DetailWindow(this);
            detailWindow.IsVisibleChanged += DetailWindow_IsVisibleChanged;
            //dispatcherTimer = new DispatcherTimer();
            //dispatcherTimer.Tick += DispatcherTimer_Tick;
            var menu = new System.Windows.Controls.ContextMenu();
            menu.ItemsSource = _trayIcon.BuildContextMenu(false).Items;
            this.ContextMenu = menu;
            //dispatcherTimer.IsEnabled = true;
        }
        #region Detail + Edge
        //private void DispatcherTimer_Tick(object sender, EventArgs e)
        //{
        //    //Console.WriteLine("DispatcherTimer_Tick: " + DateTime.Now);
        //    if (CheckHasFullScreenApp(out bool notSure))
        //    {
        //        dispatcherTimer.IsEnabled = false;
        //        HideAllView(true);
        //    }
        //    else
        //    {
        //        if (dispatcherTimer.Interval < maxSpan)
        //        {
        //            dispatcherTimer.Interval += spaceTimeSpan;
        //        }
        //    }
        //}
        private void DetailWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (detailWindow.Visibility == Visibility.Hidden)
            {
                TryToEdgeHide();
            }
        }
        public void NewData(UDStatistic statistics)
        {
            if (detailWindow?.Visibility == Visibility.Visible)
            {
                detailWindow.NewData(statistics.items, statistics.timeSpan);
            }
            View.Monitor.MonitorProcess.Me?.NewData(statistics.items, statistics.timeSpan);
        }
        //
        private int uCallBackMsg, taskBarCreatedMsg;

        private DetailWindow detailWindow;

        public bool isEdgeHide = false;
        private double edgeHideSpace = 4;

        private double oldLeft, oldTop;
        private DateTime leftPressTime = DateTime.Now;
        private DispatcherTimer dispatcherTimer;
        private TimeSpan minSpan = new TimeSpan(2500000);// 0.2s
        private TimeSpan maxSpan = new TimeSpan(0, 0, 7);
        private TimeSpan spaceTimeSpan = new TimeSpan(7500000);

        public readonly Thickness windowPadding = new Thickness(-3, 0, -3, -3);
        private void HideAllView(bool hide)
        {
            if (hide)
            {
                Hide();
                detailWindow.OthersWantHide(true);
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
                uint ret = WinAPIWrapper.SHAppBarMessage((int)Services.Detail.ABMsg.ABM_NEW, ref abd);
                // Check whether there is a full screen app now.
                HideAllView(CheckHasFullScreenApp(out bool a));
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
            if (foreGroundWindowName != "explorer" && !foreWindow.Equals(new WindowInteropHelper(this).Handle))
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
            return result;
        }


        public readonly Thickness windowMargin = new Thickness(-3, 3, -3, 0);
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
            int Chart = 0;
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
            int Chart = 0;
            if (Program.ReadSetting("Chart", "0")=="1")
            {
                Chart = 100;
            }
            else { Chart = 0; }
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

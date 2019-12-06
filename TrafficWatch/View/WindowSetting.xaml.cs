using EarTrumpet.Services;
using System;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TrafficWatch
{
    /// <summary>
    /// Interaction logic for WindowSetting.xaml
    /// </summary>
    public partial class WindowSetting : Window
    {
        public WindowSetting()
        {
            InitializeComponent();
            Init();
            Network();
            App.Pin(this);
            //CmbInterface
        }
        void Network()
        {
            var nicArr = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface nicnac in nicArr)
            {
                if (nicnac.SupportsMulticast  )
                {
                    CmbInterface.Items.Add(nicnac);
                    //CmbInterface.Items.Add(nicnac.Name);
                }

            }
        }
        #region them
        private bool clicked = false;
        private Point lmAbs = new Point();
        private void Init()
        {
            this.Loaded += MainWindow_Loaded;
            this.MouseMove += PnMouseMove;
            this.MouseDown += PnMouseDown;
            this.MouseUp += PnMouseUp;
            this.MouseLeave += MainWindow_MouseLeave;
            if (this.WindowState == WindowState.Normal)
            {
                ButtonMaximized.Content = "¨";
            }
            else
            {
                ButtonMaximized.Content = "q";
            }
        }
        private void MainWindow_MouseLeave(object sender, MouseEventArgs e)
        {
            clicked = false;
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
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
                SolidColorBrush Color = (SolidColorBrush)FindResource("WindowForeground");
                Color.Color = Colors.White; GBlur.Visibility = Visibility.Visible;
            }

        }
        void PnMouseDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            clicked = true;
            this.lmAbs = e.GetPosition(this);
            this.lmAbs.Y = Convert.ToInt16(this.Top) + this.lmAbs.Y;
            this.lmAbs.X = Convert.ToInt16(this.Left) + this.lmAbs.X;
        }

        void PnMouseUp(object sender, System.Windows.Input.MouseEventArgs e)
        {
            clicked = false;
        }

        void PnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (clicked)
            {
                Point MousePosition = e.GetPosition(this);
                Point MousePositionAbs = new Point()
                {
                    X = Convert.ToInt16(this.Left) + MousePosition.X,
                    Y = Convert.ToInt16(this.Top) + MousePosition.Y
                };
                Left = this.Left + (MousePositionAbs.X - this.lmAbs.X);
                Top = this.Top + (MousePositionAbs.Y - this.lmAbs.Y);
                this.lmAbs = MousePositionAbs;
            }
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();

        }

        private void ButtonMinimized_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void ButtonMaximized_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                ButtonMaximized.Content = "¨";
            }
            else
            {
                this.WindowState = WindowState.Maximized;
                ButtonMaximized.Content = "q";
            }
        }
        #endregion

        private void CmbInterface_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NetworkInterface nic = CmbInterface.SelectedItem as NetworkInterface;

            // Grab the stats for that interface
            IPv4InterfaceStatistics interfaceStats = nic.GetIPv4Statistics();
            var Send = interfaceStats.BytesSent;
            var Received = interfaceStats.BytesReceived;
            lblBytesSent.Content = Conv(Send).ToString() + "/s";

            lblBytesReceived.Content = Conv(Received) + "/s";
        }
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
    }
}

using EarTrumpet.Services;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using TrafficWatch.Extensions;
using TrafficWatch.Services.Detail;

namespace TrafficWatch.View
{
    /// <summary>
    /// Interaction logic for Toster.xaml
    /// </summary>
    public partial class Toster : Window
    {
        readonly System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
        public Toster(string Message)
        {
            InitializeComponent();
            Text.Text = Message;
            InitializeTimer();
            UpdateWindowPosition();
            Loaded += Toster_Loaded;
        }

        private void Toster_Loaded(object sender, RoutedEventArgs e)
        {
            App.Pin(this);
            double Left = this.Left;
            DoubleAnimation da = new DoubleAnimation
            {
                From = Left + Width,
                To = Left,
                Duration = new Duration(TimeSpan.FromSeconds(0.4))
            };
            this.BeginAnimation(Window.LeftProperty, da);
        }

        private void InitializeTimer()
        {
            timer.Interval = new TimeSpan(0, 0, 5);
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
        public void UpdateWindowPosition()
        {
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
        #region Disable Focus
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            //Set the window style to noactivate.
            WindowInteropHelper helper = new WindowInteropHelper(this);
            WinAPIWrapper.SetWindowLong(helper.Handle, WinAPIWrapper.GWL_EXSTYLE,
                GetWindowLong(helper.Handle, WinAPIWrapper.GWL_EXSTYLE) | WinAPIWrapper.WS_EX_NOACTIVATE);
        }

        //private const int GWL_EXSTYLE = -20;
        //private const int WS_EX_NOACTIVATE = 0x08000000;

        //[DllImport("user32.dll")]
        //public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        #endregion

    }
}

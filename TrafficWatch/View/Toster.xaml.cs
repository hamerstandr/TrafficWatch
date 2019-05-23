using EarTrumpet.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TrafficWatch.Extensions;
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

    }
}

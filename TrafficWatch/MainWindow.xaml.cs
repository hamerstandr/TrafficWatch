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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TrafficWatch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            MessageBox.Show("MainWindow s");
            InitializeComponent(); Init();
            MessageBox.Show("MainWindow E");
        }
        #region theme
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
            if(App.IsWindows8orhigher())
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
                BlurWindowExtensions.SetGlass(this);


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
                this.Left = this.Left + (MousePositionAbs.X - this.lmAbs.X);
                this.Top = this.Top + (MousePositionAbs.Y - this.lmAbs.Y);
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
    }
}

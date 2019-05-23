using EarTrumpet.Services;
using TrafficWatch.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;

namespace TrafficWatch.Control
{
    public class TrayIcon
    {
        private readonly System.Windows.Forms.NotifyIcon _trayIcon;
        public static TrayIcon Me;
        public static Version version = Assembly.GetEntryAssembly().GetName().Version;
        public static string Name = Assembly.GetEntryAssembly().GetName().Name;
        public TrayIcon()
        {
            if (App.IsWindows8orhigher())
                SetResources();
            _trayIcon = new System.Windows.Forms.NotifyIcon();
            _trayIcon.MouseClick += TrayIcon_MouseClick;
            _trayIcon.DoubleClick += TrayIcon_DoubleClick;
            _trayIcon.Icon = new System.Drawing.Icon(Application.GetResourceStream(new Uri("pack://application:,,,/TrafficWatch;component/Tray.ico")).Stream); //GetTrayIconByDPI();
            _trayIcon.Text = string.Concat(Name);
            _trayIcon.Visible = true;

            Application.Current.Exit += App_Exit;
        }

        private void TrayIcon_DoubleClick(object sender, EventArgs e)
        {
            if (_trayIcon.BalloonTipText != string.Empty)
                _trayIcon.ShowBalloonTip(10);
        }

        //private Icon GetTrayIconByDPI()
        //{
        //    var scale = DpiHelper.GetCurrentScaleFactor().Vertical;

        //    if (App.IsWin10)
        //        return scale > 1 ? Resources.app_white : Resources.app_white_16;
        //    return scale > 1 ? Resources.app : Resources.app_16;
        //}
        private void App_Exit(object sender, ExitEventArgs e)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
        }
        ResourceDictionary myResourceDictionary;
        void SetResources()
        {
            myResourceDictionary = new ResourceDictionary
            {
                Source =
        new Uri("Control/MenuThame.xaml",
                UriKind.RelativeOrAbsolute)
            };

            App.Current.Resources.MergedDictionaries.Add(myResourceDictionary);
        }
        public ContextMenu BuildContextMenu(bool _Style)
        {
            Style Stylecontent =null;
            if (App.IsWindows8orhigher() && _Style)
                Stylecontent = (Style)App.Current.FindResource("ContextMenuDarkOnly");
            ContextMenu cm = new ContextMenu { Style = Stylecontent };
            //cm.Opacity = 0.9;
            //cm.FlowDirection = SystemSettings.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            if (_Style)
            {
                cm.Opened += ContextMenu_Opened;
                cm.Closed += ContextMenu_Closed;
                cm.StaysOpen = true; // To be removed on open.
            }
            Style menuItemStyle = null;
            if (App.IsWindows8orhigher() && _Style)
                menuItemStyle = (Style)App.Current.FindResource("MenuItemDarkOnly");
            var AddItem = new Func<string, ICommand, bool, KeyGesture, MenuItem> ((displayName, action, IsChecked, KeyGesture) =>
            {
                var mnuItem = new MenuItem
                {
                    Header = displayName,
                    Command = action,
                    Style = menuItemStyle,
                    InputGestureText = (KeyGesture != null ? KeyGesture.Key.ToString() + "+" + KeyGesture.Modifiers.ToString() : ""),
                    IsChecked = IsChecked
                };
                if (KeyGesture != null)
                    mnuItem.InputBindings.Add(new InputBinding(action, KeyGesture));
                cm.Items.Add(mnuItem);
                return mnuItem;
            });

            Style separatorStyle = null;
            // Static items
            if (App.IsWindows8orhigher() && _Style)
               separatorStyle = (Style)App.Current.FindResource("MenuItemSeparatorDarkOnly");

            AddItem($"v{version}", AboutCommand, false, null);
            cm.Items.Add(new Separator { Style = separatorStyle });
            AddItem("StartUp", AutorunCommand, (Program.ReadSetting("StartUp", "0") == "0" ? false : true), null);
            cm.Items.Add(new Separator { Style = separatorStyle });
            AddItem("Top Windows", TopWindowsCommand, (Program.ReadSetting("TopWindows", "0") == "0" ? false : true), null);
            var x = AddItem("Hide when close to edge", EdgeHideWindowCommand, Settings.Default.edgeHide, null);
            x.IsEnabled = WindowMenuEdgeHideEnabled;
            AddItem("Chart", ChartCommand, (Program.ReadSetting("Chart", "0") == "0" ? false : true), null);
            cm.Items.Add(new Separator { Style = separatorStyle });
            AddItem("Info Interface", InfoInterfaceCommand, false, null);
            AddItem("History", HistoryCommand, false, null);
            cm.Items.Add(new Separator { Style = separatorStyle });
            
            AddItem("Quit", ExitCommand, false, new KeyGesture(Key.Q, ModifierKeys.Alt));

            //cm.Items.Add(new Separator { Style = separatorStyle });
            //var aboutString = "درباره ما";

            //_trayIcon.ContextMenu.Popup += (sender, e) => { _itemAutorun.IsChecked = AutoStartupHelper.IsAutorun(); };
            return cm;
        }
        public static bool WindowMenuEdgeHideEnabled=true;
        private void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {

        }
        public void BalloonTipText(string Text)
        {
            _trayIcon.BalloonTipText = Text;
        }
        public void BalloonTipTitle(string Title)
        {
            _trayIcon.BalloonTipTitle = (Title);
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            var cm = (ContextMenu)sender;
            User32.SetForegroundWindow(((HwndSource)HwndSource.FromVisual(cm)).Handle);
            cm.Focus();
            cm.StaysOpen = false;
            ((Popup)cm.Parent).PopupAnimation = PopupAnimation.None;
        }
        public RelayCommand TopWindowsCommand = new RelayCommand(TopWindows_Click);
        public static void TopWindows_Click()
        {
            TrafficWatch.PopWindow.Me.TopWindows();
            //var TopWindows = App.ReadSetting("TopWindows", "0");
        }
        public RelayCommand EdgeHideWindowCommand = new RelayCommand(EdgeHideWindow_Click);
        public static void EdgeHideWindow_Click()
        {
            PopWindow.Me.TryToSetEdgeHide(!Settings.Default.edgeHide);
        }
        public RelayCommand ChartCommand = new RelayCommand(Chart_Click);
        public static void Chart_Click()
        {
            TrafficWatch.PopWindow.Me.ShowChart();
        }
        public RelayCommand InfoInterfaceCommand = new RelayCommand(InfoInterface_Click);
        public static void InfoInterface_Click()
        {
            new TrafficWatch.WindowSetting().ShowDialog();
        }
        public RelayCommand HistoryCommand = new RelayCommand(History_Click);
        public static void History_Click()
        {
            new View.Detail.historyWindow().ShowDialog();
        }
        public RelayCommand AutorunCommand = new RelayCommand(Autorun_Click);
        public static void Autorun_Click()
        {
            var ret = Program.ReadSetting("StartUp", "0");
            if (ret == "0")
            { Program.AddStartup();  }
            else
            {
                Program.RemoveStartup();;
            }
        }

        public RelayCommand AboutCommand = new RelayCommand(About_Click);
        public static void About_Click()
        {
            // MessageBox.Show("Company :pooi.moe\nEditor :hamerstandr")
            string aboutItem = String.Format("نام برنامه : {0}\n نسخه: {1} ...", Name, version);
            MessageBox.Show(aboutItem + "\n" + "تمام حقوق مادی و معنوی برنامه \n مربوط به حامد مومن میباشد.");
        }

        public RelayCommand ExitCommand = new RelayCommand(Exit_Click);
        public static void Exit_Click()
        {
            System.Windows.Application.Current.Shutdown();
            Environment.Exit(0);
        }

        public void Cloce()
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
        }
        public void Show()
        {
            _trayIcon.Visible = true;
        }
        public event Action Invoked = delegate { };
        void TrayIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                Invoked.Invoke();
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                var cm = BuildContextMenu(true);
                cm.Placement = PlacementMode.Mouse;
                cm.IsOpen = true;
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Middle)
            {

            }
        }
        public static void ShowNotification(string title, string content, bool isError = false, int timeout = 5000,
            Action clickEvent = null,
            Action closeEvent = null)
        {
            var icon = GetInstance()._trayIcon;
            icon.ShowBalloonTip(timeout, title, content, isError ? System.Windows.Forms.ToolTipIcon.Error : System.Windows.Forms.ToolTipIcon.Info);
            icon.BalloonTipClicked += OnIconOnBalloonTipClicked;
            icon.BalloonTipClosed += OnIconOnBalloonTipClosed;

            void OnIconOnBalloonTipClicked(object sender, EventArgs e)
            {
                clickEvent?.Invoke();
                icon.BalloonTipClicked -= OnIconOnBalloonTipClicked;
            }


            void OnIconOnBalloonTipClosed(object sender, EventArgs e)
            {
                closeEvent?.Invoke();
                icon.BalloonTipClosed -= OnIconOnBalloonTipClosed;
            }
        }
        public static TrayIcon GetInstance()
        {
            return Me ?? (Me = new TrayIcon());
        }
    }
}

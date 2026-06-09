using TrafficWatch.Services;
using TrafficWatch.Services.Dashboard;
using TrafficWatch.Models.Dashboard;
using System;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Net;
using System.Collections.Generic;
using System.Linq;

namespace TrafficWatch
{
    /// <summary>
    /// Interaction logic for WindowSetting.xaml
    /// </summary>
    public partial class WindowSetting : Window
    {
        private readonly DashboardAddonService _addonService;
        private Server _server;
        
        public WindowSetting()
        {
            InitializeComponent();
            _addonService = DashboardAddonService.Instance;
            
            // ثبت هندلر رویداد تغییر وضعیت افزونه
            _addonService.OnAddonStateChanged += OnAddonStateChanged_Handler;
            
            Init();
            Network();
            LoadAddonsList();
            LoadDashboardSettings();
            LoadServerInfo();
            UpdateActiveAddonsTab();
            App.Pin(this);
            //CmbInterface
        }
        
        /// <summary>
        /// بارگذاری تنظیمات داشبورد از Settings
        /// </summary>
        private void LoadDashboardSettings()
        {
            chkWebServerEnabled.IsChecked = Properties.Settings.Default.WebServerEnabled;
            txtPort.Text = Properties.Settings.Default.HttpPort.ToString();
            chkSecurityEnabled.IsChecked = Properties.Settings.Default.SecurityEnabled;
            txtSecurityToken.Text = Properties.Settings.Default.SecurityToken;
            txtAllowedIPs.Text = Properties.Settings.Default.AllowedIPs;
            
            UpdateAccessInfo();
        }
        
        /// <summary>
        /// بروزرسانی اطلاعات دسترسی
        /// </summary>
        private void UpdateAccessInfo()
        {
            if (!chkWebServerEnabled.IsChecked.HasValue || !chkWebServerEnabled.IsChecked.Value)
            {
                txtAccessInfo.Text = "Server is currently disabled. Enable it to see access information.";
                return;
            }
            
            try
            {
                int port = int.Parse(txtPort.Text);
                string hostName = Dns.GetHostName();
                string localIp = GetLocalIPAddress();
                
                txtAccessInfo.Text = $"Server is running!\n\n" +
                                   $"Local Access: http://127.0.0.1:{port}/state/\n" +
                                   $"Network Access: http://{localIp}:{port}/state/\n" +
                                   $"Hostname: http://{hostName}:{port}/state/\n\n" +
                                   $"Security: {(chkSecurityEnabled.IsChecked.HasValue && chkSecurityEnabled.IsChecked.Value ? "Enabled" : "Disabled")}";
            }
            catch
            {
                txtAccessInfo.Text = "Invalid port number.";
            }
        }
        
        /// <summary>
        /// دریافت آدرس IP محلی
        /// </summary>
        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "Unknown";
        }
        
        /// <summary>
        /// بارگذاری لیست افزونه‌ها در تب Addons
        /// </summary>
        private void LoadAddonsList()
        {
            AddonsListPanel.Children.Clear();
            
            var addons = _addonService.GetAllAddons();
            
            foreach (var addon in addons)
            {
                var addonControl = CreateAddonControl(addon);
                AddonsListPanel.Children.Add(addonControl);
            }
        }
        
        /// <summary>
        /// بروزرسانی تب افزونه‌های فعال
        /// </summary>
        private void UpdateActiveAddonsTab()
        {
            ActiveAddonsPanel.Children.Clear();
            
            var enabledAddons = _addonService.GetEnabledAddons();
            
            if (enabledAddons.Count == 0)
            {
                ActiveAddonsPanel.Children.Add(new TextBlock
                {
                    Text = "No active addons",
                    Foreground = Brushes.Gray,
                    FontStyle = FontStyles.Italic
                });
                return;
            }
            
            foreach (var addon in enabledAddons)
            {
                var border = new Border
                {
                    BorderBrush = Brushes.Green,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(5),
                    Margin = new Thickness(0, 0, 0, 10),
                    Padding = new Thickness(10),
                    Background = Brushes.LightGreen
                };
                
                var stackPanel = new StackPanel();
                stackPanel.Children.Add(new TextBlock
                {
                    Text = addon.Name,
                    FontWeight = FontWeights.Bold,
                    FontSize = 14,
                    Foreground = Brushes.DarkGreen
                });
                
                stackPanel.Children.Add(new TextBlock
                {
                    Text = addon.Description,
                    FontSize = 12,
                    Foreground = Brushes.DarkGreen,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 5, 0, 0)
                });
                
                stackPanel.Children.Add(new TextBlock
                {
                    Text = $"Status: Active | Version: {addon.Version}",
                    FontSize = 11,
                    Foreground = Brushes.Green,
                    Margin = new Thickness(0, 5, 0, 0)
                });
                
                border.Child = stackPanel;
                ActiveAddonsPanel.Children.Add(border);
            }
        }
        
        /// <summary>
        /// بارگذاری اطلاعات سرور
        /// </summary>
        private void LoadServerInfo()
        {
            try
            {
                bool isServerEnabled = Properties.Settings.Default.WebServerEnabled;
                int port = Properties.Settings.Default.HttpPort;
                string localIp = GetLocalIPAddress();
                
                if (isServerEnabled)
                {
                    txtServerStatus.Text = "Server is RUNNING";
                    txtServerStatus.Foreground = Brushes.Green;
                    txtServerPort.Text = $"Port: {port}";
                    txtServerIP.Text = $"Local IP: {localIp}";
                }
                else
                {
                    txtServerStatus.Text = "Server is STOPPED";
                    txtServerStatus.Foreground = Brushes.Red;
                    txtServerPort.Text = $"Configured Port: {port}";
                    txtServerIP.Text = $"Local IP: {localIp}";
                }
            }
            catch (Exception ex)
            {
                txtServerStatus.Text = "Error: " + ex.Message;
                txtServerStatus.Foreground = Brushes.Red;
            }
        }
        
        /// <summary>
        /// ایجاد کنترل UI برای یک افزونه با قابلیت تنظیمات خاص
        /// </summary>
        private Border CreateAddonControl(AddonInfo addon)
        {
            var border = new Border
            {
                BorderBrush = (Brush)FindResource("WindowForeground"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(5),
                Margin = new Thickness(0, 0, 0, 10),
                Padding = new Thickness(10)
            };
            
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            
            // اطلاعات افزونه
            var infoStack = new StackPanel();
            infoStack.Children.Add(new TextBlock
            {
                Text = addon.Name,
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Foreground = (Brush)FindResource("WindowForeground")
            });
            
            infoStack.Children.Add(new TextBlock
            {
                Text = addon.Description,
                FontSize = 12,
                Foreground = Brushes.Gray,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 5, 0, 5)
            });
            
            var statusText = addon.IsInstalled ? "Installed" : "Not Installed";
            var statusColor = addon.IsInstalled ? Brushes.Green : Brushes.Red;
            
            infoStack.Children.Add(new TextBlock
            {
                Text = $"Status: {statusText} | Version: {addon.Version}",
                FontSize = 11,
                Foreground = statusColor
            });
            
            Grid.SetColumn(infoStack, 0);
            Grid.SetRow(infoStack, 0);
            
            // دکمه فعال/غیرفعال
            var toggleButton = new CheckBox
            {
                Content = "Enabled",
                IsChecked = addon.IsEnabled && addon.IsInstalled,
                IsEnabled = addon.IsInstalled,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0)
            };
            
            toggleButton.Checked += (s, e) =>
            {
                _addonService.SetAddonEnabled(addon.Id, true);
            };
            
            toggleButton.Unchecked += (s, e) =>
            {
                _addonService.SetAddonEnabled(addon.Id, false);
            };
            
            Grid.SetColumn(toggleButton, 1);
            Grid.SetRow(toggleButton, 0);
            
            // دکمه تنظیمات خاص افزونه
            var settingsButton = new Button
            {
                Content = "Settings",
                Width = 80,
                Height = 25,
                Margin = new Thickness(10, 10, 0, 0),
                IsEnabled = addon.IsInstalled
            };
            
            settingsButton.Click += (s, e) =>
            {
                ShowAddonSettings(addon);
            };
            
            Grid.SetColumn(settingsButton, 0);
            Grid.SetRow(settingsButton, 1);
            Grid.SetColumnSpan(settingsButton, 2);
            
            grid.Children.Add(infoStack);
            grid.Children.Add(toggleButton);
            grid.Children.Add(settingsButton);
            
            border.Child = grid;
            
            return border;
        }
        
        /// <summary>
        /// نمایش پنجره تنظیمات خاص برای یک افزونه
        /// </summary>
        private void ShowAddonSettings(AddonInfo addon)
        {
            var settingsWindow = new Window
            {
                Title = $"{addon.Name} - Settings",
                Width = 400,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };
            
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            
            var stackPanel = new StackPanel
            {
                Margin = new Thickness(15)
            };
            
            stackPanel.Children.Add(new TextBlock
            {
                Text = $"Settings for {addon.Name}",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 15)
            });
            
            // نمایش تنظیمات موجود
            if (addon.Settings != null && addon.Settings.Count > 0)
            {
                foreach (var setting in addon.Settings)
                {
                    var settingPanel = new StackPanel
                    {
                        Margin = new Thickness(0, 0, 0, 10)
                    };
                    
                    settingPanel.Children.Add(new TextBlock
                    {
                        Text = setting.Key,
                        FontWeight = FontWeights.SemiBold,
                        Margin = new Thickness(0, 0, 0, 5)
                    });
                    
                    if (setting.Value is bool boolValue)
                    {
                        var checkBox = new CheckBox
                        {
                            Content = "Enabled",
                            IsChecked = boolValue,
                            Tag = setting.Key
                        };
                        
                        checkBox.Checked += (s, e) =>
                        {
                            addon.Settings[setting.Key] = true;
                            _addonService.UpdateAddonSettings(addon.Id, addon.Settings);
                        };
                        
                        checkBox.Unchecked += (s, e) =>
                        {
                            addon.Settings[setting.Key] = false;
                            _addonService.UpdateAddonSettings(addon.Id, addon.Settings);
                        };
                        
                        settingPanel.Children.Add(checkBox);
                    }
                    else if (setting.Value is int intValue)
                    {
                        var textBox = new TextBox
                        {
                            Text = intValue.ToString(),
                            Width = 100,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            Tag = setting.Key
                        };
                        
                        textBox.LostFocus += (s, e) =>
                        {
                            if (int.TryParse(textBox.Text, out int value))
                            {
                                addon.Settings[setting.Key] = value;
                                _addonService.UpdateAddonSettings(addon.Id, addon.Settings);
                            }
                        };
                        
                        settingPanel.Children.Add(textBox);
                    }
                    else
                    {
                        var textBox = new TextBox
                        {
                            Text = setting.Value?.ToString() ?? "",
                            Width = 200,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            Tag = setting.Key
                        };
                        
                        textBox.LostFocus += (s, e) =>
                        {
                            addon.Settings[setting.Key] = textBox.Text;
                            _addonService.UpdateAddonSettings(addon.Id, addon.Settings);
                        };
                        
                        settingPanel.Children.Add(textBox);
                    }
                    
                    stackPanel.Children.Add(settingPanel);
                }
            }
            else
            {
                stackPanel.Children.Add(new TextBlock
                {
                    Text = "No specific settings available for this addon.",
                    Foreground = Brushes.Gray,
                    FontStyle = FontStyles.Italic
                });
            }
            
            // دکمه ذخیره و بستن
            var closeButton = new Button
            {
                Content = "Close",
                Width = 100,
                Height = 30,
                Margin = new Thickness(0, 15, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            
            closeButton.Click += (s, e) =>
            {
                settingsWindow.Close();
            };
            
            stackPanel.Children.Add(closeButton);
            scrollViewer.Content = stackPanel;
            settingsWindow.Content = scrollViewer;
            
            settingsWindow.ShowDialog();
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
        
        #region Dashboard Server Settings Event Handlers
        
        private void chkWebServerEnabled_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.WebServerEnabled = true;
            Properties.Settings.Default.Save();
            UpdateAccessInfo();
            LoadServerInfo();
            
            // راه‌اندازی مجدد سرور
            RestartServer();
        }
        
        private void chkWebServerEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.WebServerEnabled = false;
            Properties.Settings.Default.Save();
            UpdateAccessInfo();
            LoadServerInfo();
            
            // توقف سرور
            StopServer();
        }
        
        private void btnSavePort_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int port = int.Parse(txtPort.Text);
                if (port < 1 || port > 65535)
                {
                    MessageBox.Show("Port must be between 1 and 65535", "Invalid Port", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                Properties.Settings.Default.HttpPort = port;
                Properties.Settings.Default.Save();
                MessageBox.Show($"Port saved successfully: {port}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                UpdateAccessInfo();
                LoadServerInfo();
                
                // راه‌اندازی مجدد سرور اگر فعال است
                if (chkWebServerEnabled.IsChecked.HasValue && chkWebServerEnabled.IsChecked.Value)
                {
                    RestartServer();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Invalid port number: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void chkSecurityEnabled_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.SecurityEnabled = true;
            Properties.Settings.Default.Save();
            UpdateAccessInfo();
        }
        
        private void chkSecurityEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.SecurityEnabled = false;
            Properties.Settings.Default.Save();
            UpdateAccessInfo();
        }
        
        private void btnGenerateToken_Click(object sender, RoutedEventArgs e)
        {
            // تولید توکن امنیتی تصادفی
            var random = new Random();
            var tokenBytes = new byte[32];
            random.NextBytes(tokenBytes);
            var token = Convert.ToBase64String(tokenBytes).Replace("+", "-").Replace("/", "_").Substring(0, 32);
            
            txtSecurityToken.Text = token;
            Properties.Settings.Default.SecurityToken = token;
            Properties.Settings.Default.Save();
            MessageBox.Show("Security token generated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private void btnSaveSecurity_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.SecurityEnabled = chkSecurityEnabled.IsChecked ?? false;
            Properties.Settings.Default.SecurityToken = txtSecurityToken.Text;
            Properties.Settings.Default.AllowedIPs = txtAllowedIPs.Text;
            Properties.Settings.Default.Save();
            
            MessageBox.Show("Security settings saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            UpdateAccessInfo();
            LoadServerInfo();
        }
        
        /// <summary>
        /// هندلر تغییر وضعیت افزونه - بروزرسانی تب افزونه‌های فعال
        /// </summary>
        private void OnAddonStateChanged_Handler(object sender, AddonStateChangedEventArgs e)
        {
            Dispatcher.InvokeAsync(() =>
            {
                LoadAddonsList();
                UpdateActiveAddonsTab();
            });
        }
        
        /// <summary>
        /// راه‌اندازی مجدد سرور وب
        /// </summary>
        private void RestartServer()
        {
            try
            {
                // توقف سرور قبلی
                StopServer();
                
                // ایجاد و شروع سرور جدید
                _server = new Server();
                _server.Start();
                
                MessageBox.Show("Web dashboard server restarted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error restarting server: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// توقف سرور وب
        /// </summary>
        private void StopServer()
        {
            try
            {
                if (_server != null)
                {
                    _server.Abort();
                    _server = null;
                }
                
                // همچنین سرور اصلی برنامه را متوقف می‌کنیم
                if (App._Server != null)
                {
                    App._Server.Abort();
                    App._Server = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error stopping server: {ex.Message}");
            }
        }
        
        #endregion
    }
}

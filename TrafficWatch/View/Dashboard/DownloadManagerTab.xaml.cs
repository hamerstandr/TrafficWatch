using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using TrafficWatch.Models;
using TrafficWatch.Models.Dashboard;
using TrafficWatch.Services;

namespace TrafficWatch.View.Dashboard
{
    /// <summary>
    /// Interaction logic for DownloadManagerTab.xaml
    /// تب نمایش اطلاعات دانلود منیجر
    /// </summary>
    public partial class DownloadManagerTab : UserControl
    {
        private readonly AddonInfo _addonInfo;
        private readonly DownloadManagerService _downloadService;
        private readonly DispatcherTimer _refreshTimer;

        public DownloadManagerTab(AddonInfo addonInfo)
        {
            InitializeComponent();
            _addonInfo = addonInfo;
            _downloadService = new DownloadManagerService();
            
            // پیکربندی سرویس بر اساس تنظیمات افزونه
            ConfigureService();
            
            // تنظیم تایمر برای بروزرسانی خودکار
            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(GetRefreshInterval())
            };
            _refreshTimer.Tick += RefreshTimer_Tick;
            _refreshTimer.Start();
            
            // بروزرسانی اولیه
            RefreshData();
        }

        private void ConfigureService()
        {
            if (_addonInfo.IsEnabled && _addonInfo.IsInstalled)
            {
                _downloadService.SetEnabled(true);
                var apiEndpoint = Dashboard.Services.Dashboard.DashboardAddonService.Instance.GetAddonApiEndpoint(_addonInfo.Id);
                if (!string.IsNullOrEmpty(apiEndpoint))
                {
                    _downloadService.SetApiEndpoint(apiEndpoint);
                }
            }
            else
            {
                _downloadService.SetEnabled(false);
            }
        }

        private int GetRefreshInterval()
        {
            if (_addonInfo.Settings != null && _addonInfo.Settings.ContainsKey("RefreshInterval"))
            {
                return Convert.ToInt32(_addonInfo.Settings["RefreshInterval"]);
            }
            return 5; // پیش‌فرض 5 ثانیه
        }

        private async void RefreshTimer_Tick(object sender, EventArgs e)
        {
            await RefreshData();
        }

        private async System.Threading.Tasks.Task RefreshData()
        {
            try
            {
                var status = await _downloadService.GetStatusAsync();

                if (status.IsRunning)
                {
                    TxtStatus.Text = "Connected";
                    TxtStatus.Foreground = System.Windows.Media.Brushes.Green;
                    TxtActiveDownloads.Text = status.ActiveDownloads.ToString();
                    TxtDownloadSpeed.Text = FormatSpeed(status.TotalDownloadSpeed);

                    // دریافت لیست دانلودهای فعال
                    var downloads = await _downloadService.GetActiveDownloadsAsync();
                    UpdateDownloadsList(downloads);
                }
                else
                {
                    TxtStatus.Text = "Disconnected";
                    TxtStatus.Foreground = System.Windows.Media.Brushes.Red;
                    TxtActiveDownloads.Text = "0";
                    TxtDownloadSpeed.Text = "0 KB/s";
                    DownloadsList.Children.Clear();
                    DownloadsList.Children.Add(new TextBlock
                    {
                        Text = "DownloadMenger2 is not running or not installed",
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Foreground = System.Windows.Media.Brushes.Gray,
                        Margin = new Thickness(0, 20, 0, 0)
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing download manager data: {ex.Message}");
            }
        }

        private void UpdateDownloadsList(System.Collections.Generic.List<DownloadItem> downloads)
        {
            DownloadsList.Children.Clear();

            if (downloads == null || downloads.Count == 0)
            {
                DownloadsList.Children.Add(new TextBlock
                {
                    Text = "No active downloads",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = System.Windows.Media.Brushes.Gray
                });
                return;
            }

            int maxItems = 10;
            if (_addonInfo.Settings != null && _addonInfo.Settings.ContainsKey("MaxItemsDisplay"))
            {
                maxItems = Convert.ToInt32(_addonInfo.Settings["MaxItemsDisplay"]);
            }

            int count = 0;
            foreach (var download in downloads)
            {
                if (count >= maxItems) break;

                var itemPanel = new StackPanel
                {
                    Margin = new Thickness(0, 5, 0, 5),
                    Orientation = Orientation.Vertical
                };

                var nameText = new TextBlock
                {
                    Text = System.IO.Path.GetFileName(download.FileName),
                    FontWeight = FontWeights.Bold,
                    Foreground = (System.Windows.Media.Brush)FindResource("WindowForeground")
                };

                var progressText = new TextBlock
                {
                    Text = $"{download.Progress:F1}% - {FormatSpeed(download.Speed)}",
                    FontSize = 12,
                    Foreground = (System.Windows.Media.Brush)FindResource("WindowForeground")
                };

                var progressBar = new ProgressBar
                {
                    Value = download.Progress,
                    Minimum = 0,
                    Maximum = 100,
                    Height = 8,
                    Margin = new Thickness(0, 3, 0, 0)
                };

                itemPanel.Children.Add(nameText);
                itemPanel.Children.Add(progressText);
                itemPanel.Children.Add(progressBar);

                DownloadsList.Children.Add(itemPanel);
                count++;
            }
        }

        private string FormatSpeed(double bytesPerSecond)
        {
            if (bytesPerSecond < 1024)
                return $"{bytesPerSecond:F0} B/s";
            if (bytesPerSecond < 1024 * 1024)
                return $"{bytesPerSecond / 1024:F1} KB/s";
            return $"{bytesPerSecond / (1024 * 1024):F1} MB/s";
        }

        public void Refresh()
        {
            RefreshData();
        }
    }
}

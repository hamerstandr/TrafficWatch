using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Threading;
using TrafficWatch.Models.Dashboard;

namespace TrafficWatch.View.Dashboard
{
    /// <summary>
    /// Interaction logic for SystemMonitorTab.xaml
    /// تب نمایش اطلاعات مانیتورینگ سیستم
    /// </summary>
    public partial class SystemMonitorTab : UserControl
    {
        private readonly AddonInfo _addonInfo;
        private readonly DispatcherTimer _refreshTimer;
        private PerformanceCounter _cpuCounter;
        private PerformanceCounter _ramCounter;

        public SystemMonitorTab(AddonInfo addonInfo)
        {
            InitializeComponent();
            _addonInfo = addonInfo;
            
            InitializePerformanceCounters();
            
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

        private void InitializePerformanceCounters()
        {
            try
            {
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing performance counters: {ex.Message}");
            }
        }

        private int GetRefreshInterval()
        {
            if (_addonInfo.Settings != null && _addonInfo.Settings.ContainsKey("RefreshInterval"))
            {
                return Convert.ToInt32(_addonInfo.Settings["RefreshInterval"]);
            }
            return 2; // پیش‌فرض 2 ثانیه
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            RefreshData();
        }

        private void RefreshData()
        {
            try
            {
                // CPU
                if (_addonInfo.Settings == null || !_addonInfo.Settings.ContainsKey("ShowCPU") || 
                    (_addonInfo.Settings.ContainsKey("ShowCPU") && Convert.ToBoolean(_addonInfo.Settings["ShowCPU"])))
                {
                    float cpuValue = _cpuCounter?.NextValue() ?? 0;
                    TxtCPU.Text = $"{cpuValue:F1}%";
                    
                    // تغییر رنگ بر اساس میزان مصرف
                    if (cpuValue > 80)
                        TxtCPU.Foreground = System.Windows.Media.Brushes.Red;
                    else if (cpuValue > 50)
                        TxtCPU.Foreground = System.Windows.Media.Brushes.Orange;
                    else
                        TxtCPU.Foreground = System.Windows.Media.Brushes.Green;
                }

                // RAM
                if (_addonInfo.Settings == null || !_addonInfo.Settings.ContainsKey("ShowRAM") || 
                    (_addonInfo.Settings.ContainsKey("ShowRAM") && Convert.ToBoolean(_addonInfo.Settings["ShowRAM"])))
                {
                    var totalRam = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / (1024 * 1024);
                    var availableRam = _ramCounter?.NextValue() ?? 0;
                    var usedRam = totalRam - availableRam;
                    var ramPercent = (usedRam / (double)totalRam) * 100;
                    
                    TxtRAM.Text = $"{usedRam:F0} MB / {totalRam:F0} MB ({ramPercent:F1}%)";
                    
                    if (ramPercent > 80)
                        TxtRAM.Foreground = System.Windows.Media.Brushes.Red;
                    else if (ramPercent > 50)
                        TxtRAM.Foreground = System.Windows.Media.Brushes.Orange;
                    else
                        TxtRAM.Foreground = System.Windows.Media.Brushes.Green;
                }

                // Disk
                if (_addonInfo.Settings == null || !_addonInfo.Settings.ContainsKey("ShowDisk") || 
                    (_addonInfo.Settings.ContainsKey("ShowDisk") && Convert.ToBoolean(_addonInfo.Settings["ShowDisk"])))
                {
                    var drive = System.IO.DriveInfo.GetDrives()[0];
                    var totalSpace = drive.TotalSize / (1024 * 1024 * 1024);
                    var freeSpace = drive.AvailableFreeSpace / (1024 * 1024 * 1024);
                    var usedSpace = totalSpace - freeSpace;
                    var diskPercent = (usedSpace / (double)totalSpace) * 100;
                    
                    TxtDisk.Text = $"{usedSpace:F1} GB / {totalSpace:F1} GB ({diskPercent:F1}%)";
                }

                // Network
                if (_addonInfo.Settings == null || !_addonInfo.Settings.ContainsKey("ShowNetwork") || 
                    (_addonInfo.Settings.ContainsKey("ShowNetwork") && Convert.ToBoolean(_addonInfo.Settings["ShowNetwork"])))
                {
                    long bytesSent = 0;
                    long bytesReceived = 0;
                    
                    foreach (var nic in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
                    {
                        if (nic.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up)
                        {
                            var stats = nic.GetIPv4Statistics();
                            bytesSent += stats.BytesSent;
                            bytesReceived += stats.BytesReceived;
                        }
                    }
                    
                    TxtNetwork.Text = $"↑ {FormatBytes(bytesSent)} | ↓ {FormatBytes(bytesReceived)}";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error refreshing system monitor data: {ex.Message}");
            }
        }

        private string FormatBytes(long bytes)
        {
            if (bytes < 1024)
                return $"{bytes} B";
            if (bytes < 1024 * 1024)
                return $"{bytes / 1024.0:F1} KB";
            if (bytes < 1024 * 1024 * 1024)
                return $"{bytes / (1024 * 1024.0):F1} MB";
            return $"{bytes / (1024 * 1024 * 1024.0):F1} GB";
        }

        public void Refresh()
        {
            RefreshData();
        }
    }
}

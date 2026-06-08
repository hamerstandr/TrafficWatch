using System;
using System.Net.Http;
using System.Threading.Tasks;
using TrafficWatch.Models;
using Newtonsoft.Json;

namespace TrafficWatch.Services
{
    /// <summary>
    /// سرویس ارتباط با DownloadMenger2
    /// این سرویس اطلاعات دانلود منیجر را از طریق API دریافت می‌کند
    /// </summary>
    public class DownloadManagerService
    {
        private readonly HttpClient _httpClient;
        private string _apiEndpoint = "http://127.0.0.1:9090";
        private bool _isEnabled = false;
        private DownloadManagerInfo _lastKnownState = new DownloadManagerInfo { IsRunning = false };

        public DownloadManagerService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(2); // Timeout کوتاه برای جلوگیری از کندی
        }

        /// <summary>
        /// فعال یا غیرفعال کردن سرویس
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            _isEnabled = enabled;
        }

        /// <summary>
        /// تنظیم آدرس API
        /// </summary>
        public void SetApiEndpoint(string endpoint)
        {
            _apiEndpoint = endpoint.TrimEnd('/');
        }

        /// <summary>
        /// بررسی وضعیت دانلود منیجر
        /// </summary>
        public async Task<DownloadManagerInfo> GetStatusAsync()
        {
            if (!_isEnabled)
            {
                return new DownloadManagerInfo 
                { 
                    IsRunning = false,
                    ApiEndpoint = _apiEndpoint
                };
            }

            try
            {
                var response = await _httpClient.GetAsync($"{_apiEndpoint}/api/status");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var status = JsonConvert.DeserializeObject<DownloadManagerInfo>(content);
                    
                    if (status != null)
                    {
                        status.IsRunning = true;
                        _lastKnownState = status;
                        return status;
                    }
                }
            }
            catch (Exception ex)
            {
                // خطا را لاگ می‌کنیم اما برنامه کرش نمی‌کند
                System.Diagnostics.Debug.WriteLine($"DownloadManager connection error: {ex.Message}");
            }

            // اگر ارتباط برقرار نشد، آخرین وضعیت شناخته شده را برمی‌گردانیم
            // اما IsRunning را false می‌کنیم
            var result = _lastKnownState.Clone();
            result.IsRunning = false;
            return result;
        }

        /// <summary>
        /// دریافت لیست دانلودهای فعال
        /// </summary>
        public async Task<System.Collections.Generic.List<DownloadItem>> GetActiveDownloadsAsync()
        {
            if (!_isEnabled)
            {
                return new System.Collections.Generic.List<DownloadItem>();
            }

            try
            {
                var response = await _httpClient.GetAsync($"{_apiEndpoint}/api/downloads/active");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var downloads = JsonConvert.DeserializeObject<System.Collections.Generic.List<DownloadItem>>(content);
                    
                    return downloads ?? new System.Collections.Generic.List<DownloadItem>();
                }
            }
            catch (Exception)
            {
                // Silent fail
            }

            return new System.Collections.Generic.List<DownloadItem>();
        }

        /// <summary>
        /// بررسی وجود دانلود منیجر در سیستم
        /// </summary>
        public static bool IsDownloadMengerInstalled()
        {
            // بررسی مسیرهای نصب معمول
            string[] possiblePaths = new[]
            {
                @"C:\Program Files\DownloadMenger2\DownloadMenger2.exe",
                @"C:\Program Files (x86)\DownloadMenger2\DownloadMenger2.exe",
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\DownloadMenger2\DownloadMenger2.exe"
            };

            foreach (var path in possiblePaths)
            {
                if (System.IO.File.Exists(path))
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// متد کمکی برای کلون کردن آبجکت
    /// </summary>
    public static class DownloadManagerInfoExtensions
    {
        public static DownloadManagerInfo Clone(this DownloadManagerInfo info)
        {
            return new DownloadManagerInfo
            {
                IsRunning = false, // همیشه false برای حالت disconnected
                Version = info.Version,
                ActiveDownloads = info.ActiveDownloads,
                QueuedDownloads = info.QueuedDownloads,
                CompletedDownloads = info.CompletedDownloads,
                TotalDownloadSpeed = info.TotalDownloadSpeed,
                TotalUploadedSpeed = info.TotalUploadedSpeed,
                DownloadLimit = info.DownloadLimit,
                UploadLimit = info.UploadLimit,
                SchedulerEnabled = info.SchedulerEnabled,
                ClipboardMonitorEnabled = info.ClipboardMonitorEnabled,
                BrowserIntegrationEnabled = info.BrowserIntegrationEnabled,
                LastError = info.LastError,
                ApiEndpoint = info.ApiEndpoint
            };
        }
    }
}

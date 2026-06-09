using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TrafficWatch.Models.Config;

namespace TrafficWatch.Services.Network
{
    /// <summary>
    /// سرویس امنیتی برای مدیریت ارتباطات شبکه داخلی (LAN)
    /// شامل: اعتبارسنجی IP، Rate Limiting، لاگ‌گیری امنیتی
    /// </summary>
    public class NetworkSecurityService
    {
        private static NetworkSecurityService? _instance;
        private static readonly object LockObj = new();
        
        private readonly NetworkSettings _settings;
        private readonly ConcurrentDictionary<string, RequestLogEntry> _requestLogs;
        private readonly object _logLock = new();
        
        public static NetworkSecurityService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (LockObj)
                    {
                        _instance ??= new NetworkSecurityService();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// رویداد ثبت خطای امنیتی
        /// </summary>
        public event EventHandler<SecurityViolationEventArgs>? OnSecurityViolation;

        private NetworkSecurityService()
        {
            _settings = NetworkSettings.Load();
            _requestLogs = new ConcurrentDictionary<string, RequestLogEntry>();
            
            // پاک‌سازی دوره‌ای لاگ‌های قدیمی
            Task.Run(async () => await CleanupOldLogsAsync());
        }

        /// <summary>
        /// اعتبارسنجی درخواست ورودی از شبکه
        /// </summary>
        /// <param name="clientIp">آدرس IP کلاینت درخواست‌دهنده</param>
        /// <param name="providedToken">توکن ارائه شده توسط کلاینت</param>
        /// <returns>نتیج اعتبارسنجی</returns>
        public SecurityValidationResult ValidateRequest(string clientIp, string? providedToken = null)
        {
            var result = new SecurityValidationResult();

            // 1. بررسی IP در لیست سفید
            if (!_settings.IsIpAllowed(clientIp))
            {
                LogSecurityViolation(clientIp, "IP_NOT_ALLOWED", $"IP {clientIp} not in whitelist");
                result.IsValid = false;
                result.ErrorMessage = "IP address not authorized";
                return result;
            }

            // 2. بررسی توکن احراز هویت
            if (!string.IsNullOrWhiteSpace(_settings.ApiAuthToken))
            {
                if (string.IsNullOrWhiteSpace(providedToken) || providedToken != _settings.ApiAuthToken)
                {
                    LogSecurityViolation(clientIp, "INVALID_TOKEN", "Invalid or missing API token");
                    result.IsValid = false;
                    result.ErrorMessage = "Invalid authentication token";
                    return result;
                }
            }

            // 3. بررسی Rate Limiting
            if (!CheckRateLimit(clientIp))
            {
                LogSecurityViolation(clientIp, "RATE_LIMIT_EXCEEDED", $"Exceeded {_settings.MaxRequestsPerMinute} requests/minute");
                result.IsValid = false;
                result.ErrorMessage = "Rate limit exceeded";
                return result;
            }

            result.IsValid = true;
            result.Message = "Request validated successfully";
            return result;
        }

        /// <summary>
        /// بررسی محدودیت نرخ درخواست‌ها
        /// </summary>
        private bool CheckRateLimit(string clientIp)
        {
            var now = DateTime.UtcNow;
            var windowStart = now.AddMinutes(-1);

            lock (_logLock)
            {
                if (!_requestLogs.TryGetValue(clientIp, out var entry))
                {
                    entry = new RequestLogEntry();
                    _requestLogs[clientIp] = entry;
                }

                // حذف درخواست‌های قدیمی خارج از پنجره زمانی
                entry.RequestTimes = entry.RequestTimes.Where(t => t > windowStart).ToList();

                // بررسی تعداد درخواست‌ها
                if (entry.RequestTimes.Count >= _settings.MaxRequestsPerMinute)
                {
                    return false;
                }

                // ثبت درخواست جدید
                entry.RequestTimes.Add(now);
                return true;
            }
        }

        /// <summary>
        /// تولید توکن امنیتی جدید
        /// </summary>
        public static string GenerateSecureToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// هش کردن توکن برای ذخیره‌سازی امن
        /// </summary>
        public static string HashToken(string token)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(token);
            var hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// ثبت نقض امنیتی
        /// </summary>
        private void LogSecurityViolation(string ip, string violationType, string details)
        {
            if (_settings.EnableSecurityLogging)
            {
                Debug.WriteLine($"[SECURITY VIOLATION] [{violationType}] IP: {ip} - {details}");
                
                // اینجا می‌توان به فایل لاگ یا سیستم مانیتورینگ نوشت
                
                OnSecurityViolation?.Invoke(this, new SecurityViolationEventArgs
                {
                    IpAddress = ip,
                    ViolationType = violationType,
                    Details = details,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// پاک‌سازی دوره‌ای لاگ‌های قدیمی
        /// </summary>
        private async Task CleanupOldLogsAsync()
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromMinutes(5));
                
                lock (_logLock)
                {
                    var keysToRemove = _requestLogs
                        .Where(kvp => kvp.Value.RequestTimes.All(t => t < DateTime.UtcNow.AddMinutes(-5)))
                        .Select(kvp => kvp.Key)
                        .ToList();

                    foreach (var key in keysToRemove)
                    {
                        _requestLogs.TryRemove(key, out _);
                    }
                }
            }
        }

        /// <summary>
        /// دریافت تنظیمات فعلی
        /// </summary>
        public NetworkSettings GetSettings() => _settings;

        /// <summary>
        /// به‌روزرسانی تنظیمات
        /// </summary>
        public void UpdateSettings(NetworkSettings newSettings)
        {
            newSettings.Save();
            // ری‌لود تنظیمات (در نسخه پیشرفته‌تر می‌توان بدون ری‌لود اعمال کرد)
        }
    }

    #region Supporting Classes

    public class SecurityValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Message { get; set; }
    }

    public class RequestLogEntry
    {
        public List<DateTime> RequestTimes { get; set; } = new();
    }

    public class SecurityViolationEventArgs : EventArgs
    {
        public string IpAddress { get; set; } = string.Empty;
        public string ViolationType { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    #endregion
}

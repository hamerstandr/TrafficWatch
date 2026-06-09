using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
using System.Net;

namespace TrafficWatch.Models.Config
{
    /// <summary>
    /// تنظیمات پیشرفته برای ارتباط از طریق شبکه داخلی (LAN)
    /// </summary>
    public class NetworkSettings
    {
        // مسیر پیش‌فرض ذخیره‌سازی تنظیمات امنیتی
        private static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TrafficWatch", "config", "network_settings.json");

        #region Properties

        /// <summary>
        /// آدرس IP سرور اصلی در شبکه داخلی (مثلاً 192.168.1.50)
        /// خالی بودن به معنی تلاش برای تشخیص خودکار یا استفاده از Broadcast است.
        /// </summary>
        public string ServerIpAddress { get; set; } = string.Empty;

        /// <summary>
        /// پورت سرویس دهی در شبکه
        /// </summary>
        public int ServerPort { get; set; } = 9090;

        /// <summary>
        /// توکن احراز هویت برای ارتباط با سرور مرکزی
        /// </summary>
        public string ApiAuthToken { get; set; } = string.Empty;

        /// <summary>
        /// لیست سفید IPهایی که مجاز به اتصال به این کلاینت هستند (برای دریافت داده)
        /// پشتیبانی از CIDR (مثلاً 192.168.1.0/24)
        /// </summary>
        public List<string> AllowedIpRanges { get; set; } = new List<string>
        {
            "192.168.1.0/24", // مثال پیش‌فرض
            "10.0.0.0/8"
        };

        /// <summary>
        /// فعال‌سازی پروتکل HTTPS/TLS برای ارتباط امن در شبکه
        /// </summary>
        public bool UseSecureConnection { get; set; } = true;

        /// <summary>
        /// زمان انقضای توکن به دقیقه (برای چرخش خودکار)
        /// </summary>
        public int TokenExpirationMinutes { get; set; } = 60;

        /// <summary>
        /// حداکثر تعداد درخواست مجاز در دقیقه (Rate Limiting)
        /// </summary>
        public int MaxRequestsPerMinute { get; set; } = 100;

        /// <summary>
        /// فعال‌سازی لاگ‌گیری رویدادهای امنیتی شبکه
        /// </summary>
        public bool EnableSecurityLogging { get; set; } = true;

        /// <summary>
        /// تایم‌اوت اتصال به میلی‌ثانیه
        /// </summary>
        public int ConnectionTimeoutMs { get; set; } = 5000;

        #endregion

        #region Methods

        /// <summary>
        /// بررسی می‌کند که آیا یک IP خاص در لیست سفید قرار دارد یا خیر
        /// </summary>
        public bool IsIpAllowed(string ipAddress)
        {
            if (!IPAddress.TryParse(ipAddress, out var targetIp))
                return false;

            foreach (var range in AllowedIpRanges)
            {
                if (IsIpInCidrRange(targetIp, range))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// منطق بررسی IP در محدوده CIDR
        /// </summary>
        private bool IsIpInCidrRange(IPAddress ip, string cidr)
        {
            var parts = cidr.Split('/');
            if (parts.Length != 2) return false;

            if (!IPAddress.TryParse(parts[0], out var networkIp)) return false;
            if (!int.TryParse(parts[1], out var prefixLength)) return false;

            var ipBytes = ip.GetAddressBytes();
            var networkBytes = networkIp.GetAddressBytes();

            // تبدیل به باینری و مقایسه بیت‌ها بر اساس Prefix Length
            long ipNum = BitConverter.ToInt32(ipBytes.Reverse().ToArray(), 0);
            long networkNum = BitConverter.ToInt32(networkBytes.Reverse().ToArray(), 0);
            
            // Mask creation
            uint mask = (uint.MaxValue << (32 - prefixLength));
            
            return (ipNum & mask) == (networkNum & mask);
        }

        /// <summary>
        /// تولید URL پایه بر اساس تنظیمات
        /// </summary>
        public string GetBaseUrl()
        {
            var protocol = UseSecureConnection ? "https" : "http";
            var ip = string.IsNullOrWhiteSpace(ServerIpAddress) ? "*" : ServerIpAddress;
            return $"{protocol}://{ip}:{ServerPort}";
        }

        /// <summary>
        /// ذخیره تنظیمات در فایل JSON رمزنگاری شده (اختیاری) یا ساده
        /// </summary>
        public void Save()
        {
            var directory = Path.GetDirectoryName(ConfigPath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigPath, json);
        }

        /// <summary>
        /// بارگذاری تنظیمات از فایل
        /// </summary>
        public static NetworkSettings Load()
        {
            if (!File.Exists(ConfigPath))
                return new NetworkSettings();

            try
            {
                var json = File.ReadAllText(ConfigPath);
                return JsonSerializer.Deserialize<NetworkSettings>(json) ?? new NetworkSettings();
            }
            catch
            {
                return new NetworkSettings();
            }
        }

        #endregion
    }
}

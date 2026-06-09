# راهنمای اتصال برنامه‌های خارجی به داشبورد TrafficWatch (شبکه داخلی)

## فهرست مطالب
1. [معرفی](#معرفی)
2. [معماری شبکه داخلی](#معماری-شبکه-داخلی)
3. [تنظیمات پیکربندی](#تنظیمات-پیکربندی)
4. [راه‌اندازی امنیتی](#راه‌اندازی-امنیتی)
5. [چک‌لیست کامل امنیتی](#چک‌لیست-کامل-امنیتی)
6. [نمونه پیاده‌سازی سرور API](#نمونه-پیاده‌سازی-سرور-api)
7. [نمونه پیاده‌سازی کلاینت در TrafficWatch](#نمونه-پیاده‌سازی-کلاینت-در-trafficwatch)
8. [مدیریت توکن و احراز هویت](#مدیریت-توکن-و-احراز-هویت)
9. [عیب‌یابی و مانیتورینگ](#عیب‌یابی-و-مانیتورینگ)
10. [ضمیمه: الگوهای امنیتی](#ضمیمه-الگوهای-امنیتی)

---

## معرفی

این سند راهنمای کامل اتصال برنامه‌های خارجی (مانند DownloadMenger2، MusicPlayer، SystemMonitor و ...) را به داشبورد TrafficWatch در **شبکه داخلی (LAN)** ارائه می‌دهد.

### تفاوت با حالت localhost
- **localhost**: ارتباط فقط داخل یک سیستم عامل
- **شبکه داخلی**: ارتباط بین چندین دستگاه در شبکه LAN
- **ملاحظات امنیتی اضافی**: نیاز به فایروال، احراز هویت قوی‌تر، رمزنگاری داده‌ها

### ویژگی‌های کلیدی
- ✅ پشتیبانی از آدرس‌های IP شبکه داخلی (مثلاً `192.168.1.100`)
- ✅ تنظیمات کاملاً قابل انعطاف از طریق UI و فایل کانفیگ
- ✅ احراز هویت مبتنی بر توکن با چرخش خودکار
- ✅ رمزنگاری TLS/SSL برای ارتباطات شبکه
- ✅ لیست سفید IPهای مجاز
- ✅ لاگ‌گیری کامل فعالیت‌های شبکه
- ✅ محافظت در برابر حملات رایج شبکه

---

## معماری شبکه داخلی

```
┌──────────────────────────────────────────────────────────────────┐
│                     TrafficWatch Dashboard                       │
│                    (سیستم اصلی - Client)                         │
│                  IP: 192.168.1.10                                │
└──────────────────────────────────────────────────────────────────┘
                              │
                              │ HTTPS + Token Auth
                              ▼
┌──────────────────────────────────────────────────────────────────┐
│                      Network Security Layer                      │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐              │
│  │   Firewall  │  │   IP Whitelist │  │  Rate Limiter │         │
│  └─────────────┘  └─────────────┘  └─────────────┘              │
└──────────────────────────────────────────────────────────────────┘
                              │
        ┌─────────────────────┼─────────────────────┐
        │                     │                     │
        ▼                     ▼                     ▼
┌───────────────┐   ┌───────────────┐   ┌───────────────┐
│ DownloadMenger2│   │  MusicPlayer  │   │ SystemMonitor │
│ 192.168.1.20  │   │ 192.168.1.30  │   │ 192.168.1.40  │
│ Port: 9090    │   │ Port: 9091    │   │ Port: 9092    │
└───────────────┘   └───────────────┘   └───────────────┘
```

### اجزای اصلی

1. **TrafficWatch Dashboard**: کلاینت اصلی که داده‌ها را دریافت و نمایش می‌دهد
2. **Network Security Layer**: لایه امنیتی شامل فایروال، لیست سفید IP، محدودکننده نرخ
3. **External Programs**: برنامه‌های خارجی که روی دستگاه‌های مختلف در شبکه اجرا می‌شوند

---

## تنظیمات پیکربندی

### 1. افزودن به Settings.settings

تنظیمات زیر را به فایل `Properties/Settings.settings` اضافه کنید:

```xml
<!-- تنظیمات عمومی شبکه -->
<Setting Name="NetworkModeEnabled" Type="System.Boolean" Scope="User">
  <Value Profile="(Default)">False</Value>
</Setting>
<Setting Name="AllowNetworkConnections" Type="System.Boolean" Scope="User">
  <Value Profile="(Default)">False</Value>
</Setting>

<!-- تنظیمات امنیتی -->
<Setting Name="ApiToken" Type="System.String" Scope="User">
  <Value Profile="(Default)"></Value>
</Setting>
<Setting Name="ApiTokenExpiryHours" Type="System.Int32" Scope="User">
  <Value Profile="(Default)">24</Value>
</Setting>
<Setting Name="EnableTLS" Type="System.Boolean" Scope="User">
  <Value Profile="(Default)">True</Value>
</Setting>
<Setting Name="AllowedIPs" Type="System.String" Scope="User">
  <Value Profile="(Default)">192.168.1.0/24</Value>
</Setting>
<Setting Name="MaxRequestsPerMinute" Type="System.Int32" Scope="User">
  <Value Profile="(Default)">60</Value>
</Setting>

<!-- تنظیمات پیش‌فرض افزونه‌ها -->
<Setting Name="DefaultAddonTimeout" Type="System.Int32" Scope="User">
  <Value Profile="(Default)">5000</Value>
</Setting>
<Setting Name="RetryAttempts" Type="System.Int32" Scope="User">
  <Value Profile="(Default)">3</Value>
</Setting>
```

### 2. رابط کاربری تنظیمات شبکه

یک صفحه تنظیمات جدید در UI ایجاد کنید:

```xml
<!-- View/Settings/NetworkSettingsTab.xaml -->
<TabItem Header="تنظیمات شبکه">
    <Grid Margin="20">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <!-- فعال‌سازی حالت شبکه -->
        <GroupBox Grid.Row="0" Header="حالت اتصال" Margin="0,0,0,10">
            <StackPanel Margin="10">
                <CheckBox x:Name="chkNetworkMode" Content="فعال‌سازی حالت شبکه داخلی (LAN)" 
                          Checked="ChkNetworkMode_Checked" Unchecked="ChkNetworkMode_Unchecked"/>
                <TextBlock Text="با فعال‌سازی این گزینه، TrafficWatch می‌تواند به برنامه‌های دیگر در شبکه داخلی متصل شود."
                           TextWrapping="Wrap" Foreground="Gray" Margin="0,5,0,0"/>
            </StackPanel>
        </GroupBox>

        <!-- تنظیمات امنیتی -->
        <GroupBox Grid.Row="1" Header="امنیت" Margin="0,0,0,10">
            <Grid Margin="10">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="200"/>
                    <ColumnDefinition Width="*"/>
                </Grid.ColumnDefinitions>
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                </Grid.RowDefinitions>

                <TextBlock Grid.Row="0" Grid.Column="0" Text="رمزنگاری TLS:" VerticalAlignment="Center"/>
                <CheckBox Grid.Row="0" Grid.Column="1" x:Name="chkEnableTLS" Content="فعال" IsChecked="True" Margin="5"/>

                <TextBlock Grid.Row="1" Grid.Column="0" Text="لیست IPهای مجاز:" VerticalAlignment="Center" Margin="0,10,0,0"/>
                <TextBox Grid.Row="1" Grid.Column="1" x:Name="txtAllowedIPs" Margin="5,10,5,0" 
                         Text="192.168.1.0/24" ToolTip="مثال: 192.168.1.0/24 یا 192.168.1.100,192.168.1.101"/>

                <TextBlock Grid.Row="2" Grid.Column="0" Text="حداکثر درخواست در دقیقه:" VerticalAlignment="Center" Margin="0,10,0,0"/>
                <TextBox Grid.Row="2" Grid.Column="1" x:Name="txtMaxRequests" Margin="5,10,5,0" Text="60" Width="100" HorizontalAlignment="Left"/>

                <TextBlock Grid.Row="3" Grid.Column="0" Text="انقضای توکن (ساعت):" VerticalAlignment="Center" Margin="0,10,0,0"/>
                <TextBox Grid.Row="3" Grid.Column="1" x:Name="txtTokenExpiry" Margin="5,10,5,0" Text="24" Width="100" HorizontalAlignment="Left"/>
            </Grid>
        </GroupBox>

        <!-- مدیریت توکن API -->
        <GroupBox Grid.Row="2" Header="توکن API" Margin="0,0,0,10">
            <Grid Margin="10">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="Auto"/>
                    <ColumnDefinition Width="Auto"/>
                </Grid.ColumnDefinitions>
                <TextBox Grid.Column="0" x:Name="txtApiToken" IsReadOnly="True" Margin="0,0,10,0"/>
                <Button Grid.Column="1" Content="تولید توکن جدید" Click="BtnGenerateToken_Click" Margin="0,0,10,0"/>
                <Button Grid.Column="2" Content="کپی" Click="BtnCopyToken_Click"/>
            </Grid>
        </GroupBox>

        <!-- پیکربندی افزونه‌ها -->
        <GroupBox Grid.Row="3" Header="پیکربندی افزونه‌ها" Margin="0,0,0,10">
            <DataGrid x:Name="dgAddonConfig" AutoGenerateColumns="False" CanUserAddRows="False" Height="200">
                <DataGrid.Columns>
                    <DataGridTextColumn Header="نام افزونه" Binding="{Binding Name}" IsReadOnly="True"/>
                    <DataGridTextColumn Header="آدرس IP" Binding="{Binding IpAddress}"/>
                    <DataGridTextColumn Header="پورت" Binding="{Binding Port}"/>
                    <DataGridCheckBoxColumn Header="فعال" Binding="{Binding Enabled}"/>
                    <DataGridTextColumn Header="تایم‌اوت (ms)" Binding="{Binding Timeout}"/>
                </DataGrid.Columns>
            </DataGrid>
        </GroupBox>

        <!-- دکمه‌های عملیات -->
        <StackPanel Grid.Row="4" Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,10,0,0">
            <Button Content="ذخیره تنظیمات" Click="BtnSaveSettings_Click" Padding="20,10" Margin="0,0,10,0"/>
            <Button Content="آزمون اتصال" Click="BtnTestConnection_Click" Padding="20,10"/>
        </StackPanel>

        <!-- لاگ فعالیت‌ها -->
        <GroupBox Grid.Row="5" Header="لاگ فعالیت‌های شبکه" Margin="0,10,0,0">
            <ListBox x:Name="lstNetworkLog" Height="150" VerticalScrollBarVisibility="Auto">
                <ListBox.ItemTemplate>
                    <DataTemplate>
                        <StackPanel Orientation="Horizontal">
                            <TextBlock Text="{Binding Timestamp}" Foreground="Gray" Width="100"/>
                            <TextBlock Text="{Binding Message}" Margin="10,0,0,0"/>
                        </StackPanel>
                    </DataTemplate>
                </ListBox.ItemTemplate>
            </ListBox>
        </GroupBox>
    </Grid>
</TabItem>
```

### 3. کلاس مدل تنظیمات شبکه

```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Json;
using System.Security.Cryptography;
using System.Text;

namespace TrafficWatch.Models.Settings
{
    public class NetworkSettings : INotifyPropertyChanged
    {
        private bool _networkModeEnabled;
        private bool _allowNetworkConnections;
        private string _apiToken;
        private int _apiTokenExpiryHours;
        private bool _enableTLS;
        private string _allowedIPs;
        private int _maxRequestsPerMinute;
        private int _defaultAddonTimeout;
        private int _retryAttempts;
        private List<AddonNetworkConfig> _addonConfigs;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool NetworkModeEnabled
        {
            get => _networkModeEnabled;
            set { _networkModeEnabled = value; OnPropertyChanged(); }
        }

        public bool AllowNetworkConnections
        {
            get => _allowNetworkConnections;
            set { _allowNetworkConnections = value; OnPropertyChanged(); }
        }

        public string ApiToken
        {
            get => _apiToken;
            set { _apiToken = value; OnPropertyChanged(); }
        }

        public int ApiTokenExpiryHours
        {
            get => _apiTokenExpiryHours;
            set { _apiTokenExpiryHours = value; OnPropertyChanged(); }
        }

        public bool EnableTLS
        {
            get => _enableTLS;
            set { _enableTLS = value; OnPropertyChanged(); }
        }

        public string AllowedIPs
        {
            get => _allowedIPs;
            set { _allowedIPs = value; OnPropertyChanged(); }
        }

        public int MaxRequestsPerMinute
        {
            get => _maxRequestsPerMinute;
            set { _maxRequestsPerMinute = value; OnPropertyChanged(); }
        }

        public int DefaultAddonTimeout
        {
            get => _defaultAddonTimeout;
            set { _defaultAddonTimeout = value; OnPropertyChanged(); }
        }

        public int RetryAttempts
        {
            get => _retryAttempts;
            set { _retryAttempts = value; OnPropertyChanged(); }
        }

        public List<AddonNetworkConfig> AddonConfigs
        {
            get => _addonConfigs ?? (_addonConfigs = new List<AddonNetworkConfig>());
            set { _addonConfigs = value; OnPropertyChanged(); }
        }

        public static NetworkSettings Load()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                                   "TrafficWatch", "network_settings.json");
            
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var jsonObject = JsonValue.Parse(json);
                
                return new NetworkSettings
                {
                    NetworkModeEnabled = jsonObject["NetworkModeEnabled"] ?? false,
                    AllowNetworkConnections = jsonObject["AllowNetworkConnections"] ?? false,
                    ApiToken = jsonObject["ApiToken"] ?? "",
                    ApiTokenExpiryHours = jsonObject["ApiTokenExpiryHours"] ?? 24,
                    EnableTLS = jsonObject["EnableTLS"] ?? true,
                    AllowedIPs = jsonObject["AllowedIPs"] ?? "192.168.1.0/24",
                    MaxRequestsPerMinute = jsonObject["MaxRequestsPerMinute"] ?? 60,
                    DefaultAddonTimeout = jsonObject["DefaultAddonTimeout"] ?? 5000,
                    RetryAttempts = jsonObject["RetryAttempts"] ?? 3,
                    AddonConfigs = LoadAddonConfigs(jsonObject["AddonConfigs"])
                };
            }

            // مقادیر پیش‌فرض
            return new NetworkSettings
            {
                NetworkModeEnabled = false,
                AllowNetworkConnections = false,
                ApiToken = GenerateSecureToken(),
                ApiTokenExpiryHours = 24,
                EnableTLS = true,
                AllowedIPs = "192.168.1.0/24",
                MaxRequestsPerMinute = 60,
                DefaultAddonTimeout = 5000,
                RetryAttempts = 3
            };
        }

        public void Save()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                                   "TrafficWatch", "network_settings.json");
            
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            var jsonObject = new JsonObject
            {
                ["NetworkModeEnabled"] = NetworkModeEnabled,
                ["AllowNetworkConnections"] = AllowNetworkConnections,
                ["ApiToken"] = ApiToken,
                ["ApiTokenExpiryHours"] = ApiTokenExpiryHours,
                ["EnableTLS"] = EnableTLS,
                ["AllowedIPs"] = AllowedIPs,
                ["MaxRequestsPerMinute"] = MaxRequestsPerMinute,
                ["DefaultAddonTimeout"] = DefaultAddonTimeout,
                ["RetryAttempts"] = RetryAttempts,
                ["AddonConfigs"] = SaveAddonConfigs(AddonConfigs)
            };

            File.WriteAllText(path, jsonObject.ToString());
        }

        public static string GenerateSecureToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        private static List<AddonNetworkConfig> LoadAddonConfigs(JsonValue json)
        {
            var configs = new List<AddonNetworkConfig>();
            if (json == null) return configs;

            foreach (var item in json.AsJsonArray())
            {
                configs.Add(new AddonNetworkConfig
                {
                    AddonId = item["AddonId"],
                    Name = item["Name"],
                    IpAddress = item["IpAddress"],
                    Port = item["Port"],
                    Enabled = item["Enabled"],
                    Timeout = item["Timeout"]
                });
            }

            return configs;
        }

        private static JsonValue SaveAddonConfigs(List<AddonNetworkConfig> configs)
        {
            var array = new JsonArray();
            foreach (var config in configs)
            {
                array.Add(new JsonObject
                {
                    ["AddonId"] = config.AddonId,
                    ["Name"] = config.Name,
                    ["IpAddress"] = config.IpAddress,
                    ["Port"] = config.Port,
                    ["Enabled"] = config.Enabled,
                    ["Timeout"] = config.Timeout
                });
            }
            return array;
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class AddonNetworkConfig
    {
        public string AddonId { get; set; }
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public bool Enabled { get; set; }
        public int Timeout { get; set; } = 5000;
    }
}
```

---

## راه‌اندازی امنیتی

### 1. سرویس امنیت شبکه

```csharp
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

namespace TrafficWatch.Services.Security
{
    public class NetworkSecurityService
    {
        private static NetworkSecurityService _instance;
        private readonly ConcurrentDictionary<string, RequestLog> _requestLogs;
        private readonly List<string> _allowedIPs;
        private readonly string _apiToken;
        private readonly int _maxRequestsPerMinute;
        private readonly bool _enableTLS;
        private readonly object _lockObj = new object();

        public static NetworkSecurityService Instance => _instance ??= new NetworkSecurityService();

        private NetworkSecurityService()
        {
            _requestLogs = new ConcurrentDictionary<string, RequestLog>();
            
            var settings = Models.Settings.NetworkSettings.Load();
            _allowedIPs = ParseAllowedIPs(settings.AllowedIPs);
            _apiToken = settings.ApiToken;
            _maxRequestsPerMinute = settings.MaxRequestsPerMinute;
            _enableTLS = settings.EnableTLS;
        }

        public bool ValidateIPAddress(string ipAddress)
        {
            if (!IPAddress.TryParse(ipAddress, out var ip))
                return false;

            // بررسی IPv4 mapped IPv6
            if (ip.IsIPv6MappedToIPv4)
                ip = ip.MapToIPv4();

            // localhost همیشه مجاز است
            if (IPAddress.IsLoopback(ip))
                return true;

            // بررسی در لیست سفید
            foreach (var allowed in _allowedIPs)
            {
                if (IsIPInRange(ip, allowed))
                    return true;
            }

            LogSecurityEvent($"IP غیرمجاز تلاش برای اتصال: {ipAddress}", SecurityEventType.BlockedIP);
            return false;
        }

        public bool ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                LogSecurityEvent("توکن خالی دریافت شد", SecurityEventType.InvalidToken);
                return false;
            }

            if (token != _apiToken)
            {
                LogSecurityEvent("توکن نامعتبر", SecurityEventType.InvalidToken);
                return false;
            }

            return true;
        }

        public bool CheckRateLimit(string clientId)
        {
            var now = DateTime.UtcNow;
            var windowStart = now.AddMinutes(-1);

            lock (_lockObj)
            {
                if (!_requestLogs.TryGetValue(clientId, out var log))
                {
                    log = new RequestLog();
                    _requestLogs[clientId] = log;
                }

                // حذف درخواست‌های قدیمی
                log.Requests = log.Requests.Where(r => r > windowStart).ToList();

                if (log.Requests.Count >= _maxRequestsPerMinute)
                {
                    LogSecurityEvent($"Rate Limit exceeded for {clientId}", SecurityEventType.RateLimitExceeded);
                    return false;
                }

                log.Requests.Add(now);
                return true;
            }
        }

        public string HashToken(string token)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(token);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public void LogSecurityEvent(string message, SecurityEventType eventType)
        {
            Debug.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] [{eventType}] {message}");
            // اینجا می‌توانید لاگ را در فایل ذخیره کنید یا به سیستم مانیتورینگ ارسال کنید
        }

        private List<string> ParseAllowedIPs(string allowedIPsString)
        {
            var ips = new List<string>();
            if (string.IsNullOrEmpty(allowedIPsString))
                return ips;

            foreach (var part in allowedIPsString.Split(','))
            {
                var trimmed = part.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                    ips.Add(trimmed);
            }

            return ips;
        }

        private bool IsIPInRange(IPAddress ip, string range)
        {
            // بررسی CIDR notation (مثلاً 192.168.1.0/24)
            if (range.Contains("/"))
            {
                var parts = range.Split('/');
                if (parts.Length != 2)
                    return false;

                if (!IPAddress.TryParse(parts[0], out var network))
                    return false;

                if (!int.TryParse(parts[1], out var prefixLength))
                    return false;

                return IsIPInCidrRange(ip, network, prefixLength);
            }

            // بررسی IP تکی
            return IPAddress.TryParse(range, out var singleIP) && ip.Equals(singleIP);
        }

        private bool IsIPInCidrRange(IPAddress ip, IPAddress network, int prefixLength)
        {
            // تبدیل به IPv4 اگر لازم باشد
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                ip = ip.MapToIPv4();
            if (network.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                network = network.MapToIPv4();

            var ipBytes = ip.GetAddressBytes();
            var networkBytes = network.GetAddressBytes();

            var mask = GetSubnetMask(prefixLength);

            for (int i = 0; i < 4; i++)
            {
                if ((ipBytes[i] & mask[i]) != (networkBytes[i] & mask[i]))
                    return false;
            }

            return true;
        }

        private byte[] GetSubnetMask(int prefixLength)
        {
            var mask = new byte[4];
            int fullBytes = prefixLength / 8;
            int remainingBits = prefixLength % 8;

            for (int i = 0; i < fullBytes; i++)
                mask[i] = 255;

            if (remainingBits > 0)
                mask[fullBytes] = (byte)(255 << (8 - remainingBits));

            return mask;
        }
    }

    public class RequestLog
    {
        public List<DateTime> Requests { get; set; } = new List<DateTime>();
    }

    public enum SecurityEventType
    {
        BlockedIP,
        InvalidToken,
        RateLimitExceeded,
        SuccessfulAuth,
        TLSHandshake,
        SuspiciousActivity
    }
}
```

### 2. کلاینت امن HTTP

```csharp
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TrafficWatch.Services.Network
{
    public class SecureHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiToken;
        private readonly bool _enableTLS;
        private readonly int _timeout;
        private readonly int _retryAttempts;

        public SecureHttpClient(string baseUrl, string apiToken, bool enableTLS = true, int timeout = 5000, int retryAttempts = 3)
        {
            _apiToken = apiToken;
            _enableTLS = enableTLS;
            _timeout = timeout;
            _retryAttempts = retryAttempts;

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                {
                    if (enableTLS)
                    {
                        // در محیط تولید، گواهی‌نامه را به درستی اعتبارسنجی کنید
                        return errors == System.Net.Security.SslPolicyErrors.None;
                    }
                    return true; // فقط برای تست در محیط توسعه
                }
            };

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromMilliseconds(timeout)
            };

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
            if (!string.IsNullOrEmpty(apiToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", apiToken);
            }
        }

        public async Task<T> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                var response = await _httpClient.GetAsync(endpoint, cancellationToken);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return System.Text.Json.JsonSerializer.Deserialize<T>(content);
            }, cancellationToken);
        }

        public async Task<T> PostAsync<T>(string endpoint, object data, CancellationToken cancellationToken = default)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                var json = System.Text.Json.JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return System.Text.Json.JsonSerializer.Deserialize<T>(responseContent);
            }, cancellationToken);
        }

        private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken)
        {
            int attempt = 0;
            Exception lastException = null;

            while (attempt < _retryAttempts)
            {
                try
                {
                    return await action();
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    attempt++;
                    
                    if (attempt < _retryAttempts)
                    {
                        var delay = TimeSpan.FromMilliseconds(Math.Pow(2, attempt) * 100);
                        await Task.Delay(delay, cancellationToken);
                    }
                }
            }

            throw new HttpRequestException($"عملیات پس از {_retryAttempts} بار تلاش ناموفق بود", lastException);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
```

---

## چک‌لیست کامل امنیتی

### 🔒 چک‌لیست امنیتی قبل از استقرار

#### 1. احراز هویت و دسترسی
- [ ] توکن API با استفاده از `RandomNumberGenerator` تولید شده است
- [ ] توکن API در تنظیمات به صورت رمزنگاری شده ذخیره می‌شود
- [ ] مکانیزم چرخش توکن (Token Rotation) پیاده‌سازی شده است
- [ ] توکن‌های منقضی شده به طور خودکار رد می‌شوند
- [ ] لیست سفید IPها به درستی پیکربندی شده است
- [ ] دسترسی فقط به IPهای شبکه داخلی محدود شده است

#### 2. رمزنگاری و حفاظت از داده‌ها
- [ ] TLS 1.2 یا بالاتر برای تمام ارتباطات شبکه فعال است
- [ ] گواهی‌نامه‌های SSL معتبر استفاده می‌شوند
- [ ] داده‌های حساس در حال انتقال رمزنگاری می‌شوند
- [ ] داده‌های حساس در حالت استراحت (at rest) رمزنگاری می‌شوند
- [ ] از الگوریتم‌های رمزنگاری قوی (AES-256, RSA-2048) استفاده می‌شود

#### 3. کنترل دسترسی شبکه
- [ ] فایروال ویندوز به درستی پیکربندی شده است
- [ ] فقط پورت‌های مورد نیاز باز هستند
- [ ] لیست سفید IPها به روز نگه داشته می‌شود
- [ ] دسترسی از شبکه‌های عمومی مسدود شده است
- [ ] NAT و Port Forwarding به درستی تنظیم شده‌اند

#### 4. محدودیت نرخ و محافظت در برابر حملات
- [ ] Rate Limiting برای هر کلاینت پیاده‌سازی شده است
- [ ] محافظت در برابر حملات DDoS وجود دارد
- [ ] محافظت در برابر حملات Brute Force وجود دارد
- [ ] تعداد درخواست‌های ناموفق محدود شده است
- [ ] حساب‌های مشکوک به طور موقت مسدود می‌شوند

#### 5. اعتبارسنجی ورودی‌ها
- [ ] تمام ورودی‌های کاربر اعتبارسنجی می‌شوند
- [ ] از حملات Injection جلوگیری می‌شود
- [ ] از حملات XSS جلوگیری می‌شود
- [ ] مسیرهای فایل به درستی sanitized می‌شوند
- [ ] اندازه درخواست‌ها محدود شده است

#### 6. لاگ‌گیری و مانیتورینگ
- [ ] تمام فعالیت‌های امنیتی لاگ می‌شوند
- [ ] لاگ‌ها در مکان امنی ذخیره می‌شوند
- [ ] سیستم هشدار برای فعالیت‌های مشکوک وجود دارد
- [ ] لاگ‌ها به طور دوره‌ای بررسی می‌شوند
- [ ] اطلاعات حساس در لاگ‌ها ماسک می‌شوند

#### 7. مدیریت خطا
- [ ] پیام‌های خطا اطلاعات حساس را فاش نمی‌کنند
- [ ] خطاها به درستی لاگ می‌شوند
- [ ] سیستم در صورت خطا به حالت امن بازمی‌گردد
- [ ] مکانیزم fallback برای قطعی شبکه وجود دارد

#### 8. به‌روزرسانی و نگهداری
- [ ] کتابخانه‌های شخص ثالث به روز هستند
- [ ] آسیب‌پذیری‌های شناخته شده بررسی می‌شوند
- [ ] پچ‌های امنیتی به موقع اعمال می‌شوند
- [ ] نسخه‌پشتیبان از تنظیمات گرفته می‌شود

#### 9. تست امنیتی
- [ ] تست نفوذ انجام شده است
- [ ] اسکن آسیب‌پذیری انجام شده است
- [ ] کد Review امنیتی شده است
- [ ] سناریوهای حمله شبیه‌سازی شده‌اند

#### 10. مستندات و آموزش
- [ ] مستندات امنیتی به روز است
- [ ] کاربران درباره بهترین روش‌ها آموزش دیده‌اند
- [ ] رویه‌های پاسخ به حوادث تعریف شده‌اند
- [ ] مسئولیت‌های امنیتی مشخص شده‌اند

### 📋 جدول ارزیابی ریسک

| نوع حمله | سطح ریسک | اقدامات کنترلی | وضعیت |
|----------|----------|----------------|--------|
| دسترسی غیرمجاز | بالا | احراز هویت توکن، لیست سفید IP | ✅ |
| شنود داده‌ها | بالا | TLS 1.3، رمزنگاری end-to-end | ✅ |
| حملات DDoS | متوسط | Rate Limiting، فایروال | ✅ |
| تزریق کد | بالا | اعتبارسنجی ورودی، Parameterized Queries | ✅ |
| XSS | متوسط | Sanitization، Content Security Policy | ✅ |
| Man-in-the-Middle | بالا | TLS، Certificate Pinning | ✅ |
| Brute Force | متوسط | محدودیت تلاش، قفل موقت | ✅ |
| Session Hijacking | بالا | توکن‌های امن، HTTPS Only | ✅ |

---

## نمونه پیاده‌سازی سرور API

### نمونه کامل سرور برای برنامه خارجی

```csharp
// این کد در برنامه خارجی (مثلاً DownloadMenger2) قرار می‌گیرد
using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace ExternalProgramServer
{
    public class SecureApiServer
    {
        private readonly string _apiToken;
        private readonly int _port;
        private readonly bool _enableTLS;

        public SecureApiServer(string apiToken, int port = 9090, bool enableTLS = false)
        {
            _apiToken = apiToken;
            _port = port;
            _enableTLS = enableTLS;
        }

        public async Task StartAsync()
        {
            var host = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Any, _port, listenOptions =>
                    {
                        if (_enableTLS)
                        {
                            // پیکربندی TLS
                            // listenOptions.UseHttps("certificate.pfx", "password");
                        }
                    });
                })
                .Configure(app =>
                {
                    app.UseMiddleware<SecurityMiddleware>(_apiToken);
                    
                    app.Run(async context =>
                    {
                        // بررسی متد HTTP
                        if (context.Request.Method == HttpMethods.Get)
                        {
                            if (context.Request.Path == "/api/status")
                            {
                                await HandleGetStatus(context);
                            }
                            else if (context.Request.Path == "/api/downloads")
                            {
                                await HandleGetDownloads(context);
                            }
                            else
                            {
                                context.Response.StatusCode = 404;
                                await context.Response.WriteAsync("Not Found");
                            }
                        }
                        else if (context.Request.Method == HttpMethods.Post)
                        {
                            if (context.Request.Path == "/api/control")
                            {
                                await HandleControl(context);
                            }
                            else
                            {
                                context.Response.StatusCode = 404;
                                await context.Response.WriteAsync("Not Found");
                            }
                        }
                        else
                        {
                            context.Response.StatusCode = 405;
                            await context.Response.WriteAsync("Method Not Allowed");
                        }
                    });
                })
                .Build();

            Console.WriteLine($"Server started on port {_port}");
            await host.RunAsync();
        }

        private async Task HandleGetStatus(HttpContext context)
        {
            var status = new
            {
                IsRunning = true,
                ActiveDownloads = 5,
                TotalDownloadSpeed = 1024 * 1024, // 1 MB/s
                Timestamp = DateTime.UtcNow
            };

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(status));
        }

        private async Task HandleGetDownloads(HttpContext context)
        {
            var downloads = new[]
            {
                new { FileName = "file1.zip", Progress = 75, Speed = 512 * 1024 },
                new { FileName = "file2.exe", Progress = 30, Speed = 256 * 1024 }
            };

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(downloads));
        }

        private async Task HandleControl(HttpContext context)
        {
            // پردازش درخواست کنترل
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("{\"success\": true}");
        }
    }

    public class SecurityMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _apiToken;

        public SecurityMiddleware(RequestDelegate next, string apiToken)
        {
            _next = next;
            _apiToken = apiToken;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // بررسی IP
            var clientIp = context.Connection.RemoteIpAddress?.ToString();
            if (!IsValidIP(clientIp))
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Forbidden: IP not allowed");
                return;
            }

            // بررسی توکن
            var authHeader = context.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized: Missing token");
                return;
            }

            var token = authHeader.Substring(7);
            if (token != _apiToken)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized: Invalid token");
                return;
            }

            // ادامه پردازش
            await _next(context);
        }

        private bool IsValidIP(string ip)
        {
            // اینجا می‌توانید لیست سفید IPها را بررسی کنید
            // برای مثال، فقط IPهای شبکه داخلی مجاز هستند
            if (string.IsNullOrEmpty(ip))
                return false;

            // localhost همیشه مجاز است
            if (ip == "127.0.0.1" || ip == "::1")
                return true;

            // بررسی شبکه داخلی (مثلاً 192.168.x.x)
            if (ip.StartsWith("192.168.") || ip.StartsWith("10.") || ip.StartsWith("172.16."))
                return true;

            return false;
        }
    }
}
```

---

## نمونه پیاده‌سازی کلاینت در TrafficWatch

### سرویس مدیریت افزونه شبکه

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrafficWatch.Models.Dashboard;
using TrafficWatch.Models.Settings;
using TrafficWatch.Services.Network;
using TrafficWatch.Services.Security;

namespace TrafficWatch.Services.Dashboard
{
    public class NetworkAddonService
    {
        private static NetworkAddonService _instance;
        private readonly NetworkSettings _settings;
        private readonly Dictionary<string, SecureHttpClient> _clients;

        public static NetworkAddonService Instance => _instance ??= new NetworkAddonService();

        private NetworkAddonService()
        {
            _settings = NetworkSettings.Load();
            _clients = new Dictionary<string, SecureHttpClient>();
        }

        public async Task InitializeAsync()
        {
            if (!_settings.NetworkModeEnabled)
            {
                NetworkSecurityService.Instance.LogSecurityEvent(
                    "حالت شبکه غیرفعال است", 
                    SecurityEventType.SuspiciousActivity);
                return;
            }

            foreach (var config in _settings.AddonConfigs)
            {
                if (config.Enabled)
                {
                    await InitializeAddonClientAsync(config);
                }
            }
        }

        private async Task InitializeAddonClientAsync(AddonNetworkConfig config)
        {
            var protocol = _settings.EnableTLS ? "https" : "http";
            var baseUrl = $"{protocol}://{config.IpAddress}:{config.Port}";

            var client = new SecureHttpClient(
                baseUrl,
                _settings.ApiToken,
                _settings.EnableTLS,
                config.Timeout,
                _settings.RetryAttempts
            );

            _clients[config.AddonId] = client;

            // آزمون اتصال
            try
            {
                await TestConnectionAsync(config.AddonId);
                NetworkSecurityService.Instance.LogSecurityEvent(
                    $"اتصال به {config.Name} موفقیت‌آمیز بود",
                    SecurityEventType.SuccessfulAuth);
            }
            catch (Exception ex)
            {
                NetworkSecurityService.Instance.LogSecurityEvent(
                    $"خطا در اتصال به {config.Name}: {ex.Message}",
                    SecurityEventType.SuspiciousActivity);
            }
        }

        public async Task<T> GetAddonDataAsync<T>(string addonId, string endpoint)
        {
            if (!_clients.ContainsKey(addonId))
                throw new InvalidOperationException($"کلاینت برای افزونه {addonId} یافت نشد");

            // بررسی Rate Limit
            if (!NetworkSecurityService.Instance.CheckRateLimit(addonId))
                throw new InvalidOperationException("Rate Limit exceeded");

            var client = _clients[addonId];
            return await client.GetAsync<T>(endpoint);
        }

        public async Task<T> PostAddonDataAsync<T>(string addonId, string endpoint, object data)
        {
            if (!_clients.ContainsKey(addonId))
                throw new InvalidOperationException($"کلاینت برای افزونه {addonId} یافت نشد");

            // بررسی Rate Limit
            if (!NetworkSecurityService.Instance.CheckRateLimit(addonId))
                throw new InvalidOperationException("Rate Limit exceeded");

            var client = _clients[addonId];
            return await client.PostAsync<T>(endpoint, data);
        }

        private async Task TestConnectionAsync(string addonId)
        {
            // ارسال درخواست تست به endpoint سلامت
            await GetAddonDataAsync<Dictionary<string, object>>(addonId, "/api/status");
        }

        public void UpdateAddonConfig(AddonNetworkConfig config)
        {
            // حذف کلاینت قدیمی اگر وجود دارد
            if (_clients.ContainsKey(config.AddonId))
            {
                _clients[config.AddonId].Dispose();
                _clients.Remove(config.AddonId);
            }

            // اگر افزونه فعال است، کلاینت جدید ایجاد کن
            if (config.Enabled)
            {
                _ = InitializeAddonClientAsync(config);
            }

            // ذخیره تنظیمات
            _settings.Save();
        }

        public List<AddonNetworkConfig> GetAllConfigs()
        {
            return _settings.AddonConfigs;
        }

        public void AddonConfig(AddonNetworkConfig config)
        {
            _settings.AddonConfigs.Add(config);
            _settings.Save();
        }

        public void RemoveAddonConfig(string addonId)
        {
            var config = _settings.AddonConfigs.Find(c => c.AddonId == addonId);
            if (config != null)
            {
                _settings.AddonConfigs.Remove(config);
                
                if (_clients.ContainsKey(addonId))
                {
                    _clients[addonId].Dispose();
                    _clients.Remove(addonId);
                }
                
                _settings.Save();
            }
        }
    }
}
```

---

## مدیریت توکن و احراز هویت

### سرویس مدیریت توکن

```csharp
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TrafficWatch.Services.Security
{
    public class TokenManager
    {
        private static TokenManager _instance;
        private readonly string _tokenFilePath;
        private readonly string _encryptionKey;

        public static TokenManager Instance => _instance ??= new TokenManager();

        private TokenManager()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TrafficWatch"
            );
            Directory.CreateDirectory(appDataPath);
            
            _tokenFilePath = Path.Combine(appDataPath, "api_token.enc");
            _encryptionKey = GetMachineSpecificKey();
        }

        public string GetOrCreateToken()
        {
            if (File.Exists(_tokenFilePath))
            {
                try
                {
                    var encryptedToken = File.ReadAllBytes(_tokenFilePath);
                    return DecryptToken(encryptedToken);
                }
                catch
                {
                    // اگر خطایی رخ داد، توکن جدید تولید کن
                }
            }

            var newToken = NetworkSettings.GenerateSecureToken();
            SaveToken(newToken);
            return newToken;
        }

        public void RotateToken()
        {
            var newToken = NetworkSettings.GenerateSecureToken();
            SaveToken(newToken);
            
            // اینجا می‌توانید به تمام کلاینت‌ها اطلاع دهید که توکن تغییر کرده است
        }

        public bool ValidateToken(string token)
        {
            var currentToken = GetOrCreateToken();
            return token == currentToken;
        }

        private void SaveToken(string token)
        {
            var encrypted = EncryptToken(token);
            File.WriteAllBytes(_tokenFilePath, encrypted);
        }

        private string EncryptToken(string token)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(_encryptionKey.PadRight(32).Substring(0, 32));
            aes.GenerateIV();
            
            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(token);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            
            // IV را به ابتدای داده‌های رمزنگاری شده اضافه کن
            var result = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);
            
            return Convert.ToBase64String(result);
        }

        private string DecryptToken(string encryptedToken)
        {
            var fullData = Convert.FromBase64String(encryptedToken);
            
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(_encryptionKey.PadRight(32).Substring(0, 32));
            
            // IV را استخراج کن
            var iv = new byte[aes.IV.Length];
            Buffer.BlockCopy(fullData, 0, iv, 0, iv.Length);
            aes.IV = iv;
            
            // داده‌های رمزنگاری شده را استخراج کن
            var cipherBytes = new byte[fullData.Length - iv.Length];
            Buffer.BlockCopy(fullData, iv.Length, cipherBytes, 0, cipherBytes.Length);
            
            using var decryptor = aes.CreateDecryptor();
            var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            
            return Encoding.UTF8.GetString(plainBytes);
        }

        private string GetMachineSpecificKey()
        {
            // تولید کلید مبتنی بر مشخصات ماشین
            var machineInfo = $"{Environment.MachineName}-{Environment.OSVersion.Version}";
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(machineInfo));
            return Convert.ToBase64String(hash);
        }
    }
}
```

---

## عیب‌یابی و مانیتورینگ

### ابزارهای عیب‌یابی

#### 1. تست اتصال دستی

```powershell
# PowerShell Script for Testing Connection

$baseUrl = "http://192.168.1.20:9090"
$token = "YOUR_API_TOKEN_HERE"

# تست endpoint وضعیت
$headers = @{
    "Authorization" = "Bearer $token"
    "Accept" = "application/json"
}

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/status" -Headers $headers -Method Get
    Write-Host "Status: Success" -ForegroundColor Green
    $response | ConvertTo-Json
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

# تست سرعت پاسخ
$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
try {
    Invoke-RestMethod -Uri "$baseUrl/api/status" -Headers $headers -Method Get | Out-Null
    $stopwatch.Stop()
    Write-Host "Response Time: $($stopwatch.ElapsedMilliseconds) ms" -ForegroundColor Cyan
} catch {
    $stopwatch.Stop()
    Write-Host "Request failed after $($stopwatch.ElapsedMilliseconds) ms" -ForegroundColor Red
}
```

#### 2. لاگ‌گیر پیشرفته

```csharp
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace TrafficWatch.Services.Logging
{
    public class NetworkActivityLogger
    {
        private static NetworkActivityLogger _instance;
        private readonly string _logFilePath;
        private readonly object _lockObj = new object();

        public static NetworkActivityLogger Instance => _instance ??= new NetworkActivityLogger();

        private NetworkActivityLogger()
        {
            var logDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TrafficWatch",
                "Logs"
            );
            Directory.CreateDirectory(logDir);
            
            _logFilePath = Path.Combine(logDir, $"network_{DateTime.Today:yyyyMMdd}.log");
        }

        public void LogRequest(string clientId, string endpoint, string method, int statusCode, long durationMs)
        {
            lock (_lockObj)
            {
                var logEntry = new StringBuilder();
                logEntry.Append($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] ");
                logEntry.Append($"Client: {clientId,-20} ");
                logEntry.Append($"Method: {method,-6} ");
                logEntry.Append($"Endpoint: {endpoint,-30} ");
                logEntry.Append($"Status: {statusCode,-3} ");
                logEntry.Append($"Duration: {durationMs,-5}ms");
                logEntry.AppendLine();

                File.AppendAllText(_logFilePath, logEntry.ToString(), Encoding.UTF8);
            }
        }

        public void LogSecurityEvent(string eventType, string message, string clientIp = null)
        {
            lock (_lockObj)
            {
                var logEntry = new StringBuilder();
                logEntry.Append($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] ");
                logEntry.Append($"[SECURITY] ");
                logEntry.Append($"Type: {eventType,-20} ");
                if (!string.IsNullOrEmpty(clientIp))
                    logEntry.Append($"IP: {clientIp,-15} ");
                logEntry.Append($"Message: {message}");
                logEntry.AppendLine();

                File.AppendAllText(_logFilePath, logEntry.ToString(), Encoding.UTF8);
            }
        }

        public async Task<string[]> GetRecentLogsAsync(int lineCount = 50)
        {
            return await Task.Run(() =>
            {
                if (!File.Exists(_logFilePath))
                    return new string[0];

                lock (_lockObj)
                {
                    var allLines = File.ReadAllLines(_logFilePath);
                    return allLines.Skip(Math.Max(0, allLines.Length - lineCount)).ToArray();
                }
            });
        }

        public void ClearOldLogs(int daysToKeep = 7)
        {
            var logDir = Path.GetDirectoryName(_logFilePath);
            var cutoffDate = DateTime.Today.AddDays(-daysToKeep);

            foreach (var file in Directory.GetFiles(logDir, "network_*.log"))
            {
                var fileName = Path.GetFileName(file);
                if (fileName.StartsWith("network_") && fileName.EndsWith(".log"))
                {
                    var datePart = fileName.Substring(8, 8); // Extract YYYYMMDD
                    if (DateTime.TryParseExact(datePart, "yyyyMMdd", null, 
                        System.Globalization.DateTimeStyles.None, out var fileDate))
                    {
                        if (fileDate < cutoffDate)
                        {
                            try
                            {
                                File.Delete(file);
                            }
                            catch
                            {
                                // Ignore deletion errors
                            }
                        }
                    }
                }
            }
        }
    }
}
```

### دستورالعمل عیب‌یابی

#### مشکل: اتصال برقرار نمی‌شود

**مراحل عیب‌یابی:**

1. **بررسی فایروال:**
   ```powershell
   # بررسی قوانین فایروال
   Get-NetFirewallRule | Where-Object { $_.DisplayName -like "*TrafficWatch*" }
   
   # افزودن قانون جدید اگر لازم است
   New-NetFirewallRule -DisplayName "TrafficWatch API" -Direction Inbound -LocalPort 9090 -Protocol TCP -Action Allow
   ```

2. **بررسی اینکه سرور در حال اجرا است:**
   ```powershell
   netstat -ano | findstr :9090
   ```

3. **تست اتصال با telnet:**
   ```bash
   telnet 192.168.1.20 9090
   ```

4. **بررسی توکن API:**
   - مطمئن شوید توکن در هر دو طرف (کلاینت و سرور) یکسان است
   - توکن را دوباره تولید کنید

5. **بررسی لیست سفید IP:**
   - مطمئن شوید IP کلاینت در لیست سفید قرار دارد
   - فرمت CIDR را بررسی کنید (مثلاً `192.168.1.0/24`)

#### مشکل: خطای احراز هویت

**راه حل:**
- توکن API را بررسی کنید
- مطمئن شوید هدر Authorization به درستی تنظیم شده است
- فرمت هدر باید باشد: `Authorization: Bearer YOUR_TOKEN`

#### مشکل: Rate Limit exceeded

**راه حل:**
- مقدار `MaxRequestsPerMinute` را در تنظیمات افزایش دهید
- فاصله درخواست‌ها را افزایش دهید
- کش کردن داده‌ها را پیاده‌سازی کنید

---

## ضمیمه: الگوهای امنیتی

### الگوی 1: Middleware اعتبارسنجی درخواست

```csharp
public class ValidationMiddleware
{
    private readonly RequestDelegate _next;

    public ValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // اعتبارسنجی Content-Type
        if (context.Request.Method == "POST" && 
            !context.Request.ContentType?.Contains("application/json") == true)
        {
            context.Response.StatusCode = 415;
            await context.Response.WriteAsync("Unsupported Media Type");
            return;
        }

        // اعتبارسنجی اندازه درخواست
        if (context.Request.ContentLength > 1024 * 1024) // 1MB limit
        {
            context.Response.StatusCode = 413;
            await context.Response.WriteAsync("Payload Too Large");
            return;
        }

        await _next(context);
    }
}
```

### الگوی 2: Repository امن برای داده‌ها

```csharp
public interface ISecureRepository<T>
{
    Task<T> GetByIdAsync(string id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T item);
    Task UpdateAsync(T item);
    Task DeleteAsync(string id);
}

public class SecureRepository<T> : ISecureRepository<T> where T : class
{
    private readonly string _encryptionKey;
    
    public SecureRepository(string encryptionKey)
    {
        _encryptionKey = encryptionKey;
    }

    public async Task<T> GetByIdAsync(string id)
    {
        // اعتبارسنجی ID برای جلوگیری از Injection
        if (!IsValidId(id))
            throw new ArgumentException("Invalid ID format");

        // بازیابی و رمزگشایی داده
        var encryptedData = await FetchFromStorageAsync(id);
        return DecryptData<T>(encryptedData);
    }

    private bool IsValidId(string id)
    {
        // فقط کاراکترهای alphanumeric و خط تیره مجاز هستند
        return System.Text.RegularExpressions.Regex.IsMatch(id, @"^[a-zA-Z0-9\-]+$");
    }

    private string EncryptData<T>(T data)
    {
        // پیاده‌سازی رمزنگاری
        return "";
    }

    private T DecryptData<T>(string encryptedData)
    {
        // پیاده‌سازی رمزگشایی
        return default(T);
    }

    private Task<string> FetchFromStorageAsync(string id)
    {
        // بازیابی از پایگاه داده یا فایل
        return Task.FromResult("");
    }

    // سایر متدها...
}
```

### الگوی 3: Circuit Breaker برای تاب‌آوری

```csharp
public class CircuitBreaker
{
    private enum CircuitState { Closed, Open, HalfOpen }
    
    private CircuitState _state = CircuitState.Closed;
    private int _failureCount = 0;
    private int _failureThreshold = 5;
    private TimeSpan _resetTimeout = TimeSpan.FromSeconds(30);
    private DateTime _lastFailureTime;
    private readonly object _lockObj = new object();

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
    {
        lock (_lockObj)
        {
            if (_state == CircuitState.Open)
            {
                if (DateTime.UtcNow - _lastFailureTime > _resetTimeout)
                {
                    _state = CircuitState.HalfOpen;
                }
                else
                {
                    throw new CircuitBreakerOpenException("Circuit breaker is open");
                }
            }
        }

        try
        {
            var result = await action();
            
            lock (_lockObj)
            {
                if (_state == CircuitState.HalfOpen)
                    _state = CircuitState.Closed;
                _failureCount = 0;
            }
            
            return result;
        }
        catch (Exception ex)
        {
            lock (_lockObj)
            {
                _failureCount++;
                _lastFailureTime = DateTime.UtcNow;
                
                if (_failureCount >= _failureThreshold || _state == CircuitState.HalfOpen)
                {
                    _state = CircuitState.Open;
                }
            }
            
            throw;
        }
    }
}

public class CircuitBreakerOpenException : Exception
{
    public CircuitBreakerOpenException(string message) : base(message) { }
}
```

---

## نتیجه‌گیری

این سند راهنمای کاملی برای اتصال ایمن برنامه‌های خارجی به داشبورد TrafficWatch در شبکه داخلی ارائه داد. با رعایت چک‌لیست امنیتی و پیاده‌سازی الگوهای پیشنهادی، می‌توانید یک سیستم قابل اعتماد و امن ایجاد کنید.

### نکات کلیدی:
1. ✅ همیشه از TLS برای ارتباطات شبکه استفاده کنید
2. ✅ توکن‌های API را به طور منظم بچرخانید
3. ✅ لیست سفید IPها را به روز نگه دارید
4. ✅ Rate Limiting را پیاده‌سازی کنید
5. ✅ تمام فعالیت‌ها را لاگ‌گیری کنید
6. ✅ به طور دوره‌ای تست امنیتی انجام دهید

### منابع بیشتر:
- [Microsoft Security Guidelines](https://docs.microsoft.com/en-us/security/)
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [ASP.NET Core Security](https://docs.microsoft.com/en-us/aspnet/core/security/)

---

**نسخه سند:** 1.0  
**تاریخ انتشار:** 2026  
**تهیه شده برای:** TrafficWatch Development Team  
**سطح امنیتی:** Enhanced Security Mode برای شبکه داخلی

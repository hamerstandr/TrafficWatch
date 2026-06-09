# راهنمای توسعه و یکپارچه‌سازی افزونه‌های داشبورد TrafficWatch

## نسخه مستندات
- **نسخه:** 2.0
- **سازگار با:** .NET 10 SDK, Visual Studio 2026
- **تاریخ به‌روزرسانی:** 2026
- **سطح امنیت:** Enhanced Security Mode

## فهرست مطالب
1. [معرفی سیستم افزونه‌ها](#معرفی-سیستم-افزونه‌ها)
2. [معماری سیستم](#معماری-سیستم)
3. [امنیت و ملاحظات امنیتی](#امنیت-و-ملاحظات-امنیتی)
4. [راه‌اندازی اولیه](#راه‌اندازی-اولیه)
5. [ایجاد افزونه جدید](#ایجاد-افزونه-جدید)
6. [یکپارچه‌سازی با DownloadMenger2](#یکپارچه‌سازی-با-downloadmenger2)
7. [تنظیمات و پیکربندی](#تنظیمات-و-پیکربندی)
8. [نمونه کدها](#نمونه-کدها)
9. [عیب‌یابی و پشتیبانی](#عیب‌یابی-و-پشتیبانی)

---

## معرفی سیستم افزونه‌ها

سیستم داشبورد TrafficWatch یک پلتفرم قابل گسترش و ایمن است که امکان اضافه کردن ماژول‌های مختلف را با رعایت اصول امنیتی فراهم می‌کند. هر افزونه می‌تواند:

- اطلاعات خاصی را نمایش دهد (دانلود، موسیقی، مانیتورینگ سیستم و...)
- تنظیمات مخصوص به خود داشته باشد
- به صورت مستقل فعال یا غیرفعال شود
- با برنامه‌های خارجی از طریق API ایمن ارتباط برقرار کند

### ویژگی‌های کلیدی

1. **نصب آسان**: افزونه‌ها به صورت خودکار شناسایی و اعتبارسنجی می‌شوند
2. **قابل پیکربندی**: هر افزونه تنظیمات مخصوص به خود را دارد
3. **ترتیب نمایش**: کاربر می‌تواند ترتیب نمایش افزونه‌ها را تغییر دهد
4. **ارتباط API ایمن**: امکان ارتباط با برنامه‌های خارجی از طریق HTTP API با احراز هویت
5. **امنیت پیشرفته**: رمزنگاری داده‌ها، محدودیت دسترسی localhost، و اعتبارسنجی ورودی‌ها

### الزامات سیستم

- **.NET SDK:** نسخه 10.0 یا بالاتر
- **Visual Studio:** نسخه 2026 یا بالاتر
- **Windows:** 10 نسخه 1903 یا بالاتر / Windows 11
- **حافظه RAM:** حداقل 4GB (8GB توصیه می‌شود)

---

## معماری سیستم

```
┌─────────────────────────────────────────────────────────┐
│                   TrafficWatch Dashboard                │
│                  (.NET 10 | VS 2026)                    │
├─────────────────────────────────────────────────────────┤
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐     │
│  │   Addon 1   │  │   Addon 2   │  │   Addon 3   │     │
│  │  (Download) │  │   (Music)   │  │  (System)   │     │
│  │  [Secure]   │  │  [Secure]   │  │  [Secure]   │     │
│  └─────────────┘  └─────────────┘  └─────────────┘     │
├─────────────────────────────────────────────────────────┤
│              DashboardAddonService                      │
│         (مدیریت افزونه‌ها + امنیت + اعتبارسنجی)          │
├─────────────────────────────────────────────────────────┤
│              Security Layer                             │
│    • Authentication • Authorization • Encryption        │
├─────────────────────────────────────────────────────────┤
│              External APIs (Localhost Only)             │
│    DownloadMenger2 :9090 | MusicPlayer :9091 | ...      │
│    [HTTPS Recommended] [API Tokens Required]            │
└─────────────────────────────────────────────────────────┘
```

### ساختار فایل‌ها

```
TrafficWatch/
├── Models/
│   └── Dashboard/
│       ├── AddonInfo.cs              # مدل‌های پایه افزونه
│       └── [AddonName]AddonInfo.cs   # مدل‌های اختصاصی
├── Services/
│   └── Dashboard/
│       ├── DashboardAddonService.cs  # سرویس مدیریت افزونه‌ها
│       ├── SecurityService.cs        # سرویس امنیت و احراز هویت
│       └── [AddonName]Service.cs     # سرویس‌های اختصاصی
├── Security/
│   ├── ApiTokenProvider.cs           # مدیریت توکن‌های API
│   ├── RequestValidator.cs           # اعتبارسنجی درخواست‌ها
│   └── DataEncryptor.cs              # رمزنگاری داده‌ها
└── View/
    └── Dashboard/
        └── [AddonName]Tab.xaml       # UI هر افزونه
```

### لایه‌های امنیتی

1. **لایه شبکه (Network Layer)**
   - محدودیت اتصال به localhost (127.0.0.1)
   - فایروال داخلی برای پورت‌های API
   - جلوگیری از حملات DDoS محلی

2. **لایه احراز هویت (Authentication Layer)**
   - API Token برای هر افزونه
   - JWT tokens برای session management
   - Refresh token rotation

3. **لایه اعتبارسنجی (Validation Layer)**
   - Input sanitization
   - XSS prevention
   - SQL injection prevention
   - Path traversal protection

4. **لایه رمزنگاری (Encryption Layer)**
   - TLS/SSL برای ارتباطات HTTPS
   - AES-256 برای داده‌های حساس
   - Secure storage برای تنظیمات

---

## امنیت و ملاحظات امنیتی {#security-section}

### اصول امنیتی برای توسعه‌دهندگان افزونه

هنگام توسعه افزونه‌های TrafficWatch، رعایت موارد زیر **الزامی** است:

#### 1. محدودیت‌های شبکه

```csharp
// ✅ صحیح - فقط localhost مجاز است
var endpoint = "http://127.0.0.1:9090";

// ❌ غلط - اتصال به شبکه خارجی ممنوع
var endpoint = "http://192.168.1.100:9090"; // رد می‌شود
var endpoint = "http://example.com/api";     // رد می‌شود
```

**قوانین:**
- تمام ارتباطات API باید به `127.0.0.1` یا `localhost` محدود شوند
- پورت‌های API باید در محدوده 9000-9999 باشند
- هیچ اتصال ورودی از شبکه خارجی پذیرفته نمی‌شود

#### 2. اعتبارسنجی ورودی‌ها

```csharp
using System.Text.RegularExpressions;

public class SecurityValidator
{
    // جلوگیری از Path Traversal
    public static bool IsValidPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return false;
        
        // بررسی کاراکترهای خطرناک
        if (path.Contains("..") || path.Contains(":") || 
            path.Contains("\\\\") || path.StartsWith("/"))
        {
            return false;
        }
        
        return true;
    }
    
    // جلوگیری از XSS
    public static string SanitizeInput(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        
        // حذف تگ‌های HTML
        input = Regex.Replace(input, "<.*?>", string.Empty);
        
        // Escape کاراکترهای خاص
        input = System.Security.SecurityElement.Escape(input);
        
        return input;
    }
    
    // بررسی پورت معتبر
    public static bool IsValidPort(int port)
    {
        return port >= 9000 && port <= 9999;
    }
}
```

#### 3. مدیریت امن توکن‌های API

```csharp
using System.Security.Cryptography;
using System.Text;

public class ApiTokenProvider
{
    private static readonly RandomNumberGenerator Rng = RandomNumberGenerator.Create();
    
    /// <summary>
    /// تولید توکن امن برای API
    /// </summary>
    public static string GenerateSecureToken(int length = 32)
    {
        var bytes = new byte[length];
        Rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
    
    /// <summary>
    /// هش کردن توکن برای ذخیره‌سازی امن
    /// </summary>
    public static string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
    
    /// <summary>
    /// بررسی اعتبار توکن
    /// </summary>
    public static bool ValidateToken(string providedToken, string storedHash)
    {
        var providedHash = HashToken(providedToken);
        return CryptographicComparison(providedHash, storedHash);
    }
    
    // مقایسه امن برای جلوگیری از Timing Attacks
    private static bool CryptographicComparison(string a, string b)
    {
        if (a == null || b == null) return false;
        if (a.Length != b.Length) return false;
        
        var result = 0;
        for (int i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];
        }
        
        return result == 0;
    }
}
```

#### 4. رمزنگاری داده‌های حساس

```csharp
using System.Security.Cryptography;

public class DataEncryptor
{
    private readonly byte[] _key;
    private readonly byte[] _iv;
    
    public DataEncryptor()
    {
        // تولید کلید و IV امن
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.GenerateKey();
        aes.GenerateIV();
        
        _key = aes.Key;
        _iv = aes.IV;
    }
    
    public byte[] Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        
        var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        return encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
    }
    
    public string Decrypt(byte[] cipherBytes)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        
        var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        return Encoding.UTF8.GetString(plainBytes);
    }
}
```

#### 5. محافظت در برابر حملات رایج

| نوع حمله | روش محافظت | پیاده‌سازی |
|----------|-----------|------------|
| **XSS** | Input sanitization | `SecurityElement.Escape()` |
| **SQL Injection** | Parameterized queries | استفاده از ORM یا parameters |
| **Path Traversal** | Path validation | `Path.GetFullPath()` + بررسی prefix |
| **DDoS** | Rate limiting | محدودیت درخواست در دقیقه |
| **Timing Attack** | Constant-time comparison | `CryptographicComparison()` |
| **Man-in-the-Middle** | HTTPS/TLS | اجباری برای ارتباطات خارجی |

### چک‌لیست امنیتی قبل از انتشار افزونه

- [ ] تمام ورودی‌ها اعتبارسنجی شده‌اند
- [ ] خروجی‌ها escape شده‌اند
- [ ] ارتباطات فقط به localhost محدود هستند
- [ ] توکن‌های API به صورت امن تولید و ذخیره شده‌اند
- [ ] داده‌های حساس رمزنگاری شده‌اند
- [ ] خطاها بدون افشای اطلاعات حساس لاگ می‌شوند
- [ ] از disposing صحیح منابع اطمینان حاصل شده
- [ ] کد توسط ابزارهای Static Analysis بررسی شده

---

## راه‌اندازی اولیه

### 1. افزودن به App.xaml.cs

در فایل `App.xaml.cs`، سرویس داشبورد را در روش `Application_Startup` راه‌اندازی کنید:

```csharp
private void Application_Startup(object sender, StartupEventArgs e)
{
    // ... کدهای موجود ...
    
    // راه‌اندازی سرویس داشبورد با تنظیمات امنیتی
    DashboardAddonService.Instance.Initialize();
    
    // فعال‌سازی لایه امنیتی
    SecurityService.Instance.EnableStrictMode();
    
    // ... ادامه کدها ...
}
```

### 2. افزودن به Settings.settings

تنظیمات زیر را به فایل `Properties/Settings.settings` اضافه کنید:

```xml
<Setting Name="DashboardAddonsEnabled" Type="System.Boolean" Scope="User">
  <Value Profile="(Default)">True</Value>
</Setting>
<Setting Name="DashboardRefreshInterval" Type="System.Int32" Scope="User">
  <Value Profile="(Default)">5</Value>
</Setting>
<!-- تنظیمات امنیتی -->
<Setting Name="ApiSecurityEnabled" Type="System.Boolean" Scope="User">
  <Value Profile="(Default)">True</Value>
</Setting>
<Setting Name="LocalhostOnly" Type="System.Boolean" Scope="User">
  <Value Profile="(Default)">True</Value>
</Setting>
<Setting Name="MaxRequestPerMinute" Type="System.Int32" Scope="User">
  <Value Profile="(Default)">100</Value>
</Setting>
```

### 3. بررسی وضعیت نصب

برنامه باید به صورت دوره‌ای وضعیت نصب بودن برنامه‌های خارجی را بررسی کند:

```csharp
// در PopWindow.xaml.cs یا MainWindow
private async void CheckInstalledAddons()
{
    // بررسی امنیتی قبل از اسکن
    if (!SecurityService.Instance.IsSecureContext())
    {
        Logger.Warn("Insecure context detected. Addon scan aborted.");
        return;
    }
    
    await DashboardAddonService.Instance.ScanAllAddonsAsync();
}
```

### 4. پیکربندی فایروال ویندوز

برای افزایش امنیت، پورت‌های API را در فایروال ویندوز محدود کنید:

```powershell
# PowerShell Script - اجرا به عنوان Administrator
# محدود کردن پورت 9090 به localhost فقط

New-NetFirewallRule -DisplayName "TrafficWatch API Port 9090" `
  -Direction Inbound `
  -LocalPort 9090 `
  -Protocol TCP `
  -Action Allow `
  -RemoteAddress 127.0.0.1
  
# تکرار برای سایر پورت‌ها
New-NetFirewallRule -DisplayName "TrafficWatch API Port 9091" `
  -Direction Inbound `
  -LocalPort 9091 `
  -Protocol TCP `
  -Action Allow `
  -RemoteAddress 127.0.0.1
```

---

## ایجاد افزونه جدید

### مرحله 1: ایجاد مدل اطلاعات

یک کلاس جدید در پوشه `Models/Dashboard` ایجاد کنید:

```csharp
using System.Collections.Generic;
using TrafficWatch.Models.Dashboard;

namespace TrafficWatch.Models.Dashboard
{
    public class MyNewAddonInfo : AddonInfo
    {
        public MyNewAddonInfo()
        {
            Id = "my-new-addon";
            Name = "My New Addon";
            Description = "Description of my new addon";
            Version = "1.0.0";
            Author = "Your Name";
            ApiPort = 9092; // پورت API
            DisplayOrder = 4; // ترتیب نمایش
            
            Settings = new Dictionary<string, object>
            {
                { "Setting1", true },
                { "Setting2", 100 },
                { "Setting3", "value" }
            };
        }
    }
}
```

### مرحله 2: ایجاد سرویس اختصاصی

یک سرویس برای مدیریت منطق افزونه ایجاد کنید:

```csharp
using System;
using System.Threading.Tasks;
using TrafficWatch.Services.Dashboard;

namespace TrafficWatch.Services.Dashboard
{
    public class MyNewAddonService
    {
        private readonly string _apiEndpoint;
        
        public MyNewAddonService()
        {
            var addon = DashboardAddonService.Instance.GetAddonById("my-new-addon");
            _apiEndpoint = DashboardAddonService.Instance.GetAddonApiEndpoint("my-new-addon");
        }
        
        public async Task<object> GetDataAsync()
        {
            // دریافت داده از API یا منابع دیگر
            // ...
            return null;
        }
    }
}
```

### مرحله 3: ایجاد UI

یک UserControl یا Tab برای نمایش افزونه ایجاد کنید:

```xml
<!-- View/Dashboard/MyNewAddonTab.xaml -->
<UserControl x:Class="TrafficWatch.View.Dashboard.MyNewAddonTab"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid>
        <!-- UI elements here -->
    </Grid>
</UserControl>
```

### مرحله 4: ثبت افزونه

در روش `RegisterDefaultAddons` از کلاس `DashboardAddonService`:

```csharp
private void RegisterDefaultAddons()
{
    // ... افزونه‌های موجود ...
    
    if (!_addons.Any(a => a.Id == "my-new-addon"))
    {
        _addons.Add(new MyNewAddonInfo());
    }
}
```

---

## یکپارچه‌سازی با DownloadMenger2

### پیش‌نیازها

- نصب بودن DownloadMenger2 نسخه 2.0 یا بالاتر
- فعال بودن API در تنظیمات DownloadMenger2
- پورت پیش‌فرض: 9090

### مراحل اتصال

#### 1. بررسی نصب بودن

سیستم به صورت خودکار مسیرهای نصب معمول را بررسی می‌کند:

```csharp
bool isInstalled = DownloadManagerService.IsDownloadMengerInstalled();
// یا
var addon = DashboardAddonService.Instance.GetAddonById("download-manager");
bool isInstalled = addon.IsInstalled;
```

#### 2. دریافت وضعیت دانلود منیجر

```csharp
var dmService = new DownloadManagerService();
dmService.SetEnabled(true);
dmService.SetApiEndpoint("http://127.0.0.1:9090");

var status = await dmService.GetStatusAsync();

if (status.IsRunning)
{
    Console.WriteLine($"Active Downloads: {status.ActiveDownloads}");
    Console.WriteLine($"Total Speed: {status.TotalDownloadSpeed} bytes/s");
}
```

#### 3. دریافت لیست دانلودها

```csharp
var downloads = await dmService.GetActiveDownloadsAsync();

foreach (var download in downloads)
{
    Console.WriteLine($"{download.FileName}: {download.Progress}%");
}
```

### تنظیمات DownloadMenger2

در برنامه DownloadMenger2، تنظیمات زیر باید فعال باشند:

1. **Enable TrafficWatch Integration**: `true`
2. **API Port**: `9090` (یا پورت دلخواه)
3. **Allow Local Connections**: `true`

### نمونه UI برای تب دانلود منیجر

```xml
<TabItem Header="Download Manager">
    <Grid Margin="10">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>
        
        <!-- Summary Panel -->
        <StackPanel Grid.Row="0" Orientation="Horizontal">
            <TextBlock Text="Active: " />
            <TextBlock x:Name="lblActiveDownloads" Text="0" />
            <TextBlock Text=" | Speed: " />
            <TextBlock x:Name="lblTotalSpeed" Text="0 KB/s" />
        </StackPanel>
        
        <!-- Downloads List -->
        <ListBox Grid.Row="1" x:Name="lstDownloads">
            <ListBox.ItemTemplate>
                <DataTemplate>
                    <StackPanel>
                        <TextBlock Text="{Binding FileName}" FontWeight="Bold"/>
                        <ProgressBar Value="{Binding Progress}" Maximum="100" Height="10"/>
                        <TextBlock Text="{Binding Speed, StringFormat={}{0:F2} KB/s}"/>
                    </StackPanel>
                </DataTemplate>
            </ListBox.ItemTemplate>
        </ListBox>
    </Grid>
</TabItem>
```

---

## تنظیمات و پیکربندی

### تنظیمات عمومی داشبورد

در فایل `Settings.settings`:

| نام تنظیم | نوع | پیش‌فرض | توضیحات |
|-----------|-----|---------|----------|
| DashboardAddonsEnabled | bool | true | فعال/غیرفعال کردن کل سیستم داشبورد |
| DashboardRefreshInterval | int | 5 | فاصله بروزرسانی (ثانیه) |
| ShowAddonTabs | bool | true | نمایش تب‌های افزونه‌ها |

### تنظیمات هر افزونه

هر افزونه می‌تواند تنظیمات مخصوص به خود را داشته باشد:

```csharp
var addon = DashboardAddonService.Instance.GetAddonById("download-manager");

// خواندن تنظیم
bool showSpeed = (bool)addon.Settings["ShowSpeed"];

// تغییر تنظیم
addon.Settings["ShowSpeed"] = false;
DashboardAddonService.Instance.UpdateAddonSettings("download-manager", addon.Settings);
```

### تغییر ترتیب نمایش

```csharp
// قرار دادن دانلود منیجر در اولویت اول
DashboardAddonService.Instance.SetAddonDisplayOrder("download-manager", 1);

// قرار دادن مانیتور سیستم در اولویت دوم
DashboardAddonService.Instance.SetAddonDisplayOrder("system-monitor", 2);
```

### فعال/غیرفعال کردن افزونه

```csharp
// غیرفعال کردن افزونه موسیقی
DashboardAddonService.Instance.SetAddonEnabled("music-player", false);

// فعال کردن مجدد
DashboardAddonService.Instance.SetAddonEnabled("music-player", true);
```

---

## نمونه کدها

### نمونه کامل: نمایش اطلاعات دانلود منیجر در داشبورد

```csharp
using System;
using System.Threading.Tasks;
using System.Windows;
using TrafficWatch.Services;
using TrafficWatch.Services.Dashboard;
using TrafficWatch.Models;

namespace TrafficWatch.ViewModel
{
    public class DownloadManagerViewModel
    {
        private readonly DownloadManagerService _dmService;
        private System.Timers.Timer _refreshTimer;
        
        public DownloadManagerViewModel()
        {
            _dmService = new DownloadManagerService();
            
            // بررسی نصب بودن
            var addon = DashboardAddonService.Instance.GetAddonById("download-manager");
            if (addon.IsInstalled && addon.IsEnabled)
            {
                Initialize();
            }
            
            // گوش دادن به تغییرات وضعیت افزونه
            DashboardAddonService.Instance.OnAddonStateChanged += OnAddonStateChanged;
        }
        
        private void OnAddonStateChanged(object sender, AddonStateChangedEventArgs e)
        {
            if (e.Addon.Id == "download-manager")
            {
                if (e.Addon.IsEnabled && e.Addon.IsInstalled)
                {
                    Initialize();
                }
                else
                {
                    Stop();
                }
            }
        }
        
        private void Initialize()
        {
            _dmService.SetEnabled(true);
            _dmService.SetApiEndpoint(DashboardAddonService.Instance.GetAddonApiEndpoint("download-manager"));
            
            _refreshTimer = new System.Timers.Timer(5000); // 5 seconds
            _refreshTimer.Elapsed += async (s, e) => await RefreshDataAsync();
            _refreshTimer.Start();
            
            // اولین بروزرسانی
            _ = RefreshDataAsync();
        }
        
        private async Task RefreshDataAsync()
        {
            try
            {
                var status = await _dmService.GetStatusAsync();
                
                if (status.IsRunning)
                {
                    // بروزرسانی UI
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ActiveDownloads = status.ActiveDownloads;
                        TotalSpeed = status.TotalDownloadSpeed;
                        // ...
                    });
                    
                    // دریافت لیست دانلودها
                    var downloads = await _dmService.GetActiveDownloadsAsync();
                    // بروزرسانی لیست...
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing download data: {ex.Message}");
            }
        }
        
        private void Stop()
        {
            _refreshTimer?.Stop();
            _refreshTimer?.Dispose();
        }
        
        // Properties for UI binding
        public int ActiveDownloads { get; private set; }
        public double TotalSpeed { get; private set; }
    }
}
```

### نمونه: ایجاد تب خودکار برای هر افزونه

```csharp
public partial class MainWindow : Window
{
    private void LoadAddonTabs()
    {
        var addons = DashboardAddonService.Instance.GetAllAddons();
        
        foreach (var addon in addons.Where(a => a.IsEnabled && a.IsInstalled))
        {
            CreateAddonTab(addon);
        }
    }
    
    private void CreateAddonTab(AddonInfo addon)
    {
        var tabItem = new TabItem
        {
            Header = addon.Name,
            Tag = addon.Id
        };
        
        // بارگذاری UserControl مربوطه
        var control = LoadAddonControl(addon.Id);
        if (control != null)
        {
            tabItem.Content = control;
            MainTabControl.Items.Add(tabItem);
        }
    }
    
    private UserControl LoadAddonControl(string addonId)
    {
        return addonId switch
        {
            "download-manager" => new View.Dashboard.DownloadManagerTab(),
            "music-player" => new View.Dashboard.MusicPlayerTab(),
            "system-monitor" => new View.Dashboard.SystemMonitorTab(),
            _ => null
        };
    }
}
```

---

## عیب‌یابی

### مشکل: افزونه نمایش داده نمی‌شود

**راه حل:**
1. بررسی کنید افزونه در `RegisterDefaultAddons` ثبت شده باشد
2. بررسی کنید `IsEnabled` و `IsInstalled` هر دو `true` باشند
3. لاگ‌ها را بررسی کنید

### مشکل: ارتباط با DownloadMenger2 برقرار نمی‌شود

**راه حل:**
1. بررسی کنید DownloadMenger2 در حال اجرا باشد
2. بررسی کنید پورت 9090 آزاد باشد
3. API را مستقیماً تست کنید:
   ```bash
   curl http://127.0.0.1:9090/api/status
   ```
4. تنظیمات DownloadMenger2 را بررسی کنید

### مشکل: تنظیمات ذخیره نمی‌شوند

**راه حل:**
1. بررسی کنید مسیر `%LocalAppData%\TrafficWatch\` وجود داشته باشد
2. دسترسی نوشتن به پوشه را بررسی کنید
3. بعد از تغییرات `SaveAddons()` فراخوانی شود

---

## بهترین روش‌ها

1. **Thread Safety**: همیشه بروزرسانی UI را در thread اصلی انجام دهید
2. **Error Handling**: تمام خطاها را مدیریت کنید تا برنامه کرش نکند
3. **Performance**: از Timer با فاصله مناسب استفاده کنید (نه خیلی کوتاه)
4. **Memory Management**: رویدادها را هنگام حذف اشتراک لغو کنید
5. **User Experience**: وضعیت نصب/اجرا را به کاربر نمایش دهید

---

## سوالات متداول

**سوال:** آیا می‌توانم افزونه‌های شخص ثالث اضافه کنم؟  
**جواب:** بله، با پیروی از ساختار `IAddonInfo` می‌توانید افزونه‌های جدید ایجاد کنید.

**سوال:** چگونه می‌توانم پورت API را تغییر دهم؟  
**جواب:** در تنظیمات هر افزونه، مقدار `ApiPort` را تغییر دهید.

**سوال:** آیا افزونه‌ها می‌توانند به اینترنت متصل شوند؟  
**جواب:** بله، اما توصیه می‌شود فقط از localhost استفاده کنید.

**سوال:** چگونه می‌توانم افزونه‌ای را کاملاً حذف کنم؟  
**جواب:** فعلاً امکان حذف کامل وجود ندارد، فقط می‌توان آن را غیرفعال کرد.

---

## تماس و پشتیبانی

برای گزارش مشکلات یا提出 پیشنهادات:
- GitHub Issues: https://github.com/hamerstandr/TrafficWatch/issues
- Email: support@trafficwatch.ir

---

**نسخه سند:** 1.0  
**تاریخ انتشار:** 2024  
**تهیه شده برای:** TrafficWatch Development Team

---

## عیب‌یابی و پشتیبانی {#troubleshooting}

### مشکل: افزونه نمایش داده نمی‌شود

**راه حل:**
1. بررسی کنید افزونه در `RegisterDefaultAddons` ثبت شده باشد
2. بررسی کنید `IsEnabled` و `IsInstalled` هر دو `true` باشند
3. لاگ‌ها را بررسی کنید
4. **بررسی امنیتی:** مطمئن شوید SecurityService فعال است

### مشکل: ارتباط با DownloadMenger2 برقرار نمی‌شود

**راه حل:**
1. بررسی کنید DownloadMenger2 در حال اجرا باشد
2. بررسی کنید پورت 9090 آزاد باشد
3. API را مستقیماً تست کنید:
   ```bash
   curl http://127.0.0.1:9090/api/status
   ```
4. تنظیمات DownloadMenger2 را بررسی کنید
5. **بررسی فایروال:** مطمئن شوید پورت در فایروال مسدود نشده باشد
6. **بررسی توکن API:** اگر از احراز هویت استفاده می‌کنید، توکن را بررسی کنید

### مشکل: تنظیمات ذخیره نمی‌شوند

**راه حل:**
1. بررسی کنید مسیر `%LocalAppData%\TrafficWatch\` وجود داشته باشد
2. دسترسی نوشتن به پوشه را بررسی کنید
3. بعد از تغییرات `SaveAddons()` فراخوانی شود
4. **رمزنگاری:** اگر داده‌ها رمزنگاری شده‌اند، کلید decryption را بررسی کنید

### مشکل: خطاهای امنیتی در لاگ مشاهده می‌شود

**راه حل:**
1. نوع خطا را شناسایی کنید (XSS, Path Traversal, etc.)
2. ورودی‌های افزونه را بررسی کنید
3. از SecurityValidator برای اعتبارسنجی استفاده کنید
4. در صورت نیاز، قوانین امنیتی را به‌روز کنید

---

## بهترین روش‌ها

### توسعه ایمن

1. **Thread Safety**: همیشه بروزرسانی UI را در thread اصلی انجام دهید
2. **Error Handling**: تمام خطاها را مدیریت کنید تا برنامه کرش نکند
3. **Performance**: از Timer با فاصله مناسب استفاده کنید (نه خیلی کوتاه)
4. **Memory Management**: رویدادها را هنگام حذف اشتراک لغو کنید
5. **User Experience**: وضعیت نصب/اجرا را به کاربر نمایش دهید
6. **Security First**: همیشه اصل "امنیت اول" را رعایت کنید

### امنیت شبکه

```csharp
// ✅ الگوی صحیح برای ارتباطات API
public class SecureApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiToken;
    
    public SecureApiClient(string baseUrl, string apiToken)
    {
        // بررسی localhost
        if (!baseUrl.Contains("127.0.0.1") && !baseUrl.Contains("localhost"))
        {
            throw new SecurityException("Only localhost connections are allowed.");
        }
        
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(5)
        };
        
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", apiToken);
    }
    
    public async Task<T> GetAsync<T>(string endpoint)
    {
        // Rate limiting
        if (!RateLimiter.Instance.CanMakeRequest())
        {
            throw new RateLimitExceededException();
        }
        
        var response = await _httpClient.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<T>();
    }
}
```

### مدیریت خطا

```csharp
public class AddonErrorHandler
{
    public static async Task<T> ExecuteWithSecurityCheck<T>(
        Func<Task<T>> operation, 
        string operationName)
    {
        try
        {
            // بررسی زمینه امنیتی
            if (!SecurityService.Instance.IsSecureContext())
            {
                Logger.Warn($"Security context violation in {operationName}");
                throw new SecurityException("Insecure execution context");
            }
            
            return await operation();
        }
        catch (SecurityException ex)
        {
            Logger.Error($"Security error in {operationName}: {ex.Message}");
            throw; // Re-throw security exceptions
        }
        catch (Exception ex)
        {
            // لاگ عمومی بدون افشای جزئیات حساس
            Logger.Error($"Error in {operationName}: {ex.GetType().Name}");
            
            // بازگشت مقدار پیش‌فرض ایمن
            return default(T);
        }
    }
}
```

---

## سوالات متداول {#faq}

**سوال:** آیا می‌توانم افزونه‌های شخص ثالث اضافه کنم؟  
**جواب:** بله، با پیروی از ساختار `IAddonInfo` و رعایت اصول امنیتی می‌توانید افزونه‌های جدید ایجاد کنید. تمام افزونه‌های شخص ثالث باید از چک‌لیست امنیتی عبور کنند.

**سوال:** چگونه می‌توانم پورت API را تغییر دهم؟  
**جواب:** در تنظیمات هر افزونه، مقدار `ApiPort` را تغییر دهید. پورت باید در محدوده 9000-9999 باشد.

**سوال:** آیا افزونه‌ها می‌توانند به اینترنت متصل شوند؟  
**جواب:** **خیر.** به دلایل امنیتی، تمام ارتباطات باید به localhost (127.0.0.1) محدود شوند. این یک الزام امنیتی اجباری است.

**سوال:** چگونه می‌توانم افزونه‌ای را کاملاً حذف کنم؟  
**جواب:** فعلاً امکان حذف کامل وجود ندارد، فقط می‌توان آن را غیرفعال کرد. در نسخه‌های آینده، قابلیت حذف کامل افزوده خواهد شد.

**سوال:** آیا HTTPS برای ارتباطات داخلی لازم است؟  
**جواب:** برای ارتباطات localhost، HTTP کافی است. اما اگر از reverse proxy استفاده می‌کنید، HTTPS توصیه می‌شود.

**سوال:** چگونه توکن‌های API را مدیریت کنم؟  
**جواب:** از کلاس `ApiTokenProvider` استفاده کنید. توکن‌ها باید به صورت دوره‌ای چرخش کنند (token rotation).

**سوال:** آیا می‌توانم از WebSocket استفاده کنم؟  
**جواب:** بله، اما باید همان محدودیت‌های امنیتی (localhost only, token auth) رعایت شود.

---

## منابع اضافی

### مستندات مرتبط

- [Microsoft Security Guidelines for .NET](https://docs.microsoft.com/en-us/dotnet/standard/security/)
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [.NET Cryptography Services](https://docs.microsoft.com/en-us/dotnet/standard/security/cryptographic-services)

### ابزارهای پیشنهادی

- **Static Analysis:** SonarQube, CodeQL
- **Dependency Check:** OWASP Dependency-Check
- **Penetration Testing:** OWASP ZAP, Burp Suite

---

## تماس و پشتیبانی

برای گزارش مشکلات یا提出 پیشنهادات:
- GitHub Issues: https://github.com/hamerstandr/TrafficWatch/issues  
- Email: support@trafficwatch.ir
- Security Reports: security@trafficwatch.ir (برای گزارش آسیب‌پذیری‌های امنیتی)

---

**نسخه سند:** 2.0  
**سازگار با:** .NET 10 SDK, Visual Studio 2026  
**تاریخ به‌روزرسانی:** 2026  
**سطح امنیت:** Enhanced Security Mode  
**تهیه شده برای:** TrafficWatch Development Team

---

## تاریخچه تغییرات

### نسخه 2.0 (2026)
- ارتقا به .NET 10 SDK و Visual Studio 2026
- افزودن بخش جامع امنیت و ملاحظات امنیتی
- اضافه کردن کدهای نمونه برای ApiTokenProvider و DataEncryptor
- افزودن چک‌لیست امنیتی برای توسعه‌دهندگان
- به‌روزرسانی راهنمای فایروال و پیکربندی شبکه
- افزودن بخش عیب‌یابی پیشرفته

### نسخه 1.0 (2024)
- انتشار اولیه مستندات
- پشتیبانی از سیستم افزونه‌ها
- یکپارچه‌سازی با DownloadMenger2

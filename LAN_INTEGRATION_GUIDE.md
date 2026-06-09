# 🌐 راهنمای جامع اتصال به داشبورد TrafficWatch از طریق شبکه داخلی (LAN)

## فهرست مطالب
1. [معرفی](#معرفی)
2. [تفاوت‌های کلیدی: localhost در مقابل شبکه LAN](#تفاوت‌های-کلیدی-localhost-در-مقابل-شبکه-lan)
3. [معماری سیستم](#معماری-سیستم)
4. [تنظیمات پیکربندی](#تنظیمات-پیکربندی)
5. [راه‌اندازی امنیتی](#راه‌اندازی-امنیتی)
6. [چک‌لیست کامل امنیتی](#چک‌لیست-کامل-امنیتی)
7. [نمونه پیاده‌سازی](#نمونه-پیاده‌سازی)
8. [عیب‌یابی](#عیب‌یابی)

---

## معرفی

این سند راهنمای کامل برای اتصال برنامه‌های خارجی به داشبورد **TrafficWatch** از طریق **شبکه داخلی (LAN)** است. برخلاف ارتباطات localhost که فقط روی یک سیستم کار می‌کنند، این راهنما امکان استریم داده‌ها را در سطح شبکه داخلی فراهم می‌کند.

### کاربردها
- اتصال چندین کلاینت به سرور مرکزی TrafficWatch
- استریم اطلاعات ترافیک شبکه به داشبوردهای توزیع‌شده
- یکپارچه‌سازی با سیستم‌های مانیتورینگ سازمانی
- اشتراک‌گذاری داده‌های دانلود منیجر بین چندین کاربر

---

## تفاوت‌های کلیدی: localhost در مقابل شبکه LAN

| ویژگی | localhost (127.0.0.1) | شبکه داخلی (LAN) |
|-------|----------------------|------------------|
| **محدوده دسترسی** | فقط سیستم محلی | تمام دستگاه‌های متصل به شبکه |
| **آدرس IP** | 127.0.0.1 | 192.168.x.x / 10.x.x.x / 172.16.x.x |
| **امنیت** | پایین (نیاز به فایروال ندارد) | بالا (نیاز به احراز هویت و رمزنگاری) |
| **پیکربندی فایروال** | لازم نیست | الزامی |
| **توکن احراز هویت** | اختیاری | اجباری |
| **رمزنگاری** | اختیاری | توصیه شده (HTTPS/TLS) |
| **Rate Limiting** | اختیاری | الزامی |

---

## معماری سیستم

```
┌──────────────────────────────────────────────────────────────────┐
│                     شبکه داخلی (LAN)                             │
│                                                                  │
│  ┌─────────────┐      ┌─────────────┐      ┌─────────────┐      │
│  │  Client 1   │      │  Client 2   │      │  Client 3   │      │
│  │ 192.168.1.10│      │ 192.168.1.11│      │ 192.168.1.12│      │
│  └──────┬──────┘      └──────┬──────┘      └──────┬──────┘      │
│         │                    │                    │              │
│         └────────────────────┼────────────────────┘              │
│                              │                                   │
│                              ▼                                   │
│                    ┌─────────────────┐                           │
│                    │   TrafficWatch  │                           │
│                    │   Main Server   │                           │
│                    │  192.168.1.50   │                           │
│                    │    Port: 9090   │                           │
│                    └────────┬────────┘                           │
│                             │                                    │
│              ┌──────────────┼──────────────┐                    │
│              │              │              │                    │
│              ▼              ▼              ▼                    │
│     ┌────────────┐ ┌────────────┐ ┌────────────┐                │
│     │ Download   │ │   Music    │ │  System    │                │
│     │  Manager   │ │  Player    │ │  Monitor   │                │
│     │  Addon     │ │  Addon     │ │  Addon     │                │
│     └────────────┘ └────────────┘ └────────────┘                │
└──────────────────────────────────────────────────────────────────┘
```

### اجزای اصلی

1. **سرور مرکزی**: نمونه اصلی TrafficWatch که داده‌ها را جمع‌آوری و توزیع می‌کند
2. **کلاینت‌ها**: برنامه‌هایی که به سرور متصل شده و داده دریافت می‌کنند
3. **افزونه‌ها**: ماژول‌های تخصصی (دانلود، موسیقی، مانیتورینگ)
4. **لایه امنیت**: شامل احراز هویت، رمزنگاری و کنترل دسترسی

---

## تنظیمات پیکربندی

### 1. فایل Settings.settings

تنظیمات زیر را به پروژه اضافه کنید:

```xml
<Setting Name="NetworkMode" Type="System.Boolean" Scope="User">
  <Value Profile="(Default)">True</Value>
</Setting>
<Setting Name="ServerIpAddress" Type="System.String" Scope="User">
  <Value Profile="(Default)"></Value>
</Setting>
<Setting Name="ServerPort" Type="System.Int32" Scope="User">
  <Value Profile="(Default)">9090</Value>
</Setting>
<Setting Name="ApiAuthToken" Type="System.String" Scope="User">
  <Value Profile="(Default)"></Value>
</Setting>
<Setting Name="UseSecureConnection" Type="System.Boolean" Scope="User">
  <Value Profile="(Default)">True</Value>
</Setting>
<Setting Name="AllowedIpRanges" Type="System.String" Scope="User">
  <Value Profile="(Default)">192.168.1.0/24,10.0.0.0/8</Value>
</Setting>
<Setting Name="MaxRequestsPerMinute" Type="System.Int32" Scope="User">
  <Value Profile="(Default)">100</Value>
</Setting>
<Setting Name="EnableSecurityLogging" Type="System.Boolean" Scope="User">
  <Value Profile="(Default)">True</Value>
</Setting>
<Setting Name="ConnectionTimeoutMs" Type="System.Int32" Scope="User">
  <Value Profile="(Default)">5000</Value>
</Setting>
```

### 2. کلاس NetworkSettings

```csharp
using TrafficWatch.Models.Config;

// بارگذاری تنظیمات
var settings = NetworkSettings.Load();

// تغییر تنظیمات
settings.ServerIpAddress = "192.168.1.50";
settings.ServerPort = 9090;
settings.ApiAuthToken = NetworkSecurityService.GenerateSecureToken();
settings.AllowedIpRanges = new List<string> 
{ 
    "192.168.1.0/24",
    "10.0.0.0/8"
};
settings.UseSecureConnection = true;
settings.MaxRequestsPerMinute = 100;

// ذخیره تنظیمات
settings.Save();
```

### 3. رابط کاربری تنظیمات

برای دسترسی به تنظیمات شبکه از کنترل `NetworkSettingsControl` استفاده کنید:

```csharp
// در MainWindow یا SettingsWindow
private void OpenNetworkSettings()
{
    var networkSettingsWindow = new Window
    {
        Title = "تنظیمات شبکه",
        Width = 550,
        Height = 700,
        Content = new NetworkSettingsControl(),
        WindowStartupLocation = WindowStartupLocation.CenterOwner,
        Owner = this
    };
    
    networkSettingsWindow.ShowDialog();
}
```

---

## راه‌اندازی امنیتی

### 1. فعال‌سازی سرویس امنیتی

در فایل `App.xaml.cs`:

```csharp
private void Application_Startup(object sender, StartupEventArgs e)
{
    // ... کدهای موجود ...
    
    // راه‌اندازی سرویس امنیتی شبکه
    var securityService = NetworkSecurityService.Instance;
    securityService.OnSecurityViolation += OnSecurityViolation;
    
    // ... ادامه کدها ...
}

private void OnSecurityViolation(object sender, SecurityViolationEventArgs e)
{
    // لاگ‌گیری یا هشدار
    Debug.WriteLine($"[SECURITY] {e.ViolationType}: {e.Details}");
    
    // ارسال ایمیل یا نوتیفیکیشن (اختیاری)
    // SendSecurityAlert(e);
}
```

### 2. پیکربندی فایروال ویندوز

اسکریپت PowerShell زیر را اجرا کنید:

```powershell
# ایجاد قانون ورودی برای پورت TrafficWatch
New-NetFirewallRule -DisplayName "TrafficWatch API" `
    -Direction Inbound `
    -LocalPort 9090 `
    -Protocol TCP `
    -Action Allow `
    -Profile Private `
    -RemoteAddress 192.168.1.0/24

# برای HTTPS (اگر استفاده می‌کنید)
New-NetFirewallRule -DisplayName "TrafficWatch HTTPS" `
    -Direction Inbound `
    -LocalPort 9443 `
    -Protocol TCP `
    -Action Allow `
    -Profile Private `
    -RemoteAddress 192.168.1.0/24
```

### 3. اعتبارسنجی درخواست‌ها

در سمت سرور، تمام درخواست‌ها را اعتبارسنجی کنید:

```csharp
public async Task<HttpResponseMessage> HandleRequestAsync(HttpRequestMessage request)
{
    // استخراج IP کلاینت
    var clientIp = request.GetClientIpAddress();
    
    // استخراج توکن از هدر
    var authToken = request.Headers.TryGetValues("X-API-Token", out var values) 
        ? values.FirstOrDefault() 
        : null;
    
    // اعتبارسنجی
    var validationResult = NetworkSecurityService.Instance.ValidateRequest(clientIp, authToken);
    
    if (!validationResult.IsValid)
    {
        return new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(validationResult.ErrorMessage)
        };
    }
    
    // پردازش درخواست معتبر
    // ...
    
    return new HttpResponseMessage(HttpStatusCode.OK);
}
```

---

## چک‌لیست کامل امنیتی

### 🔐 1. احراز هویت و کنترل دسترسی

- [ ] استفاده از توکن‌های API قوی (حداقل 32 بایت)
- [ ] ذخیره‌سازی امن توکن‌ها (هش شده با SHA-256)
- [ ] پیاده‌سازی لیست سفید IP (IP Whitelist)
- [ ] پشتیبانی از CIDR برای محدوده‌های IP
- [ ] غیرفعال کردن احراز هویت برای محیط‌های تست
- [ ] چرخش دوره‌ای توکن‌ها (Token Rotation)

### 🔒 2. رمزنگاری و حفاظت از داده‌ها

- [ ] فعال‌سازی HTTPS/TLS برای تمام ارتباطات
- [ ] استفاده از TLS 1.2 یا بالاتر
- [ ] رمزنگاری داده‌های حساس در حال انتقال
- [ ] عدم ارسال اطلاعات حساس در URL
- [ ] استفاده از هدرهای امنیتی مناسب

### 🌐 3. کنترل دسترسی شبکه

- [ ] محدود کردن دسترسی به subnetهای مشخص
- [ ] پیکربندی فایروال ویندوز برای پورت‌ها
- [ ] غیرفعال کردن پورت‌های غیرضروری
- [ ] استفاده از VLAN برای جداسازی ترافیک
- [ ] مانیتورینگ ترافیک ورودی/خروجی

### ⏱️ 4. محدودیت نرخ و محافظت در برابر حملات

- [ ] پیاده‌سازی Rate Limiting (حداکثر درخواست در دقیقه)
- [ ] محافظت در برابر حملات DDoS
- [ ] جلوگیری از Brute Force Attacks
- [ ] استفاده از Circuit Breaker Pattern
- [ ] لاگ‌گیری و هشدار برای ترافیک غیرعادی

### ✅ 5. اعتبارسنجی ورودی‌ها

- [ ] اعتبارسنجی تمام ورودی‌های کاربر
- [ ] جلوگیری از SQL Injection
- [ ] جلوگیری از XSS (Cross-Site Scripting)
- [ ] جلوگیری از Path Traversal
- [ ] Sanitization داده‌های ورودی

### 📝 6. لاگ‌گیری و مانیتورینگ

- [ ] ثبت تمام تلاش‌های ناموفق احراز هویت
- [ ] لاگ‌گیری نقض‌های امنیتی
- [ ] مانیتورینگ بلادرنگ ترافیک شبکه
- [ ] هشدار خودکار برای فعالیت‌های مشکوک
- [ ] نگهداری لاگ‌ها برای حداقل 90 روز

### ⚠️ 7. مدیریت خطا

- [ ] عدم نمایش جزئیات خطا به کاربر نهایی
- [ ] ثبت خطاها در لاگ‌های داخلی
- [ ] استفاده از پیام‌های خطای عمومی
- [ ] جلوگیری از نشت اطلاعات در خطاها

### 🔄 8. به‌روزرسانی و نگهداری

- [ ] به‌روزرسانی منظم کتابخانه‌های شخص ثالث
- [ ] اعمال پچ‌های امنیتی .NET
- [ ] بازبینی دوره‌ای کد از نظر امنیتی
- [ ] تست نفوذپذیری دوره‌ای

### 🧪 9. تست امنیتی

- [ ] انجام تست‌های امنیتی خودکار (SAST/DAST)
- [ ] استفاده از ابزارهای تحلیل کد (SonarQube, CodeQL)
- [ ] تست دستی با OWASP ZAP
- [ ] بازبینی کد توسط همکاران

### 📚 10. مستندات و آموزش

- [ ] مستندسازی تنظیمات امنیتی
- [ ] آموزش تیم توسعه در مورد بهترین روش‌ها
- [ ] ایجاد راهنمای پاسخ به حوادث امنیتی
- [ ] به‌روزرسانی مستندات پس از هر تغییر

---

## جدول ارزیابی ریسک

| نوع حمله | احتمال | تأثیر | ریسک کلی | راهکار کاهش |
|----------|--------|-------|----------|-------------|
| **احراز هویت ضعیف** | متوسط | بالا | **بالا** | توکن‌های قوی + Rate Limiting |
| **شنود ترافیک** | بالا | بالا | **بحرانی** | HTTPS/TLS اجباری |
| **DDoS** | متوسط | بالا | **بالا** | Rate Limiting + Firewall |
| **Brute Force** | بالا | متوسط | **بالا** | Token Expiration + Lockout |
| **SQL Injection** | پایین | بالا | **متوسط** | Parameterized Queries |
| **XSS** | پایین | متوسط | **متوسط** | Input Sanitization |
| **Path Traversal** | پایین | بالا | **متوسط** | Path Validation |
| **نشت اطلاعات** | متوسط | بالا | **بالا** | Error Handling صحیح |

---

## نمونه پیاده‌سازی

### الف) سرور API امن

```csharp
using System.Net;
using System.Text.Json;
using TrafficWatch.Services.Network;

namespace TrafficWatch.HttpServer
{
    public class SecureApiServer
    {
        private readonly HttpListener _listener;
        private readonly CancellationTokenSource _cts;

        public SecureApiServer(string prefix = "http://+:9090/")
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(prefix);
            _cts = new CancellationTokenSource();
        }

        public async Task StartAsync()
        {
            _listener.Start();
            Console.WriteLine($"Server started on {_listener.Prefixes}");

            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    _ = ProcessRequestAsync(context);
                }
                catch (HttpListenerException) when (_cts.Token.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        private async Task ProcessRequestAsync(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            // استخراج IP کلاینت
            var clientIp = request.RemoteEndPoint.Address.ToString();

            // استخراج توکن
            var token = request.Headers["X-API-Token"];

            // اعتبارسنجی
            var validation = NetworkSecurityService.Instance.ValidateRequest(clientIp, token);

            if (!validation.IsValid)
            {
                response.StatusCode = (int)HttpStatusCode.Forbidden;
                var errorBytes = System.Text.Encoding.UTF8.GetBytes(validation.ErrorMessage);
                response.ContentLength64 = errorBytes.Length;
                await response.OutputStream.WriteAsync(errorBytes, 0, errorBytes.Length);
                response.Close();
                return;
            }

            // پردازش درخواست معتبر
            try
            {
                var responseData = await HandleApiRequest(request);
                var json = JsonSerializer.Serialize(responseData);
                var bytes = System.Text.Encoding.UTF8.GetBytes(json);

                response.StatusCode = (int)HttpStatusCode.OK;
                response.ContentType = "application/json; charset=utf-8";
                response.ContentLength64 = bytes.Length;
                await response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
            }
            catch (Exception ex)
            {
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                var errorBytes = System.Text.Encoding.UTF8.GetBytes("Internal Server Error");
                response.ContentLength64 = errorBytes.Length;
                await response.OutputStream.WriteAsync(errorBytes, 0, errorBytes.Length);
            }
            finally
            {
                response.Close();
            }
        }

        private Task<object> HandleApiRequest(HttpListenerRequest request)
        {
            // پیاده‌سازی منطق API
            return Task.FromResult<object>(new { Status = "OK", Timestamp = DateTime.UtcNow });
        }

        public void Stop()
        {
            _cts.Cancel();
            _listener.Stop();
        }
    }
}
```

### ب) کلاینت امن برای اتصال به سرور

```csharp
using System.Net.Http;
using System.Net.Http.Headers;
using TrafficWatch.Models.Config;

namespace TrafficWatch.Services.Network
{
    public class SecureApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly NetworkSettings _settings;

        public SecureApiClient(NetworkSettings settings)
        {
            _settings = settings;
            
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMilliseconds(_settings.ConnectionTimeoutMs),
                BaseAddress = new Uri(_settings.GetBaseUrl())
            };

            // افزودن توکن به هدرها
            if (!string.IsNullOrWhiteSpace(_settings.ApiAuthToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", _settings.ApiAuthToken);
                
                _httpClient.DefaultRequestHeaders.Add("X-API-Token", _settings.ApiAuthToken);
            }

            // تنظیمات SSL/TLS
            if (_settings.UseSecureConnection)
            {
                // اطمینان از استفاده از TLS 1.2+
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
            }
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();
                
                var json = await response.Content.ReadAsStringAsync();
                return System.Text.Json.JsonSerializer.Deserialize<T>(json);
            }
            catch (HttpRequestException ex)
            {
                // لاگ‌گیری خطا
                Debug.WriteLine($"HTTP Error: {ex.Message}");
                throw;
            }
        }

        public async Task<T> PostAsync<T>(string endpoint, object data)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(data);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();
            
            var responseJson = await response.Content.ReadAsStringAsync();
            return System.Text.Json.JsonSerializer.Deserialize<T>(responseJson);
        }
    }
}
```

### ج) استفاده در برنامه کلاینت

```csharp
// در برنامه کلاینت
var settings = new NetworkSettings
{
    ServerIpAddress = "192.168.1.50",
    ServerPort = 9090,
    ApiAuthToken = "your-secure-token-here",
    UseSecureConnection = false, // برای HTTP داخلی
    MaxRequestsPerMinute = 100
};

var client = new SecureApiClient(settings);

// دریافت وضعیت از سرور
var status = await client.GetAsync<ServerStatus>("/api/status");

Console.WriteLine($"Server Status: {status.IsRunning}");
Console.WriteLine($"Active Connections: {status.ActiveConnections}");
```

---

## عیب‌یابی

### مشکل: اتصال برقرار نمی‌شود

**راه حل:**
1. بررسی کنید سرور در حال اجرا باشد
2. پورت 9090 (یا پورت تنظیم شده) آزاد باشد
3. فایروال ویندوز اجازه دسترسی دهد
4. IP سرور در شبکه قابل دسترس باشد

```powershell
# تست اتصال
Test-NetConnection -ComputerName 192.168.1.50 -Port 9090
```

### مشکل: خطای احراز هویت

**راه حل:**
1. توکن API را در هر دو طرف (سرور و کلاینت) بررسی کنید
2. از یکسان بودن توکن اطمینان حاصل کنید
3. توکن جدید تولید کنید:
   ```csharp
   var newToken = NetworkSecurityService.GenerateSecureToken();
   ```

### مشکل: Rate Limiting

**راه حل:**
1. مقدار `MaxRequestsPerMinute` را افزایش دهید
2. از کش کردن داده‌ها استفاده کنید
3. درخواست‌ها را بهینه کنید

### مشکل: HTTPS/TLS

**راه حل:**
1. گواهی SSL معتبر نصب کنید
2. از پروتکل TLS 1.2 یا بالاتر استفاده کنید
3. در محیط توسعه می‌توانید موقتاً HTTPS را غیرفعال کنید

---

## بهترین روش‌ها

1. **همیشه از توکن استفاده کنید**: حتی در شبکه داخلی
2. **HTTPS را فعال کنید**: برای محافظت از داده‌ها
3. **لاگ‌گیری را فعال نگه دارید**: برای تشخیص مشکلات امنیتی
4. **تنظیمات را دوره‌ای بازبینی کنید**: به‌روزرسانی توکن‌ها و IPها
5. **از CIDR استفاده کنید**: برای مدیریت آسان‌تر محدوده‌های IP
6. **Rate Limiting را تنظیم کنید**: برای جلوگیری از سوءاستفاده

---

## منابع اضافی

- [Microsoft Security Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/security/)
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [TLS Best Practices](https://wiki.mozilla.org/Security/Server_Side_TLS)

---

**نسخه سند:** 2.0  
**تاریخ انتشار:** 2026  
**سازگار با:** .NET 10 SDK, Visual Studio 2026  
**تهیه شده برای:** TrafficWatch Development Team

# راهنمای کامل سیستم افزونه‌ها و داشبورد TrafficWatch

## معرفی

این مستند راهنمای کامل طراحی، پیاده‌سازی و استفاده از سیستم افزونه‌ها و داشبورد در برنامه مانیتور شبکه TrafficWatch است.

## ویژگی‌های اصلی

### 1. سرور وب داشبورد
- **وضعیت پیش‌فرض**: خاموش
- **فعال‌سازی**: از طریق تنظیمات (Settings)
- **آدرس دسترسی**: `http://<system-ip>:<port>/state/`
- **نمایش زنده**: تغییرات شبکه به صورت لحظه‌ای

### 2. تنظیمات سرور
- **تنظیم پورت**: قابل تنظیم در بازه 1-65535 (پیش‌فرض: 8080)
- **تنظیمات امنیتی**:
  - فعال/غیرفعال کردن احراز هویت با توکن
  - تولید توکن امنیتی تصادفی
  - تعیین IPهای مجاز (با پیشوند جدا شده با کاما)

### 3. سیستم افزونه‌ها

#### شناسایی خودکار افزونه‌ها
- اسکن سرویس‌های ویندوز برای شناسایی برنامه‌های اجرایی
- شناسایی دانلود منیجر (DownloadMenger2)
- شناسایی پخش‌کننده‌های موسیقی
- مانیتور سیستم (افزونه داخلی)

#### مدیریت افزونه‌ها در تنظیمات
هر افزونه دارای بخش مخصوص به خود در تب Addons است که شامل:
- نام و توضیحات افزونه
- وضعیت نصب (Installed/Not Installed)
- نسخه افزونه
- گزینه فعال/غیرفعال کردن
- دکمه Settings برای تنظیمات خاص

#### تب‌های داشبورد
برای هر افزونه فعال، یک تب جداگانه در داشبورد اضافه می‌شود:
- **تب Network**: نمایش ترافیک شبکه
- **تب Download Manager**: نمایش دانلودهای فعال
- **تب Music Player**: نمایش موزیک در حال پخش
- **تب System Monitor**: نمایش مصرف CPU، RAM و Disk

## ساختار فنی

### فایل‌های اصلی

#### 1. DashboardAddonService.cs
سرویس اصلی مدیریت افزونه‌ها:
```csharp
public class DashboardAddonService
{
    // متدهای اصلی:
    - Initialize()          // راه‌اندازی اولیه
    - GetAllAddons()        // دریافت تمام افزونه‌ها
    - GetEnabledAddons()    // دریافت افزونه‌های فعال
    - SetAddonEnabled()     // فعال/غیرفعال کردن
    - UpdateAddonSettings() // بروزرسانی تنظیمات
    - ScanWindowsServicesForAddons() // اسکن سرویس‌های ویندوز
}
```

#### 2. AddonInfo.cs
مدل‌های اطلاعات افزونه:
```csharp
public class AddonInfo : IAddonInfo
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Version { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsInstalled { get; set; }
    public Dictionary<string, object> Settings { get; set; }
    public int ApiPort { get; set; }
}

// کلاس‌های مشتق شده:
- DownloadManagerAddonInfo
- MusicPlayerAddonInfo
- SystemMonitorAddonInfo
```

#### 3. WindowSetting.xaml
رابط کاربری تنظیمات:
- تب Network: تنظیمات کارت شبکه
- تب Dashboard Server: تنظیمات سرور وب
- تب Addons: مدیریت افزونه‌ها
- تب Server Info: اطلاعات سرور و افزونه‌های فعال

#### 4. Server.cs
سرور وب HTTP:
- پشتیبانی از Routeهای مختلف
- مسیر `/state/` برای داشبورد اصلی
- مسیرهای `/Download`, `/Upload`, `/MaxSpeed` و غیره

## نحوه استفاده

### 1. نصب و راه‌اندازی اولیه

```bash
# نصب برنامه
TrafficWatch.Setup.exe

# اجرای برنامه
# برنامه به صورت پیش‌فرض اجرا می‌شود اما سرور وب خاموش است
```

### 2. فعال‌سازی سرور وب داشبورد

1. باز کردن تنظیمات (Settings)
2. رفتن به تب "Dashboard Server"
3. فعال کردن گزینه "Enable Web Dashboard Server"
4. تنظیم پورت (مثلاً 8080)
5. کلیک روی "Save Port"
6. (اختیاری) فعال‌سازی تنظیمات امنیتی:
   - فعال کردن "Enable Security (Token Authentication)"
   - کلیک روی "Generate" برای تولید توکن
   - وارد کردن IPهای مجاز
   - کلیک روی "Save Security Settings"

### 3. دسترسی به داشبورد

پس از فعال‌سازی سرور، می‌توانید از آدرس‌های زیر به داشبورد دسترسی پیدا کنید:

```
Local Access:    http://127.0.0.1:8080/state/
Network Access:  http://<local-ip>:8080/state/
Hostname:        http://<hostname>:8080/state/
```

### 4. مدیریت افزونه‌ها

#### مشاهده افزونه‌ها
1. باز کردن تنظیمات
2. رفتن به تب "Addons"
3. مشاهده لیست افزونه‌های شناسایی شده

#### فعال/غیرفعال کردن افزونه
1. در تب Addons، افزونه مورد نظر را پیدا کنید
2. اگر وضعیت "Installed" است، چک‌باکس "Enabled" را فعال کنید
3. تغییرات به صورت خودکار ذخیره می‌شوند

#### تنظیمات خاص افزونه
1. در تب Addons، روی دکمه "Settings" افزونه کلیک کنید
2. پنجره تنظیمات باز می‌شود
3. تنظیمات مورد نظر را تغییر دهید:
   - مقادیر Boolean: با چک‌باکس
   - مقادیر عددی: با TextBox
   - مقادیر متنی: با TextBox
4. تغییرات به صورت خودکار ذخیره می‌شوند
5. کلیک روی "Close" برای بستن پنجره

### 5. مشاهده افزونه‌های فعال

1. باز کردن تنظیمات
2. رفتن به تب "Server Info"
3. مشاهده:
   - وضعیت سرور (RUNNING/STOPPED)
   - پورت و IP سرور
   - لیست افزونه‌های فعال در بخش "Active Addons"

## توسعه افزونه جدید

### مراحل ایجاد افزونه

1. **ایجاد کلاس اطلاعات افزونه**:
```csharp
public class MyCustomAddonInfo : AddonInfo
{
    public MyCustomAddonInfo()
    {
        Id = "my-custom-addon";
        Name = "My Custom Addon";
        Description = "Description of my addon";
        Version = "1.0.0";
        Author = "Your Name";
        ApiPort = 9099; // پورت API
        DisplayOrder = 4;
        
        Settings = new Dictionary<string, object>
        {
            { "Setting1", true },
            { "Setting2", 10 },
            { "Setting3", "value" }
        };
    }
}
```

2. **ثبت افزونه در سرویس**:
```csharp
private void RegisterDefaultAddons()
{
    // ... سایر افزونه‌ها
    
    if (!_addons.Any(a => a.Id == "my-custom-addon"))
    {
        _addons.Add(new MyCustomAddonInfo());
    }
}
```

3. **بررسی نصب بودن**:
```csharp
public void CheckInstallationStatus(string addonId)
{
    var addon = GetAddonById(addonId);
    if (addon != null)
    {
        switch (addonId.ToLower())
        {
            case "my-custom-addon":
                addon.IsInstalled = CheckIfMyAddonIsInstalled();
                break;
        }
    }
}
```

4. **ایجاد Route در سرور**:
```csharp
new Route {
    Name = "MyCustomAddon",
    UrlRegex = @"^/myaddon",
    Method = "GET",
    Callable = (HttpRequest request) => {
        return new HttpResponse()
        {
            ContentAsUTF8 = GetMyAddonData(),
            ReasonPhrase = "OK",
            StatusCode = "200"
        };
    }
}
```

## رویدادهای مهم

### OnAddonStateChanged
این رویداد زمانی رخ می‌دهد که وضعیت یک افزونه تغییر کند:
- فعال/غیرفعال شدن
- نصب/حذف شدن

```csharp
_addonService.OnAddonStateChanged += (sender, e) =>
{
    // بروزرسانی UI
    LoadAddonsList();
    UpdateActiveAddonsTab();
};
```

## ذخیره‌سازی تنظیمات

تنظیمات افزونه‌ها در فایل JSON ذخیره می‌شوند:
```
Path: %LOCALAPPDATA%\TrafficWatch\DashboardAddons.json
```

فرمت فایل:
```json
[
  {
    "id": "download-manager",
    "name": "Download Manager",
    "isEnabled": true,
    "isInstalled": true,
    "settings": {
      "ShowActiveDownloads": true,
      "ShowSpeed": true,
      "RefreshInterval": 5
    }
  }
]
```

## نکات مهم

### امنیت
- همیشه از توکن‌های امنیتی قوی استفاده کنید
- IPهای مجاز را محدود کنید
- پورت‌های غیر استاندارد انتخاب کنید

### عملکرد
- تعداد افزونه‌های فعال را محدود نگه دارید
- RefreshInterval را مناسب تنظیم کنید
- از اسکن مداوم سرویس‌ها خودداری کنید

### عیب‌یابی
- اگر سرور شروع نمی‌شود، پورت را بررسی کنید
- اگر افزونه‌ای نمایش داده نمی‌شود، لاگ‌ها را بررسی کنید
- اگر داشبورد لود نمی‌شود، اتصال شبکه را بررسی کنید

## نمونه‌های عملی

### مثال 1: افزودن دانلود منیجر به عنوان سرویس ویندوز

1. نصب DownloadMenger2 به عنوان سرویس ویندوز
2. اجرای TrafficWatch
3. اسکن خودکار سرویس‌ها
4. فعال‌سازی افزونه در تب Addons
5. مشاهده تب Download Manager در داشبورد

### مثال 2: تنظیم امنیت برای دسترسی شبکه

1. فعال‌سازی سرور وب
2. فعال‌سازی Security
3. تولید توکن جدید
4. وارد کردن IPهای مجاز: `192.168.1.,10.0.0.`
5. ذخیره تنظیمات
6. دسترسی از دستگاه‌های مجاز با ارسال توکن

## نتیجه‌گیری

سیستم افزونه‌ها و داشبورد TrafficWatch یک پلتفرم قدرتمند و انعطاف‌پذیر برای مانیتورینگ شبکه و یکپارچه‌سازی با برنامه‌های دیگر فراهم می‌کند. با دنبال کردن این راهنما، می‌توانید به راحتی از ویژگی‌های موجود استفاده کرده یا افزونه‌های جدید توسعه دهید.

---
**نسخه مستند**: 1.0  
**تاریخ به‌روزرسانی**: 2024  
**مخزن پروژه**: TrafficWatch

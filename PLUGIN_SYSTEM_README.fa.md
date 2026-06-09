# راهنمای سیستم افزونه‌های TrafficWatch با استفاده از سرویس‌های ویندوز

## معرفی

این سند نحوه پیاده‌سازی و استفاده از سیستم افزونه‌ها در TrafficWatch را توضیح می‌دهد که از طریق **Named Pipes** و **سرویس‌های ویندوز** ارتباط برقرار می‌کنند.

## معماری سیستم

```
┌─────────────────────┐         Named Pipe         ┌──────────────────────┐
│   برنامه اصلی      │ <------------------------> │   سرویس ویندوز       │
│   (TrafficWatch)   │      TrafficWatchPluginPipe│   (افزونه)           │
│                     │                            │                      │
│ - NamedPipeServer  │                            │ - NamedPipeClient    │
│ - DashboardAddon   │                            │ - PluginClient       │
│   Service          │                            │                      │
└─────────────────────┘                            └──────────────────────┘
```

## سناریوی اجرا

1. **اجرای سرویس ویندوز (برنامه میزبان)**
   - سرویس `ServerTrafice` به عنوان یک سرویس ویندوز نصب و اجرا می‌شود
   - این سرویس سرور HTTP و Named Pipe را راه‌اندازی می‌کند

2. **فعال‌سازی سرور Named Pipe**
   - در `App.xaml.cs`، سرور Named Pipe به صورت خودکار هنگام شروع برنامه فعال می‌شود
   - سرور روی pipe name `TrafficWatchPluginPipe` گوش می‌دهد

3. **اجرای برنامه‌های دیگر (افزونه‌ها)**
   - هر برنامه‌ای که بخواهد به عنوان افزونه عمل کند، باید:
     - به عنوان سرویس ویندوز نصب شود (اختیاری)
     - از کلاس `PluginClient` برای ارتباط استفاده کند
     - هنگام شروع، خود را ثبت نام کند

4. **ثبت نام خودکار افزونه**
   - وقتی افزونه‌ای به Named Pipe متصل می‌شود و درخواست `register` ارسال می‌کند:
     - اطلاعات افزونه در لیست افزونه‌ها اضافه می‌شود
     - تب مربوطه در داشبورد به صورت خودکار ایجاد می‌شود
     - تنظیمات ذخیره می‌شود

5. **نمایش در داشبورد**
   - افزونه‌های فعال به صورت خودکار تب جدیدی در داشبورد ایجاد می‌کنند
   - از کنترل‌های XAML مانند `DownloadManagerTab` یا `GenericAddonTab` استفاده می‌شود

## نحوه ساخت یک افزونه جدید

### مرحله ۱: ایجاد پروژه سرویس ویندوز

```csharp
// در پروژه افزونه خود، از کلاس PluginClient استفاده کنید
public class MyPluginService
{
    private PluginClient _client;
    
    public void Start()
    {
        _client = new PluginClient("my-plugin-id", "My Plugin Name");
        
        // ثبت نام در TrafficWatch
        _ = _client.RegisterAsync(apiPort: 8080);
        
        // ارسال heartbeat هر 30 ثانیه
        Task.Run(async () =>
        {
            while (true)
            {
                await _client.SendHeartbeatAsync();
                await Task.Delay(30000);
            }
        });
    }
}
```

### مرحله ۲: نصب به عنوان سرویس ویندوز

```powershell
# با استفاده از sc.exe
sc create MyPluginService binPath= "C:\Path\To\MyPlugin.exe" start= auto

# یا با استفاده از InstallUtil
InstallUtil.exe C:\Path\To\MyPlugin.exe
```

### مرحله ۳: افزودن UI برای داشبورد (اختیاری)

اگر می‌خواهید افزونه شما تب خاصی در داشبورد داشته باشد:

1. ایجاد فایل XAML:
```xml
<!-- View/Dashboard/MyPluginTab.xaml -->
<UserControl x:Class="TrafficWatch.View.Dashboard.MyPluginTab">
    <Grid>
        <TextBlock Text="My Plugin Content" />
    </Grid>
</UserControl>
```

2. افزودن به `DashboardView.xaml.cs`:
```csharp
case "my-plugin-id":
    view = new MyPluginTab(addon);
    break;
```

## پروتکل ارتباطی

### درخواست ثبت نام (Register)

```json
{
  "method": "register",
  "parameters": {
    "id": "unique-addon-id",
    "name": "Addon Name",
    "version": "1.0.0",
    "author": "Developer Name",
    "description": "Addon description",
    "isEnabled": true,
    "isInstalled": true,
    "displayOrder": 100,
    "iconPath": "",
    "apiPort": 8080,
    "apiEndpoint": "http://localhost:8080/api",
    "settings": {}
  },
  "id": "request-uuid"
}
```

### پاسخ موفقیت‌آمیز

```json
{
  "success": true,
  "data": {
    "Message": "Addon registered successfully",
    "AddonId": "unique-addon-id"
  },
  "id": "request-uuid"
}
```

### درخواست Heartbeat

```json
{
  "method": "heartbeat",
  "parameters": {
    "timestamp": "2024-01-01T12:00:00"
  },
  "id": "request-uuid"
}
```

## فایل‌های کلیدی

| فایل | توضیح |
|------|-------|
| `NamedPipePluginServer.cs` | سرور Named Pipe در TrafficWatch |
| `PluginClientExample.cs` | کلاینت نمونه برای افزونه‌ها |
| `DashboardAddonService.cs` | مدیریت افزونه‌های داشبورد |
| `DashboardView.xaml.cs` | نمایش تب‌های افزونه در داشبورد |
| `App.xaml.cs` | مقداردهی اولیه سیستم افزونه‌ها |

## نکات امنیتی

1. **احراز هویت**: در حال حاضر هر کلاینتی می‌تواند به Named Pipe متصل شود. برای محیط‌های تولیدی، باید احراز هویت اضافه شود.

2. **مجوزها**: افزونه‌ها فقط باید به داده‌هایی دسترسی داشته باشند که کاربر مجاز کرده است.

3. **ساندباکس**: افزونه‌ها در فرآیندهای جداگانه اجرا می‌شوند تا از کرش کردن برنامه اصلی جلوگیری شود.

## عیب‌یابی

### افزونه در داشبورد نمایش داده نمی‌شود

1. بررسی کنید که Named Pipe Server در حال اجرا است
2. لاگ‌های Debug را بررسی کنید
3. مطمئن شوید که افزونه درخواست register را ارسال کرده است

### خطای اتصال Named Pipe

```csharp
// اطمینان حاصل کنید که نام Pipe صحیح است
const string PipeName = "TrafficWatchPluginPipe";

// بررسی کنید که سرور در حال اجرا است
// در ویندوز، از PipeList برای دیدن Pipeهای فعال استفاده کنید
```

## مثال کامل: ساخت افزونه دانلود منیجر

```csharp
using System;
using System.ServiceProcess;
using TrafficWatch.Services.PluginSystem;

namespace DownloadManagerPlugin
{
    public class DownloadManagerService : ServiceBase
    {
        private PluginClient _client;
        
        protected override void OnStart(string[] args)
        {
            // شروع سرویس دانلود منیجر
            // ...
            
            // ثبت نام به عنوان افزونه
            _client = new PluginClient("download-manager", "Download Manager");
            _ = _client.RegisterAsync(apiPort: 9090);
        }
        
        protected override void OnStop()
        {
            // توقف سرویس
        }
    }
}
```

## نتیجه‌گیری

این سیستم افزونه‌ها امکان گسترش آسان TrafficWatch را بدون تغییر در کد منبع اصلی فراهم می‌کند. هر برنامه‌ای می‌تواند با پیروی از پروتکل Named Pipe و ارسال درخواست‌های JSON مناسب، به عنوان یک افزونه عمل کند و در داشبورد نمایش داده شود.

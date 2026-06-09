# راهنمای استریم داده از افزونه‌ها به TrafficWatch

## معرفی
این سند نحوه استریم داده‌های زنده (مانند اطلاعات موسیقی در حال پخش) از برنامه‌های شخص ثالث به داشبورد TrafficWatch را توضیح می‌دهد.

## معماری ارتباطی

```
┌─────────────────┐     Named Pipe      ┌──────────────────┐
│  برنامه افزونه  │ ◄─────────────────► │   TrafficWatch   │
│  (Music Player) │    TrafficWatch     │   (Server)       │
│                 │    PluginPipe       │                  │
└─────────────────┘                     └──────────────────┘
       │                                        │
       │ 1. اتصال و ثبت نام                    │ 1. گوش دادن به پایپ
       │ 2. ارسال استریم داده                  │ 2. پردازش داده‌ها
       │ 3. Heartbeat                           │ 3. بروزرسانی داشبورد
```

## سناریوی اجرا

### مرحله 1: اجرای سرویس اصلی
```bash
# نصب و شروع سرویس TrafficWatch
sc create TrafficWatch binPath="C:\Path\To\ServerTrafice.exe"
sc start TrafficWatch
```

### مرحله 2: فعال‌سازی سرور Named Pipe
به محض اجرای برنامه، `NamedPipePluginServer` به صورت خودکار در `App.xaml.cs` فعال می‌شود و روی پایپ `TrafficWatchPluginPipe` گوش می‌دهد.

### مرحله 3: اجرای برنامه افزونه (مثلاً پخش کننده موسیقی)
برنامه افزونه با استفاده از `PluginClientExample` به سرور متصل می‌شود:

```csharp
// در برنامه پخش کننده موسیقی
var musicPlugin = new MusicStreamerPlugin();
await musicPlugin.StartAsync();
```

### مرحله 4: ثبت نام خودکار
افزونه به صورت خودکار ثبت شده و در داشبورد TrafficWatch نمایش داده می‌شود.

### مرحله 5: استریم داده‌های موسیقی
اطلاعات آهنگ در حال پخش به صورت بلادرنگ به داشبورد ارسال می‌شود.

## پروتکل ارتباطی

### فرمت پیام‌ها

#### 1. درخواست ثبت نام (Register)
```json
{
  "action": "register",
  "name": "Music Player Plugin",
  "version": "2.0.0",
  "icon": "🎵"
}
```

**پاسخ:**
```json
{
  "action": "registered",
  "id": "a1b2c3d4",
  "status": "success"
}
```

#### 2. ارسال داده استریم (Stream Data)
```json
{
  "action": "stream_data",
  "timestamp": "2024-01-15T10:30:00.000Z",
  "payload": {
    "type": "now_playing",
    "title": "Bohemian Rhapsody",
    "artist": "Queen",
    "album": "A Night at the Opera",
    "duration": "5:55",
    "progress": 45,
    "isPlaying": true
  }
}
```

#### 3. Heartbeat
```json
{
  "action": "heartbeat"
}
```

## نمونه کد کامل افزونه موسیقی

```csharp
using System;
using System.Threading.Tasks;
using TrafficWatch.Services.PluginSystem;

namespace MusicPlayerAddon
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("🎵 Music Player Addon Starting...");
            
            var plugin = new MusicStreamerPlugin();
            await plugin.StartAsync();
            
            Console.WriteLine("Press any key to stop...");
            Console.ReadKey();
            
            plugin.Stop();
        }
    }
}
```

## رویدادهای DashboardAddonService

برای دریافت داده‌های استریم در داشبورد، از رویداد `OnDataReceived` استفاده کنید:

```csharp
// در ViewModel داشبورد
DashboardAddonService.Instance.OnDataReceived += (sender, e) =>
{
    // e.AddonId: شناسه افزونه
    // e.Data: داده‌های JSON استریم شده
    
    var data = JsonDocument.Parse(e.Data);
    
    if (data.RootElement.TryGetProperty("payload", out var payload))
    {
        // بروزرسانی UI با اطلاعات موسیقی
        string title = payload.GetProperty("title").GetString();
        string artist = payload.GetProperty("artist").GetString();
        
        // آپدیت تب موسیقی در داشبورد
        UpdateMusicTab(title, artist);
    }
};
```

## فایل‌های مرتبط

| فایل | توضیحات |
|------|---------|
| `NamedPipePluginServer.cs` | سرور Named Pipe در TrafficWatch |
| `PluginClientExample.cs` | کلاینت نمونه برای افزونه‌ها |
| `DashboardAddonService.cs` | سرویس مدیریت افزونه‌ها و رویدادها |
| `MusicStreamerPlugin` | نمونه کامل پخش کننده موسیقی |

## نکات مهم

1. **امنیت**: Named Pipe فقط به کاربران محلی اجازه اتصال می‌دهد
2. **Performance**: داده‌های استریم باید سبک باشند (JSON کوچک)
3. **Heartbeat**: هر 30 ثانیه برای حفظ اتصال ارسال شود
4. **قطع اتصال**: در صورت قطع اتصال، افزونه از لیست حذف می‌شود

## عیب‌یابی

| مشکل | راه حل |
|------|--------|
| اتصال برقرار نمی‌شود | مطمئن شوید سرویس TrafficWatch در حال اجرا است |
| داده‌ها نمایش داده نمی‌شوند | رویداد `OnDataReceived` را در Dashboard subscribe کنید |
| افزونه ثبت نمی‌شود | فرمت JSON درخواست register را بررسی کنید |

## گسترش آینده

- پشتیبانی از چندین Named Pipe برای افزونه‌های مختلف
- افزودن احراز هویت برای افزونه‌ها
- امکان ارسال فرمان از سرور به افزونه (Play, Pause, Stop)

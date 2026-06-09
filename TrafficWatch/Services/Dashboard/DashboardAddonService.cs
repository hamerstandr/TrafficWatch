using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TrafficWatch.Models.Dashboard;

namespace TrafficWatch.Services.Dashboard
{
    /// <summary>
    /// سرویس مدیریت افزونه‌های داشبورد
    /// این سرویس امکان نصب، حذف، فعال/غیرفعال کردن و تنظیمات افزونه‌ها را فراهم می‌کند
    /// همچنین افزونه‌ها را از طریق سرویس‌های ویندوز شناسایی می‌کند
    /// </summary>
    public class DashboardAddonService
    {
        private static DashboardAddonService _instance;
        private readonly List<AddonInfo> _addons;
        private readonly string _addonsConfigPath;
        private bool _isInitialized;

        private DashboardAddonService()
        {
            _addons = new List<AddonInfo>();
            _addonsConfigPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TrafficWatch",
                "DashboardAddons.json"
            );
        }

        public static DashboardAddonService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DashboardAddonService();
                }
                return _instance;
            }
        }

        /// <summary>
        /// راه‌اندازی اولیه سرویس
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;

            LoadAddons();
            RegisterDefaultAddons();
            ScanWindowsServicesForAddons();
            SaveAddons();
            
            _isInitialized = true;
        }

        /// <summary>
        /// ثبت افزونه‌های پیش‌فرض
        /// </summary>
        private void RegisterDefaultAddons()
        {
            // دانلود منیجر
            if (!_addons.Any(a => a.Id == "download-manager"))
            {
                var dmAddon = new DownloadManagerAddonInfo();
                dmAddon.IsInstalled = DownloadManagerService.IsDownloadMengerInstalled();
                dmAddon.IsEnabled = dmAddon.IsInstalled;
                _addons.Add(dmAddon);
            }

            // پخش کننده موسیقی (پیش‌فرض غیرفعال)
            if (!_addons.Any(a => a.Id == "music-player"))
            {
                _addons.Add(new MusicPlayerAddonInfo());
            }

            // مانیتور سیستم (پیش‌فرض فعال)
            if (!_addons.Any(a => a.Id == "system-monitor"))
            {
                _addons.Add(new SystemMonitorAddonInfo());
            }
        }

        /// <summary>
        /// اسکن سرویس‌های ویندوز برای شناسایی افزونه‌ها
        /// برنامه‌های اجرایی که به عنوان سرویس ویندوز ثبت شده‌اند به عنوان افزونه اضافه می‌شوند
        /// </summary>
        private void ScanWindowsServicesForAddons()
        {
            try
            {
                var services = ServiceController.GetServices();
                
                foreach (var service in services)
                {
                    // بررسی سرویس‌هایی که ممکن است افزونه باشند
                    // مثلاً سرویس دانلود منیجر یا پخش کننده موسیقی
                    string serviceName = service.ServiceName.ToLower();
                    string displayName = service.DisplayName.ToLower();
                    
                    // بررسی برای دانلود منیجر
                    if ((serviceName.Contains("download") || displayName.Contains("download")) && 
                        !_addons.Any(a => a.Id == "download-manager"))
                    {
                        var dmAddon = _addons.FirstOrDefault(a => a.Id == "download-manager");
                        if (dmAddon != null)
                        {
                            dmAddon.IsInstalled = true;
                            dmAddon.Settings["ServiceName"] = service.ServiceName;
                        }
                    }
                    
                    // بررسی برای پخش کننده موسیقی
                    if ((serviceName.Contains("music") || serviceName.Contains("media") || 
                         displayName.Contains("music") || displayName.Contains("media")) && 
                        !_addons.Any(a => a.Id == "music-player-installed"))
                    {
                        if (!_addons.Any(a => a.Id == "music-player-installed"))
                        {
                            var musicAddon = new AddonInfo
                            {
                                Id = "music-player-installed",
                                Name = service.DisplayName,
                                Description = $"Music player service: {service.ServiceName}",
                                Version = "1.0.0",
                                Author = "Windows Service",
                                IsInstalled = true,
                                IsEnabled = false,
                                DisplayOrder = 10,
                                ApiPort = 9091,
                                Settings = new Dictionary<string, object>
                                {
                                    { "ServiceName", service.ServiceName },
                                    { "ShowNowPlaying", true },
                                    { "ShowAlbumArt", true }
                                }
                            };
                            _addons.Add(musicAddon);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error scanning Windows services: {ex.Message}");
            }
        }

        /// <summary>
        /// بارگذاری افزونه‌ها از فایل تنظیمات
        /// </summary>
        private void LoadAddons()
        {
            try
            {
                if (File.Exists(_addonsConfigPath))
                {
                    var json = File.ReadAllText(_addonsConfigPath);
                    var loadedAddons = JsonConvert.DeserializeObject<List<AddonInfo>>(json);
                    
                    if (loadedAddons != null)
                    {
                        _addons.Clear();
                        _addons.AddRange(loadedAddons);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading addons: {ex.Message}");
            }
        }

        /// <summary>
        /// ذخیره افزونه‌ها در فایل تنظیمات
        /// </summary>
        public void SaveAddons()
        {
            try
            {
                var directory = Path.GetDirectoryName(_addonsConfigPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonConvert.SerializeObject(_addons, Formatting.Indented);
                File.WriteAllText(_addonsConfigPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving addons: {ex.Message}");
            }
        }

        /// <summary>
        /// دریافت تمام افزونه‌ها
        /// </summary>
        public List<AddonInfo> GetAllAddons()
        {
            return _addons.OrderBy(a => a.DisplayOrder).ToList();
        }

        /// <summary>
        /// دریافت افزونه‌های فعال
        /// </summary>
        public List<AddonInfo> GetEnabledAddons()
        {
            return _addons.Where(a => a.IsEnabled && a.IsInstalled).OrderBy(a => a.DisplayOrder).ToList();
        }

        /// <summary>
        /// دریافت یک افزونه بر اساس ID
        /// </summary>
        public AddonInfo GetAddonById(string id)
        {
            return _addons.FirstOrDefault(a => a.Id == id);
        }

        /// <summary>
        /// فعال/غیرفعال کردن افزونه
        /// </summary>
        public void SetAddonEnabled(string id, bool enabled)
        {
            var addon = GetAddonById(id);
            if (addon != null)
            {
                addon.IsEnabled = enabled;
                SaveAddons();
                
                // رویداد تغییر وضعیت افزونه
                OnAddonStateChanged?.Invoke(this, new AddonStateChangedEventArgs(addon));
            }
        }

        /// <summary>
        /// تنظیم ترتیب نمایش افزونه
        /// </summary>
        public void SetAddonDisplayOrder(string id, int order)
        {
            var addon = GetAddonById(id);
            if (addon != null)
            {
                addon.DisplayOrder = order;
                SaveAddons();
            }
        }

        /// <summary>
        /// بروزرسانی تنظیمات یک افزونه
        /// </summary>
        public void UpdateAddonSettings(string id, Dictionary<string, object> settings)
        {
            var addon = GetAddonById(id);
            if (addon != null)
            {
                addon.Settings = settings;
                SaveAddons();
            }
        }

        /// <summary>
        /// بررسی نصب بودن یک برنامه خارجی
        /// </summary>
        public void CheckInstallationStatus(string addonId)
        {
            var addon = GetAddonById(addonId);
            if (addon != null)
            {
                bool wasInstalled = addon.IsInstalled;
                
                switch (addonId.ToLower())
                {
                    case "download-manager":
                        addon.IsInstalled = DownloadManagerService.IsDownloadMengerInstalled();
                        break;
                    case "music-player":
                        // TODO: بررسی نصب بودن پخش کننده موسیقی
                        addon.IsInstalled = false;
                        break;
                    case "system-monitor":
                        // همیشه نصب است چون داخلی است
                        addon.IsInstalled = true;
                        break;
                }

                if (wasInstalled != addon.IsInstalled)
                {
                    OnAddonStateChanged?.Invoke(this, new AddonStateChangedEventArgs(addon));
                    SaveAddons();
                }
            }
        }

        /// <summary>
        /// اسکن تمام افزونه‌ها برای بررسی وضعیت نصب
        /// </summary>
        public async Task ScanAllAddonsAsync()
        {
            await Task.Run(() =>
            {
                foreach (var addon in _addons)
                {
                    CheckInstallationStatus(addon.Id);
                }
            });
        }

        /// <summary>
        /// رویداد تغییر وضعیت افزونه
        /// </summary>
        public event EventHandler<AddonStateChangedEventArgs> OnAddonStateChanged;

        /// <summary>
        /// دریافت آدرس API یک افزونه
        /// </summary>
        public string GetAddonApiEndpoint(string addonId)
        {
            var addon = GetAddonById(addonId);
            if (addon != null && !string.IsNullOrEmpty(addon.ApiEndpoint))
            {
                return addon.ApiEndpoint;
            }
            
            // آدرس پیش‌فرض بر اساس پورت
            if (addon?.ApiPort > 0)
            {
                return $"http://127.0.0.1:{addon.ApiPort}";
            }
            
            return "";
        }
    }

    /// <summary>
    /// آرگومان‌های رویداد تغییر وضعیت افزونه
    /// </summary>
    public class AddonStateChangedEventArgs : EventArgs
    {
        public AddonInfo Addon { get; }

        public AddonStateChangedEventArgs(AddonInfo addon)
        {
            Addon = addon;
        }
    }
}

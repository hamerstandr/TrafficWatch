using System;
using System.Collections.Generic;
using System.Windows.Controls;
using TrafficWatch.Models.Dashboard;
using TrafficWatch.Services.Dashboard;

namespace TrafficWatch.View.Dashboard
{
    /// <summary>
    /// Interaction logic for DashboardView.xaml
    /// نمای اصلی داشبورد که تب‌های افزونه‌ها را مدیریت می‌کند
    /// </summary>
    public partial class DashboardView : UserControl
    {
        private readonly DashboardAddonService _addonService;
        private readonly Dictionary<string, UserControl> _addonViews;

        public DashboardView()
        {
            InitializeComponent();
            _addonService = DashboardAddonService.Instance;
            _addonViews = new Dictionary<string, UserControl>();
            
            // ثبت رویداد تغییر وضعیت افزونه‌ها
            _addonService.OnAddonStateChanged += AddonService_OnAddonStateChanged;
            
            // بارگذاری اولیه تب‌ها
            LoadAddonTabs();
        }

        /// <summary>
        /// بارگذاری تب‌های افزونه‌ها
        /// </summary>
        private void LoadAddonTabs()
        {
            // پاک کردن تب‌های قبلی (به جز تب اول)
            while (AddonTabControl.Items.Count > 1)
            {
                AddonTabControl.Items.RemoveAt(AddonTabControl.Items.Count - 1);
            }

            var enabledAddons = _addonService.GetEnabledAddons();

            if (enabledAddons.Count == 0)
            {
                NoAddonsGrid.Visibility = System.Windows.Visibility.Visible;
                AddonTabControl.Visibility = System.Windows.Visibility.Collapsed;
                return;
            }

            NoAddonsGrid.Visibility = System.Windows.Visibility.Collapsed;
            AddonTabControl.Visibility = System.Windows.Visibility.Visible;

            foreach (var addon in enabledAddons)
            {
                CreateAddonTab(addon);
            }
        }

        /// <summary>
        /// ایجاد تب برای یک افزونه
        /// </summary>
        private void CreateAddonTab(AddonInfo addon)
        {
            var tabItem = new TabItem
            {
                Header = addon.Name,
                Tag = addon.Id
            };

            // دریافت یا ایجاد view برای افزونه
            var addonView = GetOrCreateAddonView(addon);
            tabItem.Content = addonView;

            AddonTabControl.Items.Add(tabItem);
        }

        /// <summary>
        /// دریافت یا ایجاد view برای افزونه
        /// </summary>
        private UserControl GetOrCreateAddonView(AddonInfo addon)
        {
            if (_addonViews.ContainsKey(addon.Id))
            {
                return _addonViews[addon.Id];
            }

            UserControl view;

            switch (addon.Id.ToLower())
            {
                case "download-manager":
                    view = new DownloadManagerTab(addon);
                    break;
                case "music-player":
                    view = new MusicPlayerTab(addon);
                    break;
                case "system-monitor":
                    view = new SystemMonitorTab(addon);
                    break;
                default:
                    // View پیش‌فرض برای افزونه‌های ناشناس
                    view = new GenericAddonTab(addon);
                    break;
            }

            _addonViews[addon.Id] = view;
            return view;
        }

        /// <summary>
        /// مدیریت رویداد تغییر وضعیت افزونه
        /// </summary>
        private void AddonService_OnAddonStateChanged(object sender, AddonStateChangedEventArgs e)
        {
            // بروزرسانی UI در ترد اصلی
            Dispatcher.InvokeAsync(() =>
            {
                LoadAddonTabs();
            });
        }

        /// <summary>
        /// بازسازی تب‌ها (مثلاً بعد از تغییر تنظیمات)
        /// </summary>
        public void RefreshTabs()
        {
            LoadAddonTabs();
        }
    }
}

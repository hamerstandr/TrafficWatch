using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TrafficWatch.Models.Config;
using TrafficWatch.Services.Network;

namespace TrafficWatch.View.Settings
{
    /// <summary>
    /// کنترل تنظیمات شبکه برای ارتباط LAN
    /// </summary>
    public partial class NetworkSettingsControl : UserControl
    {
        private NetworkSettings _currentSettings;
        private bool _isTokenVisible = false;

        public NetworkSettingsControl()
        {
            InitializeComponent();
            LoadSettings();
        }

        /// <summary>
        /// بارگذاری تنظیمات فعلی در فرم
        /// </summary>
        private void LoadSettings()
        {
            _currentSettings = NetworkSettings.Load();

            txtServerIp.Text = _currentSettings.ServerIpAddress;
            txtServerPort.Text = _currentSettings.ServerPort.ToString();
            txtApiToken.Text = _currentSettings.ApiAuthToken;
            txtAllowedIpRanges.Text = string.Join("\n", _currentSettings.AllowedIpRanges);
            chkUseSecureConnection.IsChecked = _currentSettings.UseSecureConnection;
            txtMaxRequestsPerMinute.Text = _currentSettings.MaxRequestsPerMinute.ToString();
            chkEnableSecurityLogging.IsChecked = _currentSettings.EnableSecurityLogging;
            txtConnectionTimeout.Text = _currentSettings.ConnectionTimeoutMs.ToString();
        }

        /// <summary>
        /// تولید توکن امنیتی جدید
        /// </summary>
        private void BtnGenerateToken_Click(object sender, RoutedEventArgs e)
        {
            var newToken = NetworkSecurityService.GenerateSecureToken();
            txtApiToken.Text = newToken;
            MessageBox.Show("توکن امنیتی جدید تولید شد.\n\nلطفاً این توکن را در مکانی امن ذخیره کنید.", 
                          "تولید توکن", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// نمایش/مخفی کردن توکن
        /// </summary>
        private void BtnShowHideToken_Click(object sender, RoutedEventArgs e)
        {
            _isTokenVisible = !_isTokenVisible;
            txtApiToken.PasswordChar = _isTokenVisible ? '\u0000' : '●';
            btnShowHideToken.Content = _isTokenVisible ? "🙈" : "👁️";
        }

        /// <summary>
        /// اعتبارسنجی ورودی عددی
        /// </summary>
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        /// <summary>
        /// نمایش چک‌لیست امنیتی
        /// </summary>
        private void BtnSecurityChecklist_Click(object sender, RoutedEventArgs e)
        {
            var checklistWindow = new SecurityChecklistWindow();
            checklistWindow.ShowDialog();
        }

        /// <summary>
        /// ذخیره تنظیمات
        /// </summary>
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // اعتبارسنجی ورودی‌ها
                if (!ValidateInputs(out string errorMessage))
                {
                    MessageBox.Show(errorMessage, "خطا در اعتبارسنجی", 
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // به‌روزرسانی تنظیمات
                _currentSettings.ServerIpAddress = txtServerIp.Text.Trim();
                _currentSettings.ServerPort = int.Parse(txtServerPort.Text);
                _currentSettings.ApiAuthToken = txtApiToken.Text.Trim();
                _currentSettings.AllowedIpRanges = txtAllowedIpRanges.Text
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();
                _currentSettings.UseSecureConnection = chkUseSecureConnection.IsChecked ?? false;
                _currentSettings.MaxRequestsPerMinute = int.Parse(txtMaxRequestsPerMinute.Text);
                _currentSettings.EnableSecurityLogging = chkEnableSecurityLogging.IsChecked ?? true;
                _currentSettings.ConnectionTimeoutMs = int.Parse(txtConnectionTimeout.Text);

                // ذخیره تنظیمات
                _currentSettings.Save();

                // به‌روزرسانی سرویس امنیتی
                NetworkSecurityService.Instance.UpdateSettings(_currentSettings);

                MessageBox.Show("تنظیمات شبکه با موفقیت ذخیره شد.\n\nتغییرات پس از راه‌اندازی مجدد اعمال خواهند شد.",
                              "ذخیره موفق", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در ذخیره تنظیمات:\n{ex.Message}", 
                              "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// اعتبارسنجی ورودی‌های فرم
        /// </summary>
        private bool ValidateInputs(out string errorMessage)
        {
            errorMessage = string.Empty;

            // بررسی پورت
            if (!int.TryParse(txtServerPort.Text, out int port) || port < 1 || port > 65535)
            {
                errorMessage = "پورت باید عددی بین 1 تا 65535 باشد.";
                return false;
            }

            // بررسی IP (در صورت وارد شدن)
            if (!string.IsNullOrWhiteSpace(txtServerIp.Text))
            {
                if (!System.Net.IPAddress.TryParse(txtServerIp.Text, out _))
                {
                    errorMessage = "آدرس IP وارد شده معتبر نیست.";
                    return false;
                }
            }

            // بررسی محدوده‌های IP
            var ipRanges = txtAllowedIpRanges.Text
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            if (ipRanges.Count == 0)
            {
                errorMessage = "حداقل یک محدوده IP باید وارد شود.";
                return false;
            }

            foreach (var range in ipRanges)
            {
                // بررسی ساده فرمت CIDR یا IP تکی
                if (!range.Contains('/') && !System.Net.IPAddress.TryParse(range, out _))
                {
                    errorMessage = $"محدوده IP نامعتبر: {range}\nفرمت صحیح: 192.168.1.0/24 یا 192.168.1.100";
                    return false;
                }
            }

            // بررسی Rate Limiting
            if (!int.TryParse(txtMaxRequestsPerMinute.Text, out int rateLimit) || rateLimit < 1)
            {
                errorMessage = "حداکثر درخواست در دقیقه باید عددی مثبت باشد.";
                return false;
            }

            // بررسی تایم‌اوت
            if (!int.TryParse(txtConnectionTimeout.Text, out int timeout) || timeout < 1000)
            {
                errorMessage = "تایم‌اوت اتصال باید حداقل 1000 میلی‌ثانیه باشد.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// انصراف و بستن
        /// </summary>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            // بازگردانی تنظیمات به حالت اولیه
            LoadSettings();
            
            // اگر در پنجره والد است، آن را ببند
            var window = Window.GetWindow(this);
            window?.Close();
        }
    }
}

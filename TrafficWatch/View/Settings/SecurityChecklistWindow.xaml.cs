using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace TrafficWatch.View.Settings
{
    /// <summary>
    /// پنجره نمایش چک‌لیست کامل امنیتی برای ارتباطات شبکه
    /// </summary>
    public partial class SecurityChecklistWindow : Window
    {
        public SecurityChecklistWindow()
        {
            InitializeComponent();
            LoadChecklistContent();
        }

        private void LoadChecklistContent()
        {
            var checklist = GetSecurityChecklist();
            
            // ایجاد محتوای RichText
            var richTextBox = new RichTextBox
            {
                IsReadOnly = true,
                Margin = new Thickness(10),
                VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto
            };

            var document = new FlowDocument();
            
            foreach (var section in checklist)
            {
                // عنوان بخش
                var sectionPara = new Paragraph(new Run(section.Title))
                {
                    FontWeight = FontWeights.Bold,
                    FontSize = 16,
                    Margin = new Thickness(0, 15, 0, 10)
                };
                document.Blocks.Add(sectionPara);

                // آیتم‌های چک‌لیست
                foreach (var item in section.Items)
                {
                    var stackPanel = new StackPanel();
                    stackPanel.Orientation = Orientation.Horizontal;
                    stackPanel.Margin = new Thickness(5, 3, 0, 3);

                    // چک‌باکس
                    var checkBox = new CheckBox
                    {
                        Content = "",
                        Margin = new Thickness(0, 0, 10, 0),
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    // متن آیتم
                    var textBlock = new TextBlock
                    {
                        Text = item,
                        TextWrapping = TextWrapping.Wrap,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    stackPanel.Children.Add(checkBox);
                    stackPanel.Children.Add(textBlock);

                    // تبدیل به Block برای اضافه کردن به مستند
                    var container = new BlockUIContainer(stackPanel);
                    document.Blocks.Add(container);
                }
            }

            // دستورالعمل‌ها
            var instructionPara = new Paragraph(new Run("\n\nدستورالعمل:\nهر مورد را بررسی کرده و در صورت رعایت شدن تیک بزنید.\nهدف: کسب حداقل 90% امتیاز برای استقرار امن."))
            {
                FontStyle = FontStyles.Italic,
                Foreground = System.Windows.Media.Brushes.Gray,
                Margin = new Thickness(0, 20, 0, 10)
            };
            document.Blocks.Add(instructionPara);

            richTextBox.Document = document;
            
            // اضافه کردن به گرید اصلی
            MainGrid.Children.Clear();
            MainGrid.RowDefinitions.Clear();
            MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
            MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            
            Grid.SetRow(richTextBox, 0);
            MainGrid.Children.Add(richTextBox);

            // دکمه بستن
            var closeButton = new Button
            {
                Content = "بستن",
                Padding = new Thickness(20, 8),
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 0)
            };
            closeButton.Click += (s, e) => Close();
            
            Grid.SetRow(closeButton, 1);
            MainGrid.Children.Add(closeButton);
        }

        private List<ChecklistSection> GetSecurityChecklist()
        {
            return new List<ChecklistSection>
            {
                new ChecklistSection
                {
                    Title = "🔐 1. احراز هویت و کنترل دسترسی",
                    Items = new List<string>
                    {
                        "□ استفاده از توکن‌های API قوی (حداقل 32 بایت)",
                        "□ ذخیره‌سازی امن توکن‌ها (هش شده با SHA-256)",
                        "□ پیاده‌سازی لیست سفید IP (IP Whitelist)",
                        "□ پشتیبانی از CIDR برای محدوده‌های IP",
                        "□ غیرفعال کردن احراز هویت برای محیط‌های تست",
                        "□ چرخش دوره‌ای توکن‌ها (Token Rotation)"
                    }
                },
                new ChecklistSection
                {
                    Title = "🔒 2. رمزنگاری و حفاظت از داده‌ها",
                    Items = new List<string>
                    {
                        "□ فعال‌سازی HTTPS/TLS برای تمام ارتباطات",
                        "□ استفاده از TLS 1.2 یا بالاتر",
                        "□ رمزنگاری داده‌های حساس در حال انتقال",
                        "□ عدم ارسال اطلاعات حساس در URL",
                        "□ استفاده از هدرهای امنیتی مناسب"
                    }
                },
                new ChecklistSection
                {
                    Title = "🌐 3. کنترل دسترسی شبکه",
                    Items = new List<string>
                    {
                        "□ محدود کردن دسترسی به subnetهای مشخص",
                        "□ پیکربندی فایروال ویندوز برای پورت‌ها",
                        "□ غیرفعال کردن پورت‌های غیرضروری",
                        "□ استفاده از VLAN برای جداسازی ترافیک",
                        "□ مانیتورینگ ترافیک ورودی/خروجی"
                    }
                },
                new ChecklistSection
                {
                    Title = "⏱️ 4. محدودیت نرخ و محافظت در برابر حملات",
                    Items = new List<string>
                    {
                        "□ پیاده‌سازی Rate Limiting (حداکثر درخواست در دقیقه)",
                        "□ محافظت در برابر حملات DDoS",
                        "□ جلوگیری از Brute Force Attacks",
                        "□ استفاده از Circuit Breaker Pattern",
                        "□ لاگ‌گیری و هشدار برای ترافیک غیرعادی"
                    }
                },
                new ChecklistSection
                {
                    Title = "✅ 5. اعتبارسنجی ورودی‌ها",
                    Items = new List<string>
                    {
                        "□ اعتبارسنجی تمام ورودی‌های کاربر",
                        "□ جلوگیری از SQL Injection",
                        "□ جلوگیری از XSS (Cross-Site Scripting)",
                        "□ جلوگیری از Path Traversal",
                        "□_sanitization داده‌های ورودی"
                    }
                },
                new ChecklistSection
                {
                    Title = "📝 6. لاگ‌گیری و مانیتورینگ",
                    Items = new List<string>
                    {
                        "□ ثبت تمام تلاش‌های ناموفق احراز هویت",
                        "□ لاگ‌گیری نقض‌های امنیتی",
                        "□ مانیتورینگ بلادرنگ ترافیک شبکه",
                        "□ هشدار خودکار برای فعالیت‌های مشکوک",
                        "□ نگهداری لاگ‌ها برای حداقل 90 روز"
                    }
                },
                new ChecklistSection
                {
                    Title = "⚠️ 7. مدیریت خطا",
                    Items = new List<string>
                    {
                        "□ عدم نمایش جزئیات خطا به کاربر نهایی",
                        "□ ثبت خطاها در لاگ‌های داخلی",
                        "□ استفاده از پیام‌های خطای عمومی",
                        "□ جلوگیری از نشت اطلاعات در خطاها"
                    }
                },
                new ChecklistSection
                {
                    Title = "🔄 8. به‌روزرسانی و نگهداری",
                    Items = new List<string>
                    {
                        "□ به‌روزرسانی منظم کتابخانه‌های شخص ثالث",
                        "□ اعمال پچ‌های امنیتی .NET",
                        "□ بازبینی دوره‌ای کد از نظر امنیتی",
                        "□ تست نفوذپذیری دوره‌ای"
                    }
                },
                new ChecklistSection
                {
                    Title = "🧪 9. تست امنیتی",
                    Items = new List<string>
                    {
                        "□ انجام تست‌های امنیتی خودکار (SAST/DAST)",
                        "□ استفاده از ابزارهای تحلیل کد (SonarQube, CodeQL)",
                        "□ تست دستی با OWASP ZAP",
                        "□ بازبینی کد توسط همکاران"
                    }
                },
                new ChecklistSection
                {
                    Title = "📚 10. مستندات و آموزش",
                    Items = new List<string>
                    {
                        "□ مستندسازی تنظیمات امنیتی",
                        "□ آموزش تیم توسعه در مورد بهترین روش‌ها",
                        "□ ایجاد راهنمای پاسخ به حوادث امنیتی",
                        "□ به‌روزرسانی مستندات پس از هر تغییر"
                    }
                }
            };
        }
    }

    public class ChecklistSection
    {
        public string Title { get; set; } = string.Empty;
        public List<string> Items { get; set; } = new();
    }
}

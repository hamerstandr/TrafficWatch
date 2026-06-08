using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace TrafficWatch.Resources.Languages
{
    public static class LanguageService
    {
        private static readonly string LanguagesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Languages");
        private static ResourceDictionary _currentLanguage;
        private static string _currentLanguageCode = "fa";

        public static string CurrentLanguageCode
        {
            get => _currentLanguageCode;
            set
            {
                if (_currentLanguageCode != value)
                {
                    _currentLanguageCode = value;
                    SetLanguage(value);
                }
            }
        }

        public static List<string> AvailableLanguages => new List<string> { "fa", "en", "ar" };

        public static void Initialize()
        {
            var savedLanguage = TrafficWatch.Properties.Settings.Default.Language;
            if (!string.IsNullOrEmpty(savedLanguage) && AvailableLanguages.Contains(savedLanguage))
            {
                _currentLanguageCode = savedLanguage;
            }
            SetLanguage(_currentLanguageCode);
        }

        public static void SetLanguage(string languageCode)
        {
            try
            {
                var app = Application.Current;
                if (app == null) return;

                // Remove existing language dictionary
                var existingDict = app.Resources.MergedDictionaries.FirstOrDefault(d => 
                    d.Source?.OriginalString.Contains("Languages/") == true);
                
                if (existingDict != null)
                {
                    app.Resources.MergedDictionaries.Remove(existingDict);
                }

                // Load new language
                var langPath = Path.Combine(LanguagesPath, $"{languageCode}.xaml");
                if (File.Exists(langPath))
                {
                    var uri = new Uri(langPath, UriKind.Absolute);
                    var dict = new ResourceDictionary { Source = uri };
                    app.Resources.MergedDictionaries.Add(dict);
                    
                    // Set flow direction for RTL languages
                    if (languageCode == "fa" || languageCode == "ar")
                    {
                        app.FlowDirection = FlowDirection.RightToLeft;
                    }
                    else
                    {
                        app.FlowDirection = FlowDirection.LeftToRight;
                    }

                    _currentLanguage = dict;
                    _currentLanguageCode = languageCode;
                    
                    // Save preference
                    TrafficWatch.Properties.Settings.Default.Language = languageCode;
                    TrafficWatch.Properties.Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading language {languageCode}: {ex.Message}");
            }
        }

        public static string GetString(string key)
        {
            if (_currentLanguage != null && _currentLanguage.Contains(key))
            {
                return _currentLanguage[key]?.ToString() ?? key;
            }
            return key;
        }
    }
}

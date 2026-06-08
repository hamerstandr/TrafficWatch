using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace TrafficWatch.Models.Dashboard
{
    /// <summary>
    /// مدل پایه برای تمام افزونه‌های داشبورد
    /// </summary>
    public interface IAddonInfo
    {
        string Id { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        string Version { get; set; }
        string Author { get; set; }
        bool IsEnabled { get; set; }
        bool IsInstalled { get; }
        int DisplayOrder { get; set; }
        string IconPath { get; set; }
        Dictionary<string, object> Settings { get; set; }
    }

    /// <summary>
    /// اطلاعات پایه یک افزونه
    /// </summary>
    public class AddonInfo : IAddonInfo, INotifyPropertyChanged
    {
        private bool _isEnabled;
        private int _displayOrder;

        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("name")]
        public string Name { get; set; } = "Unknown Addon";

        [JsonProperty("description")]
        public string Description { get; set; } = "";

        [JsonProperty("version")]
        public string Version { get; set; } = "1.0.0";

        [JsonProperty("author")]
        public string Author { get; set; } = "Unknown";

        [JsonProperty("isEnabled")]
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        [JsonIgnore]
        public bool IsInstalled { get; set; } = false;

        [JsonProperty("displayOrder")]
        public int DisplayOrder
        {
            get => _displayOrder;
            set
            {
                _displayOrder = value;
                OnPropertyChanged(nameof(DisplayOrder));
            }
        }

        [JsonProperty("iconPath")]
        public string IconPath { get; set; } = "";

        [JsonProperty("settings")]
        public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();

        [JsonProperty("apiEndpoint")]
        public string ApiEndpoint { get; set; } = "";

        [JsonProperty("apiPort")]
        public int ApiPort { get; set; } = 0;

        [JsonProperty("lastUpdateCheck")]
        public DateTime? LastUpdateCheck { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// اطلاعات مخصوص دانلود منیجر
    /// </summary>
    public class DownloadManagerAddonInfo : AddonInfo
    {
        public DownloadManagerAddonInfo()
        {
            Id = "download-manager";
            Name = "Download Manager";
            Description = "Integration with DownloadMenger2 for download management";
            Version = "1.0.0";
            Author = "Hamid Reza Estandi";
            ApiPort = 9090;
            DisplayOrder = 1;
            
            Settings = new Dictionary<string, object>
            {
                { "ShowActiveDownloads", true },
                { "ShowSpeed", true },
                { "ShowProgress", true },
                { "RefreshInterval", 5 },
                { "MaxItemsDisplay", 10 }
            };
        }
    }

    /// <summary>
    /// اطلاعات مخصوص پخش کننده موسیقی
    /// </summary>
    public class MusicPlayerAddonInfo : AddonInfo
    {
        public MusicPlayerAddonInfo()
        {
            Id = "music-player";
            Name = "Music Player";
            Description = "Integration with music player applications";
            Version = "1.0.0";
            Author = "TrafficWatch Team";
            ApiPort = 9091;
            DisplayOrder = 2;
            
            Settings = new Dictionary<string, object>
            {
                { "ShowNowPlaying", true },
                { "ShowAlbumArt", true },
                { "ShowPlaybackControls", false },
                { "RefreshInterval", 3 }
            };
        }
    }

    /// <summary>
    /// اطلاعات مخصوص مانیتورینگ سیستم
    /// </summary>
    public class SystemMonitorAddonInfo : AddonInfo
    {
        public SystemMonitorAddonInfo()
        {
            Id = "system-monitor";
            Name = "System Monitor";
            Description = "CPU, RAM, and Disk usage monitoring";
            Version = "1.0.0";
            Author = "TrafficWatch Team";
            ApiPort = 0; // Local monitoring, no API needed
            DisplayOrder = 3;
            
            Settings = new Dictionary<string, object>
            {
                { "ShowCPU", true },
                { "ShowRAM", true },
                { "ShowDisk", true },
                { "ShowNetwork", true },
                { "RefreshInterval", 2 }
            };
        }
    }
}

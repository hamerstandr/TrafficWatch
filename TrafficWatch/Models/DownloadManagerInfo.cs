using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TrafficWatch.Models
{
    /// <summary>
    /// مدل اطلاعات دانلود منیجر برای ارتباط با DownloadMenger2
    /// </summary>
    public class DownloadManagerInfo
    {
        [JsonProperty("isRunning")]
        public bool IsRunning { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; } = "Unknown";

        [JsonProperty("activeDownloads")]
        public int ActiveDownloads { get; set; }

        [JsonProperty("queuedDownloads")]
        public int QueuedDownloads { get; set; }

        [JsonProperty("completedDownloads")]
        public int CompletedDownloads { get; set; }

        [JsonProperty("totalDownloadSpeed")]
        public double TotalDownloadSpeed { get; set; } // Bytes per second

        [JsonProperty("totalUploadedSpeed")]
        public double TotalUploadedSpeed { get; set; } // Bytes per second

        [JsonProperty("downloadLimit")]
        public double DownloadLimit { get; set; } // Bytes per second, 0 = unlimited

        [JsonProperty("uploadLimit")]
        public double UploadLimit { get; set; } // Bytes per second, 0 = unlimited

        [JsonProperty("schedulerEnabled")]
        public bool SchedulerEnabled { get; set; }

        [JsonProperty("clipboardMonitorEnabled")]
        public bool ClipboardMonitorEnabled { get; set; }

        [JsonProperty("browserIntegrationEnabled")]
        public bool BrowserIntegrationEnabled { get; set; }

        [JsonProperty("lastError")]
        public string LastError { get; set; }

        [JsonProperty("apiEndpoint")]
        public string ApiEndpoint { get; set; } = "http://127.0.0.1:9090";
    }

    /// <summary>
    /// مدل یک دانلود تکی
    /// </summary>
    public class DownloadItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("fileName")]
        public string FileName { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; } // Downloading, Queued, Paused, Completed, Error

        [JsonProperty("progress")]
        public double Progress { get; set; } // 0-100

        [JsonProperty("downloadedSize")]
        public long DownloadedSize { get; set; }

        [JsonProperty("totalSize")]
        public long TotalSize { get; set; }

        [JsonProperty("speed")]
        public double Speed { get; set; }

        [JsonProperty("eta")]
        public TimeSpan Eta { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }
    }
}

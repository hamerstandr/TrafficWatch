using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TrafficWatch.Services.PluginSystem
{
    /// <summary>
    /// نمونه کلاینت برای استفاده در برنامه‌های دیگر (افزونه‌ها)
    /// این کلاس باید در پروژه افزونه کپی شود
    /// </summary>
    public class PluginClient
    {
        private const string PipeName = "TrafficWatchPluginPipe";
        private readonly string _addonId;
        private readonly string _addonName;

        public PluginClient(string addonId, string addonName)
        {
            _addonId = addonId;
            _addonName = addonName;
        }

        /// <summary>
        /// ثبت نام افزونه در TrafficWatch
        /// </summary>
        public async Task<bool> RegisterAsync(int apiPort = 0, string apiEndpoint = "")
        {
            try
            {
                var request = new
                {
                    method = "register",
                    parameters = new
                    {
                        id = _addonId,
                        name = _addonName,
                        version = "1.0.0",
                        author = "Addon Developer",
                        description = $"Plugin: {_addonName}",
                        isEnabled = true,
                        isInstalled = true,
                        displayOrder = 100,
                        iconPath = "",
                        apiPort = apiPort,
                        apiEndpoint = apiEndpoint,
                        settings = new { }
                    },
                    id = Guid.NewGuid().ToString()
                };

                return await SendRequestAsync(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registering plugin: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// ارسال پیام Heartbeat برای نشان دادن فعال بودن
        /// </summary>
        public async Task<bool> SendHeartbeatAsync()
        {
            try
            {
                var request = new
                {
                    method = "heartbeat",
                    parameters = new { timestamp = DateTime.Now },
                    id = Guid.NewGuid().ToString()
                };

                return await SendRequestAsync(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending heartbeat: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// ارسال درخواست عمومی به سرور
        /// </summary>
        private async Task<bool> SendRequestAsync(object requestData)
        {
            try
            {
                using (var client = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut))
                {
                    // اتصال با تایم‌اوت 5 ثانیه
                    await client.ConnectAsync(5000);

                    var writer = new StreamWriter(client, Encoding.UTF8) { AutoFlush = true };
                    var reader = new StreamReader(client, Encoding.UTF8);

                    // ارسال درخواست
                    string json = JsonConvert.SerializeObject(requestData);
                    await writer.WriteLineAsync(json);

                    // دریافت پاسخ
                    string responseJson = await reader.ReadLineAsync();
                    if (!string.IsNullOrEmpty(responseJson))
                    {
                        var response = JsonConvert.DeserializeObject<dynamic>(responseJson);
                        if (response != null && response.success == true)
                        {
                            Console.WriteLine($"Plugin '{_addonName}' operation successful.");
                            return true;
                        }
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error communicating with host: {ex.Message}");
                return false;
            }
        }
    }
}

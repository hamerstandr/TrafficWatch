using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TrafficWatch.Services.PluginSystem
{
    /// <summary>
    /// مدیریت ارتباط Named Pipe برای ارتباط با افزونه‌های سرویس ویندوز
    /// </summary>
    public class NamedPipePluginServer
    {
        private const string PipeName = "TrafficWatchPluginPipe";
        private CancellationTokenSource _cancellationTokenSource;
        private Task _serverTask;
        private readonly Dashboard.DashboardAddonService _addonService;

        public NamedPipePluginServer()
        {
            _addonService = Dashboard.DashboardAddonService.Instance;
        }

        /// <summary>
        /// شروع سرور Named Pipe برای دریافت ارتباط از افزونه‌ها
        /// </summary>
        public void Start()
        {
            if (_serverTask != null && !_serverTask.IsCompleted)
            {
                System.Diagnostics.Debug.WriteLine("Named Pipe server is already running.");
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            _serverTask = Task.Run(() => RunServerAsync(_cancellationTokenSource.Token));
            
            System.Diagnostics.Debug.WriteLine($"Named Pipe server started. Pipe Name: {PipeName}");
        }

        /// <summary>
        /// توقف سرور Named Pipe
        /// </summary>
        public void Stop()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _serverTask?.Wait(TimeSpan.FromSeconds(5));
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }

            System.Diagnostics.Debug.WriteLine("Named Pipe server stopped.");
        }

        /// <summary>
        /// حلقه اصلی سرور Named Pipe
        /// </summary>
        private async Task RunServerAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var pipeServer = new NamedPipeServerStream(
                        PipeName,
                        PipeDirection.InOut,
                        NamedPipeServerStream.MaxAllowedServerInstances,
                        PipeTransmissionMode.Message,
                        PipeOptions.Asynchronous
                    );

                    // انتظار برای اتصال کلاینت
                    await pipeServer.WaitForConnectionAsync(cancellationToken);

                    // پردازش اتصال در یک تسک جداگانه
                    _ = Task.Run(() => HandleClientAsync(pipeServer, cancellationToken));
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in Named Pipe server: {ex.Message}");
                    await Task.Delay(1000, cancellationToken);
                }
            }
        }

        /// <summary>
        /// پردازش پیام‌های دریافتی از یک کلاینت (افزونه)
        /// </summary>
        private async Task HandleClientAsync(NamedPipeServerStream pipeServer, CancellationToken cancellationToken)
        {
            try
            {
                using (pipeServer)
                {
                    var reader = new StreamReader(pipeServer, Encoding.UTF8);
                    var writer = new StreamWriter(pipeServer, Encoding.UTF8) { AutoFlush = true };

                    while (!cancellationToken.IsCancellationRequested && pipeServer.IsConnected)
                    {
                        string line = await reader.ReadLineAsync();
                        if (string.IsNullOrEmpty(line))
                            break;

                        // پردازش درخواست JSON
                        var response = await ProcessRequestAsync(line);

                        // ارسال پاسخ
                        await writer.WriteLineAsync(JsonConvert.SerializeObject(response));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error handling client: {ex.Message}");
            }
        }

        /// <summary>
        /// پردازش درخواست دریافتی از افزونه
        /// </summary>
        private async Task<PluginResponse> ProcessRequestAsync(string requestJson)
        {
            try
            {
                var request = JsonConvert.DeserializeObject<PluginRequest>(requestJson);
                
                switch (request.Method?.ToLower())
                {
                    case "register":
                        return await HandleRegisterAsync(request);
                    
                    case "heartbeat":
                        return await HandleHeartbeatAsync(request);
                    
                    case "get_data":
                        return await HandleGetDataAsync(request);
                    
                    default:
                        return new PluginResponse
                        {
                            Success = false,
                            Error = $"Unknown method: {request.Method}"
                        };
                }
            }
            catch (Exception ex)
            {
                return new PluginResponse
                {
                    Success = false,
                    Error = $"Error processing request: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// ثبت نام افزونه جدید
        /// </summary>
        private async Task<PluginResponse> HandleRegisterAsync(PluginRequest request)
        {
            var addonInfo = request.Parameters?.ToObject<Models.Dashboard.AddonInfo>();
            
            if (addonInfo == null || string.IsNullOrEmpty(addonInfo.Id))
            {
                return new PluginResponse
                {
                    Success = false,
                    Error = "Invalid addon information"
                };
            }

            // بررسی اینکه آیا افزونه قبلاً ثبت شده است
            var existingAddon = _addonService.GetAddonById(addonInfo.Id);
            
            if (existingAddon == null)
            {
                // افزودن افزونه جدید
                addonInfo.IsInstalled = true;
                addonInfo.IsEnabled = true;
                
                // استفاده از Reflection برای افزودن به لیست داخلی
                var addonsField = typeof(Dashboard.DashboardAddonService).GetField("_addons", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (addonsField != null)
                {
                    var addons = addonsField.GetValue(_addonService) as System.Collections.Generic.List<Models.Dashboard.AddonInfo>;
                    if (addons != null)
                    {
                        addons.Add(addonInfo);
                        
                        // ذخیره تنظیمات
                        _addonService.SaveAddons();
                        
                        // اطلاع‌رسانی تغییر وضعیت
                        _addonService.OnAddonStateChanged?.Invoke(
                            _addonService, 
                            new Dashboard.AddonStateChangedEventArgs(addonInfo)
                        );

                        System.Diagnostics.Debug.WriteLine($"Addon registered: {addonInfo.Name} ({addonInfo.Id})");
                    }
                }
            }
            else
            {
                // بروزرسانی اطلاعات افزونه موجود
                existingAddon.IsInstalled = true;
                existingAddon.IsEnabled = true;
                existingAddon.ApiPort = addonInfo.ApiPort;
                existingAddon.ApiEndpoint = addonInfo.ApiEndpoint;
                
                _addonService.SaveAddons();
                _addonService.OnAddonStateChanged?.Invoke(
                    _addonService, 
                    new Dashboard.AddonStateChangedEventArgs(existingAddon)
                );

                System.Diagnostics.Debug.WriteLine($"Addon updated: {existingAddon.Name} ({existingAddon.Id})");
            }

            return new PluginResponse
            {
                Success = true,
                Data = new { Message = "Addon registered successfully", AddonId = addonInfo.Id }
            };
        }

        /// <summary>
        /// پردازش پیام Heartbeat از افزونه
        /// </summary>
        private async Task<PluginResponse> HandleHeartbeatAsync(PluginRequest request)
        {
            return new PluginResponse
            {
                Success = true,
                Data = new { Status = "OK", Timestamp = DateTime.Now }
            };
        }

        /// <summary>
        /// دریافت داده از افزونه
        /// </summary>
        private async Task<PluginResponse> HandleGetDataAsync(PluginRequest request)
        {
            // اینجا می‌توان داده‌های خاصی از افزونه دریافت کرد
            return new PluginResponse
            {
                Success = true,
                Data = new { Message = "Data received" }
            };
        }
    }

    /// <summary>
    /// مدل درخواست از افزونه
    /// </summary>
    public class PluginRequest
    {
        [JsonProperty("method")]
        public string Method { get; set; }
        
        [JsonProperty("parameters")]
        public dynamic Parameters { get; set; }
        
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// مدل پاسخ به افزونه
    /// </summary>
    public class PluginResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
        
        [JsonProperty("data")]
        public dynamic Data { get; set; }
        
        [JsonProperty("error")]
        public string Error { get; set; }
        
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}

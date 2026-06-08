# TrafficWatch - Network Traffic Monitor

## Overview
TrafficWatch is a Windows-based network traffic monitoring application that provides real-time monitoring of network usage, process-level traffic analysis, and historical data tracking.

## Features
- **Real-time Monitoring**: Live display of download and upload speeds
- **Process Tracking**: Monitor network traffic by individual processes
- **Historical Data**: Track daily, monthly, and total traffic statistics
- **Web Dashboard**: Access traffic statistics via built-in web server at `http://127.0.0.1:8080/state/index.html`
- **System Tray Integration**: Runs quietly in the system tray
- **Multi-desktop Support**: Works across virtual desktops (Windows 10+)
- **Theme Support**: Follows Windows system theme colors

## Requirements
- **Operating System**: Windows 7, 8, 10, or 11
- **.NET Framework**: 4.7 or higher ([Download](https://dotnet.microsoft.com/download/thank-you/net47))
- **WinPcap**: Required for packet capture ([Download](https://www.winpcap.org/))
- **Network Adapter**: Active network interface for monitoring

## Installation

### Prerequisites
1. Install .NET Framework 4.7 or higher
2. Install WinPcap from [winpcap.org](https://www.winpcap.org/)

### Build from Source
```bash
# Open TrafficWatch.sln in Visual Studio 2019 or later
# Build the solution in Release mode
# The executable will be in TrafficWatch/bin/Release/
```

## Usage

### Running the Application
1. Launch `TrafficWatch.exe`
2. The application will start monitoring your network traffic
3. View the main window for real-time statistics
4. Access the web dashboard at: `http://127.0.0.1:8080/state/index.html`

### Web Dashboard
The built-in web server provides:
- Real-time download/upload speeds
- Historical statistics (daily, monthly, total)
- Responsive design for mobile and desktop
- Auto-refresh every 2 seconds

### System Tray Menu
- **Show/Hide**: Toggle the main window visibility
- **Edge Hide**: Auto-hide window at screen edge
- **Settings**: Configure application preferences
- **Exit**: Close the application

## Project Structure

```
TrafficWatch/
├── TrafficWatch/          # Main WPF Application
│   ├── Control/           # Custom UI controls
│   ├── Services/          # Core services (capture, themes, etc.)
│   ├── View/              # XAML views and windows
│   ├── Resources/Pages/   # Web dashboard files
│   └── Server.cs          # HTTP server implementation
├── HttpServer/            # Lightweight HTTP server library
├── VirtualDesktop/        # Virtual desktop management
└── ServerTrafice/         # Windows Service version
```

## Dependencies

### NuGet Packages
- **LiveCharts** (0.9.7): Chart visualization
- **log4net** (2.0.8): Logging framework
- **SharpPcap** (4.5.0): Packet capture library
- **PacketDotNet** (0.16.0): Network packet parsing

### External Libraries
- **Vue.js** (2.6.10): Web dashboard frontend
- **Axios** (0.16.2): HTTP client for web dashboard

## Configuration

Application settings are stored in user settings and can be accessed through the Settings window:
- Startup on boot
- Transparency effects
- Update notifications
- Window position

## API Endpoints

The built-in HTTP server exposes the following endpoints:

| Endpoint | Description |
|----------|-------------|
| `/state/index.html` | Web dashboard |
| `/Download` | Current download speed |
| `/Upload` | Current upload speed |
| `/MaxSpeed` | Maximum recorded speed |
| `/Daydownload` | Today's download total |
| `/Dayupload` | Today's upload total |
| `/Monthdownload` | Monthly download total |
| `/Monthupload` | Monthly upload total |
| `/Totaldownload` | All-time download total |
| `/Totalupload` | All-time upload total |

## Troubleshooting

### WinPcap Not Found
If you receive a WinPcap error:
1. Download WinPcap from [winpcap.org](https://www.winpcap.org/)
2. Install with administrator privileges
3. Restart the application

### No Traffic Displayed
- Ensure you have an active network connection
- Run the application as administrator
- Check if the correct network adapter is selected

### Web Dashboard Not Loading
- Verify the application is running
- Check if port 8080 is available
- Try accessing `http://localhost:8080/state/index.html`

## Development

### Building
```bash
# Using Visual Studio
Open TrafficWatch.sln
Build > Build Solution

# Using MSBuild
msbuild TrafficWatch.sln /p:Configuration=Release
```

### Code Structure
- **App.xaml.cs**: Application startup and initialization
- **MainWindow.xaml**: Main application window
- **PopWindow.xaml**: Edge-hiding popup window
- **Server.cs**: HTTP server for web dashboard
- **Services/Detail/CaptureManager.cs**: Network packet capture

## License
This project uses several open-source libraries. Please refer to their respective licenses.

## Contributing
Contributions are welcome! Please feel free to submit pull requests or report issues.

## Support
For issues and questions, please visit the GitHub repository.

---

**Note**: This application requires administrative privileges to capture network traffic.

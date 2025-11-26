# PixTouch

A modern, touch-friendly control interface for Pixera media server (by AVStumpfl) with support for Loupedeck hardware controllers.

## Overview

PixTouch provides professional media server control similar to MA Lighting or Chamsys lighting consoles. It features:

- **Cross-platform Avalonia UI** - Modern, touch-friendly interface
- **TCP JSON-RPC API Integration** - Robust connection to Pixera media server
- **Virtual Encoders & Faders** - 6 rotary encoders and 8 faders for parameter control
- **Loupedeck Support** - Hardware control surface integration via Logitech Actions SDK
- **Dark Theme** - Professional control room friendly interface
- **Configuration Management** - Persistent settings and control mappings

## Project Structure

```
pixtouch/
├── src/
│   ├── PixTouch/                    # Main Avalonia application
│   │   ├── ViewModels/              # MVVM ViewModels with ReactiveUI
│   │   ├── Views/                   # Avalonia XAML views
│   │   └── Services/                # Application services
│   ├── PixTouch.Core/               # Shared library
│   │   ├── Api/                     # Pixera TCP JSON-RPC client
│   │   ├── Models/                  # Data models
│   │   ├── Services/                # Core services
│   │   ├── Protocol/                # Communication protocols
│   │   └── Configuration/           # Settings management
│   └── PixTouch.LoupedeckPlugin/    # Loupedeck Actions SDK plugin
│       ├── PixTouchPlugin.cs        # Main plugin implementation
│       └── manifest.json            # Plugin manifest
├── docs/                            # Documentation
└── README.md
```

## Technology Stack

- **.NET 8** - Cross-platform framework
- **Avalonia UI** - Modern XAML-based UI framework
- **ReactiveUI** - Reactive MVVM extensions
- **Newtonsoft.Json** - JSON serialization
- **System.Reactive** - Reactive Extensions

## Features

### Pixera API Integration

- TCP JSON-RPC connection to Pixera (default port 1400)
- Auto-reconnect and error recovery
- Connection status monitoring
- Support for Pixera API namespaces:
  - `Pixera.Timelines` - Timeline, Layer, Clip, Cue control
  - `Pixera.Resources` - Resource management
  - `Pixera.Screens` - Screen and camera control
  - `Pixera.Projectors` - Projector management
  - `Pixera.Direct` - Low-latency parameter control

### Control Surface

**6 Virtual Rotary Encoders:**
- Mappable to any Pixera parameter
- Display current value, label, and range
- Support for coarse and fine adjustments
- Visual feedback with formatted values

**8 Virtual Faders:**
- Timeline/layer opacity control
- Volume control
- Custom parameter mapping
- Real-time value display

### Loupedeck Integration

The Loupedeck plugin acts as a "dumb terminal" with all logic controlled by PixTouch:
- Bidirectional communication via TCP (port 19790)
- 6 hardware encoders mapped to virtual encoders
- LCD buttons for mode switching
- Visual feedback synchronized with PixTouch

## Building

### Prerequisites

- .NET SDK 8.0 or later
- Visual Studio 2022, VS Code, or Rider (optional)

### Build Instructions

```bash
# Clone the repository
git clone https://github.com/albertorighetto/pixtouch.git
cd pixtouch

# Build the solution
dotnet build

# Run the application
dotnet run --project src/PixTouch/PixTouch.csproj
```

## Configuration

Configuration is stored in `%APPDATA%/PixTouch/config.json` (Windows) or `~/.config/PixTouch/config.json` (Linux/macOS).

### Example Configuration

```json
{
  "Connection": {
    "Host": "localhost",
    "Port": 1400,
    "AutoConnect": true
  },
  "Loupedeck": {
    "Enabled": true,
    "Port": 19790
  },
  "EncoderMappings": [
    {
      "Id": "encoder-1",
      "Label": "Pos X",
      "ParameterPath": "Timeline 1.Layer 1.Position.x",
      "MinValue": -1000.0,
      "MaxValue": 1000.0,
      "DefaultValue": 0.0,
      "CoarseStep": 10.0,
      "FineStep": 1.0,
      "DisplayFormat": "{0:F1}"
    }
  ],
  "FaderMappings": [],
  "UiPreferences": {
    "DarkTheme": true,
    "FullScreenMode": false,
    "WindowWidth": 1280,
    "WindowHeight": 720
  }
}
```

## Usage

### Connecting to Pixera

1. Launch PixTouch
2. Enter your Pixera server hostname/IP and port (default: localhost:1400)
3. Connection status will be displayed in the top bar

### Using Encoders

- Each encoder displays its label, current value, and range
- Values are synchronized with Pixera in real-time
- Configure encoder mappings in the configuration file

### Using Faders

- Faders provide linear control over parameters
- Useful for opacity, volume, and other 0-100% parameters
- Real-time visual feedback

### Loupedeck Integration

1. Ensure PixTouch is running
2. Launch the Loupedeck plugin
3. The plugin will automatically connect to PixTouch (port 19790)
4. Hardware encoders and buttons will be synchronized

## Development

### Core Architecture

**PixeraApiClient**: Handles TCP JSON-RPC communication with Pixera
```csharp
var client = new PixeraApiClient();
await client.ConnectAsync("localhost", 1400);
var result = await client.InvokeAsync<object>("Pixera.Direct.SetParameter", new {
    path = "Timeline 1.Opacity",
    value = 75.0
});
```

**ControlSurfaceService**: Manages virtual encoders and faders
```csharp
var controlSurface = new ControlSurfaceService();
controlSurface.SetEncoderMapping(0, new ControlMapping {
    Label = "Opacity",
    ParameterPath = "Timeline 1.Opacity",
    MinValue = 0,
    MaxValue = 100
});
```

**LoupedeckBridgeService**: Communication with Loupedeck plugin
```csharp
var bridge = new LoupedeckBridgeService();
await bridge.StartServerAsync(19790);
await bridge.UpdateEncoderDisplayAsync(0, "Opacity", "75.0", "#00FF00");
```

## Roadmap

- [ ] Enhanced UI with element selection panel
- [ ] Property editor for timeline/layer/effect management
- [ ] Preset system for saving parameter configurations
- [ ] Group operations for batch control
- [ ] Multi-touch gesture support
- [ ] Full-screen mode optimization
- [ ] Comprehensive Pixera API namespace implementations

## Contributing

Contributions are welcome! Please feel free to submit issues, fork the repository, and create pull requests.

## License

This project is licensed under the MIT License.

## Acknowledgments

- Pixera by AVStumpfl - Professional media server
- Avalonia UI - Cross-platform UI framework
- Logitech Actions SDK - Hardware integration
- ReactiveUI - Reactive MVVM framework

## Support

For issues, questions, or contributions, please visit the [GitHub repository](https://github.com/albertorighetto/pixtouch).


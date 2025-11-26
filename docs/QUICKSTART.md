# PixTouch - Quick Start Guide

## What is PixTouch?

PixTouch is a modern, touch-friendly control interface for Pixera media server. It provides professional-grade control similar to MA Lighting or Chamsys consoles, with support for hardware control surfaces like Loupedeck.

## Quick Start (5 minutes)

### 1. Installation

```bash
# Clone the repository
git clone https://github.com/albertorighetto/pixtouch.git
cd pixtouch

# Build the application
dotnet build

# Run PixTouch
dotnet run --project src/PixTouch/PixTouch.csproj
```

### 2. First Launch

When you launch PixTouch for the first time:

1. **Connection Setup**
   - Enter your Pixera server hostname (default: localhost)
   - Enter port (default: 1400)
   - The app will attempt to connect automatically

2. **Interface Overview**
   - **Top Bar**: Connection status and settings
   - **Left Panel**: 6 rotary encoders and 8 faders
   - **Right Panel**: Property editor (future)

### 3. Configure Encoders

Edit the configuration file at:
- Windows: `%APPDATA%/PixTouch/config.json`
- Linux/macOS: `~/.config/PixTouch/config.json`

Example encoder configuration:
```json
{
  "EncoderMappings": [
    {
      "Label": "Pos X",
      "ParameterPath": "Timeline 1.Layer 1.Position.x",
      "MinValue": -1000.0,
      "MaxValue": 1000.0,
      "DisplayFormat": "{0:F1}"
    }
  ]
}
```

### 4. Use Encoders

- Each encoder displays its current value in real-time
- Values are synchronized with Pixera
- Supports both coarse and fine adjustments

### 5. (Optional) Set Up Loupedeck

If you have a Loupedeck device:

1. Ensure PixTouch is running
2. Enable Loupedeck in config:
   ```json
   {
     "Loupedeck": {
       "Enabled": true,
       "Port": 19790
     }
   }
   ```
3. Install the Loupedeck plugin
4. Hardware encoders will sync with PixTouch

## Common Use Cases

### Control Timeline Opacity

```json
{
  "Id": "fader-1",
  "Label": "Timeline 1 Opacity",
  "ParameterPath": "Timeline 1.Opacity",
  "MinValue": 0.0,
  "MaxValue": 100.0,
  "DisplayFormat": "{0:F0}%"
}
```

### Control Layer Position

```json
{
  "Id": "encoder-1",
  "Label": "Pos X",
  "ParameterPath": "Timeline 1.Layer 1.Position.x",
  "MinValue": -1000.0,
  "MaxValue": 1000.0,
  "CoarseStep": 10.0,
  "FineStep": 1.0,
  "DisplayFormat": "{0:F1}"
}
```

### Control Color

```json
{
  "Id": "fader-6",
  "Label": "Red",
  "ParameterPath": "Timeline 1.Layer 1.Color.r",
  "MinValue": 0.0,
  "MaxValue": 255.0,
  "DisplayFormat": "{0:F0}"
}
```

## Keyboard Shortcuts (Future)

- `Ctrl+F` - Toggle full-screen mode
- `Ctrl+,` - Open settings
- `Space` - Play/Pause
- `Ctrl+S` - Save current state

## Troubleshooting

### Cannot Connect to Pixera

1. Verify Pixera is running
2. Check hostname and port in config
3. Verify firewall settings
4. Check network connectivity

### Encoders Not Updating

1. Verify parameter path is correct
2. Check PixTouch is connected to Pixera
3. Verify encoder mapping in config
4. Check Pixera supports the parameter

### Loupedeck Not Connecting

1. Verify PixTouch is running
2. Check Loupedeck plugin is installed
3. Verify port 19790 is not in use
4. Check firewall settings

## Next Steps

- Read the [full documentation](docs/)
- Learn about [API usage](docs/API_USAGE.md)
- Set up [Loupedeck](docs/LOUPEDECK_SETUP.md)
- Understand the [architecture](docs/ARCHITECTURE.md)
- [Contribute](docs/CONTRIBUTING.md) to the project

## Getting Help

- **Issues**: [GitHub Issues](https://github.com/albertorighetto/pixtouch/issues)
- **Discussions**: [GitHub Discussions](https://github.com/albertorighetto/pixtouch/discussions)
- **Documentation**: [docs/](docs/)

## Features at a Glance

✅ Cross-platform (Windows, Linux, macOS)  
✅ Dark theme for control rooms  
✅ 6 rotary encoders  
✅ 8 faders  
✅ Real-time parameter control  
✅ Auto-reconnect to Pixera  
✅ Loupedeck hardware support  
✅ Configurable mappings  
✅ JSON-RPC API integration  

🔄 Element selection panel (coming soon)  
🔄 Preset management (coming soon)  
🔄 Touch gestures (coming soon)  
🔄 Full-screen mode (coming soon)  

## Example Workflows

### Live Performance Setup

1. Configure Timeline 1-4 opacity on faders 1-4
2. Configure position/scale on encoders 1-4
3. Use Loupedeck for hands-on control
4. Save configuration for future shows

### Content Creation

1. Map layer parameters to encoders
2. Adjust position, scale, rotation in real-time
3. Fine-tune with fine mode
4. See results immediately in Pixera

### System Operation

1. Control master volume with fader
2. Switch between timelines
3. Adjust global parameters
4. Monitor connection status

## System Requirements

- **OS**: Windows 10+, Linux (Ubuntu 20.04+), macOS 11+
- **.NET**: .NET 8.0 Runtime
- **RAM**: 512 MB minimum
- **Display**: 1280x720 minimum
- **Network**: TCP access to Pixera server

## Performance Tips

- Use wired network connection for lowest latency
- Close unnecessary applications
- Use Direct parameter control for real-time operations
- Configure appropriate step sizes for your use case

## License

MIT License - See LICENSE file for details

---

**Ready to start?** Run `dotnet run --project src/PixTouch/PixTouch.csproj` and connect to your Pixera server!

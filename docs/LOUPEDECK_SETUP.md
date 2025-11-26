# Loupedeck Plugin Setup Guide

## Overview

The PixTouch Loupedeck plugin enables hardware control surface integration using the Logitech Actions SDK. The plugin acts as a "dumb terminal" with all logic controlled by the main PixTouch application.

## Architecture

```
┌─────────────────┐         TCP Socket          ┌──────────────────┐
│                 │      (localhost:19790)       │                  │
│  PixTouch App   │◄────────────────────────────►│ Loupedeck Plugin │
│                 │                              │                  │
└─────────────────┘                              └──────────────────┘
        │                                                 │
        │ Pixera API                                     │ Hardware
        ▼                                                 ▼
┌─────────────────┐                              ┌──────────────────┐
│  Pixera Server  │                              │ Loupedeck Device │
└─────────────────┘                              └──────────────────┘
```

## Communication Protocol

The plugin communicates with PixTouch using JSON messages over TCP.

### Message Types

#### From PixTouch to Plugin (Display Updates)

**Encoder Update:**
```json
{
  "type": "encoder_update",
  "encoder_id": 0,
  "label": "Pos X",
  "value": "125.5",
  "color": "#00FF00"
}
```

**Button Update:**
```json
{
  "type": "button_update",
  "button_id": "play",
  "label": "Play",
  "color": "#00FF00"
}
```

#### From Plugin to PixTouch (User Input)

**Encoder Input:**
```json
{
  "type": "encoder_input",
  "encoder_id": 0,
  "delta": 1,
  "fine_mode": false
}
```

**Button Input:**
```json
{
  "type": "button_input",
  "button_id": "play",
  "pressed": true
}
```

## Plugin Installation

### Using Logitech Actions SDK

1. **Install Logitech Actions SDK**
   - Download from: https://logitech.github.io/actions-sdk-docs/
   - Install according to SDK instructions

2. **Build the Plugin**
   ```bash
   cd src/PixTouch.LoupedeckPlugin
   dotnet build -c Release
   ```

3. **Deploy the Plugin**
   - Copy the built plugin to the Logitech Actions plugin directory
   - Follow Logitech Actions SDK deployment guidelines

4. **Configure Plugin Manifest**
   - The `manifest.json` defines available actions
   - Customize for your specific Loupedeck device

### Manual Plugin Development

If developing your own plugin implementation:

```csharp
using PixTouch.LoupedeckPlugin;

var plugin = new PixTouchPlugin();

// Connect to PixTouch
await plugin.ConnectAsync("localhost", 19790);

// Handle messages from PixTouch
plugin.MessageReceived += (sender, message) =>
{
    switch (message.Type)
    {
        case "encoder_update":
            // Update encoder display
            UpdateEncoderDisplay(
                message.EncoderId.Value,
                message.Label,
                message.Value,
                message.Color
            );
            break;
            
        case "button_update":
            // Update button display
            UpdateButtonDisplay(
                message.ButtonId,
                message.Label,
                message.Color
            );
            break;
    }
};

// Send encoder input to PixTouch
async Task OnEncoderRotated(int encoderId, int delta, bool fineMode)
{
    await plugin.SendEncoderInputAsync(encoderId, delta, fineMode);
}

// Send button press to PixTouch
async Task OnButtonPressed(string buttonId)
{
    await plugin.SendButtonInputAsync(buttonId, true);
}

async Task OnButtonReleased(string buttonId)
{
    await plugin.SendButtonInputAsync(buttonId, false);
}
```

## Hardware Mapping

### Loupedeck Live

**Encoders (6):**
- Encoder 1 (Top Left)
- Encoder 2 (Top Center)
- Encoder 3 (Top Right)
- Encoder 4 (Bottom Left)
- Encoder 5 (Bottom Center)
- Encoder 6 (Bottom Right)

**Touch Buttons:**
- Play, Pause, Stop
- Previous Cue, Next Cue
- Mode switches (Position, Rotation, Scale, Color)

**LCD Buttons:**
- Dynamic buttons with customizable labels
- Visual feedback with color coding

## Configuration

The Loupedeck bridge can be configured in the PixTouch config file:

```json
{
  "Loupedeck": {
    "Enabled": true,
    "Port": 19790
  }
}
```

## Troubleshooting

### Plugin Not Connecting

1. Verify PixTouch is running
2. Check that Loupedeck bridge is enabled in config
3. Verify port 19790 is not in use by another application
4. Check firewall settings

### Encoders Not Responding

1. Verify encoder mappings are configured in PixTouch
2. Check that PixTouch is connected to Pixera
3. Verify the parameter paths are correct

### Display Not Updating

1. Check connection status in PixTouch
2. Verify JSON message format is correct
3. Check plugin logs for errors

## Development Tips

1. **Test Without Hardware**: Use the PixTouchPlugin class directly for testing
2. **Message Logging**: Enable logging to debug communication issues
3. **Error Handling**: The plugin automatically reconnects if connection is lost
4. **Thread Safety**: All plugin methods are thread-safe

## Example: Custom Button Action

```csharp
// In your plugin implementation
plugin.Connected += async (sender, e) =>
{
    Console.WriteLine("Connected to PixTouch");
};

plugin.Disconnected += (sender, e) =>
{
    Console.WriteLine("Disconnected from PixTouch");
};

// Handle play button
await plugin.SendButtonInputAsync("play", true);  // Pressed
await Task.Delay(100);
await plugin.SendButtonInputAsync("play", false); // Released
```

## References

- Logitech Actions SDK: https://logitech.github.io/actions-sdk-docs/
- Loupedeck Device SDK: https://developer.loupedeck.com/
- PixTouch Repository: https://github.com/albertorighetto/pixtouch

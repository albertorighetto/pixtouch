# PixTouch Architecture

## System Architecture

```
┌────────────────────────────────────────────────────────────────────┐
│                         PixTouch Application                        │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │                        Avalonia UI Layer                      │  │
│  │  ┌────────────────┐  ┌────────────────┐  ┌────────────────┐  │  │
│  │  │  MainWindow    │  │  Encoder View  │  │  Fader View    │  │  │
│  │  │  ViewModel     │  │                │  │                │  │  │
│  │  └────────────────┘  └────────────────┘  └────────────────┘  │  │
│  └──────────────────────────────────────────────────────────────┘  │
│                                  │                                  │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │                       Service Layer                           │  │
│  │  ┌──────────────────┐  ┌──────────────────┐  ┌────────────┐ │  │
│  │  │ ControlSurface   │  │ LoupedeckBridge  │  │   Config   │ │  │
│  │  │    Service       │  │    Service       │  │  Service   │ │  │
│  │  └──────────────────┘  └──────────────────┘  └────────────┘ │  │
│  └──────────────────────────────────────────────────────────────┘  │
│                       │                    │                        │
└───────────────────────┼────────────────────┼────────────────────────┘
                        │                    │
                        │                    │ TCP Socket
                        │                    │ (localhost:19790)
                        │                    │
                        │                    ▼
                        │         ┌──────────────────────┐
                        │         │  Loupedeck Plugin    │
                        │         │  (Actions SDK)       │
                        │         └──────────────────────┘
                        │                    │
                        │                    ▼
                        │         ┌──────────────────────┐
                        │         │  Loupedeck Hardware  │
                        │         │  (6 Encoders + Btns) │
                        │         └──────────────────────┘
                        │
                        │ TCP JSON-RPC
                        │ (host:1400)
                        │
                        ▼
             ┌─────────────────────┐
             │   Pixera Server     │
             │   (AVStumpfl)       │
             └─────────────────────┘
```

## Component Responsibilities

### PixTouch Application (Avalonia UI)

**MainWindowViewModel**
- Central application state
- Connection management
- Encoder/Fader coordination
- Event aggregation

**Views**
- Dark theme UI
- Touch-friendly controls
- Real-time value display
- Connection status

### PixTouch.Core Library

**PixeraApiClient**
- TCP JSON-RPC communication
- Auto-reconnect logic
- Request/response handling
- Error recovery

**ControlSurfaceService**
- Encoder/Fader management
- Value mapping and transformation
- Input event handling
- Parameter synchronization

**LoupedeckBridgeService**
- TCP server for plugin
- Display update messages
- Input event forwarding
- Connection management

**ConfigurationService**
- JSON configuration storage
- Settings persistence
- Default values
- Path management

### PixTouch.LoupedeckPlugin

**PixTouchPlugin**
- TCP client to PixTouch
- Hardware event capture
- Display rendering
- Button handling

## Data Flow

### Encoder Input Flow

```
Hardware Encoder Rotation
         │
         ▼
  Loupedeck Plugin
   (captures delta)
         │
         │ JSON-RPC Message
         ▼
LoupedeckBridgeService
  (receives input)
         │
         │ Event
         ▼
  MainWindowViewModel
   (processes input)
         │
         │ Update
         ▼
ControlSurfaceService
  (calculates new value)
         │
         ├─────────────────┐
         │                 │
         ▼                 ▼
  PixeraApiClient    UI Update
 (sends to Pixera)   (display)
         │                 │
         ▼                 ▼
  Pixera Server    Loupedeck Display
```

### Display Update Flow

```
ControlSurfaceService
  (value changes)
         │
         ├──────────────┬──────────────┐
         ▼              ▼              ▼
   UI Binding   LoupedeckBridge  PixeraAPI
     Update        Service         Sync
         │              │              │
         ▼              │              ▼
  Avalonia View         │        Pixera Server
     Updates            │
         │              │
         │              │ JSON Message
         │              ▼
         │      Loupedeck Plugin
         │              │
         │              ▼
         │       Hardware Display
         │            Update
         └──────────────┘
```

## Threading Model

### Main UI Thread
- Avalonia UI updates
- User interaction
- ViewModel property changes

### Network Thread Pool
- TCP socket I/O
- JSON-RPC requests
- Async/await operations

### Background Services
- Auto-reconnect timer
- Connection monitoring
- Configuration auto-save

## Configuration Management

### Configuration Hierarchy

```
AppConfiguration
├── ConnectionSettings
│   ├── Host
│   ├── Port
│   └── AutoConnect
├── LoupedeckSettings
│   ├── Enabled
│   └── Port
├── EncoderMappings[]
│   └── ControlMapping
│       ├── Label
│       ├── ParameterPath
│       ├── MinValue
│       ├── MaxValue
│       └── DisplayFormat
├── FaderMappings[]
│   └── ControlMapping
└── UiPreferences
    ├── DarkTheme
    ├── FullScreenMode
    ├── WindowWidth
    └── WindowHeight
```

## Communication Protocols

### Pixera JSON-RPC Protocol

**Request:**
```json
{
  "jsonrpc": "2.0",
  "method": "Pixera.Direct.SetParameter",
  "params": {
    "path": "Timeline 1.Opacity",
    "value": 75.0
  },
  "id": 1
}
```

**Response:**
```json
{
  "jsonrpc": "2.0",
  "result": null,
  "id": 1
}
```

**Error:**
```json
{
  "jsonrpc": "2.0",
  "error": {
    "code": -32601,
    "message": "Method not found"
  },
  "id": 1
}
```

### Loupedeck Bridge Protocol

**Encoder Update (PixTouch → Plugin):**
```json
{
  "type": "encoder_update",
  "encoder_id": 0,
  "label": "Pos X",
  "value": "125.5",
  "color": "#00FF00"
}
```

**Encoder Input (Plugin → PixTouch):**
```json
{
  "type": "encoder_input",
  "encoder_id": 0,
  "delta": 1,
  "fine_mode": false
}
```

## Error Handling Strategy

### Connection Errors
- Auto-reconnect with exponential backoff
- Max 5 retry attempts
- Visual status indication
- Error message display

### API Errors
- Graceful degradation
- Error logging
- User notification
- Fallback to cached values

### Plugin Errors
- Silent reconnection
- Maintain state
- Queue messages during disconnect
- Resume on reconnect

## Performance Considerations

### Real-time Control
- Direct parameter control (<10ms latency)
- Buffered updates
- Throttled UI updates
- Efficient JSON serialization

### Memory Management
- Dispose pattern for connections
- Weak event handlers
- Observable collection cleanup
- Configuration caching

### Network Optimization
- Keep-alive connections
- Message batching
- Compression (future)
- Delta updates (future)

## Security Considerations

### Network Security
- Local network only by default
- No authentication (relies on network security)
- Firewall-friendly ports
- No sensitive data in config

### Input Validation
- Parameter path validation
- Value range checking
- JSON schema validation
- Rate limiting (future)

## Extensibility Points

### Adding New Controls
1. Add model to PixTouch.Core.Models
2. Update ControlSurfaceService
3. Add ViewModel properties
4. Update UI View
5. Add configuration

### Adding API Namespaces
1. Add extension methods to PixeraApiExtensions
2. Add models if needed
3. Update documentation
4. Add tests

### Custom Plugins
1. Implement using PixTouchPlugin
2. Follow bridge protocol
3. Add manifest.json
4. Deploy to Actions SDK

## Testing Strategy

### Unit Tests (Future)
- Core logic isolation
- Mock network I/O
- Configuration handling
- Value transformations

### Integration Tests (Future)
- End-to-end scenarios
- Mock Pixera server
- Plugin simulation
- Configuration loading

### Manual Testing
- Visual UI inspection
- Hardware testing
- Connection scenarios
- Error conditions

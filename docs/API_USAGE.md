# Pixera API Usage Guide

## Overview

This document provides examples of using the PixeraApiClient to interact with the Pixera media server.

## Basic Connection

```csharp
using PixTouch.Core.Api;

var client = new PixeraApiClient();

// Connect to Pixera
await client.ConnectAsync("localhost", 1400);

// Monitor connection state
client.ConnectionStateChanged += (sender, e) =>
{
    Console.WriteLine($"Connection state changed: {e.PreviousState} -> {e.NewState}");
    if (!string.IsNullOrEmpty(e.ErrorMessage))
    {
        Console.WriteLine($"Error: {e.ErrorMessage}");
    }
};

// Disconnect when done
await client.DisconnectAsync();
```

## API Namespaces

### Pixera.Direct - Low-latency Parameter Control

The `Pixera.Direct` namespace provides low-latency parameter control, ideal for real-time encoder/fader control.

```csharp
// Set a parameter value
await client.InvokeAsync<object>("Pixera.Direct.SetParameter", new
{
    path = "Timeline 1.Opacity",
    value = 75.0
});

// Get a parameter value
var value = await client.InvokeAsync<double>("Pixera.Direct.GetParameter", new
{
    path = "Timeline 1.Opacity"
});
```

### Pixera.Timelines - Timeline Control

```csharp
// Start a timeline
await client.InvokeAsync<object>("Pixera.Timelines.StartTimeline", new
{
    timelineId = "Timeline 1"
});

// Stop a timeline
await client.InvokeAsync<object>("Pixera.Timelines.StopTimeline", new
{
    timelineId = "Timeline 1"
});

// Pause a timeline
await client.InvokeAsync<object>("Pixera.Timelines.PauseTimeline", new
{
    timelineId = "Timeline 1"
});

// Jump to cue
await client.InvokeAsync<object>("Pixera.Timelines.JumpToCue", new
{
    timelineId = "Timeline 1",
    cueId = "Cue 1"
});
```

### Pixera.Layers - Layer Control

```csharp
// Set layer opacity
await client.InvokeAsync<object>("Pixera.Layers.SetOpacity", new
{
    timelineId = "Timeline 1",
    layerId = "Layer 1",
    opacity = 50.0
});

// Set layer position
await client.InvokeAsync<object>("Pixera.Layers.SetPosition", new
{
    timelineId = "Timeline 1",
    layerId = "Layer 1",
    x = 100.0,
    y = 200.0,
    z = 0.0
});

// Set layer scale
await client.InvokeAsync<object>("Pixera.Layers.SetScale", new
{
    timelineId = "Timeline 1",
    layerId = "Layer 1",
    x = 150.0,
    y = 150.0
});

// Set layer rotation
await client.InvokeAsync<object>("Pixera.Layers.SetRotation", new
{
    timelineId = "Timeline 1",
    layerId = "Layer 1",
    x = 0.0,
    y = 0.0,
    z = 45.0
});
```

### Pixera.Resources - Resource Management

```csharp
// List resources
var resources = await client.InvokeAsync<object[]>("Pixera.Resources.GetResourceList", null);

// Assign resource to layer
await client.InvokeAsync<object>("Pixera.Resources.AssignResourceToLayer", new
{
    resourceId = "Resource 1",
    timelineId = "Timeline 1",
    layerId = "Layer 1"
});
```

### Pixera.Screens - Screen Management

```csharp
// List screens
var screens = await client.InvokeAsync<object[]>("Pixera.Screens.GetScreenList", null);

// Set screen configuration
await client.InvokeAsync<object>("Pixera.Screens.SetScreenConfig", new
{
    screenId = "Screen 1",
    width = 1920,
    height = 1080
});
```

### Pixera.Session - Project Management

```csharp
// Save project
await client.InvokeAsync<object>("Pixera.Session.SaveProject", new
{
    path = "/path/to/project.pxp"
});

// Load project
await client.InvokeAsync<object>("Pixera.Session.LoadProject", new
{
    path = "/path/to/project.pxp"
});
```

## Error Handling

The API client throws `PixeraApiException` when errors occur:

```csharp
try
{
    await client.InvokeAsync<object>("Pixera.Invalid.Method", null);
}
catch (PixeraApiException ex)
{
    Console.WriteLine($"Pixera API Error {ex.ErrorCode}: {ex.Message}");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Not connected: {ex.Message}");
}
```

## Auto-Reconnect

The client automatically attempts to reconnect if the connection is lost:

```csharp
client.ConnectionStateChanged += (sender, e) =>
{
    if (e.NewState == ConnectionState.Reconnecting)
    {
        Console.WriteLine("Connection lost, attempting to reconnect...");
    }
    else if (e.NewState == ConnectionState.Connected && 
             e.PreviousState == ConnectionState.Reconnecting)
    {
        Console.WriteLine("Reconnected successfully!");
    }
};
```

## Thread Safety

The `PixeraApiClient` is thread-safe and can be called from multiple threads:

```csharp
// Multiple concurrent API calls
var tasks = new[]
{
    client.InvokeAsync<object>("Pixera.Direct.SetParameter", new { path = "Timeline 1.Opacity", value = 50.0 }),
    client.InvokeAsync<object>("Pixera.Direct.SetParameter", new { path = "Timeline 2.Opacity", value = 75.0 }),
    client.InvokeAsync<object>("Pixera.Direct.SetParameter", new { path = "Timeline 3.Opacity", value = 100.0 })
};

await Task.WhenAll(tasks);
```

## Best Practices

1. **Reuse the client instance**: Create one `PixeraApiClient` instance and reuse it throughout your application
2. **Handle connection state**: Always monitor `ConnectionStateChanged` events
3. **Use Pixera.Direct for real-time**: For encoder/fader control, use `Pixera.Direct` namespace
4. **Batch operations**: When updating multiple parameters, send all requests concurrently
5. **Dispose properly**: Always dispose the client when done to release resources

## Integration with ControlSurfaceService

```csharp
var client = new PixeraApiClient();
var controlSurface = new ControlSurfaceService();

await client.ConnectAsync("localhost", 1400);

// Handle encoder input
controlSurface.EncoderInput += async (sender, e) =>
{
    if (client.CurrentState == ConnectionState.Connected)
    {
        await client.InvokeAsync<object>("Pixera.Direct.SetParameter", new
        {
            path = e.ParameterPath,
            value = e.NewValue
        });
    }
};

// Set up encoder mapping
controlSurface.SetEncoderMapping(0, new ControlMapping
{
    Label = "Opacity",
    ParameterPath = "Timeline 1.Opacity",
    MinValue = 0,
    MaxValue = 100,
    DefaultValue = 100,
    CoarseStep = 5.0,
    FineStep = 1.0
});
```

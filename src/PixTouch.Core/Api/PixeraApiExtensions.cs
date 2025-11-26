using PixTouch.Core.Models;

namespace PixTouch.Core.Api;

/// <summary>
/// Extension methods for PixeraApiClient providing convenient access to common operations
/// </summary>
public static class PixeraApiExtensions
{
    // Timeline operations
    public static Task StartTimelineAsync(this PixeraApiClient client, string timelineId)
        => client.InvokeAsync<object>("Pixera.Timelines.StartTimeline", new { timelineId });

    public static Task StopTimelineAsync(this PixeraApiClient client, string timelineId)
        => client.InvokeAsync<object>("Pixera.Timelines.StopTimeline", new { timelineId });

    public static Task PauseTimelineAsync(this PixeraApiClient client, string timelineId)
        => client.InvokeAsync<object>("Pixera.Timelines.PauseTimeline", new { timelineId });

    public static Task JumpToCueAsync(this PixeraApiClient client, string timelineId, string cueId)
        => client.InvokeAsync<object>("Pixera.Timelines.JumpToCue", new { timelineId, cueId });

    // Direct parameter control
    public static Task SetParameterAsync(this PixeraApiClient client, string path, double value)
        => client.InvokeAsync<object>("Pixera.Direct.SetParameter", new { path, value });

    public static Task<double> GetParameterAsync(this PixeraApiClient client, string path)
        => client.InvokeAsync<double>("Pixera.Direct.GetParameter", new { path });

    // Layer operations
    public static Task SetLayerOpacityAsync(this PixeraApiClient client, string timelineId, string layerId, double opacity)
        => client.InvokeAsync<object>("Pixera.Layers.SetOpacity", new { timelineId, layerId, opacity });

    public static Task SetLayerPositionAsync(this PixeraApiClient client, string timelineId, string layerId, double x, double y, double z = 0)
        => client.InvokeAsync<object>("Pixera.Layers.SetPosition", new { timelineId, layerId, x, y, z });

    public static Task SetLayerScaleAsync(this PixeraApiClient client, string timelineId, string layerId, double x, double y)
        => client.InvokeAsync<object>("Pixera.Layers.SetScale", new { timelineId, layerId, x, y });

    public static Task SetLayerRotationAsync(this PixeraApiClient client, string timelineId, string layerId, double x, double y, double z)
        => client.InvokeAsync<object>("Pixera.Layers.SetRotation", new { timelineId, layerId, x, y, z });
}

namespace PixTouch.Core.Models;

public class ControlMapping
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Label { get; set; } = string.Empty;
    public string ParameterPath { get; set; } = string.Empty;
    public double MinValue { get; set; } = 0.0;
    public double MaxValue { get; set; } = 100.0;
    public double DefaultValue { get; set; } = 0.0;
    public double CoarseStep { get; set; } = 1.0;
    public double FineStep { get; set; } = 0.1;
    public bool SyncWithPixera { get; set; } = true;
    public string DisplayFormat { get; set; } = "{0:F2}";
}

using Newtonsoft.Json;

namespace PixTouch.Core.Protocol;

public class LoupedeckMessage
{
    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;

    [JsonProperty("encoder_id")]
    public int? EncoderId { get; set; }

    [JsonProperty("label")]
    public string? Label { get; set; }

    [JsonProperty("value")]
    public string? Value { get; set; }

    [JsonProperty("color")]
    public string? Color { get; set; }

    [JsonProperty("delta")]
    public int? Delta { get; set; }

    [JsonProperty("fine_mode")]
    public bool? FineMode { get; set; }

    [JsonProperty("button_id")]
    public string? ButtonId { get; set; }

    [JsonProperty("pressed")]
    public bool? Pressed { get; set; }
}

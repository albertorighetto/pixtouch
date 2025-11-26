using Newtonsoft.Json;

namespace PixTouch.Core.Api;

public class JsonRpcRequest
{
    [JsonProperty("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";

    [JsonProperty("method")]
    public string Method { get; set; } = string.Empty;

    [JsonProperty("params")]
    public object? Params { get; set; }

    [JsonProperty("id")]
    public int Id { get; set; }
}

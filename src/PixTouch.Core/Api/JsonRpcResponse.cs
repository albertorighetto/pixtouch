using Newtonsoft.Json;

namespace PixTouch.Core.Api;

public class JsonRpcResponse<T>
{
    [JsonProperty("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";

    [JsonProperty("result")]
    public T? Result { get; set; }

    [JsonProperty("error")]
    public JsonRpcError? Error { get; set; }

    [JsonProperty("id")]
    public int Id { get; set; }
}

public class JsonRpcError
{
    [JsonProperty("code")]
    public int Code { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; } = string.Empty;

    [JsonProperty("data")]
    public object? Data { get; set; }
}

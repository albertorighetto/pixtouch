namespace PixTouch.Core.Api;

public enum ConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Reconnecting,
    Error
}

public class ConnectionStateChangedEventArgs : EventArgs
{
    public ConnectionState NewState { get; set; }
    public ConnectionState PreviousState { get; set; }
    public string? ErrorMessage { get; set; }
}

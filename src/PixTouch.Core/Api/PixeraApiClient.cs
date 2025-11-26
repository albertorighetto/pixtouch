using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace PixTouch.Core.Api;

public class PixeraApiClient : IDisposable
{
    private TcpClient? _tcpClient;
    private NetworkStream? _networkStream;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    private int _requestId = 0;
    private ConnectionState _connectionState = ConnectionState.Disconnected;
    private string _host = string.Empty;
    private int _port = 1400;
    private CancellationTokenSource? _reconnectCts;
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private readonly object _stateLock = new();
    private bool _disposed = false;

    public event EventHandler<ConnectionStateChangedEventArgs>? ConnectionStateChanged;

    public ConnectionState CurrentState
    {
        get
        {
            lock (_stateLock)
            {
                return _connectionState;
            }
        }
        private set
        {
            lock (_stateLock)
            {
                if (_connectionState != value)
                {
                    var previous = _connectionState;
                    _connectionState = value;
                    OnConnectionStateChanged(new ConnectionStateChangedEventArgs
                    {
                        PreviousState = previous,
                        NewState = value
                    });
                }
            }
        }
    }

    public async Task ConnectAsync(string host, int port = 1400, CancellationToken cancellationToken = default)
    {
        _host = host;
        _port = port;
        
        CurrentState = ConnectionState.Connecting;

        try
        {
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(host, port, cancellationToken);
            _networkStream = _tcpClient.GetStream();
            _reader = new StreamReader(_networkStream, Encoding.UTF8);
            _writer = new StreamWriter(_networkStream, Encoding.UTF8) { AutoFlush = true };

            CurrentState = ConnectionState.Connected;
            StartAutoReconnect();
        }
        catch (Exception ex)
        {
            CurrentState = ConnectionState.Error;
            OnConnectionStateChanged(new ConnectionStateChangedEventArgs
            {
                PreviousState = ConnectionState.Connecting,
                NewState = ConnectionState.Error,
                ErrorMessage = ex.Message
            });
            throw;
        }
    }

    public async Task DisconnectAsync()
    {
        StopAutoReconnect();
        await CloseConnectionAsync();
    }

    public async Task<T?> InvokeAsync<T>(string method, object? parameters = null, CancellationToken cancellationToken = default)
    {
        if (CurrentState != ConnectionState.Connected)
        {
            throw new InvalidOperationException("Not connected to Pixera");
        }

        var request = new JsonRpcRequest
        {
            Method = method,
            Params = parameters,
            Id = Interlocked.Increment(ref _requestId)
        };

        await _sendLock.WaitAsync(cancellationToken);
        try
        {
            var requestJson = JsonConvert.SerializeObject(request);
            await _writer!.WriteLineAsync(requestJson);

            var responseJson = await _reader!.ReadLineAsync(cancellationToken);
            if (string.IsNullOrEmpty(responseJson))
            {
                throw new InvalidOperationException("Received empty response from Pixera");
            }

            var response = JsonConvert.DeserializeObject<JsonRpcResponse<T>>(responseJson);
            
            if (response?.Error != null)
            {
                throw new PixeraApiException(response.Error.Message, response.Error.Code);
            }

            return response!.Result;
        }
        finally
        {
            _sendLock.Release();
        }
    }

    private void StartAutoReconnect()
    {
        _reconnectCts = new CancellationTokenSource();
        _ = Task.Run(() => MonitorConnectionAsync(_reconnectCts.Token));
    }

    private void StopAutoReconnect()
    {
        _reconnectCts?.Cancel();
        _reconnectCts?.Dispose();
        _reconnectCts = null;
    }

    private async Task MonitorConnectionAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && CurrentState != ConnectionState.Disconnected)
        {
            await Task.Delay(5000, cancellationToken);

            if (_tcpClient?.Connected == false && CurrentState == ConnectionState.Connected)
            {
                CurrentState = ConnectionState.Reconnecting;
                await ReconnectAsync(cancellationToken);
            }
        }
    }

    private async Task ReconnectAsync(CancellationToken cancellationToken)
    {
        const int maxRetries = 5;
        int retryCount = 0;

        while (retryCount < maxRetries && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                await CloseConnectionAsync();
                await Task.Delay(2000 * (retryCount + 1), cancellationToken);
                await ConnectAsync(_host, _port, cancellationToken);
                return;
            }
            catch
            {
                retryCount++;
                if (retryCount >= maxRetries)
                {
                    CurrentState = ConnectionState.Error;
                }
            }
        }
    }

    private async Task CloseConnectionAsync()
    {
        CurrentState = ConnectionState.Disconnected;
        
        try
        {
            _reader?.Dispose();
            _writer?.Dispose();
            _networkStream?.Dispose();
            _tcpClient?.Dispose();
        }
        catch
        {
            // Ignore cleanup errors
        }
        
        _reader = null;
        _writer = null;
        _networkStream = null;
        _tcpClient = null;
        
        await Task.CompletedTask;
    }

    protected virtual void OnConnectionStateChanged(ConnectionStateChangedEventArgs e)
    {
        ConnectionStateChanged?.Invoke(this, e);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        StopAutoReconnect();
        CloseConnectionAsync().Wait();
        _sendLock?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}

public class PixeraApiException : Exception
{
    public int ErrorCode { get; }

    public PixeraApiException(string message, int errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }
}

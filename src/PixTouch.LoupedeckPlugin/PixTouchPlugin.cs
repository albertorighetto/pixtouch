using System.Net.Sockets;
using System.Text;
using PixTouch.Core.Protocol;
using Newtonsoft.Json;

namespace PixTouch.LoupedeckPlugin;

/// <summary>
/// Main plugin class that connects to PixTouch application
/// This plugin acts as a "dumb terminal" - all logic is controlled by PixTouch
/// </summary>
public class PixTouchPlugin : IDisposable
{
    private TcpClient? _client;
    private NetworkStream? _stream;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    private CancellationTokenSource? _cts;
    private bool _isRunning;
    private readonly SemaphoreSlim _sendLock = new(1, 1);

    public event EventHandler<LoupedeckMessage>? MessageReceived;
    public event EventHandler? Connected;
    public event EventHandler? Disconnected;

    public bool IsConnected => _client?.Connected == true;

    /// <summary>
    /// Connect to PixTouch application
    /// </summary>
    public async Task ConnectAsync(string host = "localhost", int port = 19790, CancellationToken cancellationToken = default)
    {
        if (_isRunning)
            throw new InvalidOperationException("Already connected");

        try
        {
            _client = new TcpClient();
            await _client.ConnectAsync(host, port, cancellationToken);
            _stream = _client.GetStream();
            _reader = new StreamReader(_stream, Encoding.UTF8);
            _writer = new StreamWriter(_stream, Encoding.UTF8) { AutoFlush = true };

            _isRunning = true;
            _cts = new CancellationTokenSource();

            OnConnected();

            _ = Task.Run(() => ReceiveMessagesAsync(_cts.Token), cancellationToken);
        }
        catch
        {
            await DisconnectAsync();
            throw;
        }
    }

    public async Task DisconnectAsync()
    {
        _isRunning = false;
        _cts?.Cancel();

        try
        {
            _reader?.Dispose();
            _writer?.Dispose();
            _stream?.Dispose();
            _client?.Dispose();
        }
        catch
        {
            // Ignore cleanup errors
        }

        _reader = null;
        _writer = null;
        _stream = null;
        _client = null;

        OnDisconnected();

        await Task.CompletedTask;
    }

    private async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (_isRunning && !cancellationToken.IsCancellationRequested && _reader != null)
            {
                var line = await _reader.ReadLineAsync(cancellationToken);
                if (string.IsNullOrEmpty(line))
                    break;

                try
                {
                    var message = JsonConvert.DeserializeObject<LoupedeckMessage>(line);
                    if (message != null)
                    {
                        OnMessageReceived(message);
                    }
                }
                catch
                {
                    // Invalid message format
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal cancellation
        }
        catch
        {
            // Connection error
        }
        finally
        {
            await DisconnectAsync();
        }
    }

    /// <summary>
    /// Send encoder input to PixTouch
    /// </summary>
    public async Task SendEncoderInputAsync(int encoderId, int delta, bool fineMode = false)
    {
        var message = new LoupedeckMessage
        {
            Type = "encoder_input",
            EncoderId = encoderId,
            Delta = delta,
            FineMode = fineMode
        };

        await SendMessageAsync(message);
    }

    /// <summary>
    /// Send button press to PixTouch
    /// </summary>
    public async Task SendButtonInputAsync(string buttonId, bool pressed)
    {
        var message = new LoupedeckMessage
        {
            Type = "button_input",
            ButtonId = buttonId,
            Pressed = pressed
        };

        await SendMessageAsync(message);
    }

    private async Task SendMessageAsync(LoupedeckMessage message)
    {
        if (!IsConnected || _writer == null)
            return;

        await _sendLock.WaitAsync();
        try
        {
            var json = JsonConvert.SerializeObject(message);
            await _writer.WriteLineAsync(json);
        }
        finally
        {
            _sendLock.Release();
        }
    }

    protected virtual void OnMessageReceived(LoupedeckMessage message)
    {
        MessageReceived?.Invoke(this, message);
    }

    protected virtual void OnConnected()
    {
        Connected?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnDisconnected()
    {
        Disconnected?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        DisconnectAsync().Wait();
        _sendLock?.Dispose();
        GC.SuppressFinalize(this);
    }
}

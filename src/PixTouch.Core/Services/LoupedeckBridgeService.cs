using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using PixTouch.Core.Protocol;

namespace PixTouch.Core.Services;

public class LoupedeckBridgeService : IDisposable
{
    private TcpListener? _listener;
    private TcpClient? _client;
    private NetworkStream? _stream;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    private CancellationTokenSource? _cts;
    private bool _isRunning;
    private readonly SemaphoreSlim _sendLock = new(1, 1);

    public event EventHandler<LoupedeckEncoderInputEventArgs>? EncoderInput;
    public event EventHandler<LoupedeckButtonInputEventArgs>? ButtonInput;
    public event EventHandler? ClientConnected;
    public event EventHandler? ClientDisconnected;

    public bool IsConnected => _client?.Connected == true;

    public async Task StartServerAsync(int port = 19790, CancellationToken cancellationToken = default)
    {
        if (_isRunning)
            throw new InvalidOperationException("Server is already running");

        _listener = new TcpListener(IPAddress.Loopback, port);
        _listener.Start();
        _isRunning = true;
        _cts = new CancellationTokenSource();

        _ = Task.Run(() => AcceptClientsAsync(_cts.Token), cancellationToken);
    }

    public async Task StopServerAsync()
    {
        _isRunning = false;
        _cts?.Cancel();
        
        await CloseClientAsync();
        
        _listener?.Stop();
        _listener = null;
        
        _cts?.Dispose();
        _cts = null;
    }

    private async Task AcceptClientsAsync(CancellationToken cancellationToken)
    {
        while (_isRunning && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (_listener != null)
                {
                    _client = await _listener.AcceptTcpClientAsync(cancellationToken);
                    _stream = _client.GetStream();
                    _reader = new StreamReader(_stream, Encoding.UTF8);
                    _writer = new StreamWriter(_stream, Encoding.UTF8) { AutoFlush = true };

                    OnClientConnected();

                    _ = Task.Run(() => HandleClientMessagesAsync(cancellationToken), cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                // Continue accepting connections
            }
        }
    }

    private async Task HandleClientMessagesAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (_isRunning && !cancellationToken.IsCancellationRequested && _reader != null)
            {
                var line = await _reader.ReadLineAsync(cancellationToken);
                if (string.IsNullOrEmpty(line))
                {
                    break;
                }

                ProcessMessage(line);
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
            await CloseClientAsync();
        }
    }

    private void ProcessMessage(string json)
    {
        try
        {
            var message = JsonConvert.DeserializeObject<LoupedeckMessage>(json);
            if (message == null)
                return;

            switch (message.Type)
            {
                case "encoder_input":
                    if (message.EncoderId.HasValue && message.Delta.HasValue)
                    {
                        OnEncoderInput(new LoupedeckEncoderInputEventArgs
                        {
                            EncoderId = message.EncoderId.Value,
                            Delta = message.Delta.Value,
                            FineMode = message.FineMode ?? false
                        });
                    }
                    break;

                case "button_input":
                    if (message.ButtonId != null && message.Pressed.HasValue)
                    {
                        OnButtonInput(new LoupedeckButtonInputEventArgs
                        {
                            ButtonId = message.ButtonId,
                            Pressed = message.Pressed.Value
                        });
                    }
                    break;
            }
        }
        catch
        {
            // Invalid message format
        }
    }

    public async Task UpdateEncoderDisplayAsync(int encoderId, string label, string value, string color = "#FFFFFF")
    {
        if (!IsConnected)
            return;

        var message = new LoupedeckMessage
        {
            Type = "encoder_update",
            EncoderId = encoderId,
            Label = label,
            Value = value,
            Color = color
        };

        await SendMessageAsync(message);
    }

    public async Task UpdateButtonDisplayAsync(string buttonId, string label, string color = "#FFFFFF")
    {
        if (!IsConnected)
            return;

        var message = new LoupedeckMessage
        {
            Type = "button_update",
            ButtonId = buttonId,
            Label = label,
            Color = color
        };

        await SendMessageAsync(message);
    }

    private async Task SendMessageAsync(LoupedeckMessage message)
    {
        if (_writer == null || !IsConnected)
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

    private async Task CloseClientAsync()
    {
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

        OnClientDisconnected();
        
        await Task.CompletedTask;
    }

    protected virtual void OnEncoderInput(LoupedeckEncoderInputEventArgs e)
    {
        EncoderInput?.Invoke(this, e);
    }

    protected virtual void OnButtonInput(LoupedeckButtonInputEventArgs e)
    {
        ButtonInput?.Invoke(this, e);
    }

    protected virtual void OnClientConnected()
    {
        ClientConnected?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnClientDisconnected()
    {
        ClientDisconnected?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        StopServerAsync().Wait();
        _sendLock?.Dispose();
        GC.SuppressFinalize(this);
    }
}

public class LoupedeckEncoderInputEventArgs : EventArgs
{
    public int EncoderId { get; set; }
    public int Delta { get; set; }
    public bool FineMode { get; set; }
}

public class LoupedeckButtonInputEventArgs : EventArgs
{
    public string ButtonId { get; set; } = string.Empty;
    public bool Pressed { get; set; }
}

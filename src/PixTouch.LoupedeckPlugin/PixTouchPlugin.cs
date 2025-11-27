namespace Loupedeck.PixTouchPlugin
{
    using System;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using PixTouch.Core.Protocol;
    using Loupedeck.PixTouchPlugin.Actions;

    /// <summary>
    /// Main plugin class that connects to PixTouch application via TCP.
    /// This plugin acts as a "dumb terminal" - all logic is controlled by PixTouch.
    /// </summary>
    public class PixTouchPlugin : Plugin
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private StreamReader _reader;
        private StreamWriter _writer;
        private CancellationTokenSource _cts;
        private Boolean _isRunning;
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);

        // Connection settings
        private const String DefaultHost = "localhost";
        private const Int32 DefaultPort = 19790;

        public override Boolean UsesApplicationApiOnly => true;
        public override Boolean HasNoApplication => false;

        public PixTouchPlugin()
        {
            // Initialize plugin logging
            PluginLog.Init(this.Log);
            PluginLog.Info("PixTouch Plugin initialized");
        }

        public override void Load()
        {
            PluginLog.Info("Loading PixTouch Plugin...");

            // Register encoder adjustments (6 encoders)
            for (var i = 1; i <= 6; i++)
            {
                var encoder = new EncoderAdjustment(this, i);
                PluginLog.Info($"Registered Encoder {i}");
            }

            // Register button commands
            var buttons = new[]
            {
                ("play", "Play"),
                ("pause", "Pause"),
                ("stop", "Stop"),
                ("next_cue", "Next Cue"),
                ("prev_cue", "Previous Cue")
            };

            foreach (var (id, name) in buttons)
            {
                var button = new ButtonCommand(this, id, name);
                PluginLog.Info($"Registered button: {name}");
            }

            // Start TCP connection to PixTouch
            Task.Run(async () => await this.ConnectToPixTouchAsync());

            PluginLog.Info("PixTouch Plugin loaded successfully");
        }

        public override void Unload()
        {
            PluginLog.Info("Unloading PixTouch Plugin...");

            this._isRunning = false;
            this._cts?.Cancel();

            try
            {
                this._reader?.Dispose();
                this._writer?.Dispose();
                this._stream?.Dispose();
                this._client?.Dispose();
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Error during cleanup: {ex.Message}");
            }

            this._sendLock?.Dispose();
            PluginLog.Info("PixTouch Plugin unloaded");
        }

        /// <summary>
        /// Connect to PixTouch application
        /// </summary>
        private async Task ConnectToPixTouchAsync()
        {
            if (this._isRunning)
            {
                PluginLog.Warning("Already connected to PixTouch");
                return;
            }

            try
            {
                PluginLog.Info($"Connecting to PixTouch at {DefaultHost}:{DefaultPort}...");
                this._client = new TcpClient();
                await this._client.ConnectAsync(DefaultHost, DefaultPort);
                this._stream = this._client.GetStream();
                this._reader = new StreamReader(this._stream, Encoding.UTF8);
                this._writer = new StreamWriter(this._stream, Encoding.UTF8) { AutoFlush = true };

                this._isRunning = true;
                this._cts = new CancellationTokenSource();

                PluginLog.Info("Connected to PixTouch successfully");

                // Start receiving messages
                _ = Task.Run(() => this.ReceiveMessagesAsync(this._cts.Token));
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Failed to connect to PixTouch: {ex.Message}");
                await this.DisconnectAsync();
            }
        }

        /// <summary>
        /// Disconnect from PixTouch
        /// </summary>
        private async Task DisconnectAsync()
        {
            this._isRunning = false;
            this._cts?.Cancel();

            try
            {
                this._reader?.Dispose();
                this._writer?.Dispose();
                this._stream?.Dispose();
                this._client?.Dispose();
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Error during disconnect: {ex.Message}");
            }

            this._reader = null;
            this._writer = null;
            this._stream = null;
            this._client = null;

            PluginLog.Info("Disconnected from PixTouch");
            await Task.CompletedTask;
        }

        /// <summary>
        /// Receive messages from PixTouch
        /// </summary>
        private async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (this._isRunning && !cancellationToken.IsCancellationRequested && this._reader != null)
                {
                    var line = await this._reader.ReadLineAsync();
                    if (String.IsNullOrEmpty(line))
                    {
                        break;
                    }

                    try
                    {
                        var message = JsonConvert.DeserializeObject<LoupedeckMessage>(line);
                        if (message != null)
                        {
                            this.HandleMessageFromPixTouch(message);
                        }
                    }
                    catch (Exception ex)
                    {
                        PluginLog.Error($"Error parsing message: {ex.Message}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                PluginLog.Info("Message receiving cancelled");
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Connection error: {ex.Message}");
            }
            finally
            {
                await this.DisconnectAsync();
            }
        }

        /// <summary>
        /// Handle incoming message from PixTouch (display updates, etc.)
        /// </summary>
        private void HandleMessageFromPixTouch(LoupedeckMessage message)
        {
            PluginLog.Info($"Received message from PixTouch: {message.Type}");
            // TODO: Handle display updates and other messages from PixTouch
            // This could update encoder displays, button states, etc.
        }

        /// <summary>
        /// Send message to PixTouch application
        /// </summary>
        public async void SendMessageToPixTouch(LoupedeckMessage message)
        {
            if (!this._isRunning || this._writer == null)
            {
                PluginLog.Warning("Cannot send message - not connected to PixTouch");
                return;
            }

            await this._sendLock.WaitAsync();
            try
            {
                var json = JsonConvert.SerializeObject(message);
                await this._writer.WriteLineAsync(json);
                PluginLog.Info($"Sent message to PixTouch: {message.Type}");
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Error sending message: {ex.Message}");
            }
            finally
            {
                this._sendLock.Release();
            }
        }
    }
}

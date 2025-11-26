using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ReactiveUI;
using PixTouch.Core.Api;
using PixTouch.Core.Configuration;
using PixTouch.Core.Models;
using PixTouch.Core.Services;

namespace PixTouch.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly PixeraApiClient _pixeraClient;
    private readonly ControlSurfaceService _controlSurface;
    private readonly LoupedeckBridgeService _loupedeckBridge;
    private readonly ConfigurationService _configService;
    
    private string _connectionStatus = "Disconnected";
    private string _pixeraHost = "localhost";
    private int _pixeraPort = 1400;
    private bool _isConnected = false;
    
    public string ConnectionStatus
    {
        get => _connectionStatus;
        set => this.RaiseAndSetIfChanged(ref _connectionStatus, value);
    }
    
    public string PixeraHost
    {
        get => _pixeraHost;
        set => this.RaiseAndSetIfChanged(ref _pixeraHost, value);
    }
    
    public int PixeraPort
    {
        get => _pixeraPort;
        set => this.RaiseAndSetIfChanged(ref _pixeraPort, value);
    }
    
    public bool IsConnected
    {
        get => _isConnected;
        set => this.RaiseAndSetIfChanged(ref _isConnected, value);
    }
    
    public ObservableCollection<EncoderControl> Encoders { get; }
    public ObservableCollection<FaderControl> Faders { get; }
    
    public MainWindowViewModel()
    {
        _pixeraClient = new PixeraApiClient();
        _controlSurface = new ControlSurfaceService();
        _loupedeckBridge = new LoupedeckBridgeService();
        _configService = new ConfigurationService();
        
        Encoders = _controlSurface.Encoders;
        Faders = _controlSurface.Faders;
        
        InitializeServices();
    }
    
    private void InitializeServices()
    {
        _pixeraClient.ConnectionStateChanged += OnConnectionStateChanged;
        _controlSurface.EncoderInput += OnEncoderInput;
        _loupedeckBridge.EncoderInput += OnLoupedeckEncoderInput;
        
        _ = LoadConfigurationAsync();
    }
    
    private async Task LoadConfigurationAsync()
    {
        var config = await _configService.LoadAsync();
        PixeraHost = config.Connection.Host;
        PixeraPort = config.Connection.Port;
        
        // Load encoder mappings
        for (int i = 0; i < Math.Min(config.EncoderMappings.Count, Encoders.Count); i++)
        {
            _controlSurface.SetEncoderMapping(i, config.EncoderMappings[i]);
        }
        
        // Load fader mappings
        for (int i = 0; i < Math.Min(config.FaderMappings.Count, Faders.Count); i++)
        {
            _controlSurface.SetFaderMapping(i, config.FaderMappings[i]);
        }
        
        // Start Loupedeck bridge if enabled
        if (config.Loupedeck.Enabled)
        {
            await _loupedeckBridge.StartServerAsync(config.Loupedeck.Port);
        }
        
        // Auto-connect if enabled
        if (config.Connection.AutoConnect)
        {
            await ConnectToPixeraAsync();
        }
    }
    
    public async Task ConnectToPixeraAsync()
    {
        try
        {
            await _pixeraClient.ConnectAsync(PixeraHost, PixeraPort);
        }
        catch (Exception ex)
        {
            ConnectionStatus = $"Error: {ex.Message}";
        }
    }
    
    public async Task DisconnectFromPixeraAsync()
    {
        await _pixeraClient.DisconnectAsync();
    }
    
    private void OnConnectionStateChanged(object? sender, ConnectionStateChangedEventArgs e)
    {
        ConnectionStatus = e.NewState.ToString();
        IsConnected = e.NewState == ConnectionState.Connected;
    }
    
    private void OnEncoderInput(object? sender, EncoderInputEventArgs e)
    {
        // Update Pixera parameter via API
        _ = UpdatePixeraParameterAsync(e.ParameterPath, e.NewValue);
        
        // Update Loupedeck display
        var encoder = Encoders[e.Index];
        _ = _loupedeckBridge.UpdateEncoderDisplayAsync(
            e.Index,
            encoder.Label,
            encoder.FormattedValue,
            "#00FF00"
        );
    }
    
    private void OnLoupedeckEncoderInput(object? sender, LoupedeckEncoderInputEventArgs e)
    {
        _controlSurface.HandleEncoderInput(e.EncoderId, e.Delta, e.FineMode);
    }
    
    private async Task UpdatePixeraParameterAsync(string parameterPath, double value)
    {
        if (!IsConnected || string.IsNullOrEmpty(parameterPath))
            return;
        
        try
        {
            // Parse parameter path and call appropriate API method
            // This is a simplified example - real implementation would need proper parsing
            await _pixeraClient.InvokeAsync<object>("Pixera.Direct.SetParameter", new
            {
                path = parameterPath,
                value = value
            });
        }
        catch
        {
            // Handle error
        }
    }
}

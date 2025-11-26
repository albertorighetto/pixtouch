using PixTouch.Core.Models;

namespace PixTouch.Core.Configuration;

public class AppConfiguration
{
    public ConnectionSettings Connection { get; set; } = new();
    public List<ControlMapping> EncoderMappings { get; set; } = new();
    public List<ControlMapping> FaderMappings { get; set; } = new();
    public LoupedeckSettings Loupedeck { get; set; } = new();
    public UiPreferences UiPreferences { get; set; } = new();
}

public class ConnectionSettings
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1400;
    public bool AutoConnect { get; set; } = true;
}

public class LoupedeckSettings
{
    public bool Enabled { get; set; } = true;
    public int Port { get; set; } = 19790;
}

public class UiPreferences
{
    public bool DarkTheme { get; set; } = true;
    public bool FullScreenMode { get; set; } = false;
    public double WindowWidth { get; set; } = 1280;
    public double WindowHeight { get; set; } = 720;
}

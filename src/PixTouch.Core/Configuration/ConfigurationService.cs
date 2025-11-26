using System.Text.Json;

namespace PixTouch.Core.Configuration;

public class ConfigurationService
{
    private readonly string _configPath;
    private AppConfiguration _configuration;

    public AppConfiguration Configuration => _configuration;

    public ConfigurationService(string? configPath = null)
    {
        _configPath = configPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PixTouch",
            "config.json"
        );

        _configuration = new AppConfiguration();
    }

    public async Task<AppConfiguration> LoadAsync()
    {
        if (!File.Exists(_configPath))
        {
            _configuration = new AppConfiguration();
            return _configuration;
        }

        try
        {
            var json = await File.ReadAllTextAsync(_configPath);
            _configuration = JsonSerializer.Deserialize<AppConfiguration>(json) ?? new AppConfiguration();
        }
        catch
        {
            _configuration = new AppConfiguration();
        }

        return _configuration;
    }

    public async Task SaveAsync(AppConfiguration? configuration = null)
    {
        configuration ??= _configuration;

        var directory = Path.GetDirectoryName(_configPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(configuration, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(_configPath, json);
        _configuration = configuration;
    }
}

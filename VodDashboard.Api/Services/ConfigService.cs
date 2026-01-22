using System.Text.Json;
using Microsoft.Extensions.Options;
using VodDashboard.Api.DTO;
using VodDashboard.Api.Models;

namespace VodDashboard.Api.Services;

public class ConfigService
{
    private readonly PipelineSettings _settings;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public ConfigService(IOptions<PipelineSettings> settings)
    {
        _settings = settings.Value;
    }

    public virtual ConfigDto? GetConfig()
    {
        if (string.IsNullOrWhiteSpace(_settings.ConfigFile))
            return null;

        if (!File.Exists(_settings.ConfigFile))
            return null;

        var json = File.ReadAllText(_settings.ConfigFile);
        return JsonSerializer.Deserialize<ConfigDto>(json);
    }

    public virtual bool SaveConfig(ConfigDto config)
    {
        if (string.IsNullOrWhiteSpace(_settings.ConfigFile))
            return false;

        try
        {
            var json = JsonSerializer.Serialize(config, _jsonOptions);

            // Atomic write: write to temp file then replace
            var tempPath = _settings.ConfigFile + ".tmp";
            File.WriteAllText(tempPath, json);
            File.Move(tempPath, _settings.ConfigFile, overwrite: true);

            return true;
        }
        catch
        {
            return false;
        }
    }
}

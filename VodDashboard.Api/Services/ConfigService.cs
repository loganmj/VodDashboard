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
            throw new InvalidOperationException("Pipeline configuration file path is not configured.");

        if (!File.Exists(_settings.ConfigFile))
            return null;

        try
        {
            string json = File.ReadAllText(_settings.ConfigFile);
            return JsonSerializer.Deserialize<ConfigDto>(json);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Failed to deserialize configuration file at '{_settings.ConfigFile}'.",
                ex);
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException(
                $"Failed to read configuration file at '{_settings.ConfigFile}'.",
                ex);
        }
    }

    public virtual void SaveConfig(ConfigDto config)
    {
        if (string.IsNullOrWhiteSpace(_settings.ConfigFile))
            throw new InvalidOperationException("Pipeline configuration file path is not configured.");

        try
        {
            string json = JsonSerializer.Serialize(config, _jsonOptions);

            // Atomic write: write to temp file then replace
            string tempPath = _settings.ConfigFile + ".tmp";
            File.WriteAllText(tempPath, json);
            File.Move(tempPath, _settings.ConfigFile, overwrite: true);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                "Failed to serialize pipeline configuration.",
                ex);
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException(
                $"Failed to write configuration file at '{_settings.ConfigFile}'.",
                ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new InvalidOperationException(
                $"Insufficient permissions to write configuration file at '{_settings.ConfigFile}'.",
                ex);
        }
    }
}

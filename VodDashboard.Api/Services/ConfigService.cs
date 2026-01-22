using Microsoft.Extensions.Options;
using System.Text.Json;
using VodDashboard.Api.DTO;
using VodDashboard.Api.Models;

namespace VodDashboard.Api.Services;

public class ConfigService(IOptions<PipelineSettings> settings)
{
    private readonly PipelineSettings _settings = settings.Value;
    private PipelineConfig? _cachedConfig;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public virtual PipelineConfig GetCachedConfig()
    {
        if (_cachedConfig == null)
        {
            _cachedConfig = GetConfig() ?? GetDefaultConfig();
        }
        return _cachedConfig;
    }

    private static PipelineConfig GetDefaultConfig()
    {
        return new PipelineConfig
        {
            InputDirectory = "./Input",
            OutputDirectory = "./Output",
            ArchiveDirectory = "./Input/Archive",
            EnableHighlights = true,
            EnableScenes = true,
            SilenceThreshold = -40
        };
    }

    public virtual PipelineConfig? GetConfig()
    {
        if (string.IsNullOrWhiteSpace(_settings.ConfigFile))
        {
            throw new InvalidOperationException("Pipeline configuration file path is not configured.");
        }

        if (!File.Exists(_settings.ConfigFile))
        {
            return null;
        }

        try
        {
            string json = File.ReadAllText(_settings.ConfigFile);
            return JsonSerializer.Deserialize<PipelineConfig>(json);
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

    public virtual void SaveConfig(PipelineConfig config)
    {
        if (string.IsNullOrWhiteSpace(_settings.ConfigFile))
        {
            throw new InvalidOperationException("Pipeline configuration file path is not configured.");
        }

        try
        {
            string json = JsonSerializer.Serialize(config, _jsonOptions);

            // Atomic write: write to temp file then replace
            string tempPath = _settings.ConfigFile + ".tmp";
            File.WriteAllText(tempPath, json);
            File.Move(tempPath, _settings.ConfigFile, overwrite: true);

            // Update cached config
            _cachedConfig = config;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Failed to serialize pipeline configuration.", ex);
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException($"Failed to write configuration file at '{_settings.ConfigFile}'.", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new InvalidOperationException($"Insufficient permissions to write configuration file at '{_settings.ConfigFile}'.", ex);
        }
    }
}

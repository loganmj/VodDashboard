using FluentAssertions;
using Microsoft.Extensions.Options;
using VodDashboard.Api.DTO;
using VodDashboard.Api.Models;
using VodDashboard.Api.Services;

namespace VodDashboard.Api.Tests.Services;

public class ConfigServiceTests : IDisposable
{
    private readonly string _testDirectory;

    public ConfigServiceTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"ConfigServiceTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            try
            {
                Directory.Delete(_testDirectory, recursive: true);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to delete test directory '{_testDirectory}': {ex}");
            }
        }
    }

    [Fact]
    public void GetConfig_WhenConfigFileIsNull_ThrowsInvalidOperationException()
    {
        // Arrange
        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = "/some/output",
            ConfigFile = null!
        });
        var service = new ConfigService(settings);

        // Act
        Action act = () => service.GetConfig();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Pipeline configuration file path is not configured.");
    }

    [Fact]
    public void GetConfig_WhenConfigFileIsEmpty_ThrowsInvalidOperationException()
    {
        // Arrange
        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = "/some/output",
            ConfigFile = ""
        });
        var service = new ConfigService(settings);

        // Act
        Action act = () => service.GetConfig();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Pipeline configuration file path is not configured.");
    }

    [Fact]
    public void GetConfig_WhenConfigFileIsWhitespace_ThrowsInvalidOperationException()
    {
        // Arrange
        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = "/some/output",
            ConfigFile = "   "
        });
        var service = new ConfigService(settings);

        // Act
        Action act = () => service.GetConfig();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Pipeline configuration file path is not configured.");
    }

    [Fact]
    public void GetConfig_WhenConfigFileDoesNotExist_ReturnsNull()
    {
        // Arrange
        var nonExistentFile = Path.Combine(_testDirectory, "nonexistent.json");
        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = "/some/output",
            ConfigFile = nonExistentFile
        });
        var service = new ConfigService(settings);

        // Act
        var result = service.GetConfig();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetConfig_WhenConfigFileExists_ReturnsDeserializedConfig()
    {
        // Arrange
        var configFile = Path.Combine(_testDirectory, "config.json");
        var configJson = @"{
  ""InputDirectory"": ""/input"",
  ""OutputDirectory"": ""/output"",
  ""ArchiveDirectory"": ""/archive"",
  ""EnableHighlights"": true,
  ""EnableScenes"": false,
  ""SilenceThreshold"": -30
}";
        File.WriteAllText(configFile, configJson);

        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = "/some/output",
            ConfigFile = configFile
        });
        var service = new ConfigService(settings);

        // Act
        var result = service.GetConfig();

        // Assert
        result.Should().NotBeNull();
        result!.InputDirectory.Should().Be("/input");
        result.OutputDirectory.Should().Be("/output");
        result.ArchiveDirectory.Should().Be("/archive");
        result.EnableHighlights.Should().BeTrue();
        result.EnableScenes.Should().BeFalse();
        result.SilenceThreshold.Should().Be(-30);
    }

    [Fact]
    public void GetConfig_WhenConfigFileHasPartialData_ReturnsConfigWithDefaults()
    {
        // Arrange
        var configFile = Path.Combine(_testDirectory, "config.json");
        var configJson = @"{
  ""InputDirectory"": ""/input""
}";
        File.WriteAllText(configFile, configJson);

        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = "/some/output",
            ConfigFile = configFile
        });
        var service = new ConfigService(settings);

        // Act
        var result = service.GetConfig();

        // Assert
        result.Should().NotBeNull();
        result!.InputDirectory.Should().Be("/input");
        result.OutputDirectory.Should().Be("");
        result.ArchiveDirectory.Should().Be("");
        result.EnableHighlights.Should().BeFalse();
        result.EnableScenes.Should().BeFalse();
        result.SilenceThreshold.Should().Be(0);
    }

    [Fact]
    public void SaveConfig_WhenConfigFileIsNull_ThrowsInvalidOperationException()
    {
        // Arrange
        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = "/some/output",
            ConfigFile = null!
        });
        var service = new ConfigService(settings);
        var config = new ConfigDto
        {
            InputDirectory = "/input",
            OutputDirectory = "/output"
        };

        // Act
        Action act = () => service.SaveConfig(config);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Pipeline configuration file path is not configured.");
    }

    [Fact]
    public void SaveConfig_WhenConfigFileIsEmpty_ThrowsInvalidOperationException()
    {
        // Arrange
        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = "/some/output",
            ConfigFile = ""
        });
        var service = new ConfigService(settings);
        var config = new ConfigDto
        {
            InputDirectory = "/input",
            OutputDirectory = "/output"
        };

        // Act
        Action act = () => service.SaveConfig(config);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Pipeline configuration file path is not configured.");
    }

    [Fact]
    public void SaveConfig_WhenConfigFileIsWhitespace_ThrowsInvalidOperationException()
    {
        // Arrange
        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = "/some/output",
            ConfigFile = "   "
        });
        var service = new ConfigService(settings);
        var config = new ConfigDto
        {
            InputDirectory = "/input",
            OutputDirectory = "/output"
        };

        // Act
        Action act = () => service.SaveConfig(config);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Pipeline configuration file path is not configured.");
    }

    [Fact]
    public void SaveConfig_WhenValidConfig_SavesFile()
    {
        // Arrange
        var configFile = Path.Combine(_testDirectory, "config.json");
        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = "/some/output",
            ConfigFile = configFile
        });
        var service = new ConfigService(settings);
        var config = new ConfigDto
        {
            InputDirectory = "/input",
            OutputDirectory = "/output",
            ArchiveDirectory = "/archive",
            EnableHighlights = true,
            EnableScenes = false,
            SilenceThreshold = -30
        };

        // Act
        service.SaveConfig(config);

        // Assert
        File.Exists(configFile).Should().BeTrue();

        // Verify file content
        var savedContent = File.ReadAllText(configFile);
        savedContent.Should().Contain("\"InputDirectory\": \"/input\"");
        savedContent.Should().Contain("\"OutputDirectory\": \"/output\"");
        savedContent.Should().Contain("\"ArchiveDirectory\": \"/archive\"");
        savedContent.Should().Contain("\"EnableHighlights\": true");
        savedContent.Should().Contain("\"EnableScenes\": false");
        savedContent.Should().Contain("\"SilenceThreshold\": -30");
    }

    [Fact]
    public void SaveConfig_WhenCalledMultipleTimes_OverwritesPreviousConfig()
    {
        // Arrange
        var configFile = Path.Combine(_testDirectory, "config.json");
        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = "/some/output",
            ConfigFile = configFile
        });
        var service = new ConfigService(settings);

        var config1 = new ConfigDto
        {
            InputDirectory = "/input1",
            SilenceThreshold = -20
        };

        var config2 = new ConfigDto
        {
            InputDirectory = "/input2",
            SilenceThreshold = -40
        };

        // Act
        service.SaveConfig(config1);
        service.SaveConfig(config2);

        // Assert
        // Verify the second config overwrote the first
        var savedConfig = service.GetConfig();
        savedConfig.Should().NotBeNull();
        savedConfig!.InputDirectory.Should().Be("/input2");
        savedConfig.SilenceThreshold.Should().Be(-40);
    }

    [Fact]
    public void SaveConfig_WhenDirectoryDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        var nonExistentDirectory = Path.Combine(_testDirectory, "nonexistent", "subdir");
        var configFile = Path.Combine(nonExistentDirectory, "config.json");
        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = "/some/output",
            ConfigFile = configFile
        });
        var service = new ConfigService(settings);
        var config = new ConfigDto
        {
            InputDirectory = "/input",
            OutputDirectory = "/output"
        };

        // Act
        Action act = () => service.SaveConfig(config);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SaveConfig_CreatesAtomicWrite()
    {
        // Arrange
        var configFile = Path.Combine(_testDirectory, "config.json");
        var tempFile = configFile + ".tmp";
        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = "/some/output",
            ConfigFile = configFile
        });
        var service = new ConfigService(settings);
        var config = new ConfigDto
        {
            InputDirectory = "/input",
            OutputDirectory = "/output"
        };

        // Act
        service.SaveConfig(config);

        // Assert
        File.Exists(configFile).Should().BeTrue();
        // Temp file should be cleaned up after atomic write
        File.Exists(tempFile).Should().BeFalse();
    }

    [Fact]
    public void GetConfig_AfterSaveConfig_ReturnsSavedConfig()
    {
        // Arrange
        var configFile = Path.Combine(_testDirectory, "config.json");
        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = "/some/output",
            ConfigFile = configFile
        });
        var service = new ConfigService(settings);
        var config = new ConfigDto
        {
            InputDirectory = "/new/input",
            OutputDirectory = "/new/output",
            ArchiveDirectory = "/new/archive",
            EnableHighlights = true,
            EnableScenes = true,
            SilenceThreshold = -25
        };

        // Act
        service.SaveConfig(config);
        var result = service.GetConfig();

        // Assert
        result.Should().NotBeNull();
        result!.InputDirectory.Should().Be("/new/input");
        result.OutputDirectory.Should().Be("/new/output");
        result.ArchiveDirectory.Should().Be("/new/archive");
        result.EnableHighlights.Should().BeTrue();
        result.EnableScenes.Should().BeTrue();
        result.SilenceThreshold.Should().Be(-25);
    }

    [Fact]
    public void GetConfig_WhenConfigFileHasInvalidJson_ThrowsInvalidOperationException()
    {
        // Arrange
        var configFile = Path.Combine(_testDirectory, "invalid.json");
        File.WriteAllText(configFile, "{invalid json content");

        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = "/some/output",
            ConfigFile = configFile
        });
        var service = new ConfigService(settings);

        // Act
        Action act = () => service.GetConfig();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"Failed to deserialize configuration file at '{configFile}'.");
    }
}

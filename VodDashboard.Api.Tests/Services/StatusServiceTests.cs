using FluentAssertions;
using Microsoft.Extensions.Options;
using VodDashboard.Api.Models;
using VodDashboard.Api.Services;

namespace VodDashboard.Api.Tests.Services;

public class StatusServiceTests : IDisposable
{
    private readonly string _testDirectory;

    public StatusServiceTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"StatusServiceTests_{Guid.NewGuid()}");
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
    public void GetStatus_WhenLogFileDoesNotExist_ReturnsNotRunning()
    {
        // Arrange
        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory,
            ConfigFile = "/some/config"
        });
        var service = new StatusService(settings);

        // Act
        var result = service.GetStatus();

        // Assert
        result.IsRunning.Should().BeFalse();
        result.CurrentFile.Should().BeNull();
        result.Stage.Should().BeNull();
        result.Percent.Should().BeNull();
        result.LastUpdated.Should().BeNull();
    }

    [Fact]
    public void GetStatus_WhenLogFileIsEmpty_ReturnsNotRunning()
    {
        // Arrange
        var logPath = Path.Combine(_testDirectory, "pipeline.log");
        File.WriteAllText(logPath, string.Empty);

        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory,
            ConfigFile = "/some/config"
        });
        var service = new StatusService(settings);

        // Act
        var result = service.GetStatus();

        // Assert
        result.IsRunning.Should().BeFalse();
        result.CurrentFile.Should().BeNull();
        result.Stage.Should().BeNull();
        result.Percent.Should().BeNull();
        result.LastUpdated.Should().BeNull();
    }

    [Fact]
    public void GetStatus_WhenLogFileContainsOnlyWhitespace_ReturnsNotRunning()
    {
        // Arrange
        var logPath = Path.Combine(_testDirectory, "pipeline.log");
        File.WriteAllText(logPath, "   \n\n  \n");

        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory,
            ConfigFile = "/some/config"
        });
        var service = new StatusService(settings);

        // Act
        var result = service.GetStatus();

        // Assert
        result.IsRunning.Should().BeFalse();
        result.CurrentFile.Should().BeNull();
        result.Stage.Should().BeNull();
        result.Percent.Should().BeNull();
        result.LastUpdated.Should().BeNull();
    }

    [Fact]
    public void GetStatus_WhenProcessingFile_ReturnsRunningWithFileAndStartingStage()
    {
        // Arrange
        var logPath = Path.Combine(_testDirectory, "pipeline.log");
        File.WriteAllText(logPath, "[2026-01-21 14:33:12] Processing file: myvideo.mp4");

        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory,
            ConfigFile = "/some/config"
        });
        var service = new StatusService(settings);

        // Act
        var result = service.GetStatus();

        // Assert
        result.IsRunning.Should().BeTrue();
        result.CurrentFile.Should().Be("myvideo.mp4");
        result.Stage.Should().Be("Starting");
        result.Percent.Should().BeNull();
        result.LastUpdated.Should().NotBeNull();
        result.LastUpdated.Should().Be(new DateTime(2026, 1, 21, 14, 33, 12));
    }

    [Fact]
    public void GetStatus_WhenStageWithoutPercent_ReturnsRunningWithStage()
    {
        // Arrange
        var logPath = Path.Combine(_testDirectory, "pipeline.log");
        File.WriteAllText(logPath, "[2026-01-21 14:33:15] Stage: silence removal");

        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory,
            ConfigFile = "/some/config"
        });
        var service = new StatusService(settings);

        // Act
        var result = service.GetStatus();

        // Assert
        result.IsRunning.Should().BeTrue();
        result.CurrentFile.Should().BeNull();
        result.Stage.Should().Be("silence removal");
        result.Percent.Should().BeNull();
        result.LastUpdated.Should().NotBeNull();
    }

    [Fact]
    public void GetStatus_WhenStageWithPercent_ReturnsRunningWithStageAndPercent()
    {
        // Arrange
        var logPath = Path.Combine(_testDirectory, "pipeline.log");
        File.WriteAllText(logPath, "[2026-01-21 14:33:15] Stage: silence removal (42%)");

        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory,
            ConfigFile = "/some/config"
        });
        var service = new StatusService(settings);

        // Act
        var result = service.GetStatus();

        // Assert
        result.IsRunning.Should().BeTrue();
        result.CurrentFile.Should().BeNull();
        result.Stage.Should().Be("silence removal");
        result.Percent.Should().Be(42);
        result.LastUpdated.Should().NotBeNull();
    }

    [Fact]
    public void GetStatus_WhenCompletedFile_ReturnsNotRunning()
    {
        // Arrange
        var logPath = Path.Combine(_testDirectory, "pipeline.log");
        File.WriteAllText(logPath, "[2026-01-21 14:33:20] Completed file: myvideo.mp4");

        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory,
            ConfigFile = "/some/config"
        });
        var service = new StatusService(settings);

        // Act
        var result = service.GetStatus();

        // Assert
        result.IsRunning.Should().BeFalse();
        result.CurrentFile.Should().BeNull();
        result.Stage.Should().BeNull();
        result.Percent.Should().BeNull();
        result.LastUpdated.Should().NotBeNull();
    }

    [Fact]
    public void GetStatus_WhenMultipleLinesInLog_ReadsLastNonEmptyLine()
    {
        // Arrange
        var logPath = Path.Combine(_testDirectory, "pipeline.log");
        var logContent = @"[2026-01-21 14:33:12] Processing file: myvideo.mp4
[2026-01-21 14:33:15] Stage: silence removal (25%)
[2026-01-21 14:33:18] Stage: scene detection (75%)

";
        File.WriteAllText(logPath, logContent);

        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory,
            ConfigFile = "/some/config"
        });
        var service = new StatusService(settings);

        // Act
        var result = service.GetStatus();

        // Assert
        result.IsRunning.Should().BeTrue();
        result.Stage.Should().Be("scene detection");
        result.Percent.Should().Be(75);
    }

    [Fact]
    public void GetStatus_WhenUnrecognizedLogFormat_ReturnsNotRunning()
    {
        // Arrange
        var logPath = Path.Combine(_testDirectory, "pipeline.log");
        File.WriteAllText(logPath, "[2026-01-21 14:33:12] Some unrecognized log message");

        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory,
            ConfigFile = "/some/config"
        });
        var service = new StatusService(settings);

        // Act
        var result = service.GetStatus();

        // Assert
        result.IsRunning.Should().BeFalse();
        result.CurrentFile.Should().BeNull();
        result.Stage.Should().BeNull();
        result.Percent.Should().BeNull();
        result.LastUpdated.Should().NotBeNull();
    }

    [Fact]
    public void GetStatus_WhenLogWithoutTimestamp_ParsesCorrectlyButNoTimestamp()
    {
        // Arrange
        var logPath = Path.Combine(_testDirectory, "pipeline.log");
        File.WriteAllText(logPath, "Processing file: myvideo.mp4");

        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory,
            ConfigFile = "/some/config"
        });
        var service = new StatusService(settings);

        // Act
        var result = service.GetStatus();

        // Assert
        result.IsRunning.Should().BeTrue();
        result.CurrentFile.Should().Be("myvideo.mp4");
        result.Stage.Should().Be("Starting");
        result.LastUpdated.Should().BeNull();
    }

    [Fact]
    public void GetStatus_WhenPercentIs100_ParsesCorrectly()
    {
        // Arrange
        var logPath = Path.Combine(_testDirectory, "pipeline.log");
        File.WriteAllText(logPath, "[2026-01-21 14:33:15] Stage: finalization (100%)");

        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory,
            ConfigFile = "/some/config"
        });
        var service = new StatusService(settings);

        // Act
        var result = service.GetStatus();

        // Assert
        result.IsRunning.Should().BeTrue();
        result.Stage.Should().Be("finalization");
        result.Percent.Should().Be(100);
    }

    [Fact]
    public void GetStatus_WhenPercentIs0_ParsesCorrectly()
    {
        // Arrange
        var logPath = Path.Combine(_testDirectory, "pipeline.log");
        File.WriteAllText(logPath, "[2026-01-21 14:33:15] Stage: initialization (0%)");

        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory,
            ConfigFile = "/some/config"
        });
        var service = new StatusService(settings);

        // Act
        var result = service.GetStatus();

        // Assert
        result.IsRunning.Should().BeTrue();
        result.Stage.Should().Be("initialization");
        result.Percent.Should().Be(0);
    }

    [Fact]
    public void GetStatus_WhenStageStartsWithParenthesis_ParsesCorrectly()
    {
        // Arrange
        var logPath = Path.Combine(_testDirectory, "pipeline.log");
        File.WriteAllText(logPath, "[2026-01-21 14:33:15] Stage: (25%)");

        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory,
            ConfigFile = "/some/config"
        });
        var service = new StatusService(settings);

        // Act
        var result = service.GetStatus();

        // Assert
        result.IsRunning.Should().BeTrue();
        result.Stage.Should().Be(string.Empty);
        result.Percent.Should().Be(25);
    }
}

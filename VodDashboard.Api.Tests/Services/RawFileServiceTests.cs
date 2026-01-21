using FluentAssertions;
using Microsoft.Extensions.Options;
using VodDashboard.Api.Models;
using VodDashboard.Api.Services;

namespace VodDashboard.Api.Tests.Services;

public class RawFileServiceTests : IDisposable
{
    private readonly string _testDirectory;

    public RawFileServiceTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"RawFileServiceTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    [Fact]
    public void GetRawFiles_WhenInputDirectoryIsWhitespace_ThrowsInvalidOperationException()
    {
        // Arrange
        var settings = new PipelineSettings
        {
            InputDirectory = "   ",
            OutputDirectory = "/some/output",
            ConfigFile = "/some/config"
        };
        var service = new RawFileService(Options.Create(settings));

        // Act
        Action act = () => service.GetRawFiles();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("PipelineSettings.InputDirectory is not configured.");
    }

    [Fact]
    public void GetRawFiles_WhenInputDirectoryIsEmpty_ThrowsInvalidOperationException()
    {
        // Arrange
        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = string.Empty,
            OutputDirectory = "/some/output",
            ConfigFile = "/some/config"
        });
        var service = new RawFileService(settings);

        // Act
        Action act = () => service.GetRawFiles();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("PipelineSettings.InputDirectory is not configured.");
    }

    [Fact]
    public void GetRawFiles_WhenInputDirectoryIsWhitespace_ThrowsInvalidOperationException()
    {
        // Arrange
        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "   ",
            OutputDirectory = "/some/output",
            ConfigFile = "/some/config"
        });
        var service = new RawFileService(settings);

        // Act
        Action act = () => service.GetRawFiles();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("PipelineSettings.InputDirectory is not configured.");
    }

    [Fact]
    public void GetRawFiles_WhenDirectoryDoesNotExist_ReturnsEmptyCollection()
    {
        // Arrange
        var nonExistentDirectory = Path.Combine(Path.GetTempPath(), $"NonExistent_{Guid.NewGuid()}");
        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = nonExistentDirectory,
            OutputDirectory = "/some/output",
            ConfigFile = "/some/config"
        });
        var service = new RawFileService(settings);

        // Act
        var result = service.GetRawFiles();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetRawFiles_WhenDirectoryIsEmpty_ReturnsEmptyCollection()
    {
        // Arrange
        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = _testDirectory,
            OutputDirectory = "/some/output",
            ConfigFile = "/some/config"
        });
        var service = new RawFileService(settings);

        // Act
        var result = service.GetRawFiles();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetRawFiles_WithValidDirectory_ReturnsMP4FilesOnly()
    {
        // Arrange
        var mp4File = Path.Combine(_testDirectory, "video.mp4");
        var txtFile = Path.Combine(_testDirectory, "readme.txt");
        var aviFile = Path.Combine(_testDirectory, "video.avi");

        File.WriteAllText(mp4File, "test content");
        File.WriteAllText(txtFile, "test content");
        File.WriteAllText(aviFile, "test content");

        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = _testDirectory,
            OutputDirectory = "/some/output",
            ConfigFile = "/some/config"
        });
        var service = new RawFileService(settings);

        // Act
        var result = service.GetRawFiles().ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].FileName.Should().Be("video.mp4");
    }

    [Fact]
    public void GetRawFiles_WithMultipleMP4Files_ReturnsOrderedByCreationTimeDescending()
    {
        // Arrange
        var file1 = Path.Combine(_testDirectory, "video1.mp4");
        var file2 = Path.Combine(_testDirectory, "video2.mp4");
        var file3 = Path.Combine(_testDirectory, "video3.mp4");

        File.WriteAllText(file1, "content");
        File.WriteAllText(file2, "content");
        File.WriteAllText(file3, "content");

        DateTime baseTime = DateTime.UtcNow;
        File.SetCreationTimeUtc(file1, baseTime.AddMinutes(-2)); // Oldest
        File.SetCreationTimeUtc(file2, baseTime.AddMinutes(-1));
        File.SetCreationTimeUtc(file3, baseTime); // Most recent
        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = _testDirectory,
            OutputDirectory = "/some/output",
            ConfigFile = "/some/config"
        });
        var service = new RawFileService(settings);

        // Act
        var result = service.GetRawFiles().ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].FileName.Should().Be("video3.mp4"); // Most recent
        result[1].FileName.Should().Be("video2.mp4");
        result[2].FileName.Should().Be("video1.mp4"); // Oldest
    }

    [Fact]
    public void GetRawFiles_WithSingleMp4File_ReturnsCorrectFileMetadata()
    {
        // Arrange
        var fileName = "test-video.mp4";
        var filePath = Path.Combine(_testDirectory, fileName);
        var content = "test content for video file";
        File.WriteAllText(filePath, content);

        var fileInfo = new FileInfo(filePath);
        var expectedSize = fileInfo.Length;
        var expectedCreationTime = new DateTimeOffset(fileInfo.CreationTimeUtc, TimeSpan.Zero);

        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = _testDirectory,
            OutputDirectory = "/some/output",
            ConfigFile = "/some/config"
        });
        var service = new RawFileService(settings);

        // Act
        var result = service.GetRawFiles().Single();

        // Assert
        result.FileName.Should().Be(fileName);
        result.SizeBytes.Should().Be(expectedSize);
        result.Created.Should().BeCloseTo(expectedCreationTime, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void GetRawFiles_WithNestedDirectories_OnlyReturnsTopLevelFiles()
    {
        // Arrange
        var topLevelFile = Path.Combine(_testDirectory, "top-level.mp4");
        var subDirectory = Path.Combine(_testDirectory, "subdirectory");
        Directory.CreateDirectory(subDirectory);
        var subLevelFile = Path.Combine(subDirectory, "sub-level.mp4");

        File.WriteAllText(topLevelFile, "content");
        File.WriteAllText(subLevelFile, "content");

        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = _testDirectory,
            OutputDirectory = "/some/output",
            ConfigFile = "/some/config"
        });
        var service = new RawFileService(settings);

        // Act
        var result = service.GetRawFiles().ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].FileName.Should().Be("top-level.mp4");
    }

    // Note: Testing UnauthorizedAccessException and IOException wrapping in GetRawFiles
    // is challenging without a file system abstraction layer. These exceptions are caught
    // and wrapped in InvalidOperationException in RawFileService.cs lines 42-49.
    // Integration tests or manual testing should verify this behavior.
}

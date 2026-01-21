using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VodDashboard.Api.DTO;
using VodDashboard.Api.Endpoints;
using VodDashboard.Api.Models;
using VodDashboard.Api.Services;

namespace VodDashboard.Api.Tests.Endpoints;

public class RawEndpointsTests : IDisposable
{
    private readonly List<string> _testDirectories = new();

    public void Dispose()
    {
        foreach (var directory in _testDirectories)
        {
            if (Directory.Exists(directory))
            {
                try
                {
                    Directory.Delete(directory, recursive: true);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Failed to delete test directory '{directory}': {ex}");
                }
            }
        }
    }

    private string CreateTestDirectory(string prefix)
    {
        var directory = Path.Combine(Path.GetTempPath(), $"{prefix}_{Guid.NewGuid()}");
        Directory.CreateDirectory(directory);
        _testDirectories.Add(directory);
        return directory;
    }

    [Fact]
    public void GetRawFiles_WithValidFiles_ReturnsOkWithFiles()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<RawFileService>>();
        var testDirectory = CreateTestDirectory("EndpointTest");
        
        var testFile = Path.Combine(testDirectory, "test.mp4");
        File.WriteAllText(testFile, "test content");

        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = testDirectory,
            OutputDirectory = "/some/output",
            ConfigFile = "/some/config"
        });

        var service = new RawFileService(settings);

        // Act
        IResult result = RawEndpoints.GetRawFilesHandler(service, mockLogger.Object);

        // Assert
        result.Should().BeOfType<Ok<IEnumerable<RawFileDTO>>>();
        var okResult = result.Should().BeOfType<Ok<IEnumerable<RawFileDTO>>>().Subject;
        okResult.Value.Should().HaveCount(1);
    }

    [Fact]
    public void GetRawFiles_WhenConfigurationError_ReturnsProblemResult()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<RawFileService>>();
        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = string.Empty,
            OutputDirectory = "/some/output",
            ConfigFile = "/some/config"
        });

        var service = new RawFileService(settings);

        // Act
        IResult result = RawEndpoints.GetRawFilesHandler(service, mockLogger.Object);

        // Assert
        result.Should().BeOfType<ProblemHttpResult>();
        var problemResult = result.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        problemResult.ProblemDetails.Title.Should().Be("Configuration Error");
    }

    [Fact]
    public void GetRawFiles_WhenNoFilesExist_ReturnsOkWithEmptyList()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<RawFileService>>();
        var testDirectory = CreateTestDirectory("EndpointTestEmpty");

        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = testDirectory,
            OutputDirectory = "/some/output",
            ConfigFile = "/some/config"
        });

        var service = new RawFileService(settings);

        // Act
        IResult result = RawEndpoints.GetRawFilesHandler(service, mockLogger.Object);

        // Assert
        result.Should().BeOfType<Ok<IEnumerable<RawFileDTO>>>();
        var okResult = result.Should().BeOfType<Ok<IEnumerable<RawFileDTO>>>().Subject;
        okResult.Value.Should().BeEmpty();
    }
}

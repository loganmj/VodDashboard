using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VodDashboard.Api.DTO;
using VodDashboard.Api.Models;
using VodDashboard.Api.Services;

namespace VodDashboard.Api.Tests.Endpoints;

public class RawEndpointsTests
{

    [Fact]
    public void GetRawFiles_WithValidFiles_ReturnsOkWithFiles()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<RawFileService>>();
        var testDirectory = Path.Combine(Path.GetTempPath(), $"EndpointTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(testDirectory);

        try
        {
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
            IResult result;
            try
            {
                var files = service.GetRawFiles();
                result = Results.Ok(files);
            }
            catch (InvalidOperationException ex)
            {
                mockLogger.Object.LogError(ex, "Configuration error: {Message}", ex.Message);
                result = Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Configuration Error");
            }
            catch (UnauthorizedAccessException ex)
            {
                mockLogger.Object.LogError(ex, "Access denied while retrieving raw files");
                result = Results.Problem(
                    detail: "Access to the file system was denied.",
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Access Denied");
            }
            catch (IOException ex)
            {
                mockLogger.Object.LogError(ex, "I/O error while retrieving raw files");
                result = Results.Problem(
                    detail: "A file system error occurred while retrieving raw files.",
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "File System Error");
            }
            catch (Exception ex)
            {
                mockLogger.Object.LogError(ex, "Unexpected error while retrieving raw files");
                result = Results.Problem(
                    detail: "An unexpected error occurred while retrieving raw files.",
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Internal Server Error");
            }

            // Assert
            result.Should().BeOfType<Ok<IEnumerable<RawFileDTO>>>();
            var okResult = result as Ok<IEnumerable<RawFileDTO>>;
            okResult!.Value.Should().HaveCount(1);
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, recursive: true);
            }
        }
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
        IResult result;
        try
        {
            var files = service.GetRawFiles();
            result = Results.Ok(files);
        }
        catch (InvalidOperationException ex)
        {
            mockLogger.Object.LogError(ex, "Configuration error: {Message}", ex.Message);
            result = Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Configuration Error");
        }
        catch (UnauthorizedAccessException ex)
        {
            mockLogger.Object.LogError(ex, "Access denied while retrieving raw files");
            result = Results.Problem(
                detail: "Access to the file system was denied.",
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Access Denied");
        }
        catch (IOException ex)
        {
            mockLogger.Object.LogError(ex, "I/O error while retrieving raw files");
            result = Results.Problem(
                detail: "A file system error occurred while retrieving raw files.",
                statusCode: StatusCodes.Status500InternalServerError,
                title: "File System Error");
        }
        catch (Exception ex)
        {
            mockLogger.Object.LogError(ex, "Unexpected error while retrieving raw files");
            result = Results.Problem(
                detail: "An unexpected error occurred while retrieving raw files.",
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Internal Server Error");
        }

        // Assert
        result.Should().BeOfType<ProblemHttpResult>();
        var problemResult = result as ProblemHttpResult;
        problemResult!.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        problemResult.ProblemDetails.Title.Should().Be("Configuration Error");
    }

    [Fact]
    public void GetRawFiles_WhenNoFilesExist_ReturnsOkWithEmptyList()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<RawFileService>>();
        var testDirectory = Path.Combine(Path.GetTempPath(), $"EndpointTestEmpty_{Guid.NewGuid()}");
        Directory.CreateDirectory(testDirectory);

        try
        {
            var settings = Options.Create(new PipelineSettings
            {
                InputDirectory = testDirectory,
                OutputDirectory = "/some/output",
                ConfigFile = "/some/config"
            });

            var service = new RawFileService(settings);

            // Act
            IResult result;
            try
            {
                var files = service.GetRawFiles();
                result = Results.Ok(files);
            }
            catch (InvalidOperationException ex)
            {
                mockLogger.Object.LogError(ex, "Configuration error: {Message}", ex.Message);
                result = Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Configuration Error");
            }
            catch (UnauthorizedAccessException ex)
            {
                mockLogger.Object.LogError(ex, "Access denied while retrieving raw files");
                result = Results.Problem(
                    detail: "Access to the file system was denied.",
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Access Denied");
            }
            catch (IOException ex)
            {
                mockLogger.Object.LogError(ex, "I/O error while retrieving raw files");
                result = Results.Problem(
                    detail: "A file system error occurred while retrieving raw files.",
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "File System Error");
            }
            catch (Exception ex)
            {
                mockLogger.Object.LogError(ex, "Unexpected error while retrieving raw files");
                result = Results.Problem(
                    detail: "An unexpected error occurred while retrieving raw files.",
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Internal Server Error");
            }

            // Assert
            result.Should().BeOfType<Ok<IEnumerable<RawFileDTO>>>();
            var okResult = result as Ok<IEnumerable<RawFileDTO>>;
            okResult!.Value.Should().BeEmpty();
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, recursive: true);
            }
        }
    }
}

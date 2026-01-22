using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using VodDashboard.Api.Controllers;
using VodDashboard.Api.DTO;
using VodDashboard.Api.Models;
using VodDashboard.Api.Services;

namespace VodDashboard.Api.Tests.Controllers;

public class RawControllerTests
{
    private Mock<RawFileService> CreateMockRawFileService()
    {
        var mockOptions = new Mock<IOptions<PipelineSettings>>();
        mockOptions.Setup(o => o.Value).Returns(new PipelineSettings { ConfigFile = "/test/config" });
        var mockConfigService = new Mock<ConfigService>(mockOptions.Object) { CallBase = false };
        return new Mock<RawFileService>(mockConfigService.Object) { CallBase = true };
    }

    [Fact]
    public void GetRawFiles_WhenServiceReturnsFiles_ReturnsOkWithFiles()
    {
        // Arrange
        var expectedFiles = new[]
        {
            new RawFileMetadata("video1.mp4", 1024, DateTimeOffset.UtcNow),
            new RawFileMetadata("video2.mp4", 2048, DateTimeOffset.UtcNow)
        };
        var mockService = CreateMockRawFileService();
        mockService.Setup(s => s.GetRawFiles()).Returns(expectedFiles);
        var controller = new RawController(mockService.Object);

        // Act
        var result = controller.GetRawFiles();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedFiles);
        mockService.Verify(s => s.GetRawFiles(), Times.Once);
    }

    [Fact]
    public void GetRawFiles_WhenServiceReturnsEmptyCollection_ReturnsOkWithEmptyCollection()
    {
        // Arrange
        var mockService = CreateMockRawFileService();
        mockService.Setup(s => s.GetRawFiles()).Returns(Enumerable.Empty<RawFileMetadata>());
        var controller = new RawController(mockService.Object);

        // Act
        var result = controller.GetRawFiles();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(Enumerable.Empty<RawFileMetadata>());
    }

    [Fact]
    public void GetRawFiles_WhenInvalidOperationExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var mockService = CreateMockRawFileService();
        mockService.Setup(s => s.GetRawFiles()).Throws(new InvalidOperationException("Configuration error"));
        var controller = new RawController(mockService.Object);

        // Act
        var result = controller.GetRawFiles();

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
        var problemDetails = objectResult.Value as ProblemDetails;
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Configuration Error");
        problemDetails.Detail.Should().Be("Configuration error");
    }

    [Fact]
    public void GetRawFiles_WhenWrappedUnauthorizedAccessExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var mockService = CreateMockRawFileService();
        var innerException = new UnauthorizedAccessException("Access denied");
        mockService.Setup(s => s.GetRawFiles()).Throws(new InvalidOperationException("Access to the configured input directory is denied.", innerException));
        var controller = new RawController(mockService.Object);

        // Act
        var result = controller.GetRawFiles();

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
        var problemDetails = objectResult.Value as ProblemDetails;
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Configuration Error");
        problemDetails.Detail.Should().Be("Access to the configured input directory is denied.");
    }

    [Fact]
    public void GetRawFiles_WhenWrappedIOExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var mockService = CreateMockRawFileService();
        var innerException = new IOException("I/O error");
        mockService.Setup(s => s.GetRawFiles()).Throws(new InvalidOperationException("An I/O error occurred while enumerating files in the configured input directory.", innerException));
        var controller = new RawController(mockService.Object);

        // Act
        var result = controller.GetRawFiles();

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
        var problemDetails = objectResult.Value as ProblemDetails;
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Configuration Error");
        problemDetails.Detail.Should().Be("An I/O error occurred while enumerating files in the configured input directory.");
    }

    [Fact]
    public void GetRawFiles_WhenUnexpectedExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var mockService = CreateMockRawFileService();
        mockService.Setup(s => s.GetRawFiles()).Throws(new Exception("Unexpected error"));
        var controller = new RawController(mockService.Object);

        // Act
        var result = controller.GetRawFiles();

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
        var problemDetails = objectResult.Value as ProblemDetails;
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Unexpected Error");
        problemDetails.Detail.Should().Be("Unexpected error");
    }
}

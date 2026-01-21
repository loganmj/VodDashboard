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
        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/test/input",
            OutputDirectory = "/test/output",
            ConfigFile = "/test/config"
        });
        return new Mock<RawFileService>(settings) { CallBase = false };
    }

    [Fact]
    public void GetRawFiles_WhenServiceReturnsFiles_ReturnsOkWithFiles()
    {
        // Arrange
        var expectedFiles = new[]
        {
            new RawFileDTO("video1.mp4", 1024, DateTimeOffset.UtcNow),
            new RawFileDTO("video2.mp4", 2048, DateTimeOffset.UtcNow)
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
        mockService.Setup(s => s.GetRawFiles()).Returns(Enumerable.Empty<RawFileDTO>());
        var controller = new RawController(mockService.Object);

        // Act
        var result = controller.GetRawFiles();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(Enumerable.Empty<RawFileDTO>());
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
}

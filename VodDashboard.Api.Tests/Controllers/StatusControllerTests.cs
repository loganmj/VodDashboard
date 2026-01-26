using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using VodDashboard.Api.Controllers;
using VodDashboard.Api.DTO;
using VodDashboard.Api.Models;
using VodDashboard.Api.Services;

namespace VodDashboard.Api.Tests.Controllers;

public class StatusControllerTests
{
    private Mock<StatusService> CreateMockStatusService()
    {
        var mockOptions = new Mock<IOptions<PipelineSettings>>();
        mockOptions.Setup(o => o.Value).Returns(new PipelineSettings 
        { 
            ConfigFile = "/test/config",
            FunctionStatusEndpoint = null
        });
        var mockConfigService = new Mock<ConfigService>(mockOptions.Object) { CallBase = false };
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        return new Mock<StatusService>(mockConfigService.Object, mockHttpClientFactory.Object, mockOptions.Object) { CallBase = true };
    }

    [Fact]
    public async Task GetStatus_WhenServiceReturnsStatus_ReturnsOkWithStatus()
    {
        // Arrange
        var expectedStatus = new JobStatus(
            IsRunning: true,
            JobId: null,
            FileName: null,
            CurrentFile: "video.mp4",
            Stage: "Processing",
            Percent: 50,
            Timestamp: DateTime.UtcNow);
        var mockService = CreateMockStatusService();
        mockService.Setup(s => s.GetStatusAsync()).ReturnsAsync(expectedStatus);
        var controller = new StatusController(mockService.Object);

        // Act
        var result = await controller.GetStatus();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedStatus);
        mockService.Verify(s => s.GetStatusAsync(), Times.Once);
    }

    [Fact]
    public async Task GetStatus_WhenServiceReturnsNotRunning_ReturnsOkWithNotRunningStatus()
    {
        // Arrange
        var expectedStatus = new JobStatus(
            IsRunning: false,
            JobId: null,
            FileName: null,
            CurrentFile: null,
            Stage: null,
            Percent: null,
            Timestamp: null);
        var mockService = CreateMockStatusService();
        mockService.Setup(s => s.GetStatusAsync()).ReturnsAsync(expectedStatus);
        var controller = new StatusController(mockService.Object);

        // Act
        var result = await controller.GetStatus();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedStatus);
    }

    [Fact]
    public async Task GetStatus_WhenInvalidOperationExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var mockService = CreateMockStatusService();
        mockService.Setup(s => s.GetStatusAsync()).ThrowsAsync(new InvalidOperationException("Configuration error"));
        var controller = new StatusController(mockService.Object);

        // Act
        var result = await controller.GetStatus();

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
    public async Task GetStatus_WhenUnexpectedExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var mockService = CreateMockStatusService();
        mockService.Setup(s => s.GetStatusAsync()).ThrowsAsync(new Exception("Unexpected error"));
        var controller = new StatusController(mockService.Object);

        // Act
        var result = await controller.GetStatus();

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
        var problemDetails = objectResult.Value as ProblemDetails;
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Unexpected Error");
        problemDetails.Detail.Should().Be("Unexpected error");
    }

    [Fact]
    public async Task GetStatus_WhenWrappedIOExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var mockService = CreateMockStatusService();
        var innerException = new IOException("File access error");
        mockService.Setup(s => s.GetStatusAsync()).ThrowsAsync(new InvalidOperationException("Unable to read pipeline status log.", innerException));
        var controller = new StatusController(mockService.Object);

        // Act
        var result = await controller.GetStatus();

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
        var problemDetails = objectResult.Value as ProblemDetails;
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Configuration Error");
        problemDetails.Detail.Should().Be("Unable to read pipeline status log.");
    }
}

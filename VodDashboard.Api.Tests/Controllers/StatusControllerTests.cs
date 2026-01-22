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
        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/test/input",
            OutputDirectory = "/test/output",
            ConfigFile = "/test/config"
        });
        return new Mock<StatusService>(settings) { CallBase = true };
    }

    [Fact]
    public void GetStatus_WhenServiceReturnsStatus_ReturnsOkWithStatus()
    {
        // Arrange
        var expectedStatus = new StatusDTO(
            IsRunning: true,
            CurrentFile: "video.mp4",
            Stage: "Processing",
            Percent: 50,
            LastUpdated: DateTime.UtcNow);
        var mockService = CreateMockStatusService();
        mockService.Setup(s => s.GetStatus()).Returns(expectedStatus);
        var controller = new StatusController(mockService.Object);

        // Act
        var result = controller.GetStatus();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedStatus);
        mockService.Verify(s => s.GetStatus(), Times.Once);
    }

    [Fact]
    public void GetStatus_WhenServiceReturnsNotRunning_ReturnsOkWithNotRunningStatus()
    {
        // Arrange
        var expectedStatus = new StatusDTO(
            IsRunning: false,
            CurrentFile: null,
            Stage: null,
            Percent: null,
            LastUpdated: null);
        var mockService = CreateMockStatusService();
        mockService.Setup(s => s.GetStatus()).Returns(expectedStatus);
        var controller = new StatusController(mockService.Object);

        // Act
        var result = controller.GetStatus();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedStatus);
    }

    [Fact]
    public void GetStatus_WhenUnexpectedExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var mockService = CreateMockStatusService();
        mockService.Setup(s => s.GetStatus()).Throws(new Exception("Unexpected error"));
        var controller = new StatusController(mockService.Object);

        // Act
        var result = controller.GetStatus();

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
    public void GetStatus_WhenIOExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var mockService = CreateMockStatusService();
        mockService.Setup(s => s.GetStatus()).Throws(new IOException("File access error"));
        var controller = new StatusController(mockService.Object);

        // Act
        var result = controller.GetStatus();

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
        var problemDetails = objectResult.Value as ProblemDetails;
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Unexpected Error");
        problemDetails.Detail.Should().Be("File access error");
    }
}

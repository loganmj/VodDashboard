using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using VodDashboard.Api.Controllers;
using VodDashboard.Api.DTO;
using VodDashboard.Api.Models;
using VodDashboard.Api.Services;

namespace VodDashboard.Api.Tests.Controllers;

public class JobControllerTests
{
    private Mock<JobService> CreateMockJobService()
    {
        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/test/input",
            OutputDirectory = "/test/output",
            ConfigFile = "/test/config"
        });
        return new Mock<JobService>(settings) { CallBase = true };
    }

    [Fact]
    public async Task GetJobs_WhenServiceReturnsJobs_ReturnsOkWithJobs()
    {
        // Arrange
        var expectedJobs = new[]
        {
            new JobSummaryDTO("job1", true, 5, 10, DateTimeOffset.UtcNow),
            new JobSummaryDTO("job2", false, 3, 7, DateTimeOffset.UtcNow)
        };
        var mockService = CreateMockJobService();
        mockService.Setup(s => s.GetJobsAsync()).ReturnsAsync(expectedJobs);
        var controller = new JobController(mockService.Object);

        // Act
        var result = await controller.GetJobs();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedJobs);
        mockService.Verify(s => s.GetJobsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetJobs_WhenServiceReturnsEmptyCollection_ReturnsOkWithEmptyCollection()
    {
        // Arrange
        var mockService = CreateMockJobService();
        mockService.Setup(s => s.GetJobsAsync()).ReturnsAsync(Enumerable.Empty<JobSummaryDTO>());
        var controller = new JobController(mockService.Object);

        // Act
        var result = await controller.GetJobs();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(Enumerable.Empty<JobSummaryDTO>());
    }

    [Fact]
    public async Task GetJobs_WhenInvalidOperationExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var mockService = CreateMockJobService();
        mockService.Setup(s => s.GetJobsAsync()).ThrowsAsync(new InvalidOperationException("Configuration error"));
        var controller = new JobController(mockService.Object);

        // Act
        var result = await controller.GetJobs();

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
    public async Task GetJobs_WhenWrappedUnauthorizedAccessExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var mockService = CreateMockJobService();
        var innerException = new UnauthorizedAccessException("Access denied");
        mockService.Setup(s => s.GetJobsAsync()).ThrowsAsync(new InvalidOperationException("Access to the configured output directory is denied.", innerException));
        var controller = new JobController(mockService.Object);

        // Act
        var result = await controller.GetJobs();

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
        var problemDetails = objectResult.Value as ProblemDetails;
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Configuration Error");
        problemDetails.Detail.Should().Be("Access to the configured output directory is denied.");
    }

    [Fact]
    public async Task GetJobs_WhenWrappedIOExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var mockService = CreateMockJobService();
        var innerException = new IOException("I/O error");
        mockService.Setup(s => s.GetJobsAsync()).ThrowsAsync(new InvalidOperationException("An I/O error occurred while enumerating directories in the configured output directory.", innerException));
        var controller = new JobController(mockService.Object);

        // Act
        var result = await controller.GetJobs();

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
        var problemDetails = objectResult.Value as ProblemDetails;
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Configuration Error");
        problemDetails.Detail.Should().Be("An I/O error occurred while enumerating directories in the configured output directory.");
    }

    [Fact]
    public async Task GetJobs_WhenUnexpectedExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var mockService = CreateMockJobService();
        mockService.Setup(s => s.GetJobsAsync()).ThrowsAsync(new Exception("Unexpected error"));
        var controller = new JobController(mockService.Object);

        // Act
        var result = await controller.GetJobs();

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

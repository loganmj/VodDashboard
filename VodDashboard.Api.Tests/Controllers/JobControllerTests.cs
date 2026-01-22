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
            new JobData("job1", true, 5, 10, DateTimeOffset.UtcNow),
            new JobData("job2", false, 3, 7, DateTimeOffset.UtcNow)
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
        mockService.Setup(s => s.GetJobsAsync()).ReturnsAsync(Enumerable.Empty<JobData>());
        var controller = new JobController(mockService.Object);

        // Act
        var result = await controller.GetJobs();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(Enumerable.Empty<JobData>());
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

    [Fact]
    public void GetJobDetail_WhenJobExists_ReturnsOkWithJobDetail()
    {
        // Arrange
        var jobId = "test-job";
        var expectedJob = new JobData(
            jobId,
            true,
            2,
            1,
            DateTimeOffset.UtcNow
        );
        var mockService = CreateMockJobService();
        mockService.Setup(s => s.GetJobDetail(jobId)).Returns(expectedJob);
        var controller = new JobController(mockService.Object);

        // Act
        var result = controller.GetJobDetail(jobId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedJob);
        mockService.Verify(s => s.GetJobDetail(jobId), Times.Once);
    }

    [Fact]
    public void GetJobDetail_WhenJobDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var jobId = "nonexistent-job";
        var mockService = CreateMockJobService();
        mockService.Setup(s => s.GetJobDetail(jobId)).Returns((JobData?)null);
        var controller = new JobController(mockService.Object);

        // Act
        var result = controller.GetJobDetail(jobId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        mockService.Verify(s => s.GetJobDetail(jobId), Times.Once);
    }

    [Fact]
    public void GetJobDetail_WhenInvalidOperationExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var jobId = "test-job";
        var mockService = CreateMockJobService();
        mockService.Setup(s => s.GetJobDetail(jobId)).Throws(new InvalidOperationException("Configuration error"));
        var controller = new JobController(mockService.Object);

        // Act
        var result = controller.GetJobDetail(jobId);

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
    public void GetJobDetail_WhenUnexpectedExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var jobId = "test-job";
        var mockService = CreateMockJobService();
        mockService.Setup(s => s.GetJobDetail(jobId)).Throws(new Exception("Unexpected error"));
        var controller = new JobController(mockService.Object);

        // Act
        var result = controller.GetJobDetail(jobId);

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
    public void GetJobDetail_WhenIdIsNull_ReturnsBadRequest()
    {
        // Arrange
        var mockService = CreateMockJobService();
        var controller = new JobController(mockService.Object);

        // Act
        var result = controller.GetJobDetail(null!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Job id must be provided.");
    }

    [Fact]
    public void GetJobDetail_WhenIdIsEmpty_ReturnsBadRequest()
    {
        // Arrange
        var mockService = CreateMockJobService();
        var controller = new JobController(mockService.Object);

        // Act
        var result = controller.GetJobDetail("");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Job id must be provided.");
    }

    [Fact]
    public void GetJobDetail_WhenIdIsWhitespace_ReturnsBadRequest()
    {
        // Arrange
        var mockService = CreateMockJobService();
        var controller = new JobController(mockService.Object);

        // Act
        var result = controller.GetJobDetail("   ");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Job id must be provided.");
    }

    [Fact]
    public void GetJobDetail_WhenIdContainsParentDirectoryReference_ReturnsBadRequest()
    {
        // Arrange
        var mockService = CreateMockJobService();
        var controller = new JobController(mockService.Object);

        // Act
        var result = controller.GetJobDetail("../parent");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Invalid job id.");
    }

    [Fact]
    public void GetJobDetail_WhenIdContainsPathTraversal_ReturnsBadRequest()
    {
        // Arrange
        var mockService = CreateMockJobService();
        var controller = new JobController(mockService.Object);

        // Act
        var result = controller.GetJobDetail("../../etc/passwd");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Invalid job id.");
    }

    [Fact]
    public void GetJobDetail_WhenIdContainsDirectorySeparator_ReturnsBadRequest()
    {
        // Arrange
        var mockService = CreateMockJobService();
        var controller = new JobController(mockService.Object);

        // Act
        var result = controller.GetJobDetail("folder/subdir");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Invalid job id.");
    }

    [Fact]
    public void GetJobDetail_WhenIdContainsForwardSlash_ReturnsBadRequest()
    {
        // Arrange
        var mockService = CreateMockJobService();
        var controller = new JobController(mockService.Object);

        // Act - Forward slash is always a directory separator
        var result = controller.GetJobDetail("job/test");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Invalid job id.");
    }

    [Fact]
    public void GetJobLog_WhenLogExists_ReturnsContentResultWithPlainText()
    {
        // Arrange
        var jobId = "test-job";
        var logContent = "Test log content\nLine 2";
        var mockService = CreateMockJobService();
        mockService.Setup(s => s.GetJobLog(jobId)).Returns(logContent);
        var controller = new JobController(mockService.Object);

        // Act
        var result = controller.GetJobLog(jobId);

        // Assert
        result.Should().BeOfType<ContentResult>();
        var contentResult = result as ContentResult;
        contentResult!.Content.Should().Be(logContent);
        contentResult.ContentType.Should().Be("text/plain");
        mockService.Verify(s => s.GetJobLog(jobId), Times.Once);
    }

    [Fact]
    public void GetJobLog_WhenLogDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var jobId = "nonexistent-job";
        var mockService = CreateMockJobService();
        mockService.Setup(s => s.GetJobLog(jobId)).Returns((string?)null);
        var controller = new JobController(mockService.Object);

        // Act
        var result = controller.GetJobLog(jobId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        mockService.Verify(s => s.GetJobLog(jobId), Times.Once);
    }

    [Fact]
    public void GetJobLog_WhenInvalidOperationExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var jobId = "test-job";
        var mockService = CreateMockJobService();
        mockService.Setup(s => s.GetJobLog(jobId)).Throws(new InvalidOperationException("Configuration error"));
        var controller = new JobController(mockService.Object);

        // Act
        var result = controller.GetJobLog(jobId);

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
    public void GetJobLog_WhenUnexpectedExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var jobId = "test-job";
        var mockService = CreateMockJobService();
        mockService.Setup(s => s.GetJobLog(jobId)).Throws(new Exception("Unexpected error"));
        var controller = new JobController(mockService.Object);

        // Act
        var result = controller.GetJobLog(jobId);

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
    public void GetJobLog_WhenIdIsNull_ReturnsBadRequest()
    {
        // Arrange
        var mockService = CreateMockJobService();
        var controller = new JobController(mockService.Object);

        // Act
        var result = controller.GetJobLog(null!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Job id must be provided.");
    }

    [Fact]
    public void GetJobLog_WhenIdIsEmpty_ReturnsBadRequest()
    {
        // Arrange
        var mockService = CreateMockJobService();
        var controller = new JobController(mockService.Object);

        // Act
        var result = controller.GetJobLog("");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Job id must be provided.");
    }

    [Fact]
    public void GetJobLog_WhenIdIsWhitespace_ReturnsBadRequest()
    {
        // Arrange
        var mockService = CreateMockJobService();
        var controller = new JobController(mockService.Object);

        // Act
        var result = controller.GetJobLog("   ");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Job id must be provided.");
    }

    [Fact]
    public void GetJobLog_WhenIdContainsParentDirectoryReference_ReturnsBadRequest()
    {
        // Arrange
        var mockService = CreateMockJobService();
        var controller = new JobController(mockService.Object);

        // Act
        var result = controller.GetJobLog("../parent");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Invalid job id.");
    }

    [Fact]
    public void GetJobLog_WhenIdContainsPathTraversal_ReturnsBadRequest()
    {
        // Arrange
        var mockService = CreateMockJobService();
        var controller = new JobController(mockService.Object);

        // Act
        var result = controller.GetJobLog("../../etc/passwd");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Invalid job id.");
    }

    [Fact]
    public void GetJobLog_WhenIdContainsDirectorySeparator_ReturnsBadRequest()
    {
        // Arrange
        var mockService = CreateMockJobService();
        var controller = new JobController(mockService.Object);

        // Act
        var result = controller.GetJobLog("folder/subdir");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Invalid job id.");
    }

    [Fact]
    public void GetJobLog_WhenIdContainsForwardSlash_ReturnsBadRequest()
    {
        // Arrange
        var mockService = CreateMockJobService();
        var controller = new JobController(mockService.Object);

        // Act
        var result = controller.GetJobLog("job/test");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Invalid job id.");
    }
}

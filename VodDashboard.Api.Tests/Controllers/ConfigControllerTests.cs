using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using VodDashboard.Api.Controllers;
using VodDashboard.Api.DTO;
using VodDashboard.Api.Models;
using VodDashboard.Api.Services;

namespace VodDashboard.Api.Tests.Controllers;

public class ConfigControllerTests
{
    private Mock<ConfigService> CreateMockConfigService()
    {
        var settings = Options.Create(new PipelineSettings
        {
            ConfigFile = "/test/config.json"
        });
        return new Mock<ConfigService>(settings) { CallBase = true };
    }

    [Fact]
    public void GetConfig_WhenConfigExists_ReturnsOkWithConfig()
    {
        // Arrange
        var expectedConfig = new PipelineConfig
        {
            InputDirectory = "/input",
            OutputDirectory = "/output",
            ArchiveDirectory = "/archive",
            EnableHighlights = true,
            EnableScenes = false,
            SilenceThreshold = -30
        };
        var mockService = CreateMockConfigService();
        mockService.Setup(s => s.GetConfig()).Returns(expectedConfig);
        var controller = new ConfigController(mockService.Object);

        // Act
        var result = controller.GetConfig();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedConfig);
        mockService.Verify(s => s.GetConfig(), Times.Once);
    }

    [Fact]
    public void GetConfig_WhenConfigDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var mockService = CreateMockConfigService();
        mockService.Setup(s => s.GetConfig()).Returns((PipelineConfig?)null);
        var controller = new ConfigController(mockService.Object);

        // Act
        var result = controller.GetConfig();

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        mockService.Verify(s => s.GetConfig(), Times.Once);
    }

    [Fact]
    public void SaveConfig_WhenSaveSucceeds_ReturnsOk()
    {
        // Arrange
        var config = new PipelineConfig
        {
            InputDirectory = "/input",
            OutputDirectory = "/output",
            ArchiveDirectory = "/archive",
            EnableHighlights = true,
            EnableScenes = false,
            SilenceThreshold = -30
        };
        var mockService = CreateMockConfigService();
        mockService.Setup(s => s.SaveConfig(It.IsAny<PipelineConfig>()));
        var controller = new ConfigController(mockService.Object);

        // Act
        var result = controller.SaveConfig(config);

        // Assert
        result.Should().BeOfType<OkResult>();
        mockService.Verify(s => s.SaveConfig(It.Is<PipelineConfig>(c =>
            c.InputDirectory == "/input" &&
            c.OutputDirectory == "/output" &&
            c.ArchiveDirectory == "/archive" &&
            c.EnableHighlights &&
            !c.EnableScenes &&
            c.SilenceThreshold == -30
        )), Times.Once);
    }

    [Fact]
    public void SaveConfig_WithMinimalConfig_CallsServiceWithCorrectData()
    {
        // Arrange
        var config = new PipelineConfig
        {
            InputDirectory = "",
            OutputDirectory = "",
            ArchiveDirectory = "",
            EnableHighlights = false,
            EnableScenes = false,
            SilenceThreshold = 0
        };
        var mockService = CreateMockConfigService();
        mockService.Setup(s => s.SaveConfig(It.IsAny<PipelineConfig>()));
        var controller = new ConfigController(mockService.Object);

        // Act
        var result = controller.SaveConfig(config);

        // Assert
        result.Should().BeOfType<OkResult>();
        mockService.Verify(s => s.SaveConfig(It.Is<PipelineConfig>(c =>
            c.InputDirectory == "" &&
            c.OutputDirectory == "" &&
            c.ArchiveDirectory == "" &&
            !c.EnableHighlights &&
            !c.EnableScenes &&
            c.SilenceThreshold == 0
        )), Times.Once);
    }

    [Fact]
    public void SaveConfig_WithCompleteConfig_CallsServiceWithCorrectData()
    {
        // Arrange
        var config = new PipelineConfig
        {
            InputDirectory = "/complete/input",
            OutputDirectory = "/complete/output",
            ArchiveDirectory = "/complete/archive",
            EnableHighlights = true,
            EnableScenes = true,
            SilenceThreshold = -40
        };
        var mockService = CreateMockConfigService();
        mockService.Setup(s => s.SaveConfig(It.IsAny<PipelineConfig>()));
        var controller = new ConfigController(mockService.Object);

        // Act
        var result = controller.SaveConfig(config);

        // Assert
        result.Should().BeOfType<OkResult>();
        mockService.Verify(s => s.SaveConfig(It.Is<PipelineConfig>(c =>
            c.InputDirectory == "/complete/input" &&
            c.OutputDirectory == "/complete/output" &&
            c.ArchiveDirectory == "/complete/archive" &&
            c.EnableHighlights &&
            c.EnableScenes &&
            c.SilenceThreshold == -40
        )), Times.Once);
    }

    [Fact]
    public void SaveConfig_WhenInvalidOperationExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var config = new PipelineConfig
        {
            InputDirectory = "/input",
            OutputDirectory = "/output"
        };
        var mockService = CreateMockConfigService();
        mockService.Setup(s => s.SaveConfig(It.IsAny<PipelineConfig>())).Throws(new InvalidOperationException("Configuration error"));
        var controller = new ConfigController(mockService.Object);

        // Act
        var result = controller.SaveConfig(config);

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
    public void SaveConfig_WhenUnexpectedExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var config = new PipelineConfig
        {
            InputDirectory = "/input",
            OutputDirectory = "/output"
        };
        var mockService = CreateMockConfigService();
        mockService.Setup(s => s.SaveConfig(It.IsAny<PipelineConfig>())).Throws(new Exception("Unexpected error"));
        var controller = new ConfigController(mockService.Object);

        // Act
        var result = controller.SaveConfig(config);

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
    public void GetConfig_WhenInvalidOperationExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var mockService = CreateMockConfigService();
        mockService.Setup(s => s.GetConfig()).Throws(new InvalidOperationException("Configuration error"));
        var controller = new ConfigController(mockService.Object);

        // Act
        var result = controller.GetConfig();

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
    public void GetConfig_WhenUnexpectedExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var mockService = CreateMockConfigService();
        mockService.Setup(s => s.GetConfig()).Throws(new Exception("Unexpected error"));
        var controller = new ConfigController(mockService.Object);

        // Act
        var result = controller.GetConfig();

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

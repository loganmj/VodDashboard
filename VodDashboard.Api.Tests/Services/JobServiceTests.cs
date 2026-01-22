using FluentAssertions;
using Moq;
using VodDashboard.Api.DTO;
using Microsoft.Extensions.Options;
using VodDashboard.Api.Models;
using VodDashboard.Api.Services;

namespace VodDashboard.Api.Tests.Services;

public class JobServiceTests : IDisposable
{
    private readonly string _testDirectory;

    public JobServiceTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"JobServiceTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            try
            {
                Directory.Delete(_testDirectory, recursive: true);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to delete test directory '{_testDirectory}': {ex}");
            }
        }
    }

    private static Mock<ConfigService> CreateMockConfigService(PipelineConfig config)
    {
        var mockOptions = new Mock<IOptions<PipelineSettings>>();
        mockOptions.Setup(o => o.Value).Returns(new PipelineSettings { ConfigFile = "/dummy/config.json" });
        var mockConfigService = new Mock<ConfigService>(mockOptions.Object);
        mockConfigService.Setup(c => c.GetCachedConfig()).Returns(config);
        return mockConfigService;
    }

    [Fact]
    public async Task GetJobs_WhenOutputDirectoryIsEmpty_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = string.Empty
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        Func<Task> act = async () => await service.GetJobsAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("PipelineConfig.OutputDirectory is not configured.");
    }

    [Fact]
    public async Task GetJobs_WhenOutputDirectoryIsWhitespace_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = "   "
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        Func<Task> act = async () => await service.GetJobsAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("PipelineConfig.OutputDirectory is not configured.");
    }

    [Fact]
    public async Task GetJobs_WhenDirectoryDoesNotExist_ReturnsEmptyCollection()
    {
        // Arrange
        var nonExistentDirectory = Path.Combine(Path.GetTempPath(), $"NonExistent_{Guid.NewGuid()}");
        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = nonExistentDirectory
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        var result = await service.GetJobsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetJobs_WhenDirectoryIsEmpty_ReturnsEmptyCollection()
    {
        // Arrange
        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        var result = await service.GetJobsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetJobs_WhenValidDirectory_ReturnsJobSummaries()
    {
        // Arrange
        var jobDir1 = Path.Combine(_testDirectory, "job1");
        var jobDir2 = Path.Combine(_testDirectory, "job2");
        Directory.CreateDirectory(jobDir1);
        Directory.CreateDirectory(jobDir2);

        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        var result = (await service.GetJobsAsync()).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(j => j.Id == "job1");
        result.Should().Contain(j => j.Id == "job2");
    }

    [Fact]
    public async Task GetJobs_WhenMultipleJobDirectories_ReturnsOrderedByCreationTimeDescending()
    {
        // Arrange
        var jobDir1 = Path.Combine(_testDirectory, "job1");
        var jobDir2 = Path.Combine(_testDirectory, "job2");
        var jobDir3 = Path.Combine(_testDirectory, "job3");

        Directory.CreateDirectory(jobDir1);
        Directory.CreateDirectory(jobDir2);
        Directory.CreateDirectory(jobDir3);

        DateTime baseTime = DateTime.UtcNow;
        Directory.SetCreationTimeUtc(jobDir1, baseTime.AddMinutes(-2)); // Oldest
        Directory.SetCreationTimeUtc(jobDir2, baseTime.AddMinutes(-1));
        Directory.SetCreationTimeUtc(jobDir3, baseTime); // Most recent

        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        var result = (await service.GetJobsAsync()).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Id.Should().Be("job3"); // Most recent
        result[1].Id.Should().Be("job2");
        result[2].Id.Should().Be("job1"); // Oldest
    }

    [Fact]
    public async Task GetJobs_WhenJobHasCleanVideo_HasCleanVideoIsTrue()
    {
        // Arrange
        var jobDir = Path.Combine(_testDirectory, "job-with-clean");
        Directory.CreateDirectory(jobDir);
        var cleanPath = Path.Combine(jobDir, "clean.mp4");
        File.WriteAllText(cleanPath, "clean video content");

        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        var result = (await service.GetJobsAsync()).Single();

        // Assert
        result.HasCleanVideo.Should().BeTrue();
    }

    [Fact]
    public async Task GetJobs_WhenJobHasNoCleanVideo_HasCleanVideoIsFalse()
    {
        // Arrange
        var jobDir = Path.Combine(_testDirectory, "job-without-clean");
        Directory.CreateDirectory(jobDir);

        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        var result = (await service.GetJobsAsync()).Single();

        // Assert
        result.HasCleanVideo.Should().BeFalse();
    }

    [Fact]
    public async Task GetJobs_WhenJobHasHighlights_CountsHighlightsCorrectly()
    {
        // Arrange
        var jobDir = Path.Combine(_testDirectory, "job-with-highlights");
        var highlightsDir = Path.Combine(jobDir, "highlights");
        Directory.CreateDirectory(highlightsDir);

        File.WriteAllText(Path.Combine(highlightsDir, "highlight1.mp4"), "content");
        File.WriteAllText(Path.Combine(highlightsDir, "highlight2.mp4"), "content");
        File.WriteAllText(Path.Combine(highlightsDir, "highlight3.mp4"), "content");
        File.WriteAllText(Path.Combine(highlightsDir, "readme.txt"), "content"); // Non-mp4

        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        var result = (await service.GetJobsAsync()).Single();

        // Assert
        result.HighlightCount.Should().Be(3);
    }

    [Fact]
    public async Task GetJobs_WhenJobHasNoHighlights_HighlightCountIsZero()
    {
        // Arrange
        var jobDir = Path.Combine(_testDirectory, "job-without-highlights");
        Directory.CreateDirectory(jobDir);

        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        var result = (await service.GetJobsAsync()).Single();

        // Assert
        result.HighlightCount.Should().Be(0);
    }

    [Fact]
    public async Task GetJobs_WhenJobHasScenes_CountsScenesCorrectly()
    {
        // Arrange
        var jobDir = Path.Combine(_testDirectory, "job-with-scenes");
        var scenesDir = Path.Combine(jobDir, "scenes");
        Directory.CreateDirectory(scenesDir);

        File.WriteAllText(Path.Combine(scenesDir, "scene1.csv"), "content");
        File.WriteAllText(Path.Combine(scenesDir, "scene2.csv"), "content");
        File.WriteAllText(Path.Combine(scenesDir, "readme.txt"), "content"); // Non-csv

        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        var result = (await service.GetJobsAsync()).Single();

        // Assert
        result.SceneCount.Should().Be(2);
    }

    [Fact]
    public async Task GetJobs_WhenJobHasNoScenes_SceneCountIsZero()
    {
        // Arrange
        var jobDir = Path.Combine(_testDirectory, "job-without-scenes");
        Directory.CreateDirectory(jobDir);

        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        var result = (await service.GetJobsAsync()).Single();

        // Assert
        result.SceneCount.Should().Be(0);
    }

    [Fact]
    public async Task GetJobs_WhenJobHasAllComponents_ReturnsCorrectJobSummary()
    {
        // Arrange
        var jobId = "complete-job";
        var jobDir = Path.Combine(_testDirectory, jobId);
        Directory.CreateDirectory(jobDir);

        // Create clean video
        var cleanPath = Path.Combine(jobDir, "clean.mp4");
        File.WriteAllText(cleanPath, "clean video content");

        // Create highlights
        var highlightsDir = Path.Combine(jobDir, "highlights");
        Directory.CreateDirectory(highlightsDir);
        File.WriteAllText(Path.Combine(highlightsDir, "h1.mp4"), "content");
        File.WriteAllText(Path.Combine(highlightsDir, "h2.mp4"), "content");

        // Create scenes
        var scenesDir = Path.Combine(jobDir, "scenes");
        Directory.CreateDirectory(scenesDir);
        File.WriteAllText(Path.Combine(scenesDir, "s1.csv"), "content");
        File.WriteAllText(Path.Combine(scenesDir, "s2.csv"), "content");
        File.WriteAllText(Path.Combine(scenesDir, "s3.csv"), "content");

        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        var result = (await service.GetJobsAsync()).Single();

        // Assert
        result.Id.Should().Be(jobId);
        result.HasCleanVideo.Should().BeTrue();
        result.HighlightCount.Should().Be(2);
        result.SceneCount.Should().Be(3);
        DirectoryInfo jobDirInfo = new DirectoryInfo(jobDir);
        DateTimeOffset expectedCreated = new DateTimeOffset(jobDirInfo.CreationTimeUtc);
        result.Created.Should().BeCloseTo(expectedCreated, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetJobs_WhenJobDirectoryHasFiles_OnlyCountsInSubdirectories()
    {
        // Arrange
        var jobDir = Path.Combine(_testDirectory, "job-with-files");
        Directory.CreateDirectory(jobDir);

        // Files in root should not affect counts
        File.WriteAllText(Path.Combine(jobDir, "random.mp4"), "content");
        File.WriteAllText(Path.Combine(jobDir, "data.csv"), "content");

        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        var result = (await service.GetJobsAsync()).Single();

        // Assert
        result.HighlightCount.Should().Be(0);
        result.SceneCount.Should().Be(0);
        result.HasCleanVideo.Should().BeFalse(); // random.mp4 is not named clean.mp4
    }

    [Fact]
    public void GetJobDetail_WhenOutputDirectoryIsEmpty_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = string.Empty
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        Action act = () => service.GetJobDetail("job1");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("PipelineConfig.OutputDirectory is not configured.");
    }

    [Fact]
    public void GetJobDetail_WhenOutputDirectoryIsWhitespace_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = "   "
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        Action act = () => service.GetJobDetail("job1");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("PipelineConfig.OutputDirectory is not configured.");
    }

    [Fact]
    public void GetJobDetail_WhenJobDoesNotExist_ReturnsNull()
    {
        // Arrange
        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        var result = service.GetJobDetail("nonexistent-job");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetJobDetail_WhenIdIsNull_ReturnsNull()
    {
        // Arrange
        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        var result = service.GetJobDetail(null!);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetJobDetail_WhenIdIsEmpty_ReturnsNull()
    {
        // Arrange
        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        var result = service.GetJobDetail("");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetJobDetail_WhenIdIsWhitespace_ReturnsNull()
    {
        // Arrange
        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        var result = service.GetJobDetail("   ");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetJobDetail_WhenIdContainsParentDirectoryReference_ReturnsNull()
    {
        // Arrange
        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        var result = service.GetJobDetail("../parent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetJobDetail_WhenIdContainsPathTraversal_ReturnsNull()
    {
        // Arrange
        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        var result = service.GetJobDetail("../../etc/passwd");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetJobDetail_WhenIdContainsDirectorySeparator_ReturnsNull()
    {
        // Arrange
        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        var result = service.GetJobDetail("folder/subdir");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetJobDetail_WhenIdContainsForwardSlash_ReturnsNull()
    {
        // Arrange
        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory
        });
        var service = new JobService(mockConfigService.Object);

        // Act - Forward slash is always a directory separator
        var result = service.GetJobDetail("job/test");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetJobDetail_WhenJobExists_ReturnsJobDetail()
    {
        // Arrange
        var jobId = "test-job";
        var jobDir = Path.Combine(_testDirectory, jobId);
        Directory.CreateDirectory(jobDir);

        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        var result = service.GetJobDetail(jobId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(jobId);
        result.HasCleanVideo.Should().BeFalse();
        result.HighlightCount.Should().Be(0);
        result.SceneCount.Should().Be(0);
    }

    [Fact]
    public void GetJobDetail_WhenJobHasCleanVideo_HasCleanVideoIsTrue()
    {
        // Arrange
        var jobId = "job-with-clean";
        var jobDir = Path.Combine(_testDirectory, jobId);
        Directory.CreateDirectory(jobDir);
        var cleanPath = Path.Combine(jobDir, "clean.mp4");
        File.WriteAllText(cleanPath, "clean video content");

        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        var result = service.GetJobDetail(jobId);

        // Assert
        result.Should().NotBeNull();
        result!.HasCleanVideo.Should().BeTrue();
    }

    [Fact]
    public void GetJobDetail_WhenJobHasHighlights_ReturnsHighlightFileNames()
    {
        // Arrange
        var jobId = "job-with-highlights";
        var jobDir = Path.Combine(_testDirectory, jobId);
        var highlightsDir = Path.Combine(jobDir, "highlights");
        Directory.CreateDirectory(highlightsDir);

        File.WriteAllText(Path.Combine(highlightsDir, "highlight1.mp4"), "content");
        File.WriteAllText(Path.Combine(highlightsDir, "highlight2.mp4"), "content");
        File.WriteAllText(Path.Combine(highlightsDir, "highlight3.mp4"), "content");
        File.WriteAllText(Path.Combine(highlightsDir, "readme.txt"), "content"); // Non-mp4

        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        var result = service.GetJobDetail(jobId);

        // Assert
        result.Should().NotBeNull();
        result!.HighlightCount.Should().Be(3);
    }

    [Fact]
    public void GetJobDetail_WhenJobHasScenes_ReturnsSceneFileNames()
    {
        // Arrange
        var jobId = "job-with-scenes";
        var jobDir = Path.Combine(_testDirectory, jobId);
        var scenesDir = Path.Combine(jobDir, "scenes");
        Directory.CreateDirectory(scenesDir);

        File.WriteAllText(Path.Combine(scenesDir, "scene1.csv"), "content");
        File.WriteAllText(Path.Combine(scenesDir, "scene2.csv"), "content");
        File.WriteAllText(Path.Combine(scenesDir, "readme.txt"), "content"); // Non-csv

        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        var result = service.GetJobDetail(jobId);

        // Assert
        result.Should().NotBeNull();
        result!.SceneCount.Should().Be(2);
    }

    [Fact]
    public void GetJobDetail_WhenJobHasAllComponents_ReturnsCompleteJobDetail()
    {
        // Arrange
        var jobId = "complete-job";
        var jobDir = Path.Combine(_testDirectory, jobId);
        Directory.CreateDirectory(jobDir);

        // Create clean video
        var cleanPath = Path.Combine(jobDir, "clean.mp4");
        File.WriteAllText(cleanPath, "clean video content");

        // Create highlights
        var highlightsDir = Path.Combine(jobDir, "highlights");
        Directory.CreateDirectory(highlightsDir);
        File.WriteAllText(Path.Combine(highlightsDir, "h1.mp4"), "content");
        File.WriteAllText(Path.Combine(highlightsDir, "h2.mp4"), "content");

        // Create scenes
        var scenesDir = Path.Combine(jobDir, "scenes");
        Directory.CreateDirectory(scenesDir);
        File.WriteAllText(Path.Combine(scenesDir, "s1.csv"), "content");
        File.WriteAllText(Path.Combine(scenesDir, "s2.csv"), "content");
        File.WriteAllText(Path.Combine(scenesDir, "s3.csv"), "content");

        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        var result = service.GetJobDetail(jobId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(jobId);
        result.HasCleanVideo.Should().BeTrue();
        result.HighlightCount.Should().Be(2);
        result.SceneCount.Should().Be(3);
        DirectoryInfo jobDirInfo = new(jobDir);
        DateTimeOffset expectedCreated = new(jobDirInfo.CreationTimeUtc);
        result.Created.Should().BeCloseTo(expectedCreated, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GetJobLog_WhenOutputDirectoryIsEmpty_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = string.Empty
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        Action act = () => service.GetJobLog("job1");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("PipelineConfig.OutputDirectory is not configured.");
    }

    [Fact]
    public void GetJobLog_WhenOutputDirectoryIsWhitespace_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = "   "
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        Action act = () => service.GetJobLog("job1");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("PipelineConfig.OutputDirectory is not configured.");
    }

    [Fact]
    public void GetJobLog_WhenJobDoesNotExist_ReturnsNull()
    {
        // Arrange
        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        var result = service.GetJobLog("nonexistent-job");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetJobLog_WhenIdIsNull_ReturnsNull()
    {
        // Arrange
        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        var result = service.GetJobLog(null!);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetJobLog_WhenIdIsEmpty_ReturnsNull()
    {
        // Arrange
        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        var result = service.GetJobLog("");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetJobLog_WhenIdIsWhitespace_ReturnsNull()
    {
        // Arrange
        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        var result = service.GetJobLog("   ");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetJobLog_WhenIdContainsParentDirectoryReference_ReturnsNull()
    {
        // Arrange
        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        var result = service.GetJobLog("../parent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetJobLog_WhenIdContainsPathTraversal_ReturnsNull()
    {
        // Arrange
        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        var result = service.GetJobLog("../../etc/passwd");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetJobLog_WhenIdContainsDirectorySeparator_ReturnsNull()
    {
        // Arrange
        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        var result = service.GetJobLog("folder/subdir");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetJobLog_WhenIdContainsForwardSlash_ReturnsNull()
    {
        // Arrange
        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        var result = service.GetJobLog("job/test");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetJobLog_WhenLogFileDoesNotExist_ReturnsNull()
    {
        // Arrange
        var jobId = "job-without-log";
        var jobDir = Path.Combine(_testDirectory, jobId);
        Directory.CreateDirectory(jobDir);

        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        var result = service.GetJobLog(jobId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetJobLog_WhenLogFileExists_ReturnsLogContent()
    {
        // Arrange
        var jobId = "job-with-log";
        var jobDir = Path.Combine(_testDirectory, jobId);
        Directory.CreateDirectory(jobDir);
        var logPath = Path.Combine(jobDir, "log.txt");
        var logContent = "Test log content\nLine 2\nLine 3";
        File.WriteAllText(logPath, logContent);

        var mockConfigService = CreateMockConfigService(new PipelineConfig
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory
        });
        var service = new JobService(mockConfigService.Object);

        // Act
        var result = service.GetJobLog(jobId);

        // Assert
        result.Should().Be(logContent);
    }
}

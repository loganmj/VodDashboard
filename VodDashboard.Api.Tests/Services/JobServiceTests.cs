using FluentAssertions;
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

    [Fact]
    public async Task GetJobs_WhenOutputDirectoryIsEmpty_ReturnsEmptyCollection()
    {
        // Arrange
        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = string.Empty,
            ConfigFile = "/some/config"
        });
        var service = new JobService(settings);

        // Act
        var result = await service.GetJobsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetJobs_WhenOutputDirectoryIsWhitespace_ReturnsEmptyCollection()
    {
        // Arrange
        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = "   ",
            ConfigFile = "/some/config"
        });
        var service = new JobService(settings);

        // Act
        var result = await service.GetJobsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetJobs_WhenDirectoryDoesNotExist_ReturnsEmptyCollection()
    {
        // Arrange
        var nonExistentDirectory = Path.Combine(Path.GetTempPath(), $"NonExistent_{Guid.NewGuid()}");
        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = nonExistentDirectory,
            ConfigFile = "/some/config"
        });
        var service = new JobService(settings);

        // Act
        var result = await service.GetJobsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetJobs_WhenDirectoryIsEmpty_ReturnsEmptyCollection()
    {
        // Arrange
        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory,
            ConfigFile = "/some/config"
        });
        var service = new JobService(settings);

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

        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory,
            ConfigFile = "/some/config"
        });
        var service = new JobService(settings);

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

        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory,
            ConfigFile = "/some/config"
        });
        var service = new JobService(settings);

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

        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory,
            ConfigFile = "/some/config"
        });
        var service = new JobService(settings);

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

        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory,
            ConfigFile = "/some/config"
        });
        var service = new JobService(settings);

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

        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory,
            ConfigFile = "/some/config"
        });
        var service = new JobService(settings);

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

        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory,
            ConfigFile = "/some/config"
        });
        var service = new JobService(settings);

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

        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory,
            ConfigFile = "/some/config"
        });
        var service = new JobService(settings);

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

        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory,
            ConfigFile = "/some/config"
        });
        var service = new JobService(settings);

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

        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory,
            ConfigFile = "/some/config"
        });
        var service = new JobService(settings);

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

        var settings = Options.Create(new PipelineSettings
        {
            InputDirectory = "/some/input",
            OutputDirectory = _testDirectory,
            ConfigFile = "/some/config"
        });
        var service = new JobService(settings);

        // Act
        var result = (await service.GetJobsAsync()).Single();

        // Assert
        result.HighlightCount.Should().Be(0);
        result.SceneCount.Should().Be(0);
        result.HasCleanVideo.Should().BeFalse(); // random.mp4 is not named clean.mp4
    }
}

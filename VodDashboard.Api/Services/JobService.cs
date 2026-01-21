using Microsoft.Extensions.Options;
using VodDashboard.Api.Models;
using VodDashboard.Api.DTO;

namespace VodDashboard.Api.Services;

public class JobService
{
    private readonly PipelineSettings _settings;

    public JobService(IOptions<PipelineSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<IEnumerable<JobSummaryDTO>> GetJobsAsync()
    {
        if (string.IsNullOrWhiteSpace(_settings.OutputDirectory))
            return Enumerable.Empty<JobSummaryDTO>();
        var dir = new DirectoryInfo(_settings.OutputDirectory);

        if (!dir.Exists)
        {
            return Enumerable.Empty<JobSummaryDTO>();
        }

        var directories = dir.EnumerateDirectories().OrderByDescending(d => d.CreationTimeUtc).ToList();
        var tasks = directories.Select(d => BuildJobSummaryAsync(d));
        return await Task.WhenAll(tasks);
    }

    private async Task<JobSummaryDTO> BuildJobSummaryAsync(DirectoryInfo jobDir)
    {
        var cleanPath = Path.Combine(jobDir.FullName, "clean.mp4");
        var highlightsDir = Path.Combine(jobDir.FullName, "highlights");
        var scenesDir = Path.Combine(jobDir.FullName, "scenes");

        var highlightCountTask = Task.Run(() =>
        {
            if (Directory.Exists(highlightsDir))
                return Directory.GetFiles(highlightsDir, "*.mp4").Length;
            return 0;
        });

        var sceneCountTask = Task.Run(() =>
        {
            if (Directory.Exists(scenesDir))
                return Directory.GetFiles(scenesDir, "*.csv").Length;
            return 0;
        });

        var hasCleanVideoTask = Task.Run(() => File.Exists(cleanPath));

        var highlightCount = await highlightCountTask;
        var sceneCount = await sceneCountTask;
        var hasCleanVideo = await hasCleanVideoTask;

        return new JobSummaryDTO
        {
            Id = jobDir.Name,
            HasCleanVideo = hasCleanVideo,
            HighlightCount = highlightCount,
            SceneCount = sceneCount,
            Created = jobDir.CreationTimeUtc
        };
    }
}
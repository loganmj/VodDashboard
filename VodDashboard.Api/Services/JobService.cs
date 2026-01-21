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

    public virtual async Task<IEnumerable<JobSummaryDTO>> GetJobsAsync()
    {
        if (string.IsNullOrWhiteSpace(_settings.OutputDirectory))
        {
            throw new InvalidOperationException("PipelineSettings.OutputDirectory is not configured.");
        }

        var dir = new DirectoryInfo(_settings.OutputDirectory);

        if (!dir.Exists)
        {
            return Enumerable.Empty<JobSummaryDTO>();
        }

        try
        {
            var directories = dir.EnumerateDirectories().OrderByDescending(d => d.CreationTimeUtc).ToList();
            var tasks = directories.Select(d => BuildJobSummaryAsync(d));
            return await Task.WhenAll(tasks);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new InvalidOperationException("Access to the configured output directory is denied.", ex);
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException("An I/O error occurred while enumerating directories in the configured output directory.", ex);
        }
    }

    private async Task<JobSummaryDTO> BuildJobSummaryAsync(DirectoryInfo jobDir)
    {
        try
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

            return new JobSummaryDTO(
                Id: jobDir.Name,
                HasCleanVideo: hasCleanVideo,
                HighlightCount: highlightCount,
                SceneCount: sceneCount,
                Created: jobDir.CreationTimeUtc
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new InvalidOperationException("Access to a job directory is denied.", ex);
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException("An I/O error occurred while processing a job directory.", ex);
        }
    }
}
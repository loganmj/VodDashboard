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

    private Task<JobSummaryDTO> BuildJobSummaryAsync(DirectoryInfo jobDir)
    {
        try
        {
            var cleanPath = Path.Combine(jobDir.FullName, "clean.mp4");
            var highlightsDir = Path.Combine(jobDir.FullName, "highlights");
            var scenesDir = Path.Combine(jobDir.FullName, "scenes");

            var highlightCount = 0;
            if (Directory.Exists(highlightsDir))
            {
                highlightCount = Directory.GetFiles(highlightsDir, "*.mp4").Length;
            }

            var sceneCount = 0;
            if (Directory.Exists(scenesDir))
            {
                sceneCount = Directory.GetFiles(scenesDir, "*.csv").Length;
            }

            var hasCleanVideo = File.Exists(cleanPath);

            return Task.FromResult(new JobSummaryDTO(
                Id: jobDir.Name,
                HasCleanVideo: hasCleanVideo,
                HighlightCount: highlightCount,
                SceneCount: sceneCount,
                Created: jobDir.CreationTimeUtc
            ));
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
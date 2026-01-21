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

    public IEnumerable<JobSummaryDTO> GetJobs()
    {
        if (string.IsNullOrWhiteSpace(_settings.OutputDirectory))
            return Enumerable.Empty<JobSummaryDTO>();
        var dir = new DirectoryInfo(_settings.OutputDirectory);

        if (!dir.Exists)
            return Enumerable.Empty<JobSummaryDTO>();

        return dir
            .EnumerateDirectories()
            .OrderByDescending(d => d.CreationTimeUtc)
            .Select(d => BuildJobSummary(d));
    }

    private JobSummaryDTO BuildJobSummary(DirectoryInfo jobDir)
    {
        var cleanPath = Path.Combine(jobDir.FullName, "clean.mp4");
        var highlightsDir = Path.Combine(jobDir.FullName, "highlights");
        var scenesDir = Path.Combine(jobDir.FullName, "scenes");

        int highlightCount = 0;
        if (Directory.Exists(highlightsDir))
            highlightCount = Directory.GetFiles(highlightsDir, "*.mp4").Length;

        int sceneCount = 0;
        if (Directory.Exists(scenesDir))
            sceneCount = Directory.GetFiles(scenesDir, "*.csv").Length;

        return new JobSummaryDTO(
            jobDir.Name,
            File.Exists(cleanPath),
            highlightCount,
            sceneCount,
            new DateTimeOffset(jobDir.CreationTimeUtc, TimeSpan.Zero));
    }
}
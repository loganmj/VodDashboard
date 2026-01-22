using VodDashboard.Api.Domain;
using VodDashboard.Api.DTO;
using VodDashboard.Api.Services;

namespace VodDashboard.Api.Services;

public class JobService(ConfigService configService)
{
    private readonly ConfigService _configService = configService;

    public virtual async Task<IEnumerable<JobData>> GetJobsAsync()
    {
        PipelineConfig config = _configService.GetCachedConfig();

        if (string.IsNullOrWhiteSpace(config.OutputDirectory))
        {
            throw new InvalidOperationException("PipelineConfig.OutputDirectory is not configured.");
        }

        var dir = new DirectoryInfo(config.OutputDirectory);

        if (!dir.Exists)
        {
            return [];
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

    public virtual JobData? GetJobDetail(string id)
    {
        PipelineConfig config = _configService.GetCachedConfig();

        if (string.IsNullOrWhiteSpace(config.OutputDirectory))
        {
            throw new InvalidOperationException("PipelineConfig.OutputDirectory is not configured.");
        }

        if (!Validation.IsValidJobId(id))
        {
            return null;
        }

        // Use Path.GetFileName to ensure we only get the filename component
        string safeId = Validation.SanitizeJobId(id);
        DirectoryInfo jobDir = new(Path.Combine(config.OutputDirectory, safeId));

        if (!jobDir.Exists)
        {
            return null;
        }

        try
        {
            var cleanPath = Path.Combine(jobDir.FullName, "clean.mp4");
            var highlightsDir = Path.Combine(jobDir.FullName, "highlights");
            var scenesDir = Path.Combine(jobDir.FullName, "scenes");

            List<string> highlights = Directory.Exists(highlightsDir)
                ? [.. Directory.GetFiles(highlightsDir, "*.mp4")
                          .Select(Path.GetFileName)
                          .Where(name => !string.IsNullOrEmpty(name))
                          .Cast<string>()]
                : [];

            List<string> scenes = Directory.Exists(scenesDir)
                ? [.. Directory.GetFiles(scenesDir, "*.csv")
                          .Select(Path.GetFileName)
                          .Where(name => !string.IsNullOrEmpty(name))
                          .Cast<string>()]
                : [];

            return new JobData(
                Id: safeId,
                HasCleanVideo: File.Exists(cleanPath),
                HighlightCount: highlights.Count,
                SceneCount: scenes.Count,
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

    public virtual string? GetJobLog(string id)
    {
        PipelineConfig config = _configService.GetCachedConfig();

        if (string.IsNullOrWhiteSpace(config.OutputDirectory))
        {
            throw new InvalidOperationException("PipelineConfig.OutputDirectory is not configured.");
        }

        if (!Validation.IsValidJobId(id))
        {
            return null;
        }

        // Use Path.GetFileName to ensure we only get the filename component
        string safeId = Validation.SanitizeJobId(id);
        string jobDir = Path.Combine(config.OutputDirectory, safeId);

        if (!Directory.Exists(jobDir))
        {
            return null;
        }

        var logPath = Path.Combine(jobDir, "log.txt");

        if (!File.Exists(logPath))
        {
            return null;
        }

        try
        {
            return File.ReadAllText(logPath);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new InvalidOperationException("Access to the log file is denied.", ex);
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException("An I/O error occurred while reading the log file.", ex);
        }
    }

    private Task<JobData> BuildJobSummaryAsync(DirectoryInfo jobDir)
    {
        try
        {
            string cleanPath = Path.Combine(jobDir.FullName, "clean.mp4");
            string highlightsDir = Path.Combine(jobDir.FullName, "highlights");
            string scenesDir = Path.Combine(jobDir.FullName, "scenes");

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

            return Task.FromResult(new JobData(
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
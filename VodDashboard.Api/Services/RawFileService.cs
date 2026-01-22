using VodDashboard.Api.Services;
using VodDashboard.Api.DTO;

namespace VodDashboard.Api.Services;

public class RawFileService(ConfigService configService)
{
    #region Private Data

    private readonly ConfigService _configService = configService;

    #endregion

    #region Public Methods

    public virtual IEnumerable<RawFileMetadata> GetRawFiles()
    {
        PipelineConfig config = _configService.GetCachedConfig();

        if (string.IsNullOrWhiteSpace(config.InputDirectory))
        {
            throw new InvalidOperationException("PipelineConfig.InputDirectory is not configured.");
        }

        var dir = new DirectoryInfo(config.InputDirectory);

        if (!dir.Exists)
        {
            return Enumerable.Empty<RawFileMetadata>();
        }

        try
        {
            return dir
                .EnumerateFiles("*.mp4", SearchOption.TopDirectoryOnly)
                .OrderByDescending(f => f.CreationTimeUtc)
                .Select(f => new RawFileMetadata(
                    f.Name,
                    f.Length,
                    new DateTimeOffset(f.CreationTimeUtc, TimeSpan.Zero)
                ));
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new InvalidOperationException("Access to the configured input directory is denied.", ex);
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException("An I/O error occurred while enumerating files in the configured input directory.", ex);
        }
    }

    #endregion
}

using Microsoft.Extensions.Options;
using VodDashboard.Api.Models;
using VodDashboard.Api.DTO;

namespace VodDashboard.Api.Services;

public class RawFileService(IOptions<PipelineSettings> settings)
{
    #region Private Data

    private readonly PipelineSettings _settings = settings.Value;

    #endregion

    #region Public Methods

    public virtual IEnumerable<RawFileMetadata> GetRawFiles()
    {
        if (string.IsNullOrWhiteSpace(_settings.InputDirectory))
        {
            throw new InvalidOperationException("PipelineSettings.InputDirectory is not configured.");
        }

        var dir = new DirectoryInfo(_settings.InputDirectory);

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

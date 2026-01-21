using Microsoft.Extensions.Options;
using VodDashboard.Api.Models;
using VodDashboard.Api.DTO;

namespace VodDashboard.Api.Services
{
    public class RawFileService(IOptions<PipelineSettings> settings)
    {
        #region Private Data

        private readonly PipelineSettings _settings = settings.Value;

        #endregion

        #region Public Methods

        public IEnumerable<RawFileDTO> GetRawFiles()
        {
            if (string.IsNullOrWhiteSpace(_settings.InputDirectory))
            {
                throw new InvalidOperationException("PipelineSettings.InputDirectory is not configured.");
            }
            if (string.IsNullOrWhiteSpace(_settings.InputDirectory))
            {
                throw new InvalidOperationException("PipelineSettings.InputDirectory is not configured.");
            }

            var dir = new DirectoryInfo(_settings.InputDirectory);

            if (!dir.Exists)
            {
                return Enumerable.Empty<RawFileDTO>();
            }

            try
            {
                return dir
                    .EnumerateFiles("*.mp4", SearchOption.TopDirectoryOnly)
                    .OrderByDescending(f => f.CreationTimeUtc)
                    .Select(f => new RawFileDTO(
                        f.Name,
                        f.Length,
                        new DateTimeOffset(f.CreationTimeUtc, TimeSpan.Zero)
                    ));
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new InvalidOperationException($"Access to the input directory '{_settings.InputDirectory}' is denied.", ex);
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException($"An I/O error occurred while enumerating files in the input directory '{_settings.InputDirectory}'.", ex);
            }
        }

        #endregion
    }

}

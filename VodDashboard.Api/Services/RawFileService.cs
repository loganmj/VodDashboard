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

        public async Task<IEnumerable<RawFileDTO>> GetRawFiles(CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_settings.InputDirectory))
            {
                throw new InvalidOperationException("PipelineSettings.InputDirectory is not configured.");
            }

            IEnumerable<RawFileDTO> result = await Task.Run(() =>
            {
                var dir = new DirectoryInfo(_settings.InputDirectory);

                if (!dir.Exists)
                {
                    return Enumerable.Empty<RawFileDTO>();
                }

                return dir
                    .EnumerateFiles("*.mp4", SearchOption.TopDirectoryOnly)
                    .OrderByDescending(f => f.CreationTimeUtc)
                    .Select(f => new RawFileDTO(
                        f.Name,
                        f.Length,
                        new DateTimeOffset(f.CreationTimeUtc, TimeSpan.Zero)
                    ))
                    .ToList();
            }, cancellationToken);

            return result;
        }

        #endregion
    }


}

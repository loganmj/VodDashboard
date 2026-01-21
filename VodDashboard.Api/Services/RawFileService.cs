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
            var dir = new DirectoryInfo(_settings.InputDirectory);

            if (!dir.Exists) 
            {
                return []; 
            }

            return dir
                .EnumerateFiles("*.mp4", SearchOption.TopDirectoryOnly)
                .OrderByDescending(f => f.CreationTimeUtc)
                .Select(f => new RawFileDTO
                {
                    FileName = f.Name,
                    SizeBytes = f.Length,
                    Created = f.CreationTimeUtc
                });
        }

        #endregion
    }


}

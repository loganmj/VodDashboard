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

        /// <summary>
        /// Retrieves raw MP4 video files from the configured input directory asynchronously.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// an enumerable sequence of <see cref="RawFileDTO"/> instances representing
        /// the MP4 files found in the input directory, ordered by creation time in
        /// descending order. Returns an empty sequence if the directory does not exist.
        /// </returns>
        public Task<IEnumerable<RawFileDTO>> GetRawFilesAsync()
        {
            try
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
                    })
                    .ToArray();
            }
            catch (UnauthorizedAccessException)
            {
                // TODO: Log unauthorized access to input directory
                return [];
            }
            catch (IOException)
            {
                // TODO: Log IO error while accessing input directory or files
                return [];
            }
            catch (System.Security.SecurityException)
            {
                // TODO: Log security error while accessing input directory or files
                return [];
            }
        }

        #endregion
    }


}

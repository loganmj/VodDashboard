using Microsoft.Extensions.Options;
using VodDashboard.Api.Models;
using VodDashboard.Api.DTO;

namespace VodDashboard.Api.Services
{
    public class RawFileService
    {
        #region Private Data

        private readonly PipelineSettings _settings;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RawFileService"/> class.
        /// Validates the input directory path at startup to ensure it's configured correctly.
        /// </summary>
        /// <param name="settings">The pipeline configuration settings.</param>
        /// <exception cref="ArgumentNullException">Thrown when settings is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the input directory path is invalid or inaccessible.</exception>
        public RawFileService(IOptions<PipelineSettings> settings)
        {
            ArgumentNullException.ThrowIfNull(settings, nameof(settings));
            
            _settings = settings.Value ?? 
                throw new ArgumentNullException(nameof(settings) + ".Value", "Pipeline settings value cannot be null");

            ValidateInputDirectory();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Validates that the input directory path is properly configured and accessible.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when the directory path is invalid or empty.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when the directory does not exist.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the directory is not accessible.</exception>
        private void ValidateInputDirectory()
        {
            if (string.IsNullOrWhiteSpace(_settings.InputDirectory))
            {
                throw new ArgumentException(
                    "InputDirectory configuration is required and cannot be empty", 
                    "InputDirectory");
            }

            try
            {
                var dir = new DirectoryInfo(_settings.InputDirectory);
                
                if (!dir.Exists)
                {
                    throw new DirectoryNotFoundException(
                        $"The configured input directory does not exist: {_settings.InputDirectory}");
                }

                // Test read access by attempting to enumerate
                _ = dir.EnumerateFileSystemInfos().FirstOrDefault();
            }
            catch (ArgumentException)
            {
                throw new ArgumentException(
                    $"The configured input directory path is invalid: {_settings.InputDirectory}", 
                    "InputDirectory");
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new UnauthorizedAccessException(
                    $"Access denied to the configured input directory: {_settings.InputDirectory}", ex);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Retrieves raw MP4 video files from the configured input directory.
        /// </summary>
        /// <returns>
        /// An enumerable sequence of <see cref="RawFileDTO"/> instances representing
        /// the MP4 files found in the input directory, ordered by creation time in
        /// descending order.
        /// </returns>
        public IEnumerable<RawFileDTO> GetRawFiles()
        {
            var dir = new DirectoryInfo(_settings.InputDirectory);

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

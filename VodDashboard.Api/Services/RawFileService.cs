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
        /// Retrieves raw MP4 video files from the configured input directory with pagination support.
        /// </summary>
        /// <param name="pageNumber">The page number to retrieve (1-based). Defaults to 1.</param>
        /// <param name="pageSize">The number of items per page. Defaults to 50.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// a <see cref="PaginatedResult{RawFileDTO}"/> with the files for the requested page,
        /// ordered by creation time in descending order.
        /// </returns>
        public Task<PaginatedResult<RawFileDTO>> GetRawFilesAsync(int pageNumber = 1, int pageSize = 50)
        {
            // Validate pagination parameters
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            if (pageSize < 1)
            {
                pageSize = 50;
            }

            // Cap maximum page size to prevent excessive memory usage
            if (pageSize > 100)
            {
                pageSize = 100;
            }

            try
            {
                var dir = new DirectoryInfo(_settings.InputDirectory);

                if (!dir.Exists)
                {
                    return Task.FromResult(new PaginatedResult<RawFileDTO>
                    {
                        Items = [],
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        TotalCount = 0
                    });
                }

                // Get all files and count them
                var allFiles = dir
                    .EnumerateFiles("*.mp4", SearchOption.TopDirectoryOnly)
                    .OrderByDescending(f => f.CreationTimeUtc)
                    .ToList();

                var totalCount = allFiles.Count;

                // Apply pagination
                var pagedFiles = allFiles
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(f => new RawFileDTO
                    {
                        FileName = f.Name,
                        SizeBytes = f.Length,
                        Created = f.CreationTimeUtc
                    })
                    .ToList();

                var result = new PaginatedResult<RawFileDTO>
                {
                    Items = pagedFiles,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };

                return Task.FromResult(result);
            }
            catch (UnauthorizedAccessException)
            {
                // TODO: Log unauthorized access to input directory
                return Task.FromResult(new PaginatedResult<RawFileDTO>
                {
                    Items = [],
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = 0
                });
            }
            catch (IOException)
            {
                // TODO: Log IO error while accessing input directory or files
                return Task.FromResult(new PaginatedResult<RawFileDTO>
                {
                    Items = [],
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = 0
                });
            }
            catch (System.Security.SecurityException)
            {
                // TODO: Log security error while accessing input directory or files
                return Task.FromResult(new PaginatedResult<RawFileDTO>
                {
                    Items = [],
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = 0
                });
            }
        }

        /// <summary>
        /// Retrieves all raw MP4 video files from the configured input directory without pagination.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// an enumerable sequence of <see cref="RawFileDTO"/> instances representing
        /// the MP4 files found in the input directory, ordered by creation time in
        /// descending order. Returns an empty sequence if the directory does not exist.
        /// </returns>
        [Obsolete("Use GetRawFilesAsync(int pageNumber, int pageSize) instead for better performance with large directories.")]
        public Task<IEnumerable<RawFileDTO>> GetRawFilesAsync()
        {
            try
            {
                var dir = new DirectoryInfo(_settings.InputDirectory);

                if (!dir.Exists)
                {
                    return Task.FromResult(Enumerable.Empty<RawFileDTO>());
                }

                var result = dir
                    .EnumerateFiles("*.mp4", SearchOption.TopDirectoryOnly)
                    .OrderByDescending(f => f.CreationTimeUtc)
                    .Select(f => new RawFileDTO
                    {
                        FileName = f.Name,
                        SizeBytes = f.Length,
                        Created = f.CreationTimeUtc
                    })
                    .ToArray();

                return Task.FromResult<IEnumerable<RawFileDTO>>(result);
            }
            catch (UnauthorizedAccessException)
            {
                // TODO: Log unauthorized access to input directory
                return Task.FromResult(Enumerable.Empty<RawFileDTO>());
            }
            catch (IOException)
            {
                // TODO: Log IO error while accessing input directory or files
                return Task.FromResult(Enumerable.Empty<RawFileDTO>());
            }
            catch (System.Security.SecurityException)
            {
                // TODO: Log security error while accessing input directory or files
                return Task.FromResult(Enumerable.Empty<RawFileDTO>());
            }
        }

        #endregion
    }


}

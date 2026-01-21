using Microsoft.AspNetCore.Mvc;
using VodDashboard.Api.Services;

namespace VodDashboard.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RawController(RawFileService rawService) : ControllerBase
    {
        #region Private Data

        private readonly RawFileService _rawService = rawService;

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets a paginated list of raw MP4 video files from the configured input directory.
        /// </summary>
        /// <param name="pageNumber">The page number to retrieve (1-based). Defaults to 1 if not specified or invalid.</param>
        /// <param name="pageSize">The number of items per page. Defaults to 50 if not specified or invalid. Maximum is 100.</param>
        /// <returns>A paginated result containing the raw file information and pagination metadata.</returns>
        [HttpGet]
        public async Task<IActionResult> GetRawFiles([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            var files = await _rawService.GetRawFilesAsync(pageNumber, pageSize);
            return Ok(files);
        }

        #endregion
    }

}

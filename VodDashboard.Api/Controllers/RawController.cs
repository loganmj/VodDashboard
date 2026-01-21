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

        [HttpGet]
        public async Task<IActionResult> GetRawFiles()
        {
            var files = await _rawService.GetRawFilesAsync();
            return Ok(files);
        }

        #endregion
    }

}

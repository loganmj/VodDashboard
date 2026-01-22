using Microsoft.AspNetCore.Mvc;
using VodDashboard.Api.DTO;
using VodDashboard.Api.Services;

namespace VodDashboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RawController(RawFileService rawService) : ControllerBase
{
    [HttpGet]
    public IActionResult GetRawFiles()
    {
        try
        {
            IEnumerable<RawFileMetadata> files = rawService.GetRawFiles();
            return Ok(files);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Configuration Error",
                detail: ex.Message);
        }
        catch (Exception ex)
        {
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Unexpected Error",
                detail: ex.Message);
        }
    }
}
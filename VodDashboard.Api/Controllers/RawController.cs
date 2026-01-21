using Microsoft.AspNetCore.Mvc;
using VodDashboard.Api.Services;

namespace VodDashboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RawController(RawFileService rawService) : ControllerBase
{
    private readonly RawFileService _rawService = rawService;

    [HttpGet]
    public IActionResult GetRawFiles()
    {
        try
        {
            var files = _rawService.GetRawFiles();
            return Ok(files);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid operation when retrieving raw files",
                detail: ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Access denied to raw files",
                detail: ex.Message);
        }
        catch (IOException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "I/O error while retrieving raw files",
                detail: ex.Message);
        }
    }
}
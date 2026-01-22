using Microsoft.AspNetCore.Mvc;
using VodDashboard.Api.Services;

namespace VodDashboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatusController(StatusService statusService) : ControllerBase
{
    [HttpGet]
    public IActionResult GetStatus()
    {
        try
        {
            var status = statusService.GetStatus();
            return Ok(status);
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

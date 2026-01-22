using Microsoft.AspNetCore.Mvc;
using VodDashboard.Api.DTO;
using VodDashboard.Api.Services;

namespace VodDashboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigController(ConfigService configService) : ControllerBase
{
    [HttpGet]
    public IActionResult GetConfig()
    {
        var config = configService.GetConfig();
        if (config == null)
            return NotFound();

        return Ok(config);
    }

    [HttpPost]
    public IActionResult SaveConfig([FromBody] ConfigDto config)
    {
        if (config == null)
            return BadRequest("Config data must be provided.");

        var saveSuccessful = configService.SaveConfig(config);
        if (!saveSuccessful)
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Save Failed",
                detail: "Failed to save config");

        return Ok();
    }
}

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
        var ok = configService.SaveConfig(config);
        if (!ok)
            return StatusCode(500, "Failed to save config");

        return Ok();
    }
}

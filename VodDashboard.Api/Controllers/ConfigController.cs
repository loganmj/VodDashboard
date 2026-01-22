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
        try
        {
            var config = configService.GetConfig();
            return config == null ? NotFound() : Ok(config);
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

    [HttpPost]
    public IActionResult SaveConfig([FromBody] PipelineConfig config)
    {
        try
        {
            configService.SaveConfig(config);
            return Ok();
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

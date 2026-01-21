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
        var files = _rawService.GetRawFiles();
        return Ok(files);
    }
}
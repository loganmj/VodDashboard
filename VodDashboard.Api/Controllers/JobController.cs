using Microsoft.AspNetCore.Mvc;
using VodDashboard.Api.Services;

namespace VodDashboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobController(JobService jobService) : ControllerBase
{
    private readonly JobService _jobService = jobService;

    [HttpGet]
    public IActionResult GetJobs()
    {
        var jobs = _jobService.GetJobs();
        return Ok(jobs);
    }
}
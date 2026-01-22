using Microsoft.AspNetCore.Mvc;
using VodDashboard.Api.Services;

namespace VodDashboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobController(JobService jobService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetJobs()
    {
        try
        {
            var jobs = await jobService.GetJobsAsync();
            return Ok(jobs);
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
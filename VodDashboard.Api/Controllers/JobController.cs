using Microsoft.AspNetCore.Mvc;
using VodDashboard.Api.Services;

namespace VodDashboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobController(JobService jobService) : ControllerBase
{
    private readonly JobService _jobService = jobService;

    [HttpGet]
    public async Task<IActionResult> GetJobs()
    {
        try
        {
            var jobs = await _jobService.GetJobsAsync();
            return Ok(jobs);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Access to job storage was denied.",
                detail: ex.Message);
        }
        catch (IOException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "An error occurred while accessing job storage.",
                detail: ex.Message);
        }
    }
}
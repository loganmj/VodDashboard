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
                title: "Access to job storage was denied.",
                detail: ex.Message,
                statusCode: StatusCodes.Status403Forbidden);
        }
        catch (IOException ex)
        {
            return Problem(
                title: "An error occurred while accessing job storage.",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
        catch (Exception ex)
        {
            return Problem(
                title: "An unexpected error occurred while retrieving jobs.",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
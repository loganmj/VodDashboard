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

    [HttpGet("{id}")]
    public IActionResult GetJobDetail(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest("Job id must be provided.");

        if (id.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
            id.Contains(Path.DirectorySeparatorChar) ||
            id.Contains(Path.AltDirectorySeparatorChar) ||
            id.Contains("..", StringComparison.Ordinal))
            return BadRequest("Invalid job id.");

        try
        {
            var job = jobService.GetJobDetail(id);

            if (job == null)
                return NotFound();

            return Ok(job);
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
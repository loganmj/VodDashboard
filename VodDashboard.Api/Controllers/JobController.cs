using Microsoft.AspNetCore.Mvc;
using VodDashboard.Api.Domain;
using VodDashboard.Api.DTO;
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
        if (!Validation.IsValidJobId(id))
        {
            return BadRequest(string.IsNullOrWhiteSpace(id) ? "Job id must be provided." : "Invalid job id.");
        }

        try
        {
            var job = jobService.GetJobDetail(id);
            return job == null ? NotFound() : Ok(job);
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

    [HttpGet("{id}/log")]
    public IActionResult GetJobLog(string id)
    {
        if (!Validation.IsValidJobId(id))
        {
            return BadRequest(string.IsNullOrWhiteSpace(id) ? "Job id must be provided." : "Invalid job id.");
        }

        try
        {
            string? log = jobService.GetJobLog(id);
            return log == null ? NotFound() : Content(log, "text/plain");
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
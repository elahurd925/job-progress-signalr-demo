using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

[Route("api/[controller]")]
[ApiController]
public class MergeController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ILogger<MergeController> _logger;

    public MergeController(AppDbContext dbContext, IBackgroundJobClient backgroundJobClient, ILogger<MergeController> logger)
    {
        _dbContext = dbContext;
        _backgroundJobClient = backgroundJobClient;
        _logger = logger;
    }

    [HttpDelete("clear-merges")]
    public async Task<IActionResult> ClearMerges()
    {
        _dbContext.MergeJobs.RemoveRange(_dbContext.MergeJobs);
        await _dbContext.SaveChangesAsync();

        return Ok("Merge jobs cleared");
    }

    [HttpPost("merge-customers")]
    public IActionResult MergeCustomers(int customerCount)
    {
        var sessionId = Guid.NewGuid();
        try {
            _backgroundJobClient.Enqueue<MergeService>(service => service.ExecuteMergeJob(sessionId, customerCount));
        } catch(Exception e) {
            _logger.LogError(e, "Error merging customers");
            return StatusCode(500);
        }

        return Ok(new { SessionId = sessionId });
    }

    [HttpGet("check-merge")]
    public async Task<IActionResult> CheckMerge()
    {
        try {
            var mergeJob = await _dbContext.MergeJobs.AsNoTracking().FirstOrDefaultAsync(j => j.Status == "Processing");
            if (mergeJob != null) {
                return Ok(new { mergeJob.SessionId });
            }
        } catch(Exception e) {
            _logger.LogError(e, "Error checking for merge in progress");
            return StatusCode(500);
        }

        return NotFound();
    }

    [HttpGet("merge-status")]
    public async Task GetMergeStatus([FromQuery] Guid sessionId)
    {
        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");

        while (!HttpContext.RequestAborted.IsCancellationRequested)
        {
            // Fetch the latest progress from the database
            var mergeProgress = await _dbContext.MergeJobs.AsNoTracking().FirstOrDefaultAsync(j => j.SessionId == sessionId);

            if (mergeProgress != null)
            {
                // Stream the JSON-serialized progress data to the client
                var progressJson = System.Text.Json.JsonSerializer.Serialize(mergeProgress);
                await Response.Body.WriteAsync(Encoding.UTF8.GetBytes($"data: {progressJson}\n\n"));
                await Response.Body.FlushAsync();
            } else {
                await Response.Body.WriteAsync(Encoding.UTF8.GetBytes("data: {}\n\n"));
                await Response.Body.FlushAsync();
            }

            // Add a delay to avoid hitting the database too frequently
            await Task.Delay(1000);
        }
    }
}

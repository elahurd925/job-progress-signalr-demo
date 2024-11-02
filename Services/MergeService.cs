using Hangfire;
using Microsoft.AspNetCore.SignalR;

public class MergeService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<MergeService> _logger;
    private readonly IHubContext<MergeHub> _hubContext;

    public MergeService(AppDbContext dbContext, ILogger<MergeService> logger, IHubContext<MergeHub> hubContext)
    {
        _dbContext = dbContext;
        _logger = logger;
        _hubContext = hubContext;
    }

    [DisableConcurrentExecution(timeoutInSeconds: 0)]
    public async Task ExecuteMergeJob(Guid sessionId, int totalCustomers)
    {
        var mergeJob = new MergeJob
        {
            SessionId = sessionId,
            TotalMergeCount = totalCustomers,
            CompletedMergeCount = 0,
            Status = "Processing",
            CreatedAtUtc = DateTime.UtcNow
        };
        _dbContext.MergeJobs.Add(mergeJob);
        await _dbContext.SaveChangesAsync();

        // Broadcast progress to connected clients
        await _hubContext.Clients.Group(sessionId.ToString()).SendAsync("ReceiveMergeStart", mergeJob);

        for (int i = 1; i <= totalCustomers; i++)
        {
            _logger.LogInformation("Starting customer merge " + i);
            // Simulate time-consuming merge operation
            await Task.Delay(3000); // 3 second delay to simulate work

            // Update completed count
            mergeJob.CompletedMergeCount = i;
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Finished customer merge " + i);
            // Broadcast progress to connected clients
            await _hubContext.Clients.Group(sessionId.ToString()).SendAsync("ReceiveMergeUpdate", mergeJob);
        }

        _logger.LogInformation("Finished merge!");
        mergeJob.Status = "Done";
        await _dbContext.SaveChangesAsync();
        // Broadcast progress to connected clients
        await _hubContext.Clients.Group(sessionId.ToString()).SendAsync("ReceiveMergeComplete", mergeJob);
    }
}

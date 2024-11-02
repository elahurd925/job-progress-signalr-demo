public class MergeJob
{
    public int Id { get; set; }
    public Guid SessionId { get; set; }
    public int TotalMergeCount { get; set; }
    public int CompletedMergeCount { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

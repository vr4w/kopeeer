namespace Kopeeer.Core;

public sealed class QueueJob
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public required string SourcePath { get; init; }

    public required string TargetFolder { get; init; }

    public required FileOperationType OperationType { get; init; }

    public JobStatus Status { get; set; } = JobStatus.Pending;

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.Now;

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public string? ErrorMessage { get; set; }

    public long TotalBytes { get; set; }

    public long TransferredBytes { get; set; }

    public double BytesPerSecond { get; set; }

    public string CurrentItem { get; set; } = string.Empty;

    public int ProgressPercent =>
        TotalBytes <= 0 ? 0 : (int)Math.Clamp(TransferredBytes * 100 / TotalBytes, 0, 100);
}


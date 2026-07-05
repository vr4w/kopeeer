using System.Text.Json.Serialization;

namespace FileOperationQueue.Core.Queue;

public sealed record QueueSnapshot
{
    public required IReadOnlyList<FileOperationJob> Jobs { get; init; }

    [JsonIgnore]
    public FileOperationJob? ActiveJob =>
        Jobs.SingleOrDefault(job => job.Status == FileOperationJobStatus.Active);

    [JsonIgnore]
    public IReadOnlyList<FileOperationJob> PendingJobs =>
        Jobs.Where(job => job.Status == FileOperationJobStatus.Queued).ToArray();
}

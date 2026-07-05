namespace FileOperationQueue.Core.Queue;

public sealed class OperationQueue(IQueueStore store, TimeProvider? timeProvider = null)
{
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;

    public async Task<FileOperationJob> EnqueueAsync(
        FileOperationKind kind,
        IEnumerable<string> sources,
        string destinationDirectory,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await store.LoadAsync(cancellationToken);
        var job = FileOperationJob.Create(kind, sources, destinationDirectory, _timeProvider);

        await store.SaveAsync(
            snapshot with { Jobs = snapshot.Jobs.Concat([job]).ToArray() },
            cancellationToken);

        return job;
    }

    public Task<QueueSnapshot> SnapshotAsync(CancellationToken cancellationToken = default) =>
        store.LoadAsync(cancellationToken);

    public async Task<FileOperationJob?> TryStartNextAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = await store.LoadAsync(cancellationToken);

        if (snapshot.Jobs.Any(job => job.Status == FileOperationJobStatus.Active))
        {
            return null;
        }

        var next = snapshot.Jobs.FirstOrDefault(job => job.Status == FileOperationJobStatus.Queued);
        if (next is null)
        {
            return null;
        }

        var active = next with
        {
            Status = FileOperationJobStatus.Active,
            StartedAt = _timeProvider.GetUtcNow(),
            ErrorMessage = null
        };

        await store.SaveAsync(ReplaceJob(snapshot, active), cancellationToken);
        return active;
    }

    public Task CompleteAsync(Guid jobId, CancellationToken cancellationToken = default) =>
        TransitionAsync(
            jobId,
            FileOperationJobStatus.Completed,
            errorMessage: null,
            cancellationToken);

    public Task FailAsync(Guid jobId, string errorMessage, CancellationToken cancellationToken = default) =>
        TransitionAsync(
            jobId,
            FileOperationJobStatus.Failed,
            errorMessage,
            cancellationToken);

    public Task CancelAsync(Guid jobId, CancellationToken cancellationToken = default) =>
        TransitionAsync(
            jobId,
            FileOperationJobStatus.Canceled,
            errorMessage: null,
            cancellationToken);

    private async Task TransitionAsync(
        Guid jobId,
        FileOperationJobStatus status,
        string? errorMessage,
        CancellationToken cancellationToken)
    {
        var snapshot = await store.LoadAsync(cancellationToken);
        var job = snapshot.Jobs.SingleOrDefault(candidate => candidate.Id == jobId)
            ?? throw new InvalidOperationException($"Queue job '{jobId}' was not found.");

        if (job.Status is FileOperationJobStatus.Completed or FileOperationJobStatus.Failed or FileOperationJobStatus.Canceled)
        {
            throw new InvalidOperationException($"Queue job '{jobId}' is already finished.");
        }

        var updated = job with
        {
            Status = status,
            FinishedAt = _timeProvider.GetUtcNow(),
            ErrorMessage = errorMessage
        };

        await store.SaveAsync(ReplaceJob(snapshot, updated), cancellationToken);
    }

    private static QueueSnapshot ReplaceJob(QueueSnapshot snapshot, FileOperationJob updatedJob) =>
        snapshot with
        {
            Jobs = snapshot.Jobs
                .Select(job => job.Id == updatedJob.Id ? updatedJob : job)
                .ToArray()
        };
}


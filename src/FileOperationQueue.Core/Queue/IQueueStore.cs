namespace FileOperationQueue.Core.Queue;

public interface IQueueStore
{
    Task<QueueSnapshot> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(QueueSnapshot snapshot, CancellationToken cancellationToken = default);
}


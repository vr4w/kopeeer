using FileOperationQueue.Core.Queue;

namespace FileOperationQueue.Core.Worker;

public sealed class NoOpFileOperationExecutor : IFileOperationExecutor
{
    public Task ExecuteAsync(FileOperationJob job, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(job);
        return Task.CompletedTask;
    }
}


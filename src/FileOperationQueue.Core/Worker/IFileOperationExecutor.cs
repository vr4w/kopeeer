using FileOperationQueue.Core.Queue;

namespace FileOperationQueue.Core.Worker;

public interface IFileOperationExecutor
{
    Task ExecuteAsync(FileOperationJob job, CancellationToken cancellationToken = default);
}


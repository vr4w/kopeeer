using FileOperationQueue.Core.Queue;

namespace FileOperationQueue.Core.Worker;

public sealed class LocalQueueWorker(OperationQueue queue, IFileOperationExecutor executor)
{
    public async Task<bool> ProcessNextAsync(CancellationToken cancellationToken = default)
    {
        var job = await queue.TryStartNextAsync(cancellationToken);
        if (job is null)
        {
            return false;
        }

        try
        {
            await executor.ExecuteAsync(job, cancellationToken);
            await queue.CompleteAsync(job.Id, cancellationToken);
            return true;
        }
        catch (OperationCanceledException)
        {
            await queue.CancelAsync(job.Id, CancellationToken.None);
            throw;
        }
        catch (Exception exception)
        {
            await queue.FailAsync(job.Id, exception.Message, CancellationToken.None);
            return true;
        }
    }
}


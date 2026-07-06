using Kopeeer.Core;

namespace Kopeeer.Worker;

public sealed class SequentialQueueProcessor(
    InMemoryJobQueue queue,
    FileOperationProcessor processor,
    IJobLogger logger)
{
    public bool IsRunning { get; private set; }

    public async Task ProcessAllAsync(
        Action? onJobChanged = null,
        Action<QueueJob>? onJobProgress = null,
        Func<QueueJob, string, TargetConflictResolution>? onTargetConflict = null,
        CancellationToken cancellationToken = default)
    {
        if (IsRunning)
        {
            return;
        }

        IsRunning = true;

        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var job = queue.NextPending();
                if (job is null)
                {
                    return;
                }

                job.Status = JobStatus.Running;
                job.StartedAt = DateTimeOffset.Now;
                job.ErrorMessage = null;
                logger.JobStarted(job);
                onJobChanged?.Invoke();

                try
                {
                    await processor.ProcessAsync(job, onJobProgress, onTargetConflict, cancellationToken);
                    job.Status = JobStatus.Completed;
                    job.TransferredBytes = job.TotalBytes;
                    job.BytesPerSecond = 0;
                    job.CurrentItem = string.Empty;
                    job.CompletedAt = DateTimeOffset.Now;
                    logger.JobCompleted(job);
                }
                catch (OperationSkippedException exception)
                {
                    job.Status = JobStatus.Skipped;
                    job.CompletedAt = DateTimeOffset.Now;
                    job.ErrorMessage = exception.Message;
                    job.BytesPerSecond = 0;
                    job.CurrentItem = string.Empty;
                    logger.JobFailed(job, exception.Message);
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    job.Status = JobStatus.Failed;
                    job.CompletedAt = DateTimeOffset.Now;
                    job.ErrorMessage = exception.Message;
                    logger.JobFailed(job, exception.Message);
                }

                onJobChanged?.Invoke();
            }
        }
        finally
        {
            IsRunning = false;
        }
    }
}


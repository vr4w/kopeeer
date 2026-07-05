using FileOperationQueue.Core.Queue;
using FileOperationQueue.Core.Worker;

namespace FileOperationQueue.Core.Tests;

public static class QueueCoreSmokeTests
{
    public static async Task EnqueuedJobCanBeProcessedAsync(string queueFilePath)
    {
        var store = new JsonFileQueueStore(queueFilePath);
        var queue = new OperationQueue(store);
        var worker = new LocalQueueWorker(queue, new NoOpFileOperationExecutor());

        var job = await queue.EnqueueAsync(
            FileOperationKind.Copy,
            [@"C:\Source\example.txt"],
            @"D:\Target");

        if (job.Status != FileOperationJobStatus.Queued)
        {
            throw new InvalidOperationException("New jobs should start in queued state.");
        }

        var processed = await worker.ProcessNextAsync();
        if (!processed)
        {
            throw new InvalidOperationException("The worker should process the queued job.");
        }

        var snapshot = await queue.SnapshotAsync();
        var completed = snapshot.Jobs.Single(candidate => candidate.Id == job.Id);
        if (completed.Status != FileOperationJobStatus.Completed)
        {
            throw new InvalidOperationException("The job should be completed after the no-op worker runs.");
        }
    }
}


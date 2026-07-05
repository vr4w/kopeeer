using System.Collections.ObjectModel;

namespace Kopeeer.Core;

public sealed class InMemoryJobQueue
{
    private readonly List<QueueJob> _jobs = [];

    public IReadOnlyList<QueueJob> Jobs => new ReadOnlyCollection<QueueJob>(_jobs);

    public QueueJob Add(string sourcePath, string targetFolder, FileOperationType operationType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetFolder);

        var job = new QueueJob
        {
            SourcePath = sourcePath.Trim(),
            TargetFolder = targetFolder.Trim(),
            OperationType = operationType
        };

        _jobs.Add(job);
        return job;
    }

    public QueueJob? NextPending() =>
        _jobs.FirstOrDefault(job => job.Status == JobStatus.Pending);
}


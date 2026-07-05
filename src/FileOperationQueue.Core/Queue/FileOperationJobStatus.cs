namespace FileOperationQueue.Core.Queue;

public enum FileOperationJobStatus
{
    Queued = 0,
    Active = 1,
    Completed = 2,
    Failed = 3,
    Canceled = 4
}


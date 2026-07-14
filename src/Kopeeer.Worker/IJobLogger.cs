using Kopeeer.Core;

namespace Kopeeer.Worker;

public interface IJobLogger
{
    void AppStarted();

    void JobAdded(QueueJob job);

    void JobStarted(QueueJob job);

    void JobCompleted(QueueJob job);

    void JobFailed(QueueJob job, string errorMessage);

    void CleanupFailed(string path, string errorMessage);
}

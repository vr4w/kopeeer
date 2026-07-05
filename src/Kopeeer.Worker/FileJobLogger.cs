using Kopeeer.Core;

namespace Kopeeer.Worker;

public sealed class FileJobLogger(string logFilePath) : IJobLogger
{
    public string LogFilePath { get; } = logFilePath;

    public void AppStarted() =>
        Write("app start");

    public void JobAdded(QueueJob job) =>
        Write($"job added id={job.Id} operation={job.OperationType} source=\"{job.SourcePath}\" target=\"{job.TargetFolder}\"");

    public void JobStarted(QueueJob job) =>
        Write($"job started id={job.Id}");

    public void JobCompleted(QueueJob job) =>
        Write($"job completed id={job.Id}");

    public void JobFailed(QueueJob job, string errorMessage) =>
        Write($"job failed id={job.Id} error=\"{errorMessage}\"");

    private void Write(string message)
    {
        var directory = Path.GetDirectoryName(LogFilePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var line = $"{DateTimeOffset.Now:O} {message}{Environment.NewLine}";
        File.AppendAllText(LogFilePath, line);
    }
}


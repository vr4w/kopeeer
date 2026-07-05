using Kopeeer.Core;

namespace Kopeeer.Worker;

public sealed class FileOperationProcessor
{
    private const int BufferSize = 1024 * 1024;

    public Task ProcessAsync(
        QueueJob job,
        Action<QueueJob>? onProgress = null,
        CancellationToken cancellationToken = default) =>
        Task.Run(() => Process(job, onProgress, cancellationToken), cancellationToken);

    private static void Process(QueueJob job, Action<QueueJob>? onProgress, CancellationToken cancellationToken)
    {
        if (!File.Exists(job.SourcePath) && !Directory.Exists(job.SourcePath))
        {
            throw new FileNotFoundException("Source file or folder does not exist.", job.SourcePath);
        }

        if (!Directory.Exists(job.TargetFolder))
        {
            throw new DirectoryNotFoundException($"Target folder does not exist: {job.TargetFolder}");
        }

        if (File.Exists(job.SourcePath))
        {
            job.TotalBytes = new FileInfo(job.SourcePath).Length;
            job.TransferredBytes = 0;
            ProcessFile(job, onProgress, cancellationToken);
            return;
        }

        job.TotalBytes = GetDirectorySize(job.SourcePath);
        job.TransferredBytes = 0;
        ProcessDirectory(job, onProgress, cancellationToken);
    }

    private static void ProcessFile(QueueJob job, Action<QueueJob>? onProgress, CancellationToken cancellationToken)
    {
        var fileName = Path.GetFileName(job.SourcePath);
        var targetPath = Path.Combine(job.TargetFolder, fileName);
        EnsureTargetDoesNotExist(targetPath);

        CopyFileWithProgress(job.SourcePath, targetPath, job, onProgress, cancellationToken);

        if (job.OperationType == FileOperationType.Move)
        {
            File.Delete(job.SourcePath);
        }
    }

    private static void ProcessDirectory(QueueJob job, Action<QueueJob>? onProgress, CancellationToken cancellationToken)
    {
        var directoryName = Path.GetFileName(
            job.SourcePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        var targetPath = Path.Combine(job.TargetFolder, directoryName);
        EnsureTargetDoesNotExist(targetPath);
        EnsureDirectoryTargetIsNotInsideSource(job.SourcePath, targetPath);

        CopyDirectory(job.SourcePath, targetPath, job, onProgress, cancellationToken);

        if (job.OperationType == FileOperationType.Move)
        {
            Directory.Delete(job.SourcePath, recursive: true);
        }
    }

    private static void CopyDirectory(
        string sourceDirectory,
        string targetDirectory,
        QueueJob job,
        Action<QueueJob>? onProgress,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(targetDirectory);

        foreach (var file in Directory.EnumerateFiles(sourceDirectory))
        {
            var targetFile = Path.Combine(targetDirectory, Path.GetFileName(file));
            CopyFileWithProgress(file, targetFile, job, onProgress, cancellationToken);
        }

        foreach (var directory in Directory.EnumerateDirectories(sourceDirectory))
        {
            var targetSubdirectory = Path.Combine(targetDirectory, Path.GetFileName(directory));
            CopyDirectory(directory, targetSubdirectory, job, onProgress, cancellationToken);
        }
    }

    private static void CopyFileWithProgress(
        string sourceFile,
        string targetFile,
        QueueJob job,
        Action<QueueJob>? onProgress,
        CancellationToken cancellationToken)
    {
        job.CurrentItem = Path.GetFileName(sourceFile);
        onProgress?.Invoke(job);

        using var source = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize);
        using var target = new FileStream(targetFile, FileMode.CreateNew, FileAccess.Write, FileShare.None, BufferSize);
        var buffer = new byte[BufferSize];
        var started = DateTimeOffset.Now;
        var lastProgress = DateTimeOffset.MinValue;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var read = source.Read(buffer, 0, buffer.Length);
            if (read == 0)
            {
                break;
            }

            target.Write(buffer, 0, read);
            job.TransferredBytes += read;
            var elapsed = Math.Max((DateTimeOffset.Now - started).TotalSeconds, 0.001);
            job.BytesPerSecond = job.TransferredBytes / elapsed;

            if ((DateTimeOffset.Now - lastProgress).TotalMilliseconds >= 120)
            {
                lastProgress = DateTimeOffset.Now;
                onProgress?.Invoke(job);
            }
        }

        onProgress?.Invoke(job);
    }

    private static long GetDirectorySize(string directory)
    {
        long total = 0;
        foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
        {
            total += new FileInfo(file).Length;
        }

        return total;
    }

    private static void EnsureTargetDoesNotExist(string targetPath)
    {
        if (File.Exists(targetPath) || Directory.Exists(targetPath))
        {
            throw new IOException($"Target already exists: {targetPath}");
        }
    }

    private static void EnsureDirectoryTargetIsNotInsideSource(string sourceDirectory, string targetDirectory)
    {
        var sourceFullPath = Path.GetFullPath(sourceDirectory)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        var targetFullPath = Path.GetFullPath(targetDirectory)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;

        if (targetFullPath.StartsWith(sourceFullPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new IOException("Target folder cannot be inside the source folder.");
        }
    }
}

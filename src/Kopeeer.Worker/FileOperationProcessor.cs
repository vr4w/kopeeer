using Kopeeer.Core;

namespace Kopeeer.Worker;

public sealed class FileOperationProcessor(IJobLogger? logger = null)
{
    private const int BufferSize = 1024 * 1024;
    private readonly IJobLogger? _logger = logger;

    public Task ProcessAsync(
        QueueJob job,
        Action<QueueJob>? onProgress = null,
        Func<QueueJob, string, TargetConflictResolution>? onTargetConflict = null,
        CancellationToken cancellationToken = default) =>
        Task.Run(() => Process(job, onProgress, onTargetConflict, cancellationToken), cancellationToken);

    private void Process(
        QueueJob job,
        Action<QueueJob>? onProgress,
        Func<QueueJob, string, TargetConflictResolution>? onTargetConflict,
        CancellationToken cancellationToken)
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
            ProcessFile(job, onProgress, onTargetConflict, cancellationToken);
            return;
        }

        job.TotalBytes = GetDirectorySize(job.SourcePath);
        job.TransferredBytes = 0;
        ProcessDirectory(job, onProgress, onTargetConflict, cancellationToken);
    }

    private void ProcessFile(
        QueueJob job,
        Action<QueueJob>? onProgress,
        Func<QueueJob, string, TargetConflictResolution>? onTargetConflict,
        CancellationToken cancellationToken)
    {
        var fileName = Path.GetFileName(job.SourcePath);
        var targetPath = ResolveTargetPath(job, Path.Combine(job.TargetFolder, fileName), onTargetConflict, cancellationToken);

        CopyFileWithProgress(job.SourcePath, targetPath, job, onProgress, cancellationToken);

        if (job.OperationType == FileOperationType.Move)
        {
            File.Delete(job.SourcePath);
        }
    }

    private void ProcessDirectory(
        QueueJob job,
        Action<QueueJob>? onProgress,
        Func<QueueJob, string, TargetConflictResolution>? onTargetConflict,
        CancellationToken cancellationToken)
    {
        var directoryName = Path.GetFileName(
            job.SourcePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        var targetPath = ResolveTargetPath(job, Path.Combine(job.TargetFolder, directoryName), onTargetConflict, cancellationToken);
        EnsureDirectoryTargetIsNotInsideSource(job.SourcePath, targetPath);

        CopyDirectory(job.SourcePath, targetPath, job, onProgress, cancellationToken);

        if (job.OperationType == FileOperationType.Move)
        {
            Directory.Delete(job.SourcePath, recursive: true);
        }
    }

    private void CopyDirectory(
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

    private void CopyFileWithProgress(
        string sourceFile,
        string targetFile,
        QueueJob job,
        Action<QueueJob>? onProgress,
        CancellationToken cancellationToken)
    {
        job.CurrentItem = Path.GetFileName(sourceFile);
        onProgress?.Invoke(job);

        var temporaryTargetFile = BuildTemporaryTargetPath(targetFile);

        try
        {
            using (var source = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize))
            using (var target = new FileStream(temporaryTargetFile, FileMode.CreateNew, FileAccess.Write, FileShare.None, BufferSize))
            {
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

                target.Flush(flushToDisk: true);
            }

            File.Move(temporaryTargetFile, targetFile);
            onProgress?.Invoke(job);
        }
        catch (Exception exception)
        {
            if (File.Exists(temporaryTargetFile))
            {
                TryDeleteTemporaryFile(temporaryTargetFile, exception);
            }

            throw;
        }
    }

    private static string BuildTemporaryTargetPath(string targetFile)
    {
        var directory = Path.GetDirectoryName(targetFile) ?? string.Empty;
        var fileName = Path.GetFileName(targetFile);
        var candidate = Path.Combine(directory, $"{fileName}.kopeeer-part");

        if (!File.Exists(candidate) && !Directory.Exists(candidate))
        {
            return candidate;
        }

        return Path.Combine(directory, $"{fileName}.{Guid.NewGuid():N}.kopeeer-part");
    }

    private void TryDeleteTemporaryFile(string temporaryTargetFile, Exception originalException)
    {
        try
        {
            File.Delete(temporaryTargetFile);
        }
        catch (Exception cleanupException)
        {
            _logger?.CleanupFailed(temporaryTargetFile, cleanupException.Message);
            throw new IOException(
                $"{originalException.Message} Also failed to remove temporary file: {temporaryTargetFile}. {cleanupException.Message}",
                originalException);
        }
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

    private static string ResolveTargetPath(
        QueueJob job,
        string targetPath,
        Func<QueueJob, string, TargetConflictResolution>? onTargetConflict,
        CancellationToken cancellationToken)
    {
        while (File.Exists(targetPath) || Directory.Exists(targetPath))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var resolution = onTargetConflict?.Invoke(job, targetPath) ?? TargetConflictResolution.Cancel;
            targetPath = resolution switch
            {
                TargetConflictResolution.Rename => BuildUniqueTargetPath(targetPath),
                TargetConflictResolution.Skip => throw new OperationSkippedException($"Skipped existing target: {targetPath}"),
                TargetConflictResolution.Cancel => throw new OperationCanceledException(cancellationToken),
                _ => throw new OperationCanceledException(cancellationToken)
            };
        }

        return targetPath;
    }

    private static string BuildUniqueTargetPath(string targetPath)
    {
        var directory = Path.GetDirectoryName(targetPath);
        var fileName = Path.GetFileNameWithoutExtension(targetPath);
        var extension = Path.GetExtension(targetPath);

        if (Directory.Exists(targetPath))
        {
            fileName = Path.GetFileName(targetPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            extension = string.Empty;
        }

        for (var index = 2; index < 10_000; index++)
        {
            var candidate = Path.Combine(directory ?? string.Empty, $"{fileName} ({index}){extension}");
            if (!File.Exists(candidate) && !Directory.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new IOException($"Could not create a unique target name for: {targetPath}");
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

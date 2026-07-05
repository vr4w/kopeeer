using Kopeeer.Core;

namespace Kopeeer.Worker;

public sealed class FileOperationProcessor
{
    public Task ProcessAsync(QueueJob job, CancellationToken cancellationToken = default) =>
        Task.Run(() => Process(job), cancellationToken);

    private static void Process(QueueJob job)
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
            ProcessFile(job);
            return;
        }

        ProcessDirectory(job);
    }

    private static void ProcessFile(QueueJob job)
    {
        var fileName = Path.GetFileName(job.SourcePath);
        var targetPath = Path.Combine(job.TargetFolder, fileName);
        EnsureTargetDoesNotExist(targetPath);

        File.Copy(job.SourcePath, targetPath, overwrite: false);

        if (job.OperationType == FileOperationType.Move)
        {
            File.Delete(job.SourcePath);
        }
    }

    private static void ProcessDirectory(QueueJob job)
    {
        var directoryName = Path.GetFileName(
            job.SourcePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        var targetPath = Path.Combine(job.TargetFolder, directoryName);
        EnsureTargetDoesNotExist(targetPath);
        EnsureDirectoryTargetIsNotInsideSource(job.SourcePath, targetPath);

        CopyDirectory(job.SourcePath, targetPath);

        if (job.OperationType == FileOperationType.Move)
        {
            Directory.Delete(job.SourcePath, recursive: true);
        }
    }

    private static void CopyDirectory(string sourceDirectory, string targetDirectory)
    {
        Directory.CreateDirectory(targetDirectory);

        foreach (var file in Directory.EnumerateFiles(sourceDirectory))
        {
            var targetFile = Path.Combine(targetDirectory, Path.GetFileName(file));
            File.Copy(file, targetFile, overwrite: false);
        }

        foreach (var directory in Directory.EnumerateDirectories(sourceDirectory))
        {
            var targetSubdirectory = Path.Combine(targetDirectory, Path.GetFileName(directory));
            CopyDirectory(directory, targetSubdirectory);
        }
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

using Kopeeer.Core;

namespace Kopeeer.App;

public sealed record StartupQueueRequest(
    FileOperationType OperationType,
    string[] SourcePaths,
    string? TargetFolder,
    bool PickTarget)
{
    public static StartupQueueRequest? TryParse(IReadOnlyList<string> args)
    {
        if (args.Count == 0 || !args.Contains("--enqueue", StringComparer.OrdinalIgnoreCase))
        {
            return TryParseLegacy(args);
        }

        FileOperationType? operationType = null;
        string? targetFolder = null;
        var pickTarget = false;
        var sources = new List<string>();

        for (var index = 0; index < args.Count; index++)
        {
            var arg = args[index];
            if (arg.Equals("--operation", StringComparison.OrdinalIgnoreCase) && index + 1 < args.Count)
            {
                operationType = args[++index].ToLowerInvariant() switch
                {
                    "copy" => FileOperationType.Copy,
                    "move" => FileOperationType.Move,
                    _ => null
                };
                continue;
            }

            if (arg.Equals("--target", StringComparison.OrdinalIgnoreCase) && index + 1 < args.Count)
            {
                targetFolder = args[++index].Trim();
                continue;
            }

            if (arg.Equals("--pick-target", StringComparison.OrdinalIgnoreCase))
            {
                pickTarget = true;
                continue;
            }

            if (arg.Equals("--sources", StringComparison.OrdinalIgnoreCase))
            {
                while (index + 1 < args.Count && !args[index + 1].StartsWith("--", StringComparison.Ordinal))
                {
                    sources.Add(args[++index].Trim());
                }
            }
        }

        var cleanSources = sources.Where(source => source.Length > 0).ToArray();
        if (operationType is null || cleanSources.Length == 0 || (!pickTarget && string.IsNullOrWhiteSpace(targetFolder)))
        {
            return null;
        }

        return new StartupQueueRequest(operationType.Value, cleanSources, targetFolder, pickTarget);
    }

    private static StartupQueueRequest? TryParseLegacy(IReadOnlyList<string> args)
    {
        if (args.Count < 2)
        {
            return null;
        }

        var operationType = args[0] switch
        {
            "--queue-copy" => FileOperationType.Copy,
            "--queue-move" => FileOperationType.Move,
            _ => (FileOperationType?)null
        };

        if (operationType is null)
        {
            return null;
        }

        var sourcePath = args[1].Trim();
        return sourcePath.Length == 0
            ? null
            : new StartupQueueRequest(operationType.Value, [sourcePath], TargetFolder: null, PickTarget: true);
    }
}
